# ERROR Relics 0.91.3 — H104 / E109 target-DLL audit

## Baseline

- Mod baseline: **ERROR Relics 0.91.3**.
- Game research baseline supplied by the author: **Beta v0.111.0**.
- Implementation facts in this expansion were traced from the uploaded target `sts2.dll` and its metadata/XML, not copied from an older public decompile.
- Important version trap caught during the audit: an older public Toy Box decompile still used 4 Wax relics; the uploaded target DLL uses **5 Wax relics**, **3 combats per melt**, and becomes used up after **15 combats**.
- Design rule: no fake reduced versions of whole Modifiers. If a relic could not be faithfully separated into an immediate Hook and/or Effect, it would be deferred instead of approximated. None of the 42 source relics below required that compromise for the fragments listed here.

## Pool result

- Hooks: **H001–H104 = 104 Hooks**.
- Effects: **E001–E109 = 109 Effects**.
- Random-generation blacklist is unchanged: **E002 and E007 only**.
- Active random Effects: **107**.
- Active random H/E combinations: **104 × 107 = 11,128**.
- Calling Bell, Toy Box, Orrery, Glass Eye and Small Capsule reward effects are **not pre-blacklisted**. They are intentionally live for public-test-style validation.

## 40-relic batch + Calling Bell / Toy Box

Risk labels here are engineering/runtime risk, not design quality.

| # | 中文确认 | Class | Hook | Effect(s) | Target behavior / key call | Risk |
|---:|---|---|---|---|---|---|
| 1 | 手里剑 | `Shuriken` | H062 | E066 | every 3 owner Attacks this turn → Strength 1; turn reset | A |
| 2 | 苦无 | `Kunai` | H063 | E067 | every 3 owner Attacks this turn → Dexterity 1; turn reset | A |
| 3 | 灯笼 | `Lantern` | H064 | E068 | owner first turn → Energy 1 | A |
| 4 | 烫嘴可可 | `VeryHotCocoa` | H065 | E069 | owner first turn → Energy 4 | A |
| 5 | 草莓 | `Strawberry` | H066 | E070 | AfterObtained → Max HP +7 | A |
| 6 | 梨子 | `Pear` | H067 | E071 | AfterObtained → Max HP +10 | A |
| 7 | 芒果 | `Mango` | H068 | E072 | AfterObtained → Max HP +14 | A |
| 8 | 布质果实 | `LoomingFruit` | H069 | E073 | AfterObtained → Max HP +31 | A |
| 9 | 营养牡蛎 | `NutritiousOyster` | H070 | E074 | AfterObtained → Max HP +11 | A |
| 10 | 金色珍珠 | `GoldenPearl` | H071 | E075 | AfterObtained → Gold 150 | A |
| 11 | 图章戒指 | `SignetRing` | H072 | E076 | AfterObtained → Gold 888 | A |
| 12 | 象牙麻将牌 | `IvoryTile` | H073 | E077 | played card EnergyValue >=3 → Energy 1 | A |
| 13 | 钗 | `Sai` | H074 | E078 | owner turn start → Block 7 | A |
| 14 | 吊灯 | `Chandelier` | H075 | E079 | owner turn 3 → Energy 3 | A |
| 15 | 玉之剑 | `SwordOfJade` | H076 | E080 | enter CombatRoom → Strength 3 | A |
| 16 | 风的女儿 | `DaughterOfTheWind` | H077 | E081 | owner Attack played → Block 4, exact GainBlock flag retained | A |
| 17 | 迷失鬼火 | `LostWisp` | H078 | E082 | owner Power played → 8 to all hittable enemies | A |
| 18 | 卡戎之灰 | `CharonsAshes` | H079 | E083 | owner card exhausted → 3 to all enemies | A |
| 19 | 遗忘之魂 | `ForgottenSoul` | H080 | E084 | owner card exhausted → CombatTargets random enemy, blunt VFX, 4 damage | A/B |
| 20 | 铁棒 | `IronClub` | H081 | E085 | `[SavedProperty]` persistent every 4 cards → draw 1 | A/B |
| 21 | 铜质鳞片 | `BronzeScales` | H082 | E086 | enter CombatRoom → Thorns 3 | A |
| 22 | 护喉甲 | `Gorget` | H083 | E087 | enter CombatRoom → Plating 4 | A |
| 23 | 螺线飞镖 | `HelicalDart` | H084 | E088 | Shiv tag → HelicalDartPower 1 | A/B |
| 24 | 永冻冰晶 | `Permafrost` | H085 | E089 | first owner Power each combat → Block 7; reset on combat room | A/B |
| 25 | 空鸟笼 | `EmptyCage` | H086 | E090 | deck removal selector, choose 2 → `RemoveFromDeck(card,true)` | B |
| 26 | 多利之镜 | `DollysMirror` | H087 | E101 | select 1 non-Quest, `RunState.CloneCard`, add Deck Bottom, preview | B |
| 27 | 橙型香盒 | `Pomander` | H088 | E091 | deck upgrade selector, choose 1 → Upgrade + preview | B |
| 28 | 扭曲锤子 | `GnarledHammer` | H089 | E102 | select up to 3, Sharp amount 3, manual confirmation, preview | B |
| 29 | 木札 | `Kifuda` | H090 | E103 | select up to 3, Adroit amount 3, manual confirmation, preview | B |
| 30 | 拳刃 | `PunchDagger` | H091 | E104 | select 1, Momentum 5, preview | B |
| 31 | 三刃回旋镖 | `TriBoomerang` | H092 | E105 | select 3, Instinct 1 + explicit `NCardEnchantVfx` | B |
| 32 | 大帽子 | `BigHat` | H093 | E092 | first turn → 2 distinct unlocked Ethereal character-pool cards to Hand Bottom | B |
| 33 | 橙色团块 | `OrangeDough` | H094 | E093 | first turn → 2 distinct Colorless cards to Hand Bottom | B |
| 34 | 发光珍珠 | `RadiantPearl` | H095 | E094 | BeforeHandDraw turn 1 → create 1 `Luminesce`, Hand Bottom | A/B |
| 35 | 破损核心 | `CrackedCore` | H096 | E095 | first turn BeforeSideTurnStart → Channel Lightning 1 | A/B |
| 36 | 共生病毒 | `SymbioticVirus` | H097 | E096 | first turn AfterSideTurnStart → Channel Dark 1 | A/B |
| 37 | 星系仪 | `Orrery` | H098 | E106 | 5 `CardReward`s, 3 choices each, regular encounter odds → `OfferCustom` | C/test live |
| 38 | 玻璃眼珠 | `GlassEye` | H099 | E107 | 5 rewards: Common, Common, Uncommon, Uncommon, Rare; 3 choices each | C/test live |
| 39 | 小型扭蛋 | `SmallCapsule` | H100 | E108 | `OfferCustom` with one non-predetermined `RelicReward(owner)` | C/test live |
| 40 | 选择悖论 | `ChoicesParadox` | H101 | E109 | 5 distinct character cards, apply Retain, choose 1 via `FromSimpleGrid`, add Hand | C/context-sensitive |
| 41 | 召唤铃铛 | `CallingBell` | H102 | E097, E098 | `AddCurseToDeck<CurseOfTheBell>`; separately Common+Uncommon+Rare RelicRewards | C/test live |
| 42 | 玩具盒 | `ToyBox` | H103, H104 | E099, E100 | offer 5 predetermined Wax relics; every 3 combats melt leftmost unmelted Wax, max 5 activations | C/test live / B melt |

