# ERROR Relics 0.94.0 — 片段依据表（H105–H116 / E110–E121）

本表是 0.94.0 第一批新增片段的**反编译依据**。所有数值、触发条件、调用顺序
都来自游戏 v0.111.0 的反编译源码，路径前缀：

`D:\STS2errorrelic\sts2-decompiled\MegaCrit\sts2\Core\Models\Relics\`

行号为反编译文件的**当前行号**（`ilspycmd 9.1.0.7988` 生成，3538 个 .cs）。
重跑反编译后行号可能平移，但类名/钩子名不变。

---

## 一、新增 Hook（12）

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原生判定条件 |
| --- | --- | --- | --- | --- |
| H105 | `H105_AfterCardDiscarded` | Tingsha / Tough Bandages | `Tingsha.cs:18`、`ToughBandages.cs:20` (`AfterCardDiscarded`) | `card.Owner == Owner && Owner.Creature.Side == Owner.Creature.CombatState.CurrentSide` |
| H106 | `H106_AfterBlockBroken` | Hand Drill | `HandDrill.cs:21` | `(breaker == Owner.Creature \|\| breaker?.PetOwner == Owner) && !target.IsPlayer` |
| H107 | `H107_AfterGoldGained` | Dragon Fruit | `DragonFruit.cs:22` | `player == Owner` |
| H108 | `H108_Every10StarsGalacticDust` | Galactic Dust | `GalacticDust.cs:88` | `spender == Owner`，累计消耗星辰达到 `StarsVar(10)` |
| H109 | `H109_Every7OrbsMetronome` | Metronome | `Metronome.cs:89` | `player == Owner`，累计生成充能球达到 `DynamicVar("OrbCount", 7)` |
| H110 | `H110_AfterHandEmptied` | Unceasing Top | `UnceasingTop.cs:16` | `player == Owner && IsValidPhase(phase)`，`IsValidPhase: (uint)(phase - 2) <= 2u` |
| H111 | `H111_CardEnteredDeck` | Bing Bong / Lucky Fysh / Darkstone Periapt | `BingBong.cs:28`、`LuckyFysh.cs:24`、`DarkstonePeriapt.cs:16` | `card.Pile?.Type == PileType.Deck && card.Owner == Owner && clonedBy == null` |
| H112 | `H112_AfterAttackBoneFlute` | Bone Flute | `BoneFlute.cs:19` | `command.Attacker == Owner.Creature` 或 `command.Attacker.PetOwner == Owner` |
| H113 | `H113_AfterItemPurchasedMawBank` | Maw Bank | `MawBank.cs:52` | `player == Owner && goldSpent > 0`（不含 Maw Bank 自身的"每局一次"状态） |
| H114 | `H114_BeforeCombatStartLatePetrifiedToad` | Petrified Toad | `PetrifiedToad.cs:16` | `BeforeCombatStartLate()`，无附加条件 |
| H115 | `H115_AfterFlushBookmark` | Bookmark | `Bookmark.cs:18` | `player == Owner` |
| H116 | `H116_AfterRestSiteHealRegalPillow` | Regal Pillow / Stone Humidifier | `RegalPillow.cs:28`、`StoneHumidifier.cs:18` | `player == Owner` |

实现位置：`ErrorRelicsCode\Relics\ErrorGeneratedRelic.Hooks105.cs`。
每个 override 先以 `HookId != ErrorHookId.Hxxx` 早退，保证同一件遗物只认领
自己的时机；判定通过后再 `Flash()` 并 `ExecuteExpandedEffect(...)`。

计数语义：

* `GalacticDustStarsSpent` 是 `[SavedProperty]`，与原版 `GalacticDust.StarsSpent`
  一样跨战斗保存，`.DamageVar` 结算后保留余数（`while (>= 10) { -= 10; ... }` 支持一次跨多个阈值）。
* `_metronomeOrbsChanneled` 是普通字段，在 `BeforeCombatStartLate()` 清零，
  对应原版 `Metronome.AfterRoomEntered / AfterCombatEnd` 的清零时机。

---

## 二、新增 Effect（12）

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原版数值 / 调用 |
| --- | --- | --- | --- | --- |
| E110 | `E110_DiscardDamageRandomEnemy3Tingsha` | Tingsha (Uncommon) | `Tingsha.cs:18` | `DamageVar(3m, ValueProp.Unpowered)`，随机目标 + `VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_blunt")` |
| E111 | `E111_DiscardGainBlock3ToughBandages` | Tough Bandages (Rare) | `ToughBandages.cs:20` | `BlockVar(3m, ValueProp.Unpowered)` |
| E112 | `E112_BreakBlockApplyVulnerable2HandDrill` | Hand Drill (Event) | `HandDrill.cs:21` | `PowerVar<VulnerablePower>(2m)`；目标泛化为随机敌人（见下） |
| E113 | `E113_GainGold15LuckyFysh` | Lucky Fysh (Uncommon) | `LuckyFysh.cs:24` | `GoldVar(15)` → `PlayerCmd.GainGold(15, Owner)` |
| E114 | `E114_GalacticDustGainBlock10` | Galactic Dust (Uncommon) | `GalacticDust.cs:88` | `BlockVar(10m, ValueProp.Unpowered)` |
| E115 | `E115_BellowsUpgradeAllHandCards` | Bellows (Rare) | `Bellows.cs:15` | `CardCmd.Upgrade(PileType.Hand.GetPile(Owner).Cards, CardPreviewStyle.HorizontalLayout)` |
| E116 | `E116_DivineDestinyGainStars7` | Divine Destiny (Starter) | `DivineDestiny.cs:18` | `StarsVar(7)` → `PlayerCmd.GainStars(7, Owner)` |
| E117 | `E117_RunicCapacitorAddOrbSlots3` | Runic Capacitor (Shop) | `RunicCapacitor.cs:18` | `RepeatVar(3)` → `OrbCmd.AddSlots(Owner, 3)` |
| E118 | `E118_FencingManualForge10` | Fencing Manual (Common) | `FencingManual.cs:21` | `ForgeVar(10)` → `ForgeCmd.Forge(10, Owner, source)` |
| E119 | `E119_DiamondDiademGainBlock20AndBlur1` | Diamond Diadem (Ancient) | `DiamondDiadem.cs:24` | `BlockVar(20m, Unpowered)` + `PowerCmd.Apply<BlurPower>(..., 1m, ...)` |
| E120 | `E120_DarkstonePeriaptGainMaxHp6` | Darkstone Periapt (Event) | `DarkstonePeriapt.cs:16` | `MaxHpVar(6m)` → `CreatureCmd.GainMaxHp(Owner.Creature, 6)` |
| E121 | `E121_TeaOfDiscourtesyAdd2DazedToDraw` | Tea of Discourtesy (Event) | `TeaOfDiscourtesy.cs:56` | `DynamicVar("DazedCount", 2m)` → `CardPileCmd.AddToCombatAndPreview<Dazed>(Owner.Creature, PileType.Draw, 2, Owner, CardPilePosition.Random)` |

