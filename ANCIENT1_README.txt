ERROR RELIC - ANCIENT1 CLEAN

STABLE BASE:
- DIAG8 Neow Proof: passed
- SHOP1 merchant ERROR relics: passed
- CHEST2 treasure ERROR relics: passed
- ELITE1 random RelicReward / Elite ERROR relics: passed

ANCIENT1 ADDS:
- One shared patch on AncientEventModel.RelicOption(
      RelicModel relic,
      string pageName,
      string? customDonePage
  )
- This is the common relic-option path for Ancient events.
- The generic RelicOption<T>() funnels into this overload.
- Every non-Neow Ancient relic option is replaced with its own
  locked ERROR before the EventOption and OnChosen callback are built.

RESULT:
- No per-Ancient patch.
- No per-Ancient-relic patch.
- A normal three-relic Ancient choice becomes three independent ERRORs.
- Neow remains on the existing Proof implementation.

ANCIENT1 DOES NOT PATCH:
- RelicFactory
- RelicCmd
- individual Ancient subclasses
- individual Ancient relic classes

Visible Proof marker:
[109-ANCIENT1]

BUILD:
C# only -> Build. No Publish needed.

TEST:
1. Use this as a fresh complete project.
2. Start a NEW singleplayer run.
3. Take [109-ANCIENT1] at Neow.
4. Reach / jump to a non-Neow Ancient.
5. If that Ancient presents three relic choices, all three should be ERROR.
6. Hover them: each should have its own locked H/E.
7. Pick one.
8. Inventory should contain the same H/E.
9. The Ancient event should finish normally.
10. Save/quit and reload.
11. Shop, treasure, and Elite should remain unchanged and working.
