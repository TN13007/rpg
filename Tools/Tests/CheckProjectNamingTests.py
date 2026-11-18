import subprocess
import sys
import unittest
from contextlib import redirect_stdout
from io import StringIO
from pathlib import Path
from unittest.mock import patch

TOOLS_DIR = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS_DIR))

from CheckProjectNaming import (  # noqa: E402
    audit_tracked_paths,
    changed_paths,
    is_camel_case,
    is_dotnet_project_name,
    is_pascal_case,
    is_snake_case,
    main,
    path_naming_issues,
    validate_branch_name,
    validate_json_document,
)


class NamingConventionTests(unittest.TestCase):
    def test_pascal_case(self):
        self.assertTrue(is_pascal_case("CharacterData"))
        self.assertTrue(is_pascal_case("CombatPanel2"))
        self.assertFalse(is_pascal_case("characterData"))
        self.assertFalse(is_pascal_case("Main_Controller"))

    def test_camel_case(self):
        self.assertTrue(is_camel_case("startingEnergy"))
        self.assertFalse(is_camel_case("StartingEnergy"))
        self.assertFalse(is_camel_case("starting_energy"))

    def test_snake_case(self):
        self.assertTrue(is_snake_case("player_ari"))
        self.assertTrue(is_snake_case("event_2_attack"))
        self.assertFalse(is_snake_case("player-ari"))
        self.assertFalse(is_snake_case("PlayerAri"))

    def test_valid_branch_name(self):
        self.assertEqual(
            [], validate_branch_name("chore/issue-30-harden-project-naming-validator")
        )

    def test_invalid_branch_name(self):
        self.assertTrue(validate_branch_name("feature/naming-check"))
        self.assertTrue(validate_branch_name("feat/issue-30-Bad-Name"))

    def test_valid_json_names(self):
        document = (
            '{"id":"goblin_ash","maxHealth":20,'
            '"effects":[{"type":"RestoreHealth"}]}'
        )
        self.assertEqual(
            [], validate_json_document("Character.json", document, None)
        )

    def test_existing_legacy_id_is_grandfathered(self):
        previous = '{"id":"goblin-a","name":"Goblin","maxHealth":20}'
        current = '{"id":"goblin-a","name":"Goblin","maxHealth":30}'
        self.assertEqual(
            [], validate_json_document("Goblin.json", current, previous)
        )

    def test_existing_legacy_ids_are_preserved_when_array_items_move(self):
        previous = (
            '{"locations":['
            '{"id":"room-a","name":"Room A","exits":["room-b"]},'
            '{"id":"room-b","name":"Room B","exits":["room-a"]}'
            ']}'
        )
        current = (
            '{"locations":['
            '{"id":"room-b","name":"Room B","exits":["room-a"]},'
            '{"id":"room-a","name":"Room A","exits":["room-b"]}'
            ']}'
        )
        self.assertEqual(
            [], validate_json_document("Locations.json", current, previous)
        )

    def test_legacy_id_survives_non_identity_field_changes(self):
        previous = (
            '{"locations":[{"id":"room-a","name":"Room A",'
            '"description":"An old description"}]}'
        )
        current = (
            '{"locations":[{"id":"room-a","name":"Room A",'
            '"description":"A revised description"}]}'
        )
        self.assertEqual(
            [], validate_json_document("Locations.json", current, previous)
        )

    def test_nested_ids_are_scoped_to_their_parent_entity(self):
        previous = (
            '{"locations":['
            '{"name":"Room A","actions":[{"id":"attack-a","label":"Attack"}]},'
            '{"name":"Room B","actions":[{"id":"attack-b","label":"Attack"}]}'
            ']}'
        )
        current = (
            '{"locations":['
            '{"name":"Room A","actions":[{"id":"attack-b","label":"Attack"}]},'
            '{"name":"Room B","actions":[{"id":"attack-b","label":"Attack"}]}'
            ']}'
        )
        issues = validate_json_document("Locations.json", current, previous)
        self.assertTrue(any("attack-b" in issue for issue in issues))

    def test_changed_id_cannot_borrow_another_entities_old_id(self):
        previous = (
            '{"characters":['
            '{"id":"goblin-a","name":"Goblin"},'
            '{"id":"slime-a","name":"Slime"}'
            ']}'
        )
        current = (
            '{"characters":['
            '{"id":"slime-a","name":"Goblin"},'
            '{"id":"slime-a","name":"Slime"}'
            ']}'
        )
        issues = validate_json_document("Characters.json", current, previous)
        self.assertTrue(any("new or changed ID/reference" in issue for issue in issues))

    def test_changed_reference_cannot_borrow_another_entities_old_id(self):
        previous = (
            '{"locations":['
            '{"id":"room_a","name":"Room A","exits":["room-b"]},'
            '{"id":"room_b","name":"Room B"},'
            '{"id":"room_c","name":"Room C"}'
            ']}'
        )
        current = (
            '{"locations":['
            '{"id":"room_a","name":"Room A","exits":["room-c"]},'
            '{"id":"room_b","name":"Room B"},'
            '{"id":"room_c","name":"Room C"}'
            ']}'
        )
        issues = validate_json_document("Locations.json", current, previous)
        self.assertTrue(any("room-c" in issue for issue in issues))

    def test_new_or_changed_legacy_style_id_is_rejected(self):
        previous = '{"id":"goblin-a","name":"Goblin"}'
        current = '{"id":"goblin-b","name":"Goblin"}'
        issues = validate_json_document("Goblin.json", current, previous)
        self.assertTrue(any("snake_case" in issue for issue in issues))

    def test_new_invalid_json_property_name_is_rejected(self):
        previous = '{"id":"player_ari","name":"Ari"}'
        current = '{"id":"player_ari","name":"Ari","max_health":20}'
        issues = validate_json_document("Character.json", current, previous)
        self.assertTrue(any("max_health" in issue for issue in issues))

    def test_renamed_invalid_json_property_name_is_rejected(self):
        previous = '{"id":"player_ari","max_health":20}'
        current = '{"id":"player_ari","max-health":20}'
        issues = validate_json_document("Character.json", current, previous)
        self.assertTrue(any("max-health" in issue for issue in issues))

    def test_unchanged_legacy_property_is_grandfathered_when_other_values_change(self):
        previous = '{"id":"player_ari","max_health":20,"name":"Ari"}'
        current = '{"id":"player_ari","max_health":20,"name":"Ari the Brave"}'
        self.assertEqual(
            [], validate_json_document("Character.json", current, previous)
        )

    def test_unchanged_legacy_property_is_grandfathered_when_array_items_move(self):
        previous = (
            '{"items":['
            '{"id":"item_a","bad_key":1,"name":"A"},'
            '{"id":"item_b","name":"B"}'
            ']}'
        )
        current = (
            '{"items":['
            '{"id":"item_b","name":"B"},'
            '{"id":"item_a","bad_key":2,"name":"A"}'
            ']}'
        )
        self.assertEqual([], validate_json_document("Items.json", current, previous))

    def test_invalid_enum_like_value_is_rejected(self):
        document = '{"timeMode":"next_day"}'
        issues = validate_json_document("Location.json", document, None)
        self.assertTrue(any("PascalCase" in issue for issue in issues))