实现位置：`ErrorRelicsCode\Fragments\ErrorEffectRegistry.cs`（`ExecuteAsync` 的
`switch` 内，位于 `default:` 之前）。

### 唯一一处刻意的目标泛化：E112

原版 Hand Drill 的易伤目标是"被击破格挡的那个敌人"，这个目标由 Hook 提供。
ERROR 的 H 与 E 是两个可任意组合的独立片段，E 片段必须自带目标，因此 E112
把目标泛化为"随机一个可命中敌人"，**数值保持原版 2 层不变**。这一点在
`ErrorEffectRegistry.cs` 的 E112 注释里同样写明。

---

## 三、同批核对但本批不落地（第二批候选）

| 原版行为 | 反编译位置 | 为什么不进本批 |
| --- | --- | --- |
| Bing Bong 复制进入牌组的牌 | `BingBong.cs:28` | 需要在 `RunState.CloneCard` 之后把克隆体追加到牌组底部，涉及牌组变更回调的递归保护 |
| Emotion Chip 触发所有充能球被动 | `EmotionChip.cs:40` | `OrbCmd.Passive` 逐球 `Cmd.Wait(0.25f)`，是长序列演出 |
| Crossbow 生成 1 张免费攻击牌 | `Crossbow.cs:17` | 需要 `CardFactory.GetDistinctForCombat` + `SetToFreeThisTurn` + 联机生成同步 |
| Power Cell 抽 2 张零费牌 | `PowerCell.cs:21` | 需要过滤 `GetWithModifiers(CostModifiers.Local) == 0` 后再随机 |
| Delicate Frond 补满药水槽 | `DelicateFrond.cs:12` | `PotionCmd.TryToProcure` 涉及药水 UI 与容量 |
| Seal of Gold 付费换能量 | `SealOfGold.cs:25` | 需要"金币不足则不触发"的条件型 Hook，且会改变经济 |
| Art of War 上回合无攻击则给能量 | `ArtOfWar.cs:55/78/89` | 需要改主文件的 `AfterCardPlayed` / `AfterSideTurnEnd` 回合状态机 |
| Petrified Toad 药水效果、Bone Tea 升级手牌、Self Forming Clay、Metronome/Dragon Fruit 的宝箱限制 | 见上表行号 | 需要药水系统或额外状态 |

