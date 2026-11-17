#!/usr/bin/env python3
"""Validate project-specific naming rules for changed files and PR branches."""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from collections import Counter
from pathlib import PurePosixPath
from typing import Any

PASCAL_CASE = re.compile(r"^[A-Z][A-Za-z0-9]*$")
DOTNET_PROJECT_NAME = re.compile(r"^[A-Z][A-Za-z0-9]*(?:\.[A-Z][A-Za-z0-9]*)+$")
CAMEL_CASE = re.compile(r"^[a-z][A-Za-z0-9]*$")
SNAKE_CASE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
BRANCH_NAME = re.compile(
    r"^(feat|fix|docs|test|refactor|chore)/issue-[1-9][0-9]*-[a-z0-9]+(?:-[a-z0-9]+)*$"
)

ENTRY_FILE_NAMES = {
    "README.md",
    "CONTRIBUTING.md",
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    "project.godot",
    "icon.svg",
    "icon.svg.import",
}
PLATFORM_DIRECTORY_NAMES = {".github", ".godot"}
SIDECAR_SUFFIXES = {".uid", ".import"}
ID_PROPERTY_NAMES = {"id", "exits"}
ENUM_PROPERTY_NAMES = {"type", "timeMode", "stat"}


def git(*args: str, check: bool = True) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *args], text=True, capture_output=True, check=check
    )


def changed_paths(base_ref: str, head_ref: str) -> list[tuple[str, str | None, str]]:
    """Return (status, new_path, old_path) entries from a NUL-delimited Git diff."""
    result = git("diff", "--name-status", "-z", f"{base_ref}...{head_ref}")
    parts = result.stdout.split("\0")
    entries: list[tuple[str, str | None, str]] = []
    index = 0

    while index < len(parts) and parts[index]:
        status = parts[index]
        index += 1

        if status.startswith(("R", "C")):
            if index + 1 >= len(parts):
                raise ValueError("Malformed rename/copy entry in git diff output.")
            old_path = parts[index]
            new_path = parts[index + 1]
            index += 2
            entries.append((status[0], new_path, old_path))
        else:
            if index >= len(parts):
                raise ValueError("Malformed file entry in git diff output.")
            path = parts[index]
            index += 1
            entries.append((status[0], path, None))

    return entries


def base_file_text(base_ref: str, path: str | None) -> str | None:
    if not path:
        return None
    result = git("show", f"{base_ref}:{path}", check=False)
    return result.stdout if result.returncode == 0 else None


def is_pascal_case(value: str) -> bool:
    return bool(PASCAL_CASE.fullmatch(value))


def is_dotnet_project_name(value: str) -> bool:
    """Accept dot-separated PascalCase segments used by .NET project names."""
    return bool(DOTNET_PROJECT_NAME.fullmatch(value))


def is_valid_directory_name(value: str) -> bool:
    return is_pascal_case(value) or is_dotnet_project_name(value)


def is_valid_file_stem(filename: str, stem: str) -> bool:
    if is_pascal_case(stem):
        return True
    return (
        PurePosixPath(filename).suffix.lower() == ".csproj"
        and is_dotnet_project_name(stem)
    )


def is_camel_case(value: str) -> bool:
    return bool(CAMEL_CASE.fullmatch(value))


def is_snake_case(value: str) -> bool:
    return bool(SNAKE_CASE.fullmatch(value))


def is_identifier_property(key: str) -> bool:
    return key in ID_PROPERTY_NAMES or key.endswith(("Id", "Ids"))


def _strip_identifier_properties(value: Any) -> Any:
    """Remove IDs/references so the remaining object can identify its owner."""
    if isinstance(value, dict):
        return {
            key: _strip_identifier_properties(child)
            for key, child in value.items()
            if not is_identifier_property(key)
        }
    if isinstance(value, list):
        normalized = [_strip_identifier_properties(child) for child in value]
        return sorted(
            normalized,
            key=lambda item: json.dumps(
                item, sort_keys=True, ensure_ascii=False, separators=(",", ":")
            ),
        )
    return value