class ChangedPathTests(unittest.TestCase):
    def _git_result(self, returncode=1):
        return subprocess.CompletedProcess(
            args=["git"], returncode=returncode, stdout="", stderr=""
        )

    def test_changed_paths_parses_added_modified_deleted_and_renamed_entries(self):
        output = (
            "A\0AddedFile.txt\0"
            "M\0ModifiedFile.cs\0"
            "D\0DeletedFile.cs\0"
            "R100\0OldFolder/OldFile.txt\0NewFolder/NewFile.txt\0"
        )
        with patch(
            "CheckProjectNaming.git",
            return_value=subprocess.CompletedProcess(
                args=["git"], returncode=0, stdout=output, stderr=""
            ),
        ):
            self.assertEqual(
                [
                    ("A", "AddedFile.txt", None),
                    ("M", "ModifiedFile.cs", None),
                    ("D", "DeletedFile.cs", None),
                    ("R", "NewFolder/NewFile.txt", "OldFolder/OldFile.txt"),
                ],
                changed_paths("base", "head"),
            )

    def test_added_file_with_unlisted_extension_is_checked(self):
        with patch("CheckProjectNaming.git", return_value=self._git_result()):
            issues = path_naming_issues("base", "A", "Data/Bad_Name.txt")
        self.assertTrue(any("Bad_Name.txt" in issue for issue in issues))

    def test_renamed_file_is_checked(self):
        with patch("CheckProjectNaming.git", return_value=self._git_result()):
            issues = path_naming_issues("base", "R", "Data/Bad_Name.bin")
        self.assertTrue(any("Bad_Name.bin" in issue for issue in issues))

    def test_godot_uid_sidecar_uses_source_file_name(self):
        with patch("CheckProjectNaming.git", return_value=self._git_result()):
            issues = path_naming_issues(
                "base", "A", "Scripts/ActionExecutionService.cs.uid"
            )
        self.assertEqual([], issues)

    def test_godot_import_sidecar_checks_asset_name(self):
        with patch("CheckProjectNaming.git", return_value=self._git_result()):
            issues = path_naming_issues(
                "base", "A", "Assets/academy_scene.svg.import"
            )
        self.assertTrue(any("academy_scene.svg.import" in issue for issue in issues))

    def test_repository_and_godot_entry_files_are_exempt(self):
        for path in (
            "README.md",
            "CONTRIBUTING.md",
            ".editorconfig",
            ".gitattributes",
            ".gitignore",
            "project.godot",
            "icon.svg",
            "icon.svg.import",
        ):
            with self.subTest(path=path):
                self.assertEqual([], path_naming_issues("base", "A", path))

    def test_github_platform_paths_are_exempt(self):
        self.assertEqual(
            [],
            path_naming_issues(
                "base", "A", ".github/workflows/dotnet-tests.yml"
            ),
        )

    def test_new_directory_must_use_pascal_case(self):
        with patch("CheckProjectNaming.git", return_value=self._git_result()):
            issues = path_naming_issues("base", "A", "bad_directory/ValidFile.bin")
        self.assertTrue(any("bad_directory" in issue for issue in issues))

    def test_existing_legacy_directory_is_not_revalidated(self):
        with patch(
            "CheckProjectNaming.git", return_value=self._git_result(returncode=0)
        ):
            issues = path_naming_issues("base", "A", "legacy_folder/ValidFile.bin")
        self.assertEqual([], issues)

    def test_modified_file_path_is_not_revalidated(self):
        with patch("CheckProjectNaming.git") as git_mock:
            issues = path_naming_issues("base", "M", "legacy_folder/bad_file.txt")
        git_mock.assert_not_called()
        self.assertEqual([], issues)