---

## 四、第二批（wave 2）：H117–H124 / E122–E128

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原生判定与数值 |
| --- | --- | --- | --- | --- |
| H117 | `H117_AfterSideTurnStartSealOfGold` | Seal of Gold (Ancient) | `SealOfGold.cs:25` | `participants.Contains(Owner.Creature) && Owner.Gold >= GoldVar(3)`，**没有回合限制** |
| H118 | `H118_BeforeSideTurnStartPowerCell` | Power Cell (Rare) | `PowerCell.cs:21` | 参战且 `PlayerCombatState.TurnNumber <= 1` |
| H119 | `H119_BeforeCombatStartDelicateFrond` | Delicate Frond (Ancient) | `DelicateFrond.cs:12` | `BeforeCombatStart()`，无附加条件 |
| H120 | `H120_FirstTurnAfterPlayerTurnStartBoneTea` | Bone Tea (Event) | `BoneTea.cs:50` | `player == Owner && TurnNumber <= 1`（原版另有"一场战斗"状态，片段不复制该状态） |
| H121 | `H121_AfterEnergyResetNoAttackArtOfWar` | Art of War (Rare) | `ArtOfWar.cs:55,78,89` | `TurnNumber > 1 && !playedAttackLastTurn` |
| H122 | `H122_AfterDamageReceivedUnblockedSelfFormingClay` | Self-Forming Clay (Uncommon) | `SelfFormingClay.cs:25` | 战斗中、`target == Owner.Creature`、`UnblockedDamage > 0` |
| H123 | `H123_AfterPlayerTurnStartLostHpEmotionChip` | Emotion Chip (Rare) | `EmotionChip.cs:21,40` | 战斗中 `target == Owner.Creature && !WasFullyBlocked` 记录，下一回合开始触发 |
| H124 | `H124_AfterSideTurnStartCrossbow` | Crossbow (Ancient) | `Crossbow.cs:17` | `participants.Contains(Owner.Creature)`，**没有回合限制** |
| E122 | `E122_SealOfGoldGainEnergy1LoseGold3` | Seal of Gold | `SealOfGold.cs:25` | `PlayerCmd.GainEnergy(1)` + `PlayerCmd.LoseGold(3)` |
| E123 | `E123_PowerCellDraw2ZeroCostCards` | Power Cell | `PowerCell.cs:21` | 抽牌堆过滤 `!CostsX && GetWithModifiers(Local) == 0` → `StableShuffle(CombatCardSelection)` → 取 2 |
| E124 | `E124_DelicateFrondFillPotionSlots` | Delicate Frond | `DelicateFrond.cs:12` | `while (HasOpenPotionSlots)` 随机药水 + `PotionCmd.TryToProcure`，失败即停 |
| E125 | `E125_ArtOfWarGainEnergy1` | Art of War | `ArtOfWar.cs:89` | `PlayerCmd.GainEnergy(1)` |
| E126 | `E126_SelfFormingClayBlockNextTurn3` | Self-Forming Clay | `SelfFormingClay.cs:25` | `PowerCmd.Apply<SelfFormingClayPower>(..., 3, ...)` |
| E127 | `E127_EmotionChipTriggerAllOrbPassives` | Emotion Chip | `EmotionChip.cs:40` | 遍历 `OrbQueue.Orbs`：`OrbCmd.Passive(..., countAffectedByHooks: true)` + `Cmd.Wait(0.25f)` |
| E128 | `E128_CrossbowAdd1FreeAttack` | Crossbow | `Crossbow.cs:17` | 卡池过滤 Attack → `GetDistinctForCombat(..., 1, CombatCardGeneration)` → `SetToFreeThisTurn()` → `AddGeneratedCardsToCombat(Hand)` |

