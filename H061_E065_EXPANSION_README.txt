ERROR Relics H061/E065 expansion
================================
Base: uploaded VISUAL2.1 project.

Added:
- Hooks H032-H061 (append-only)
- Effects E033-E065 (append-only)
- Final pool: 61 Hooks x 65 Effects = 3965 theoretical combinations.

Important:
- Existing Merchant/Chest/Elite/Ancient/VISUAL2.1 patches were not redesigned.
- E009/E010 source order was not normalized.
- BigMushroom hand-draw modifier is intentionally excluded.
- Nunchaku/TuningFork counters are SavedProperty.
- ReptileTrinket uses ReptileTrinketPower, not plain StrengthPower.
- RoyalPoison uses Unblockable|Unpowered damage.

BUILD: C# only. Use Build, not Publish.

This package was statically assembled in an environment without the .NET SDK.
Run a local Release build against your sts2.dll before in-game testing.