def _stable_signature(value: Any) -> str:
    normalized = _strip_identifier_properties(value)
    return json.dumps(
        normalized, sort_keys=True, ensure_ascii=False, separators=(",", ":")
    )


def _owner_identity_signature(value: dict[str, Any]) -> str:
    """Prefer stable authored labels; avoid treating wrapper objects as entities."""
    identity = {
        key: value[key]
        for key in ("name", "label", "title")
        if isinstance(value.get(key), str)
    }
    if identity:
        return json.dumps(
            identity, sort_keys=True, ensure_ascii=False, separators=(",", ":")
        )

    if any(is_identifier_property(key) for key in value):
        return _stable_signature(value)
    return ""


def collect_id_occurrences(
    value: Any, path: str = "$"
) -> list[tuple[str, str, str, str]]:
    """Collect (owner path, owner signature, field name, value) as stable tuples.

    Array indexes are intentionally normalized to [*], so moving an entity in an
    array does not make its unchanged ID look new. The non-ID fields identify the
    owning object, preventing an ID on one object from being borrowed by another.
    """
    occurrences: list[tuple[str, str, str, str]] = []

    def visit(node: Any, node_path: str, ancestor_identity: str = "") -> None:
        if isinstance(node, dict):
            local_identity = _owner_identity_signature(node)
            owner_signature = f"{ancestor_identity}/{local_identity}"
            for key, child in node.items():
                if not is_identifier_property(key):
                    continue
                if isinstance(child, str):
                    occurrences.append((node_path, owner_signature, key, child))
                elif isinstance(child, list):
                    for item in child:
                        if isinstance(item, str):
                            occurrences.append((node_path, owner_signature, key, item))

            for key, child in node.items():
                visit(child, f"{node_path}.{key}", owner_signature)
        elif isinstance(node, list):
            for child in node:
                visit(child, f"{node_path}[*]", ancestor_identity)

    visit(value, path)
    return occurrences


def collect_property_occurrences(value: Any) -> list[tuple[str, str]]:
    """Collect (parent path, property name), normalizing array indexes."""
    occurrences: list[tuple[str, str]] = []

    def visit(node: Any, node_path: str) -> None:
        if isinstance(node, dict):
            for key, child in node.items():
                occurrences.append((node_path, key))
                visit(child, f"{node_path}.{key}")
        elif isinstance(node, list):
            for child in node:
                visit(child, f"{node_path}[*]")

    visit(value, "$")
    return occurrences


def validate_json_document(
    path: str, current_text: str, previous_text: str | None
) -> list[str]:
    issues: list[str] = []

    try:
        current = json.loads(current_text)
    except json.JSONDecodeError as error:
        return [
            f"{path}: invalid JSON at line {error.lineno}, "
            f"column {error.colno}: {error.msg}"
        ]

    previous: Any = None
    if previous_text:
        try:
            previous = json.loads(previous_text)
        except json.JSONDecodeError:
            # An invalid previous document cannot provide a reliable baseline.
            previous = None

    previous_properties = Counter(
        collect_property_occurrences(previous) if previous is not None else []
    )
    for parent_path, key in collect_property_occurrences(current):
        occurrence = (parent_path, key)
        if previous_properties[occurrence] > 0:
            previous_properties[occurrence] -= 1
            continue
        if not is_camel_case(key):
            issues.append(
                f"{path}:{parent_path}: new or renamed JSON property '{key}' "
                "must use camelCase."
            )

    for node in _walk_json(current):
        for key, child in node.items():
            if (
                key in ENUM_PROPERTY_NAMES
                and isinstance(child, str)
                and not is_pascal_case(child)
            ):
                issues.append(
                    f"{path}: enum-like value at property '{key}' "
                    f"('{child}') must use PascalCase."
                )

    previous_ids = Counter(
        collect_id_occurrences(previous) if previous is not None else []
    )
    for owner_path, owner_signature, key, identifier in collect_id_occurrences(current):
        occurrence = (owner_path, owner_signature, key, identifier)
        if previous_ids[occurrence] > 0:
            previous_ids[occurrence] -= 1
            continue
        if not is_snake_case(identifier):
            issues.append(
                f"{path}:{owner_path}.{key}: new or changed ID/reference "
                f"'{identifier}' must use snake_case."
            )

    return issues


