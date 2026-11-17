#!/usr/bin/env python3
"""Validate project-specific naming rules for changed files and PR branches."""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import PurePosixPath
from typing import Any

PASCAL_CASE = re.compile(r"^[A-Z][A-Za-z0-9]*$")
CAMEL_CASE = re.compile(r"^[a-z][A-Za-z0-9]*$")
SNAKE_CASE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
BRANCH_NAME = re.compile(
    r"^(feat|fix|docs|test|refactor|chore)/issue-[1-9][0-9]*-[a-z0-9]+(?:-[a-z0-9]+)*$"
)

CHECKED_FILE_SUFFIXES = {
    ".cs", ".tscn", ".tres", ".res", ".svg", ".json", ".md", ".py", ".yml", ".yaml"
}
ENTRY_FILE_NAMES = {"README.md", "CONTRIBUTING.md"}
PLATFORM_DIRECTORY_NAMES = {".github", "ISSUE_TEMPLATE", "workflows", "RPG.Tests"}
ID_PROPERTY_NAMES = {"id", "exits"}
ENUM_PROPERTY_NAMES = {"type", "timeMode", "stat"}


def git(*args: str, check: bool = True) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *args], text=True, capture_output=True, check=check
    )


def changed_paths(base_ref: str, head_ref: str) -> list[tuple[str, str | None, str]]:
    """Return (status, new_path, old_path) for changed entries."""
    result = git("diff", "--name-status", "-z", f"{base_ref}...{head_ref}")
    parts = result.stdout.split("\0")
    entries: list[tuple[str, str | None, str]] = []
    index = 0

    while index < len(parts) and parts[index]:
        status = parts[index]
        index += 1

        if status.startswith(("R", "C")):
            old_path = parts[index]
            new_path = parts[index + 1]
            index += 2
            entries.append((status[0], new_path, old_path))
        else:
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


def is_camel_case(value: str) -> bool:
    return bool(CAMEL_CASE.fullmatch(value))


def is_snake_case(value: str) -> bool:
    return bool(SNAKE_CASE.fullmatch(value))


def collect_id_values(
    value: Any, path: str = "$", result: dict[str, str] | None = None
) -> dict[str, str]:
    if result is None:
        result = {}

    if isinstance(value, dict):
        for key, child in value.items():
            child_path = f"{path}.{key}"
            if key in ID_PROPERTY_NAMES or key.endswith(("Id", "Ids")):
                if isinstance(child, str):
                    result[child_path] = child
                elif isinstance(child, list):
                    for index, item in enumerate(child):
                        if isinstance(item, str):
                            result[f"{child_path}[{index}]"] = item
            collect_id_values(child, child_path, result)
    elif isinstance(value, list):
        for index, child in enumerate(value):
            collect_id_values(child, f"{path}[{index}]", result)

    return result


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
            # A previously invalid file is treated as having no valid ID baseline.
            previous = None

    previous_ids = collect_id_values(previous) if previous is not None else {}
    previous_id_values = set(previous_ids.values())
    current_ids = collect_id_values(current)

    def walk(value: Any, current_path: str = "$") -> None:
        if isinstance(value, dict):
            for key, child in value.items():
                child_path = f"{current_path}.{key}"
                if not is_camel_case(key):
                    issues.append(
                        f"{path}:{child_path}: JSON property '{key}' must use camelCase."
                    )
                if (
                    key in ENUM_PROPERTY_NAMES
                    and isinstance(child, str)
                    and not is_pascal_case(child)
                ):
                    issues.append(
                        f"{path}:{child_path}: enum-like value '{child}' "
                        "must use PascalCase."
                    )
                walk(child, child_path)
        elif isinstance(value, list):
            for index, child in enumerate(value):
                walk(child, f"{current_path}[{index}]")

    walk(current)

    for id_path, identifier in current_ids.items():
        if identifier in previous_id_values:
            continue  # Preserve legacy IDs even when their list position changes.
        if not is_snake_case(identifier):
            issues.append(
                f"{path}:{id_path}: new or changed ID '{identifier}' must use snake_case."
            )

    return issues


def path_naming_issues(base_ref: str, status: str, path: str) -> list[str]:
    """Check names only for newly added or renamed paths; do not penalize legacy paths."""
    if status not in {"A", "R", "C"}:
        return []

    issues: list[str] = []
    parts = PurePosixPath(path).parts

    for index, directory in enumerate(parts[:-1]):
        if directory in PLATFORM_DIRECTORY_NAMES or directory.startswith("."):
            continue
        directory_path = "/".join(parts[: index + 1])
        exists_in_base = (
            git("cat-file", "-e", f"{base_ref}:{directory_path}", check=False).returncode
            == 0
        )
        if not exists_in_base and not is_pascal_case(directory):
            issues.append(
                f"{path}: new directory '{directory_path}' must use PascalCase."
            )

    filename = parts[-1]
    if filename in ENTRY_FILE_NAMES:
        return issues

    suffix = PurePosixPath(filename).suffix
    if suffix in CHECKED_FILE_SUFFIXES:
        stem = filename[: -len(suffix)]
        if not is_pascal_case(stem):
            issues.append(
                f"{path}: new or renamed file '{filename}' "
                "must use PascalCase for its name."
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
    parser.add_argument("--base", required=True, help="Base commit/ref for the diff.")
    parser.add_argument("--head", required=True, help="Head commit/ref for the diff.")
    parser.add_argument("--branch", default="", help="Pull request source branch name.")
    parser.add_argument(
        "--check-branch", action="store_true", help="Validate the branch name."
    )
    args = parser.parse_args()

    issues: list[str] = []

    if args.check_branch:
        issues.extend(validate_branch_name(args.branch))

    try:
        changes = changed_paths(args.base, args.head)
    except subprocess.CalledProcessError as error:
        print(error.stderr or "Could not calculate changed paths.", file=sys.stderr)
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