### 实现上的两个取舍

1. **Art of War 的回合结转点**：原版在 `AfterSideTurnEnd` 结转本回合标志，该 hook 已被主文件占用，
   因此改在下一个己方回合开始的 `BeforeSideTurnStart` 里结转（早于 `AfterEnergyReset`）。
   判定条件（不是第一回合、上一回合没打攻击）与数值（1 能量）不变。
2. **Bing Bong 不单独做 Effect**：它的"复制刚进牌组的牌"必须知道**具体是哪张牌**，
   而 ERROR 的 E 片段是独立、无参数的。硬做只能变成"复制一张随机牌"，
   那已经是另一个机制（modifier 型），所以只保留 H111 作为通用触发器。
   同理 Emo Chip 的"受伤记账"由 H123 承担，E127 只负责"触发所有球被动"。

### 第二批的计数与文案同步

- `ErrorProofRelic` 文案：124 Hooks × 128 Effects = 15,872。
- `tests\Program.cs` 断言：124 / 126（E002、E007 仍然排除）。
- `ErrorEffectSourceMetadata.IsAncientSource` 新增 E122（Seal of Gold）、E124（Delicate Frond）、E128（Crossbow）。

---

## 五、第三批（wave 3）：H125–H129 / E129–E133

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原生判定与数值 |
| --- | --- | --- | --- | --- |
| H125 | `H125_AfterCombatVictoryEliteWarHammer` | War Hammer (Ancient) | `WarHammer.cs:15` | `room.RoomType == RoomType.Elite` |
| H126 | `H126_AfterCombatVictoryBurningBlood` | Burning Blood (Starter) | `BurningBlood.cs:13` | `!Owner.Creature.IsDead` |
| H127 | `H127_AfterCombatVictoryBlackBlood` | Black Blood (Starter) | `BlackBlood.cs:16` | `!Owner.Creature.IsDead` |
| H128 | `H128_AfterSideTurnEndLunarPastry` | Lunar Pastry (Rare) | `LunarPastry.cs:15` | `participants.Contains(Owner.Creature)` |
| H129 | `H129_AfterCardGeneratedForCombatRegalite` | Regalite (Uncommon) | `Regalite.cs:26` | `creator == Owner` 且每回合一次 |
| E129 | `E129_WarHammerUpgrade4RandomDeckCards` | War Hammer | `WarHammer.cs:15` | `CardsVar(4)`：`StableShuffle(Rng.Niche).Take(4)` → `CardCmd.Upgrade` |
| E130 | `E130_BurningBloodHeal6` | Burning Blood | `BurningBlood.cs:13` | `HealVar(6)` |
| E131 | `E131_BlackBloodHeal12` | Black Blood | `BlackBlood.cs:16` | `HealVar(12)` |
| E132 | `E132_LunarPastryGain1Star` | Lunar Pastry | `LunarPastry.cs:15` | `StarsVar(1)` |
| E133 | `E133_RegaliteGainBlock4` | Regalite | `Regalite.cs:26` | `BlockVar(4, Unpowered)`，`fast: true` |

### 这一批特别要注意的时机

`AfterCombatVictory` 是在**战斗已经结束之后**才触发的，此时
`CombatManager.Instance.IsInProgress` 可能已是 `false`。
E129（升级牌组）、E130 / E131（回血）在战斗外执行本身也不会崩，
所以不需要（也不应该）进崩溃黑名单。

---