def _walk_json(value: Any):
    if isinstance(value, dict):
        yield value
        for child in value.values():
            yield from _walk_json(child)
    elif isinstance(value, list):
        for child in value:
            yield from _walk_json(child)


def _file_name_stem(filename: str) -> str:
    """Return the logical name before its extension and Godot sidecar suffixes."""
    name = filename
    while True:
        suffix = PurePosixPath(name).suffix
        if suffix not in SIDECAR_SUFFIXES:
            break
        name = name[: -len(suffix)]

    suffix = PurePosixPath(name).suffix
    if suffix:
        name = name[: -len(suffix)]
    return name


def _suggest_pascal_case_name(value: str) -> str | None:
    """Suggest PascalCase only when separators make the conversion unambiguous."""
    segments = re.split(r"[_-]+", value)
    if len(segments) < 2 or any(
        not re.fullmatch(r"[A-Za-z0-9]+", segment) for segment in segments
    ):
        return None
    return "".join(segment[0].upper() + segment[1:].lower() for segment in segments)


def _suggested_file_name(filename: str) -> str | None:
    """Suggest a renamed file while preserving source and Godot sidecar suffixes."""
    stem = _file_name_stem(filename)
    suggested_stem = _suggest_pascal_case_name(stem)
    if suggested_stem is None:
        return None
    return suggested_stem + filename[len(stem) :]


def tracked_paths() -> list[str]:
    """Return all paths currently tracked by Git."""
    result = git("ls-files", "-z")
    return [path for path in result.stdout.split("\0") if path]


def audit_tracked_paths(paths: list[str]) -> tuple[list[str], int]:
    """Audit every tracked path, including unchanged legacy names.

    Directory findings are emitted once even when several tracked files share the
    same directory. Platform-managed trees and repository entry files keep their
    existing exceptions.
    """
    issues: list[str] = []
    checked_directories: set[str] = set()

    for path in sorted(set(paths)):
        parts = PurePosixPath(path).parts
        if not parts or any(part in PLATFORM_DIRECTORY_NAMES for part in parts[:-1]):
            continue

        for index, directory in enumerate(parts[:-1]):
            if directory in PLATFORM_DIRECTORY_NAMES:
                continue

            directory_path = "/".join(parts[: index + 1])
            if directory_path in checked_directories:
                continue
            checked_directories.add(directory_path)

            if not is_valid_directory_name(directory):
                suggestion = _suggest_pascal_case_name(directory)
                suggestion_text = (
                    f" Suggested directory name: '{suggestion}'."
                    if suggestion
                    else ""
                )
                issues.append(
                    f"{path}: directory '{directory_path}' must use PascalCase."
                    f"{suggestion_text}"
                )

        filename = parts[-1]
        if filename in ENTRY_FILE_NAMES:
            continue

        stem = _file_name_stem(filename)
        if not is_valid_file_stem(filename, stem):
            suggested_name = _suggested_file_name(filename)
            suggestion_text = ""
            if suggested_name:
                suggested_path = str(PurePosixPath(path).with_name(suggested_name))
                suggestion_text = f" Suggested path: '{suggested_path}'."
            issues.append(
                f"{path}: logical file name '{stem}' must use PascalCase."
                f"{suggestion_text}"
            )

    return issues, len(checked_directories)


