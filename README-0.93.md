# ERROR Relics 0.93 — Visual V3

Built cleanly from the original 0.92.5 source.

V3 specifically fixes the reward-preview vs post-pickup inventory mismatch:
both RelicReward.CreateIcon and NRelic.Reload now call the same
ApplyFullAppearance routine, explicitly restoring the same source texture and
the same deterministic ERROR visual parameters after each UI rebuild.

No gameplay RNG is consumed. No Control rotation/scale/position is changed.
Proof is excluded as ERROR source art, and its old opaque-square outline asset
is replaced with a transparent silhouette outline.

## Visual V4 diagnostic fix
V3 revealed an important mismatch: RelicReward.CreateIcon always rendered the
ERROR from VisualSourceBigIconPath, while the top-bar NRelic intentionally
switched to VisualSourceIconPath for small inventory icons. V4 makes both paths
use the exact same Big source texture and reapplies the same appearance after
NRelicInventoryHolder Ready/RefreshStatus/OnStatusChanged.