## 六、第四批（wave 4）：H130–H133 / E134–E137

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原生判定与数值 |
| --- | --- | --- | --- | --- |
| H130 | `H130_BeforeSideTurnEndCloakClasp` | Cloak Clasp (Rare) | `CloakClasp.cs:22` | 回合结束、参战、手牌不为空 |
| H131 | `H131_EnterCombatDivineRight` | Divine Right (Starter) | `DivineRight.cs:12` | `room is CombatRoom` |
| H132 | `H132_EnterCombatEmberTea` | Ember Tea (Event) | `EmberTea.cs:58` | `room is CombatRoom`（原版另限 5 场，片段不设上限） |
| H133 | `H133_AfterSideTurnStartLeftoverEnergyPaelsTears` | Pael's Tears (Ancient) | `PaelsTears.cs:39` | 回合结束记录 `PlayerCombatState.Energy > 0`，下回合开始触发 |
| E134 | `E134_CloakClaspGainBlockPerHandCard` | Cloak Clasp | `CloakClasp.cs:22` | `cards.Count * BlockVar(1, Unpowered)` |
| E135 | `E135_DivineRightGainStars3` | Divine Right | `DivineRight.cs:12` | `StarsVar(3)` |
| E136 | `E136_EmberTeaGainStrength2` | Ember Tea | `EmberTea.cs:58` | `PowerVar<StrengthPower>(2)` |
| E137 | `E137_PaelsTearsGainEnergy2` | Pael's Tears | `PaelsTears.cs:39` | `EnergyVar(2)` |

### 这一批特别要注意的时机

H133 是"两个时刻合作"的触发条件：`BeforeSideTurnEnd` 只记账（是否有剩余能量），
真正的 Effect 在下一个 `AfterSideTurnStart` 发出 —— 与原版 Pael's Tears 完全一致。
标志位是非持久字段 `_paelsTearsLeftoverEnergy`，每回合结束都会被重写，不需要额外清零。

H131 / H132 在"进入战斗房间"时触发，那一刻 `IsInProgress` 未必已为 true，
所以这两个 Effect 不进崩溃黑名单，而是在 Effect 内部对 `CombatState is null` 判空返回。

---

## 七、组合规模与安全性

* 枚举总量：Hook 149 可用 + 1 个保留位（H001–H150，其中 H144 已取消），Effect 144（E001–E144）。
* 随机池：E002、E007 继续排除，实际 149 × 142 = **21,158** 种组合；
  理论笛卡尔积 149 × 144 = **21,456**。
* 崩溃黑名单（**不是白名单**）：`ErrorExecutionCompatibility.WouldCrashOutsideCombat`
  （原 `RequiresActiveCombat`）。设计原则由用户定死 —— 唯一底线是游戏不崩，
  片段跑出 NOOP 属于正确行为，因此只登记"战斗外执行必然读到 null `CombatState` /
  非法战斗堆栈"的 Effect，其余一律放行，新增 Effect 不需要为完整性补表。
* 未做任何延迟补发：战斗外触发的"战斗类"效果直接跳过，不 Pending、不跨房间补发。

---

## 八、第五批（wave 5）：H134–H137 / E138–E141

| 编号 | 枚举名 | 来源遗物 | 反编译位置 | 原生判定与数值 |
| --- | --- | --- | --- | --- |
| H134 | `H134_FirstTurnAfterSideTurnStartInfusedCore` | Infused Core (Starter) | `InfusedCore.cs:22` | 参战且 `TurnNumber <= 1` |
| H135 | `H135_FirstTurnAfterSideTurnStartBread` | Bread (Shop) | `Bread.cs:41` | 参战且 `TurnNumber == 1` |
| H136 | `H136_EliteFirstTurnAfterSideTurnStartBoomingConch` | Booming Conch (Ancient) | `BoomingConch.cs:44` | 参战、`TurnNumber <= 1`、`RunState.CurrentRoom?.RoomType == Elite` |
| H137 | `H137_FirstTurnBeforeHandDrawBlessedAntler` | Blessed Antler (Ancient) | `BlessedAntler.cs:48` | `player == Owner` 且 `TurnNumber == 1` |
| E138 | `E138_InfusedCoreChannel3Lightning` | Infused Core | `InfusedCore.cs:22` | `DynamicVar("Lightning", 3)` 次 `OrbCmd.Channel<LightningOrb>` |
| E139 | `E139_BreadLoseEnergy2` | Bread | `Bread.cs:41` | `EnergyVar("LoseEnergy", 2)` |
| E140 | `E140_BoomingConchGainEnergy1` | Booming Conch | `BoomingConch.cs:44` | `EnergyVar(1)` |
| E141 | `E141_BlessedAntlerAdd3DazedToDraw` | Blessed Antler | `BlessedAntler.cs:48` | `CardsVar(3)` 张 `Dazed`，随机位置入抽牌堆 |

