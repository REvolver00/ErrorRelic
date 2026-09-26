ERROR RELIC - VISUAL2 CLEAN

BASE:
ANCIENT1 stable gameplay build.

VISUAL1 IS NOT INCLUDED.

VISUAL2 intentionally does NOT patch:
- RelicModel.Icon
- RelicModel.IconOutline
- RelicModel.BigIcon
- RelicModel image paths
- RelicCmd
- RelicFactory
- reward/chest/shop acquisition logic

VISUAL2 only:
1. saves three vanilla source texture paths on the ERROR instance;
2. NRelic.Reload postfix changes what the UI TextureRect displays;
3. RelicReward.CreateIcon postfix covers random relic reward UI;
4. applies deterministic flip/rotation/scale/tint to TextureRect.

The ERROR's actual model still internally uses the original relic.png.
Therefore a visual failure falls back to the old working appearance
instead of changing relic acquisition.

Visible Proof marker:
[109-VISUAL2]

BUILD:
Build only. No Publish.

FIRST TEST:
1. New run.
2. Take [109-VISUAL2].
3. Enter a shop.
4. DO NOT buy anything yet.
5. Check whether the ERROR icons visibly use corrupted vanilla relic art.

If shop visuals changed:
6. Buy one.
7. Confirm acquisition does not freeze.
8. Confirm inventory keeps the same source art / orientation / tint.
9. Save, quit, reload.

Then test treasure, elite reward and Ancient.

Expected visual corruption in VISUAL2:
- wrong vanilla relic image instead of the shared ERROR image
- horizontal and/or vertical flip
- sometimes 90/180/270 rotation
- slight size error
- medium color/brightness tint

No pixel-level RGB slicing yet. That comes only after VISUAL2 proves
the UI-only architecture is stable.
