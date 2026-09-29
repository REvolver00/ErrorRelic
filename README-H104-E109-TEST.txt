ERROR Relics 0.92 — H104 / E109 TEST PACKAGE

目标游戏程序集：作者上传的 Beta v0.111.0 sts2.dll
Mod 版本：ERROR Relics 0.92 / BaseLib 3.4.7

本批：
- 审计原先 40 个候选 + Calling Bell + Toy Box，共 42 件原版遗物。
- Hook：H061 -> H104
- Effect：E065 -> E109
- 当前随机池：104 Hooks x 107 active Effects = 11,128 combinations
- 继续临时禁用随机生成：E002、E007
- Calling Bell / Toy Box 的危险奖励效果已按原版完整流程实现并保留在随机池中，没有缩水或延后。
- Whole Modifier 类不强拆；本批只收目标 DLL 能证明可独立拆出的 Hook / Effect。

高风险优先测试：
- E098 Calling Bell：Common + Uncommon + Rare relic rewards
- E099 Toy Box：5 Wax relic rewards
- E106 Orrery：5 card rewards
- E107 Glass Eye：2 Common + 2 Uncommon + 1 Rare card rewards
- E108 Small Capsule：1 random relic reward
- E109 Choices Paradox：5 Retain candidates -> choose 1 to hand
- E090/E091/E101-E105：牌组选择 / 复制 / 升级 / 附魔流程

Toy Box 目标 DLL 常量：
- 5 Wax relics
- every 3 combats melt 1
- used up after 15 combats / 5 melts

验证状态：
- H/E 连续性、引用、DynamicVar 覆盖、随机池过滤和源码静态结构检查通过。
- 当前执行环境没有 .NET 9 / Godot.NET SDK，因此此包未在这里完成真实 dotnet build。
- 请在正常 ERROR Relics 开发环境中编译后，优先执行 docs/TEST-PLAN-H104-E109.md。

详细反编译依据：
- docs/TARGET-DLL-AUDIT-H104-E109.md
- docs/BUILD-VALIDATION-H104-E109.md
- docs/TEST-PLAN-H104-E109.md


0.92 cleanup:
- Proof relic updated to 0.92: 104 Hooks / 109 Effects / 11,336 theoretical H/E combinations.
- E002 and E007 remain excluded, so the active random pool is 11,128 combinations.
- H062-H104 / E066-E109 participate in the same Chinese H/E description splicing as earlier fragments.
- Hook, Effect, and generated-relic implementations are consolidated into their main registry/class files; no version-tagged expansion source files remain.
