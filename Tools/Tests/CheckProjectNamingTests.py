import sys
import unittest
from pathlib import Path

TOOLS_DIR = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS_DIR))

from CheckProjectNaming import (  # noqa: E402
    is_camel_case,
    is_pascal_case,
    is_snake_case,
    validate_branch_name,
    validate_json_document,
)


class NamingConventionTests(unittest.TestCase):
    def test_pascal_case(self):
        self.assertTrue(is_pascal_case("CharacterData"))
        self.assertTrue(is_pascal_case("CombatPanel2"))
        self.assertTrue(is_pascal_case("characterData") is False)
        # Acronym normalization such as UI vs Ui is not inferred by this structural check.
        self.assertTrue(is_pascal_case("UITheme"))
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
            [], validate_branch_name("chore/issue-28-project-naming-validators")
        )

    def test_invalid_branch_name(self):
        self.assertTrue(validate_branch_name("feature/naming-check"))
        self.assertTrue(validate_branch_name("feat/issue-28-Bad-Name"))

    def test_valid_json_names(self):
        document = (
            '{"id":"goblin_ash","maxHealth":20,'
            '"effects":[{"type":"RestoreHealth"}]}'
        )
        self.assertEqual(
            [], validate_json_document("Character.json", document, None)
        )

    def test_existing_legacy_id_is_grandfathered(self):
        previous = '{"id":"goblin-a","maxHealth":20}'
        current = '{"id":"goblin-a","maxHealth":30}'
        self.assertEqual(
            [], validate_json_document("Goblin.json", current, previous)
        )

    def test_new_or_changed_legacy_style_id_is_rejected(self):
        previous = '{"id":"goblin-a"}'
        current = '{"id":"goblin-b"}'
        issues = validate_json_document("Goblin.json", current, previous)
        self.assertTrue(any("snake_case" in issue for issue in issues))

    def test_invalid_json_property_name_is_rejected(self):
        document = '{"max_health":20}'
        issues = validate_json_document("Character.json", document, None)
        self.assertTrue(any("camelCase" in issue for issue in issues))

    def test_invalid_enum_like_value_is_rejected(self):
        document = '{"timeMode":"next_day"}'
        issues = validate_json_document("Location.json", document, None)
        self.assertTrue(any("PascalCase" in issue for issue in issues))


if __name__ == "__main__":
    unittest.main()
