import subprocess
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
        output = "Scripts/CombatService.cs\0RPG.Tests/CombatServiceTests.cs\0Scripts/Old.cs\0"
        existing = {
            Path("Scripts/CombatService.cs"),
            Path("RPG.Tests/CombatServiceTests.cs"),
        }
        with (
            patch(
                "Validate.git",
                return_value=subprocess.CompletedProcess(
                    args=["git"], returncode=0, stdout=output, stderr=""
                ),
            ) as git_mock,
            patch("Validate.ROOT", Path(".")),
            patch("pathlib.Path.is_file", side_effect=lambda path: path in existing),
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