def path_naming_issues(base_ref: str, status: str, path: str) -> list[str]:
    """Check newly added/renamed paths without revalidating unchanged legacy paths."""
    if status not in {"A", "R", "C"}:
        return []

    parts = PurePosixPath(path).parts
    if not parts:
        return []

    # GitHub workflow/template names and Godot's generated cache have their own
    # conventions; do not apply the project's PascalCase rule inside these trees.
    if any(part in PLATFORM_DIRECTORY_NAMES for part in parts[:-1]):
        return []

    issues: list[str] = []

    for index, directory in enumerate(parts[:-1]):
        if directory in PLATFORM_DIRECTORY_NAMES:
            continue
        directory_path = "/".join(parts[: index + 1])
        exists_in_base = (
            git("cat-file", "-e", f"{base_ref}:{directory_path}", check=False).returncode
            == 0
        )
        if not exists_in_base and not is_valid_directory_name(directory):
            suggestion = _suggest_pascal_case_name(directory)
            suggestion_text = (
                f" Suggested directory name: '{suggestion}'." if suggestion else ""
            )
            issues.append(
                f"{path}: new directory '{directory_path}' must use PascalCase."
                f"{suggestion_text}"
            )

    filename = parts[-1]
    if filename in ENTRY_FILE_NAMES:
        return issues

    stem = _file_name_stem(filename)
    if not is_valid_file_stem(filename, stem):
        suggested_name = _suggested_file_name(filename)
        suggestion_text = (
            f" Suggested file name: '{suggested_name}'." if suggested_name else ""
        )
        issues.append(
            f"{path}: new or renamed file '{filename}' must use PascalCase."
            f"{suggestion_text}"
        )

    return issues

def validate_branch_name(branch: str) -> list[str]:
    if BRANCH_NAME.fullmatch(branch):
        return []
    return [
        f"Branch '{branch}' must match "
        "'<type>/issue-<number>-<kebab-case-description>'. "
        "Allowed types: feat, fix, docs, test, refactor, chore."
    ]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base", help="Base commit/ref for the diff.")
    parser.add_argument("--head", help="Head commit/ref for the diff.")
    parser.add_argument(
        "--audit-all",
        action="store_true",
        help="Audit every Git-tracked path, including unchanged legacy paths.",
    )
    parser.add_argument(
        "--report-only",
        action="store_true",
        help="Print audit findings without returning a failure status.",
    )
    parser.add_argument("--branch", default="", help="Pull request source branch name.")
    parser.add_argument(
        "--check-branch", action="store_true", help="Validate the branch name."
    )
    args = parser.parse_args()

    if args.report_only and not args.audit_all:
        parser.error("--report-only requires --audit-all.")

    issues: list[str] = []

    if args.check_branch:
        issues.extend(validate_branch_name(args.branch))

    if args.audit_all:
        try:
            paths = tracked_paths()
        except (subprocess.CalledProcessError, OSError) as error:
            details = getattr(error, "stderr", None) or str(error)
            print(details or "Could not list Git-tracked paths.", file=sys.stderr)
            return 2

        audit_issues, directory_count = audit_tracked_paths(paths)
        print(
            "Full repository path naming audit: "
            f"{len(paths)} tracked files and {directory_count} directories checked."
        )
        issues.extend(audit_issues)
        if issues:
            print(f"Project naming audit found {len(issues)} violation(s):")
            for issue in issues:
                print(f"- {issue}")
            if args.report_only:
                print("Audit report only: findings do not fail this run.")
                return 0
            return 1

        print("Full repository path naming audit passed.")
        return 0

    if not args.base or not args.head:
        parser.error("--base and --head are required unless --audit-all is used.")

    try:
        changes = changed_paths(args.base, args.head)
    except (subprocess.CalledProcessError, ValueError) as error:
        details = getattr(error, "stderr", None) or str(error)
        print(details or "Could not calculate changed paths.", file=sys.stderr)
        return 2

    for status, path, old_path in changes:
        if not path:
            continue

        issues.extend(path_naming_issues(args.base, status, path))

        if path.lower().endswith(".json") and status != "D":
            try:
                with open(path, "r", encoding="utf-8") as source:
                    current_text = source.read()
            except OSError as error:
                issues.append(f"{path}: cannot read JSON file: {error}")
                continue
            previous_text = base_file_text(args.base, old_path or path)
            issues.extend(validate_json_document(path, current_text, previous_text))

    if issues:
        print("Project naming validation failed:")
        for issue in issues:
            print(f"- {issue}")
        return 1

    print(f"Project naming validation passed ({len(changes)} changed paths checked).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
