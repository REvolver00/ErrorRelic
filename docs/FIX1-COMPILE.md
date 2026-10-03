# H104/E109 test package - compile fix 1

Fixed:
- `ErrorEffectExpansion0913.cs`
- Dolly's Mirror no longer accesses the non-public
  `RelicModel.SelectionScreenPrompt` member.
- The selector now uses the vanilla localization key directly:
  `relics / DOLLYS_MIRROR.selectionScreenPrompt`.

This is a compile-access fix only. The intended Dolly's Mirror behavior is unchanged.