### 这一批特别要注意的时机

原版 Infused Core / Bread / Booming Conch 都只在**第一回合**生效，判据分别是
`TurnNumber <= 1`、`== 1`、`== 1`，片段照抄，不做"每回合"改写。
Booming Conch 额外要求当前房间是精英房，用 `owner.RunState.CurrentRoom?.RoomType`
读取（与原版 `player.RunState.CurrentRoom` 同源）。
Blessed Antler 的造卡必须走 `combatState.CreateCard<Dazed>(Owner)` 再
`CardPileCmd.AddGeneratedCardsToCombat(..., PileType.Draw, ...)`，与原生一致。

---

## 九、第六批（wave 6）：H138–H142（原创组合时机）

这一批不再有可逐字对齐的遗物：下面五个 Hook 在 v0.111.0 的 300 个遗物里
**没有任何一个使用**。触发条件直接按 `AbstractModel` 的签名与 `Hooks/Hook.cs`
的分发语义定义，E 片段复用既有池，不新增效果。

| 编号 | 枚举名 | 原生签名 | 反编译位置 | 触发判定 |
| --- | --- | --- | --- | --- |
| H138 | `H138_AfterRewardTaken` | `AfterRewardTaken(Player, Reward)` | `AbstractModel.cs:1131` / `Hook.cs:1104` | 本遗物主人领取奖励 |
| H139 | `H139_AfterRestSiteSmith` | `AfterRestSiteSmith(Player)` | `AbstractModel.cs:1121` / `Hook.cs:1092` | 本遗物主人在营火锻打 |
| H140 | `H140_AfterActEntered` | `AfterActEntered()` | `AbstractModel.cs:208` / `Hook.cs:68` | 进入新的一幕 |
| H141 | `H141_AfterBlockGained` | `AfterBlockGained(Creature, decimal, ValueProp, CardModel?)` | `AbstractModel.cs:330` / `Hook.cs:143` | 本遗物主人获得格挡且 amount > 0 |
| H142 | `H142_AfterDamageGiven` | `AfterDamageGiven(PlayerChoiceContext, Creature?, DamageResult, ValueProp, Creature, CardModel?)` | `AbstractModel.cs:593` / `Hook.cs:401` | 本遗物主人或其宠物造成伤害 |

实现位置：`ErrorRelicsCode\Relics\ErrorGeneratedRelic.Hooks138.cs`（新的 partial 分片，
避免继续膨胀 71KB 的主文件）。五个 override 都只做"条件判断 + Flash + ExecuteExpandedEffect"，
不新增任何延迟或队列。

H140（进入新的一幕）与 H139（营火锻打）都在战斗外触发，配到战斗类 E 片段时
由崩溃黑名单直接跳过，属于允许的 NOOP。

---

## 十、第七批（wave 7）：H143–H147（被单个遗物使用的时机）

这些时机在 v0.111.0 里只被一个（或两个）遗物使用，且效果可以离散化，
因此作为独立 Hook 收进来；E 片段仍复用既有池，不新增效果。

| 编号 | 枚举名 | 来源遗物 | 原生签名 | 反编译位置 |
| --- | --- | --- | --- | --- |
| H143 | `H143_AfterPotionProcured` | Belt Buckle | `AfterPotionProcured(PotionModel)` | `AbstractModel.cs:1039` / `Hook.cs:984` |
| H144 | `H144_ReservedCardEnteredCombat` | ~~Ghost Seed~~ | ~~`AfterCardEnteredCombat(CardModel)`~~ | 已取消，见下方说明 |
| H145 | `H145_AfterCreatureAddedToCombat` | Philosophers Stone / Fur Coat | `AfterCreatureAddedToCombat(Creature)` | `AbstractModel.cs:565` / `Hook.cs:374` |
| H146 | `H146_AfterPreventingDeath` | Lizard Tail | `AfterPreventingDeath(Creature)` | `AbstractModel.cs:1097` / `Hook.cs:1056` |
| H147 | `H147_AfterTakingExtraTurn` | Pael's Eye | `AfterTakingExtraTurn(Player)` | `AbstractModel.cs:1220` / `Hook.cs:1232` |

