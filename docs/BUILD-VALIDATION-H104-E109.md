# ERROR Relics H104 / E109 build-validation note

Target mod baseline: ERROR Relics 0.91.3.
Target game assembly audited: the user-supplied Beta v0.111.0 `sts2.dll` from `data_sts2_windows_x86_64`.

## Completed validation

- Decompiled and traced the source relics and the game APIs used by H062-H104 / E066-E109 against the supplied target `sts2.dll`.
- Confirmed target enum values and overloads used by card selection, card rewards, relic rewards, enchantments, Orb channeling, Wax relic creation/melting, and VFX calls.
- Confirmed Calling Bell target flow: add Curse of the Bell, wait 0.75 s, then offer Common/Uncommon/Rare relic rewards.
- Confirmed Toy Box target constants and flow: 5 Wax relics, one melt every 3 combats, source relic used up at 15 combats, and the scaled post-melt wait.
- Ran repository static validation for continuous H/E IDs, undefined enum references, DynamicVar coverage, implementation/text coverage, generator blacklist, and lexical delimiter balance.
- Static result: 104 Hooks, 109 Effects; only E002 and E007 are excluded from random generation; 104 x 107 = 11,128 active random combinations.

## Build-chain limitation in this environment

A real `dotnet build` was not completed inside this sandbox because it does not provide a .NET 9 / Godot.NET build toolchain, and external SDK download was blocked by the execution environment. This package therefore must not be described as locally compiled here.

The source is prepared for compilation in the normal ERROR Relics development environment. Before Workshop/public distribution, build it against the same v0.111.0 game data and BaseLib environment used by the project, then run the included `TEST-PLAN-H104-E109.md` multiplayer stress cases.
