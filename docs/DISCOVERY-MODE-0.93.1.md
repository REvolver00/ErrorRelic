# Discovery Mode — 0.93.1

The display controls now encode three modes without requiring a custom three-state widget:

1. `ShowFullEffects = true` -> Full display. `DataCrackRevealMode` is ignored.
2. `ShowFullEffects = false`, `DataCrackRevealMode = false` -> Hidden. Both H and E are hidden.
3. `ShowFullEffects = false`, `DataCrackRevealMode = true` -> Discovery Mode.

Discovery Mode:
- New ERROR: H??? / E???.
- Proof gains +1 charge at combat start, max 4.
- Right-click your own ERROR once: spend 1, reveal E only.
- Right-click it again: spend 1, reveal H; now fully known.
- Reveal state is saved separately as EffectRevealed + HookRevealed.
- Old Data Crack saves remain compatible: old EffectRevealed is retained; HookRevealed defaults true.
- Remote players' ERRORs are always fully visible in multiplayer. Only your own relics obey your display/discovery setting.
- Right-clicking a remote player's relic cannot spend their Proof charge because reveal is restricted to LocalContext.NetId.