实现位置：`ErrorRelicsCode\Relics\ErrorGeneratedRelic.Hooks143.cs`。

判定口径（逐字对齐来源遗物）：
- H143 只认 `potion.Owner == Owner`（原版 Belt Buckle 在 `AfterPotionProcured` 里刷新状态并移除敏捷）。
- **H144 已取消**（用户决定，2026-10-30）：触发点落在战斗初始化期间，玩家看不懂什么时候该触发，还引出过选择器未就绪的崩溃。枚举位保留（值仍是 143）以免 H145–H150 整体前移、让老存档里的遗物换成别的时机；它不参与生成，也没有任何 override，所以永远不会触发。
- H145 排除玩家生物，只在有生物加入战斗时触发。
- H146 只认 `creature == Owner.Creature`（原版 Lizard Tail 在阻止死亡后按最大生命百分比回血）。
- H147 只认 `player == Owner`（原版 Pael's Eye 用掉本场额外回合）。

这些时机在战斗外也可能触发，配到战斗类 E 片段时由崩溃黑名单兜底，属于允许的 NOOP。

---

## 十一、第八批（wave 8）：E142–E144（补 E 片段空缺）

这一批不新增 Hook，只补「原版有离散效果、但池子里没有对应 E 片段」的空缺。
三条效果都来自普通遗物，数值逐字取自 `CanonicalVars`。

| 编号 | 枚举名 | 来源遗物 | 原版数值 | 反编译位置 |
| --- | --- | --- | --- | --- |
| E142 | `E142_GainGold12MawBank` | Maw Bank | `GoldVar(12)` | `MawBank.cs:CanonicalVars` |
| E143 | `E143_GainBlock2BoneFlute` | Bone Flute | `BlockVar(2, Unpowered)` | `BoneFlute.cs:29` |
| E144 | `E144_Draw2BagOfPreparation` | Bag of Preparation | `CardsVar(2)` | `BagOfPreparation.cs:ModifyHandDraw` |

实现位置：`ErrorRelicsCode\Fragments\ErrorEffectRegistry.cs`（新增 3 个 case + 3 条英文）。

崩溃黑名单：E143（读 `Creature` 的格挡）与 E144（读手牌堆）登记进
`WouldCrashOutsideCombat`；E142 只是加金币，不设限。

Bag of Preparation 原版是「第一回合多抽 2 张」的 `ModifyHandDraw` 修正器，
做成片段后语义改为一次性「抽 2 张」，这是本批唯一的语义偏移，已在枚举注释中标注。

---

## 十二、第九批（wave 9）：H148–H150（收尾的三个触发时机）

至此"触发型"遗物时机全部收完；剩下的 Hook 都是 modifier / 奖励改写 / 价格改写型，
按既定原则不做 Effect。

| 编号 | 枚举名 | 来源遗物 | 原生签名 | 反编译位置 |
| --- | --- | --- | --- | --- |
| H148 | `H148_AfterPotionDiscarded` | Belt Buckle | `AfterPotionDiscarded(PotionModel)` | `AbstractModel.cs:1029` / `Hook.cs:972` |
| H149 | `H149_BeforeCombatRewardOffered` | Lasting Candy | `BeforeCombatRewardOffered(RewardsSet, CombatRoom)` | `AbstractModel.cs:532` / `Hook.cs:340` |
| H150 | `H150_AfterCurrentHpChanged` | Meat on the Bone / Red Skull | `AfterCurrentHpChanged(Creature, decimal)` | `AbstractModel.cs:578` / `Hook.cs:386` |

实现位置：`ErrorRelicsCode\Relics\ErrorGeneratedRelic.Hooks148.cs`。

**刻意不收的时机**：`AfterPreventingDraw`（Fiddle）。`Hook.cs:1068` 只把该回调发给
"阻止抽牌的那个模型"本身，而不是所有遗物，片段永远收不到，收进来只是死代码。

