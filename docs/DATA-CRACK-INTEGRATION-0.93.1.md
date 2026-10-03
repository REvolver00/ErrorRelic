# Discovery Mode integration — 0.93.1

Display settings form three semantic states using two checkboxes:

- `ShowFullEffects=true`: Full display. Discovery checkbox is ignored.
- `ShowFullEffects=false`, `DataCrackRevealMode=false`: Hidden. H/E are both hidden.
- `ShowFullEffects=false`, `DataCrackRevealMode=true`: Discovery Mode.

Discovery Mode rules:

- New ERRORs begin with both H and E hidden.
- Proof gains +1 charge at each combat start, max 4.
- Right-clicking your own ERROR spends one charge. First reveal exposes E; second exposes H.
- `EffectRevealed` and `HookRevealed` are saved per relic.
- In multiplayer, a remote player's ERROR is always shown with full H/E; discovery applies only to the local player's own ERRORs.
- H/E gameplay data is never removed or rerolled.
