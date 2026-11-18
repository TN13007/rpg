#!/usr/bin/env python3
"""Validate local Markdown links, heading anchors and active Docs navigation."""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
import unicodedata
from collections import deque
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
LINK_PATTERN = re.compile(r"!?(?:\[[^\]]*\])\((<[^>]+>|[^)\s]+)(?:\s+[^)]*)?\)")
HEADING_PATTERN = re.compile(r"^#{1,6}\s+(.+?)\s*#*\s*$")
FENCE_PATTERN = re.compile(r"^\s*(`{3,}|~{3,})")


def tracked_markdown_files(root: Path) -> set[str]:
    """Return tracked Markdown paths using Git's repository index."""
    result = subprocess.run(
        ["git", "ls-files", "*.md", "*.MD"],
        cwd=root,
        text=True,
        capture_output=True,
        check=True,
    )
    return {path.replace("\\", "/") for path in result.stdout.splitlines()}


def strip_code_blocks(markdown: str) -> list[str]:
    """Remove fenced code blocks so examples are not interpreted as navigation."""
    output: list[str] = []
    fence_char = ""
    fence_length = 0
    for line in markdown.splitlines():
        match = FENCE_PATTERN.match(line)
        if match:
            marker = match.group(1)
            if not fence_char:
                fence_char, fence_length = marker[0], len(marker)
            elif marker[0] == fence_char and len(marker) >= fence_length:
                fence_char, fence_length = "", 0
            continue
        if not fence_char:
            output.append(line)
    return output


def heading_slug(text: str) -> str:
    """Create a GitHub-style heading slug for ordinary Markdown headings."""
    text = re.sub(r"\x60([^\x60]+)\x60", r"\1", text)
    text = re.sub(r"!?\[([^\]]+)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"<[^>]+>", "", text)
    text = text.replace("*", "").replace("_", "")
    text = unicodedata.normalize("NFKC", text).lower()
    text = re.sub(r"[^\w\- ]", "", text, flags=re.UNICODE)
    return re.sub(r"[\s]+", "-", text).strip("-")


def heading_anchors(markdown: str) -> set[str]:
    anchors: set[str] = set()
    counts: dict[str, int] = {}
    for line in strip_code_blocks(markdown):
        match = HEADING_PATTERN.match(line)
        if not match:
            continue
        base = heading_slug(match.group(1))
        count = counts.get(base, 0)
        counts[base] = count + 1
        anchors.add(base if count == 0 else f"{base}-{count}")
    return anchors


def markdown_links(markdown: str):
    for line_number, line in enumerate(strip_code_blocks(markdown), start=1):
        for match in LINK_PATTERN.finditer(line):
            yield line_number, match.group(1).strip("<>")


def resolve_local_target(root: Path, source: Path, target: str, tracked: set[str]):
    """Resolve a local Markdown target; return (path, fragment, error)."""
    parsed = urlsplit(target)
    if parsed.scheme or parsed.netloc:
        return None, "", None
    raw_path = unquote(parsed.path)
    fragment = unquote(parsed.fragment)
    if raw_path:
        candidate = (source.parent / raw_path).resolve()
        try:
            relative = candidate.relative_to(root.resolve()).as_posix()
        except ValueError:
            return None, fragment, "target escapes the repository"
        if relative not in tracked:
            return None, fragment, f"file does not exist or is not tracked: {relative}"
        return candidate, fragment, None
    return source, fragment, None


def validate_markdown(root: Path, tracked_files: set[str]) -> list[str]:
    """Validate local file links and resolvable heading fragments."""
    issues: list[str] = []
    root = root.resolve()
    for relative in sorted(path for path in tracked_files if path.lower().endswith(".md")):
        source = root / relative
        if not source.is_file():
            continue
        markdown = source.read_text(encoding="utf-8")
        for line_number, target in markdown_links(markdown):
            destination, fragment, error = resolve_local_target(root, source, target, tracked_files)
            if error:
                issues.append(f"{relative}:{line_number}: {target}: {error}")
                continue
            if fragment and destination and destination.suffix.lower() == ".md":
                target_text = destination.read_text(encoding="utf-8")
                if fragment not in heading_anchors(target_text):
                    issues.append(
                        f"{relative}:{line_number}: {target}: heading anchor not found: #{fragment}"
                    )
    return issues


def active_document_orphans(root: Path, tracked_files: set[str]) -> list[str]:
    """Find active Docs Markdown files unreachable from README navigation."""
    root = root.resolve()
    active_docs = {
        path for path in tracked_files
        if path.startswith("Docs/") and path.lower().endswith(".md")
        and not path.startswith("Docs/Archive/")
    }
    graph: dict[str, set[str]] = {}
    for relative in tracked_files:
        if not relative.lower().endswith(".md") or relative.startswith("Docs/Archive/"):
            continue
        source = root / relative
        if not source.is_file():
            continue
        targets: set[str] = set()
        for _, target in markdown_links(source.read_text(encoding="utf-8")):
            destination, _, error = resolve_local_target(root, source, target, tracked_files)
            if not error and destination:
                try:
                    resolved = destination.relative_to(root).as_posix()
                except ValueError:
                    continue
                if resolved in active_docs:
                    targets.add(resolved)
        graph[relative] = targets

    reachable: set[str] = set()
    queue = deque(["README.md"])
    while queue:
        current = queue.popleft()
        if current in reachable:
            continue
        reachable.add(current)
        queue.extend(graph.get(current, set()) - reachable)
    return sorted(active_docs - reachable)


def validate(root: Path, tracked_files: set[str]) -> list[str]:
    issues = validate_markdown(root, tracked_files)
    issues.extend(
        f"{path}: active document is not reachable from README.md navigation"
        for path in active_document_orphans(root, tracked_files)
    )
    return issues


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT, help=argparse.SUPPRESS)
    args = parser.parse_args()
    try:
        tracked = tracked_markdown_files(args.root)
        issues = validate(args.root, tracked)
    except (OSError, subprocess.CalledProcessError) as error:
        print(f"Markdown validation could not run: {error}", file=sys.stderr)
        return 2
    if issues:
        print("Markdown documentation validation failed:")
        for issue in issues:
            print(f"- {issue}")
        return 1
    print("Markdown documentation validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
