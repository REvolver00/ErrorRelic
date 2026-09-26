ERROR RELIC - VISUAL2.1 CLEAN

BASE:
VISUAL2, whose UI-only source-art replacement worked.

FIX:
A rotated/scaled TextureRect could visually move away from the
MerchantRelicHolder click rectangle, making a visibly displaced relic
appear unclickable.

VISUAL2.1:
- NRelic wrapper MouseFilter = Ignore
- Icon MouseFilter = Ignore
- Outline MouseFilter = Ignore
- no Control RotationDegrees
- no Control Scale
- keeps:
  - vanilla source relic art
  - horizontal/vertical flip
  - deterministic color/tint corruption

NO GAMEPLAY FLOW CHANGES.
Shop/chest/elite/ancient logic is untouched.

Visible marker:
[109-VISUAL2.1]

BUILD ONLY. No Publish.

TEST FIRST:
1. New run.
2. Take [109-VISUAL2.1].
3. Enter shop.
4. Confirm all three ERROR relic slots are clickable.
5. Buy the middle slot specifically.
6. Confirm it is obtained normally.

Then check treasure / elite / ancient briefly.
