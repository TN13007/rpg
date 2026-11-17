#!/usr/bin/env python3
"""Run the repository's complete local validation with the same steps as CI."""

from __future__ import annotations

import argparse
import shlex
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
APP_PROJECT = "RPG.csproj"
TEST_PROJECT = "RPG.Tests/RPG.Tests.csproj"


def git(*arguments: str) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *arguments],
        cwd=ROOT,
        text=True,
        capture_output=True,
        check=True,
    )


def changed_csharp_files(base_ref: str, head_ref: str) -> list[str]:
    """Return existing changed C# paths; deleted files are excluded."""
    result = git(
        "diff",
        "--name-only",
        "-z",
        f"{base_ref}...{head_ref}",
        "--",
        "*.cs",
    )
    return [
        path
        for path in result.stdout.split("\0")
        if path and (ROOT / path).is_file()
    ]


def csharp_project_for(path: str) -> str:
    return TEST_PROJECT if path.startswith("RPG.Tests/") else APP_PROJECT


def build_csharp_format_commands(paths: list[str]) -> list[list[str]]:
    """Group changed C# files by project and create IDE1006 verification commands."""
    commands: list[list[str]] = []
    for project in (APP_PROJECT, TEST_PROJECT):
        project_paths = [
            path for path in paths if csharp_project_for(path) == project
        ]
        if not project_paths:
            continue
        commands.append(
            [
                "dotnet",
                "format",
                "style",
                project,
                "--no-restore",
                "--verify-no-changes",
                "--diagnostics",
                "IDE1006",
                "--severity",
                "warn",
                "--include",
                *project_paths,
            ]
        )
    return commands


def run_command(command: list[str]) -> int:
    print(f"\n> {shlex.join(command)}", flush=True)
    result = subprocess.run(command, cwd=ROOT, check=False)
    if result.returncode != 0:
        print(
            f"Validation stopped: command exited with code {result.returncode}.",
            file=sys.stderr,
        )
    return result.returncode


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Run all automated repository checks used by GitHub Actions."
    )
    parser.add_argument("--base", required=True, help="Base ref/SHA for the diff.")
    parser.add_argument(
        "--head", default="HEAD", help="Head ref/SHA for the diff (default: HEAD)."
    )
    parser.add_argument("--branch", default="", help="Current source branch name.")
    parser.add_argument(
        "--check-branch",
        action="store_true",
        help="Validate the source branch naming convention (used for PRs).",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_arguments()

    try:
        commands = [
            [
                sys.executable,
                "-m",
                "unittest",
                "discover",
                "-s",
                "Tools/Tests",
                "-p",
                "*Tests.py",
                "-v",
            ],
            [
                sys.executable,
                "Tools/CheckProjectNaming.py",
                "--base",
                args.base,
                "--head",
                args.head,
                "--branch",
                args.branch,
            ],
        ]
        if args.check_branch:
            commands[1].append("--check-branch")

        changed_files = changed_csharp_files(args.base, args.head)
        format_commands = build_csharp_format_commands(changed_files)
    except (subprocess.CalledProcessError, OSError) as error:
        details = error.stderr.strip() if isinstance(error, subprocess.CalledProcessError) and error.stderr else str(error)
        print(f"Could not prepare validation: {details}", file=sys.stderr)
        return 2

    # Run fast Python checks first so failures are reported before .NET work.
    for command in commands:
        code = run_command(command)
        if code != 0:
            return code

    if format_commands:
        for command in format_commands:
            code = run_command(command)
            if code != 0:
                return code
    else:
        print("\nC# naming check: skipped (no changed C# files).")

    return run_command(
        ["dotnet", "test", TEST_PROJECT, "--configuration", "Release"]
    )


if __name__ == "__main__":
    raise SystemExit(main())