class FullRepositoryAuditTests(unittest.TestCase):
    def test_audit_finds_unchanged_legacy_files_and_directories_once(self):
        issues, directory_count = audit_tracked_paths(
            [
                "bad_folder/first_bad_file.json",
                "bad_folder/SecondFile.cs",
                "ValidFolder/ThirdFile.md",
            ]
        )

        self.assertEqual(2, directory_count)
        self.assertEqual(2, len(issues))
        self.assertTrue(any("directory 'bad_folder'" in issue for issue in issues))
        self.assertTrue(any("Suggested directory name: 'BadFolder'" in issue for issue in issues))
        self.assertTrue(any("Suggested path: 'bad_folder/FirstBadFile.json'" in issue for issue in issues))

    def test_audit_accepts_dotnet_project_directory_and_project_file(self):
        issues, directory_count = audit_tracked_paths(
            ["RPG.Tests/RPG.Tests.csproj"]
        )

        self.assertEqual([], issues)
        self.assertEqual(1, directory_count)
        self.assertTrue(is_dotnet_project_name("RPG.Tests"))

    def test_audit_suggests_pascal_case_for_simple_lowercase_names(self):
        issues, _ = audit_tracked_paths(
            ["Data/Characters/goblin.json", "Data/Characters/player.json"]
        )

        self.assertEqual(2, len(issues))
        self.assertTrue(any("Data/Characters/Goblin.json" in issue for issue in issues))
        self.assertTrue(any("Data/Characters/Player.json" in issue for issue in issues))

    def test_audit_suggests_names_for_godot_asset_and_import_sidecar(self):
        issues, _ = audit_tracked_paths(
            [
                "Assets/academy_scene.svg",
                "Assets/academy_scene.svg.import",
            ]
        )

        self.assertEqual(2, len(issues))
        self.assertTrue(any("Assets/AcademyScene.svg" in issue for issue in issues))
        self.assertTrue(any("Assets/AcademyScene.svg.import" in issue for issue in issues))

    def test_audit_detects_upper_snake_case_markdown_names(self):
        issues, _ = audit_tracked_paths(
            ["Docs/Design/COMBAT_PANEL_REVIEW.md"]
        )

        self.assertEqual(1, len(issues))
        self.assertIn(
            "Suggested path: 'Docs/Design/CombatPanelReview.md'",
            issues[0],
        )

    def test_audit_excludes_platform_trees_and_entry_files(self):
        issues, directory_count = audit_tracked_paths(
            [
                ".github/ISSUE_TEMPLATE/bug_report.md",
                ".godot/imported/bad_name.svg",
                "README.md",
                "CONTRIBUTING.md",
            ]
        )

        self.assertEqual([], issues)
        self.assertEqual(0, directory_count)


class FullRepositoryAuditCliTests(unittest.TestCase):
    def test_audit_returns_failure_when_tracked_path_violates_naming(self):
        output = StringIO()
        with (
            patch("sys.argv", ["CheckProjectNaming.py", "--audit-all"]),
            patch(
                "CheckProjectNaming.tracked_paths",
                return_value=["Data/bad_file.json"],
            ),
            redirect_stdout(output),
        ):
            result = main()

        self.assertEqual(1, result)
        self.assertIn("Project naming audit found 1 violation(s)", output.getvalue())

    def test_audit_passes_when_all_tracked_paths_follow_naming(self):
        output = StringIO()
        with (
            patch("sys.argv", ["CheckProjectNaming.py", "--audit-all"]),
            patch(
                "CheckProjectNaming.tracked_paths",
                return_value=[
                    "Data/AcademyLocations.json",
                    "Data/Characters/Player.json",
                ],
            ),
            redirect_stdout(output),
        ):
            result = main()

        self.assertEqual(0, result)
        self.assertIn("Full repository path naming audit passed.", output.getvalue())


if __name__ == "__main__":
    unittest.main()
