import subprocess
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from Validate import (
    APP_PROJECT,
    TEST_PROJECT,
    build_csharp_format_commands,
    changed_csharp_files,
    csharp_project_for,
)


class CSharpChangeDetectionTests(unittest.TestCase):
    def test_changed_csharp_files_uses_three_dot_diff_and_filters_deleted_files(self):
        output = (
            "Scripts/CombatService.cs\0"
            "RPG.Tests/CombatServiceTests.cs\0"
            "Scripts/Old.cs\0"
        )
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            existing_paths = (
                root / "Scripts" / "CombatService.cs",
                root / "RPG.Tests" / "CombatServiceTests.cs",
            )
            for path in existing_paths:
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("// test fixture", encoding="utf-8")

            with (
                patch("Validate.ROOT", root),
                patch(
                    "Validate.git",
                    return_value=subprocess.CompletedProcess(
                        args=["git"], returncode=0, stdout=output, stderr=""
                    ),
                ) as git_mock,
            ):
                self.assertEqual(
                    ["Scripts/CombatService.cs", "RPG.Tests/CombatServiceTests.cs"],
                    changed_csharp_files("origin/main", "HEAD"),
                )

        git_mock.assert_called_once_with(
            "diff", "--name-only", "-z", "origin/main...HEAD", "--", "*.cs"
        )

    def test_test_project_paths_are_separated_from_application_paths(self):
        self.assertEqual(TEST_PROJECT, csharp_project_for("RPG.Tests/CharacterTests.cs"))
        self.assertEqual(APP_PROJECT, csharp_project_for("Scripts/CombatService.cs"))

    def test_format_commands_are_created_only_for_projects_with_changed_files(self):
        commands = build_csharp_format_commands(
            ["Scripts/CombatService.cs", "RPG.Tests/CharacterTests.cs"]
        )
        self.assertEqual(2, len(commands))
        self.assertIn(APP_PROJECT, commands[0])
        self.assertIn(TEST_PROJECT, commands[1])
        self.assertIn("--verify-no-changes", commands[0])
        self.assertIn("IDE1006", commands[1])

    def test_no_changed_csharp_files_produces_no_format_commands(self):
        self.assertEqual([], build_csharp_format_commands([]))


if __name__ == "__main__":
    unittest.main()