**明确不做的 Hook 类别**（与用户约定一致，已逐项确认没有离散效果可提取）：
`Modify*` / `TryModify*` / `Should*` / `IsAllowed*` / `AfterModifying*` /
`BeforePowerAmountChanged` / `ModifyOrbValue` / `ModifyXValue` / `ModifyMerchantPrice` /
`ModifyGeneratedMap*` / `TryModifyCardReward*` / `TryModifyRewards*` /
`TryModifyRestSite*` / `ModifyHandDraw` / `ModifyMaxEnergy` / `ModifyDamage*` /
`ModifyBlock*` / `ModifyHpLost*` / `ShouldDie*` / `ShouldClearBlock` / `AfterCloned`。

---

## 十三、文案同步位置

| 位置 | 内容 |
| --- | --- |
| `ErrorRelicsCode\Fragments\ErrorDescriptionText.cs` | `Hooks[]` / `Effects[]` 两个下标对齐的字符串数组，追加 12+12 条中文 |
| `ErrorRelicsCode\Fragments\ErrorHookRegistry.cs` | `GetText(ErrorHookId)` 的 12 条英文时机文本 |
| `ErrorRelicsCode\Fragments\ErrorEffectRegistry.cs` | `GetText(ErrorEffectId)` 的 12 条英文效果文本 |
| `ErrorRelicsCode\Relics\ErrorProofRelic.cs` | 核心池计数文案（149 × 144 = 21,456） |
| `tests\Program.cs` | 可达性断言（149 / 142） |

`ErrorDescriptionText.Hooks[]` 与 `Effects[]` 的**下标必须与枚举数值严格一致**
（`Lookup` 用 `number - 1` 取值）。E009/E010 的历史存储顺序不能改动。

---

## 十四、组合黑名单（实测反馈，2026-10-30）

这一节记录**实测发现的危险组合**，与单个 Effect 的黑名单（`WouldCrashOutsideCombat`）不是一回事：

- `WouldCrashOutsideCombat` 管的是「这个 Effect 在战斗外执行会读到 null」；
- 这里的黑名单管的是「这个 Hook 会被自己的 Effect 再次喂出来」，只拦**特定组合**。

实现位置：`ErrorRelicsCode\Fragments\ErrorFragmentBlacklist.cs`，由 `ErrorExecutionCompatibility.ExecuteCoreAsync` 在真正执行 Effect 前查询，命中即跳过（NOOP），不报错、不弹窗。

### 软锁类：自我喂养

| Hook | 被拉的 Effect | 现象 |
| --- | --- | --- |
| `H141_AfterBlockGained` | `E027` `E045` `E050` `E111` `E114` `E133` `E134` `E143` | 给格挡 → 触发 AfterBlockGained → 再给格挡，无限循环，游戏软锁 |
| `H150_AfterCurrentHpChanged` | `E030` `E032` `E033` `E048` `E049` `E063` `E120` `E130` `E131` | 回血/涨最大生命 → 触发 AfterCurrentHpChanged → 再回血，无限循环，游戏软锁 |

### 崩溃类：先隔离

| Hook | Effect | 现象 |
| --- | --- | --- |
| ~~`H144_AfterCardEnteredCombat`~~ | `E109_ChoicesParadoxChoose1Of5RetainToHand` | 已随 H144 整体取消：触发点本身被移除，这条组合不再可能出现，黑名单条目也已删除 |

### 仍在观察

- **伤害递归**：`H142_AfterDamageGiven` × 伤害类 Effect 会连锁，实测几乎能秒掉所有敌人，但尚未确认是否真的无界（需要挑杀不掉的敌人继续测）。确认无界后再决定进黑名单还是加重入保护。

### 回归覆盖

`tests\Program.cs` 已为上述组合加了断言：17 个软锁组合（8 个格挡 + 9 个治疗/最大生命）必须命中黑名单，同一批 Effect 在无关 Hook（如 `H142`）上必须仍然放行，避免把黑名单写成「按 Effect 一刀切」；另外还断言 H144 保留位永远不会被生成、且不影响后续枚举数值。
