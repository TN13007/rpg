# Playable Journey — QA Verification Record

**Related issue:** [#10 — Verify playable journey in Godot](https://github.com/TN13007/rpg/issues/10)  
**Related implementation:** [PR #9 — Location Event Gameplay Loop](https://github.com/TN13007/rpg/pull/9)  
**Record type:** Retrospective record of the repository owner's completion status  
**Status:** Owner-marked pass; raw execution evidence was not retained in the Issue.

## 1. What the repository records

Issue #10 is closed. At closure, the repository owner marked all five acceptance criteria as complete:

- Main scene opens without runtime errors.
- Exploration → Event → Combat/Sneak → Exploration works.
- Time and character resources update as expected.
- No UI or node-reference errors appear in Output.
- Manual verification is recorded on the Issue.

The Issue describes the target environment as Godot 4.7.2 .NET and the tested journey as starting in the Dormitory at Day 1, 07:30, travelling among the four prototype locations, checking both Event outcomes, exercising Combat actions and checking the character panel and resources.

This record reports those checks as **passed according to the owner's completed Issue checklist**. It does not claim that this document independently reproduces the test.

## 2. Verification summary

| Verification area | Recorded result | Source / limitation |
|---|---|---|
| Main scene startup | Pass — owner-marked | Issue #10 acceptance criterion; no Godot Output log attached |
| Location travel | Pass — owner-marked | Included in the playable-journey acceptance criterion; no step-by-step route log attached |
| Event appears / does not appear | Pass — owner-marked | Included in the Issue #10 scope; no random seed or run log retained |
| Confront → Combat → Exploration | Pass — owner-marked | Included in the end-to-end journey criterion; no screenshot/video attached |
| Sneak → Exploration | Pass — owner-marked | Included in the end-to-end journey criterion; no per-action resource log attached |
| Combat actions and outcomes | Pass — owner-marked | Issue scope includes Attack, Defend, Dodge, victory and defeat; detailed result sequence not recorded |
| UI and node references | Pass — owner-marked | No Output log or screenshot retained for independent inspection |
| Health, Energy, Gold and time | Pass — owner-marked | Per-action before/after values were not recorded |

## 3. Automated test evidence

The Location Event implementation's GitHub Actions run for commit `f5f018dfe31adb4d600ea4efff95f6987e45250c` completed successfully:

- Workflow: `.NET Tests`
- Result: 64 passed, 0 failed
- Run: [GitHub Actions run #43](https://github.com/TN13007/rpg/actions/runs/43769290553)

This is automated .NET test evidence. It is not proof of Godot scene startup, rendered UI, mouse interaction or the manual journey.

## 4. Evidence not retained

The closed Issue has no comments or attachments containing:

- Godot Output logs.
- Screenshots or video of the tested journey.
- A run-by-run test table with exact observed values.
- The exact local build/editor revision used for the manual run.

Therefore the correct audit status is **owner-confirmed pass, with incomplete supporting evidence**, rather than independently reproducible QA evidence.

## 5. Follow-up for the next runtime test

For future manual verification, attach at least:

1. A screenshot of the running main scene and the final Godot Output panel.
2. One short recording or ordered screenshots showing Event → Confront/Sneak → return to Exploration.
3. Before/after values for time, Energy, Health and Gold for the tested choices.
4. The tested branch and commit SHA.

Do not backfill missing observations from memory as measured values.
