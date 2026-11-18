import sys
import tempfile
import unittest
from pathlib import Path

TOOLS_DIR = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS_DIR))

from CheckMarkdown import (  # noqa: E402
    active_document_orphans,
    heading_anchors,
    heading_slug,
    markdown_links,
    validate,
)


class HeadingAnchorTests(unittest.TestCase):
    def test_github_style_slug_removes_punctuation_and_formats_spaces(self):
        self.assertEqual("session-3-exploration-pipeline", heading_slug("Session 3: Exploration Pipeline"))

    def test_heading_slug_preserves_underscores(self):
        self.assertEqual("api_reference_v2", heading_slug("API_Reference_v2"))

    def test_duplicate_headings_receive_numeric_suffixes(self):
        anchors = heading_anchors("# Setup\n\n## Setup\n\n## Setup")
        self.assertEqual({"setup", "setup-1", "setup-2"}, anchors)

    def test_headings_inside_fenced_code_are_ignored(self):
        self.assertEqual({"real-heading"}, heading_anchors("# Real Heading\n\n```md\n# Fake Heading\n```"))


class MarkdownValidationTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary_directory.name)
        (self.root / "Docs").mkdir()
        (self.root / "README.md").write_text(
            "# Project\n\n[Guide](Docs/Guide.md)\n", encoding="utf-8"
        )
        (self.root / "Docs" / "Guide.md").write_text(
            "# Guide\n\n[Section](#section)\n\n## Section\n", encoding="utf-8"
        )
        (self.root / "Assets").mkdir()
        (self.root / "Assets" / "icon.svg").write_text("<svg />", encoding="utf-8")
        self.tracked = {"README.md", "Docs/Guide.md", "Assets/icon.svg"}

    def tearDown(self):
        self.temporary_directory.cleanup()

    def test_valid_relative_file_and_heading_links_pass(self):
        self.assertEqual([], validate(self.root, self.tracked))

    def test_tracked_non_markdown_target_passes(self):
        (self.root / "README.md").write_text("# Project\n\n[Guide](Docs/Guide.md) [Icon](Assets/icon.svg)", encoding="utf-8")
        self.assertEqual([], validate(self.root, self.tracked))

    def test_missing_file_fails(self):
        (self.root / "README.md").write_text("[Missing](Docs/Missing.md)", encoding="utf-8")
        issues = validate(self.root, self.tracked)
        self.assertTrue(any("file does not exist or is not tracked" in issue for issue in issues))

    def test_path_traversal_outside_repository_fails(self):
        (self.root / "README.md").write_text("[Escape](../../outside.md)", encoding="utf-8")
        issues = validate(self.root, self.tracked)
        self.assertTrue(any("target escapes the repository" in issue for issue in issues))

    def test_missing_heading_anchor_fails(self):
        (self.root / "README.md").write_text("[Missing](Docs/Guide.md#unknown)", encoding="utf-8")
        issues = validate(self.root, self.tracked)
        self.assertTrue(any("heading anchor not found" in issue for issue in issues))

    def test_external_links_are_ignored(self):
        (self.root / "README.md").write_text(
            "# Project\n\n[Web](https://example.com) [Mail](mailto:test@example.com) [Anchor](#project)",
            encoding="utf-8",
        )
        self.assertEqual([], validate(self.root, {"README.md"}))

    def test_archive_documents_are_excluded_from_orphan_detection(self):
        (self.root / "Docs" / "Archive").mkdir()
        (self.root / "Docs" / "Archive" / "Old.md").write_text("# Old", encoding="utf-8")
        tracked = self.tracked | {"Docs/Archive/Old.md"}
        self.assertEqual([], active_document_orphans(self.root, tracked))

    def test_unlinked_active_document_is_reported_as_orphan(self):
        (self.root / "Docs" / "Orphan.md").write_text("# Orphan", encoding="utf-8")
        tracked = self.tracked | {"Docs/Orphan.md"}
        self.assertEqual(["Docs/Orphan.md"], active_document_orphans(self.root, tracked))

    def test_nested_active_document_is_reachable_through_parent(self):
        (self.root / "Docs" / "Reference.md").write_text(
            "# Reference\n\n[Child](Child.md)", encoding="utf-8"
        )
        (self.root / "Docs" / "Child.md").write_text("# Child", encoding="utf-8")
        (self.root / "Docs" / "Guide.md").write_text(
            "# Guide\n\n[Section](#section)\n\n## Section\n\n[Reference](Reference.md)",
            encoding="utf-8",
        )
        tracked = self.tracked | {"Docs/Reference.md", "Docs/Child.md"}
        self.assertEqual([], active_document_orphans(self.root, tracked))

    def test_inline_code_examples_are_not_treated_as_links(self):
        source = "Use `[Fake](Docs/Missing.md)` but [Guide](Docs/Guide.md)."
        self.assertEqual([(1, "Docs/Guide.md")], list(markdown_links(source)))

    def test_code_examples_are_not_treated_as_links(self):
        source = "[Guide](Docs/Guide.md)\n\n```md\n[Fake](Docs/Missing.md)\n```"
        self.assertEqual([(1, "Docs/Guide.md")], list(markdown_links(source)))


if __name__ == "__main__":
    unittest.main()