## High-risk effects intentionally left active

### E098 — Calling Bell: three relic rewards

Target `CallingBell.AfterObtained()` is independently structured as:

1. `CardPileCmd.AddCurseToDeck<CurseOfTheBell>(Owner)`
2. `Cmd.Wait(0.75f, false)`
3. `RewardsCmd.OfferCustom(Owner, GenerateRewards())`

`GenerateRewards()` constructs one Common, one Uncommon and one Rare `RelicReward`. E097 and E098 therefore represent real separable source actions; this is not an approximation.

E098 is risky when rewired into mid-combat/action-stack Hooks because `OfferCustom` opens the full reward flow and awaits screen closure. It remains active by author request.

### E099 / E100 — Toy Box

Target constants in the supplied DLL:

- Wax relics: **5**
- combats per melt: **3**
- used-up threshold: **15 combats**

E099 reproduces the target acquisition path:

`RelicFactory.PullNextRelicFromFront(owner)` → `ToMutable()` → `IsWax = true` → predetermined `RelicReward(relic, owner)` → `RewardsCmd.OfferCustom`.

Because these rewards already contain concrete relic models, they are predetermined. The existing ERROR reward-conversion patch deliberately skips predetermined relic rewards, so they remain Wax vanilla relics through that path.

E100 finds the first `Owner.Relics` item satisfying `IsWax && !IsMelted`, executes `RelicCmd.Melt(relic)`, then preserves Toy Box's exact post-melt `Cmd.CustomScaledWait(0.5f, 0.75f, false, CancellationToken.None)`.

### E108 — Small Capsule

This creates `new RelicReward(owner)` rather than a predetermined reward. It intentionally passes through the existing ERROR relic-reward conversion path when applicable. Recursive/cascade acquisition is therefore a deliberate test surface, not pre-disabled behavior.

### E109 — Choices Paradox

Vanilla receives a valid `PlayerChoiceContext` from its native `AfterPlayerTurnStart` Hook. ERROR can reconnect E109 to Hooks that have no choice context and currently use `ThrowingPlayerChoiceContext`.

To uphold the no-crash/no-desync rule without delaying or changing timing:

- active combat is required;
- if the current ERROR Hook supplied `ThrowingPlayerChoiceContext`, E109 immediately no-ops;
- otherwise it executes immediately with the current Hook's actual `PlayerChoiceContext`.

This is intentionally different from creating a fresh unsynchronized UI context, which could allow peers to make independent choices.

## Notes on faithfully retained edge behavior

- Forgotten Soul retains its blunt-hit VFX before damage.
- Tri-Boomerang retains the explicit `NCardEnchantVfx` creation path.
- Dolly's Mirror retains the non-Quest filter, run-state clone, Deck Bottom insertion and pile-add preview.
- Gnarled Hammer / Kifuda retain non-cancelable manual-confirmation multi-select behavior.
- Toy Box retains persistent combat counting and the scaled post-melt wait.
- Iron Club retains a saved cross-combat card counter rather than resetting each combat.
- Permafrost sets its per-combat activation state after the Effect task returns, matching source ordering.

## Whole-Modifier policy

This batch does **not** establish a generic system for whole-relic Modifiers. A future relic whose behavior is fundamentally a persistent Modifier/rule rewrite should be marked deferred instead of being reduced into an approximate delayed or partial Effect. Independent edge actions may still be extracted when the target DLL proves that they are real separable calls.
