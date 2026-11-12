public enum ErrorHookId
{
    // =========================================================
    // H001
    // 来源遗物：Vajra
    //
    // Hook：
    // 进入战斗时
    // =========================================================
    H001_EnterCombat,


    // =========================================================
    // H002
    // 来源遗物：Mercury Hourglass
    //
    // Hook：
    // 自己的回合开始时
    // =========================================================
    H002_PlayerTurnStart,


    // =========================================================
    // H003
    // 来源遗物：Intimidating Helmet
    //
    // Hook：
    // 自己打出一张牌
    // 并且这次打牌使用至少 2 点 Energy
    // =========================================================
    H003_PlayCardEnergy2Plus,


    // =========================================================
    // H004
    // 来源遗物：Old Coin
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H004_AfterObtained,


    // =========================================================
    // H005
    // 来源遗物：Mummified Hand
    //
    // Hook：
    // 战斗中
    // 自己打出一张 Power 牌之后
    // =========================================================
    H005_PowerCardPlayed,


    // =========================================================
    // H006
    // 来源遗物：Distinguished Cape
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    //
    // 和 H004 的 Hook 逻辑完全相同，
    // 但保留独立 Fragment ID 和随机权重。
    // =========================================================
    H006_AfterObtained,


    // =========================================================
    // H007
    // 来源遗物：Toolbox
    //
    // Hook：
    // 第一回合起始手牌抽取之前
    //
    // 原版条件：
    // BeforeHandDraw
    // + player == Owner
    // + TurnNumber == 1
    // =========================================================
    H007_FirstTurnBeforeHandDraw,


    // =========================================================
    // H008
    // 来源遗物：Astrolabe
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    //
    // 原版 Astrolabe 使用 AfterObtained。
    //
    // 与 H004 Old Coin、
    // H006 Distinguished Cape
    // 使用相同的原生 Hook。
    //
    // 但来源遗物不同，
    // 所以保留独立 Fragment ID 和随机权重。
    // =========================================================
    H008_AfterObtained,


    // =========================================================
    // H009
    // 来源遗物：Royal Stamp
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    //
    // 与 H004 / H006 / H008 使用相同的原生
    // AfterObtained Hook。
    //
    // 但来源遗物不同，
    // 所以保留独立 Fragment ID 和随机权重。
    // =========================================================
    H009_AfterObtained,


    // =========================================================
    // H010
    // 来源遗物：Nutritious Soup
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H010_AfterObtained,


    // =========================================================
    // H011
    // 来源遗物：Pael's Claw
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H011_AfterObtained,


    // =========================================================
    // H012
    // 来源遗物：Sand Castle
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H012_AfterObtained,


    // =========================================================
    // H013
    // 来源遗物：War Paint
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H013_AfterObtained,


    // =========================================================
    // H014
    // 来源遗物：Whetstone
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H014_AfterObtained,


    // =========================================================
    // H015
    // 来源遗物：Neow's Talisman
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H015_AfterObtained,


    // =========================================================
    // H016
    // 来源遗物：Pael's Horn
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H016_AfterObtained,


    // =========================================================
    // H017
    // 来源遗物：Neow's Torment
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H017_AfterObtained,


    // =========================================================
    // H018
    // 来源遗物：Storybook
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H018_AfterObtained,


    // =========================================================
    // H019
    // 来源遗物：Jewelry Box
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H019_AfterObtained,


    // =========================================================
    // H020
    // 来源遗物：Tanx's Whistle
    //
    // Hook：
    // 获得 / 拾取这件遗物时
    // =========================================================
    H020_AfterObtained,


    // =========================================================
    // H021
    // 来源遗物：Happy Flower
    //
    // Hook：
    // 每经过自己的 3 个回合触发一次。
    //
    // 原版持久化计数：
    // TurnsSeen = (TurnsSeen + 1) % 3
    // 战斗结束不清零。
    // =========================================================
    H021_Every3TurnsHappyFlower,


    // =========================================================
    // H022
    // 来源遗物：Stone Calendar
    //
    // Hook：
    // 本场战斗第 7 回合结束时。
    //
    // 原版不使用跨战斗计数，
    // 直接检查 PlayerCombatState.TurnNumber == 7。
    // =========================================================
    H022_EndOfTurn7StoneCalendar,


    // =========================================================
    // H023
    // 来源遗物：Pendulum
    //
    // Hook：
    // 每经过自己的 3 个回合触发一次。
    //
    // 原版持久化计数：
    // TurnsSeen = (TurnsSeen + 1) % 3
    // 战斗结束不清零。
    // =========================================================
    H023_Every3TurnsPendulum,


    // =========================================================
    // H024
    // 来源遗物：Sparkling Rouge
    // 原版 Hook：AfterBlockCleared(Creature creature)
    // 条件：creature == Owner.Creature && TurnNumber == 3
    // =========================================================
    H024_Turn3AfterBlockClearedSparklingRouge,


    // =========================================================
    // H025
    // 来源遗物：Oddly Smooth Stone
    // 原版 Hook：AfterRoomEntered(AbstractRoom room)
    // 条件：room is CombatRoom
    // =========================================================
    H025_EnterCombatOddlySmoothStone,


    // =========================================================
    // H026
    // 来源遗物：Horn Cleat
    // 原版 Hook：AfterBlockCleared(Creature creature)
    // 条件：creature == Owner.Creature && TurnNumber == 2
    // =========================================================
    H026_Turn2AfterBlockClearedHornCleat,


    // =========================================================
    // H027
    // 来源遗物：Captain's Wheel
    // 原版 Hook：AfterBlockCleared(Creature creature)
    // 条件：creature == Owner.Creature && TurnNumber == 3
    //
    // 与 H024 条件相同，但来源遗物不同，
    // 所以保留独立 Fragment ID。
    // =========================================================
    H027_Turn3AfterBlockClearedCaptainsWheel,


    // =========================================================
    // H028
    // 来源遗物：Candelabra
    // 原版 Hook：AfterSideTurnStart(...)
    // 条件：participants.Contains(Owner.Creature)
    //       && TurnNumber == 2
    // =========================================================
    H028_Turn2SideTurnStartCandelabra,


    // =========================================================
    // H029
    // 来源遗物：Meal Ticket
    //
    // 原版 Hook：
    // AfterRoomEntered(AbstractRoom room)
    //
    // 条件：
    // Owner.Creature.IsDead == false
    // && room is MerchantRoom
    // =========================================================
    H029_EnterMerchantMealTicket,


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    //
    // 语义：
    // 每累计 Exhaust 自己的 5 张牌时触发一次。
    //
    // 原版：
    // - 非 Ethereal Exhaust 立即累计并检查
    // - Ethereal Exhaust 先暂存在 EtherealCount
    // - 自己一侧回合结束时统一加入 CardsExhausted
    // - CardsExhausted 是 SavedProperty，跨战斗保留
    // =========================================================
    H030_Every5ExhaustsJossPaper,


    // =========================================================
    // H031
    // 来源遗物：Planisphere
    //
    // 原版 Hook：
    // AfterRoomEntered(AbstractRoom room)
    //
    // 条件：
    // Owner.Creature.IsDead == false
    // && CurrentMapPoint != null
    // && CurrentMapPoint.PointType == Unknown
    // && CurrentRoomCount <= 1
    // =========================================================
    H031_EnterFirstUnknownRoomPlanisphere
,
    H032_AfterObtainedBigMushroom,
    H033_EnemyDeathGremlinHorn,
    H034_Every3SkillsThisTurnLetterOpener,
    H035_Every3AttacksThisTurnKusarigama,
    H036_Every3AttacksThisTurnOrnamentalFan,
    H037_Every10AttacksPersistentNunchaku,
    H038_Every10SkillsPersistentTuningFork,
    H039_FirstTurnBeforeSideTurnStartBagOfMarbles,
    H040_FirstTurnBeforeSideTurnStartTwistedFunnel,
    H041_FirstTurnAfterSideTurnStartAkabeko,
    H042_FirstUnblockedDamageEachCombatCentennialPuzzle,
    H043_EndTurnIfZeroBlockOrichalcum,
    H044_EndTurnIfNoAttackRippleBasin,
    H045_AfterTurnEndIfBlock10ParryingShield,
    H046_BeforeBossCombatPantograph,
    H047_FirstTurnStartLateBloodVial,
    H048_BeforeCombatStartAnchor,
    H049_EnterCombatDataDisk,
    H050_FirstTurnAfterPlayerTurnStartFestivePopper,
    H051_AfterObtainedLeesWaffle,
    H052_AfterShuffleTheAbacus,
    H053_EndTurnEmptyHandScreamingFlagon,
    H054_PowerCardPlayedGamePiece,
    H055_FirstTurnBeforeSideTurnStartRedMask,
    H056_AfterSideTurnStartBrimstone,
    H057_FirstTurnBeforeHandDrawNinjaScroll,
    H058_AfterCombatEndChosenCheese,
    H059_VictoryAtHalfHpMeatOnTheBone,
    H060_AfterPotionUsedReptileTrinket,
    H061_FirstTurnAfterPlayerTurnStartRoyalPoison,

    // =========================================================
    // H062-H104 expansion audited against the uploaded target sts2.dll.
    // Duplicate native hook shapes remain independent source fragments.
    // =========================================================
    H062_Every3AttacksThisTurnShuriken,
    H063_Every3AttacksThisTurnKunai,
    H064_FirstTurnAfterSideTurnStartLantern,
    H065_FirstTurnAfterSideTurnStartVeryHotCocoa,
    H066_AfterObtainedStrawberry,
    H067_AfterObtainedPear,
    H068_AfterObtainedMango,
    H069_AfterObtainedLoomingFruit,
    H070_AfterObtainedNutritiousOyster,
    H071_AfterObtainedGoldenPearl,
    H072_AfterObtainedSignetRing,
    H073_PlayCardEnergy3PlusIvoryTile,
    H074_AfterSideTurnStartSai,
    H075_Turn3AfterSideTurnStartChandelier,
    H076_EnterCombatSwordOfJade,
    H077_AttackCardPlayedDaughterOfTheWind,
    H078_PowerCardPlayedLostWisp,
    H079_CardExhaustedCharonsAshes,
    H080_CardExhaustedForgottenSoul,
    H081_Every4CardsPersistentIronClub,
    H082_EnterCombatBronzeScales,
    H083_EnterCombatGorget,
    H084_ShivCardPlayedHelicalDart,
    H085_FirstPowerEachCombatPermafrost,
    H086_AfterObtainedEmptyCage,
    H087_AfterObtainedDollysMirror,
    H088_AfterObtainedPomander,
    H089_AfterObtainedGnarledHammer,
    H090_AfterObtainedKifuda,
    H091_AfterObtainedPunchDagger,
    H092_AfterObtainedTriBoomerang,
    H093_FirstTurnAfterSideTurnStartBigHat,
    H094_FirstTurnAfterSideTurnStartOrangeDough,
    H095_FirstTurnBeforeHandDrawRadiantPearl,
    H096_FirstTurnBeforeSideTurnStartCrackedCore,
    H097_FirstTurnAfterSideTurnStartSymbioticVirus,
    H098_AfterObtainedOrrery,
    H099_AfterObtainedGlassEye,
    H100_AfterObtainedSmallCapsule,
    H101_FirstTurnAfterPlayerTurnStartChoicesParadox,
    H102_AfterObtainedCallingBell,
    H103_AfterObtainedToyBox,
    H104_Every3CombatsUpToFiveToyBox,


    // =========================================================
    // H105-H116 expansion audited against the same v0.111.0 sts2.dll.
    //
    // 这一批补的是此前完全没有被任何 Hook 覆盖的 12 个原生时机，
    // 每条都注明反编译来源（Core\Models\Relics\<Relic>.cs）与
    // AbstractModel 里的虚函数行号，方便以后核对。
    // =========================================================

    // =========================================================
    // H105
    // 来源遗物：Tingsha / Tough Bandages
    //
    // Hook：
    // 战斗中，你自己弃掉一张牌之后
    //
    // 原生：AfterCardDiscarded(PlayerChoiceContext, CardModel)
    // 反编译：AbstractModel.cs:384
    //        Tingsha.cs:18 / ToughBandages.cs:20
    // =========================================================
    H105_AfterCardDiscarded,


    // =========================================================
    // H106
    // 来源遗物：Hand Drill
    //
    // Hook：
    // 你（或你的宠物）击破一个敌人的格挡之后
    //
    // 原生：AfterBlockBroken(PlayerChoiceContext, Creature, Creature?)
    // 反编译：AbstractModel.cs:347 / HandDrill.cs:21
    // =========================================================
    H106_AfterBlockBroken,


    // =========================================================
    // H107
    // 来源遗物：Dragon Fruit
    //
    // Hook：
    // 你自己获得金币之后
    //
    // 原生：AfterGoldGained(Player)
    // 反编译：AbstractModel.cs:767 / DragonFruit.cs:22
    // =========================================================
    H107_AfterGoldGained,


    // =========================================================
    // H108
    // 来源遗物：Galactic Dust
    //
    // Hook：
    // 你每累计消耗 10 点星辰
    //
    // 原生：AfterStarsSpent(int, Player)
    // 反编译：AbstractModel.cs:1175 / GalacticDust.cs:88
    // （余数保留，跨战斗不丢失，与原版计数器一致）
    // =========================================================
    H108_Every10StarsGalacticDust,


    // =========================================================
    // H109
    // 来源遗物：Metronome
    //
    // Hook：
    // 你每生成 7 个充能球
    //
    // 原生：AfterOrbChanneled(PlayerChoiceContext, Player, OrbModel)
    // 反编译：AbstractModel.cs:977 / Metronome.cs:89
    // =========================================================
    H109_Every7OrbsMetronome,


    // =========================================================
    // H110
    // 来源遗物：Unceasing Top
    //
    // Hook：
    // 你的手牌被清空之后
    //
    // 原生：AfterHandEmptied(PlayerChoiceContext, Player)
    // 反编译：AbstractModel.cs:804 / UnceasingTop.cs:16
    // =========================================================
    H110_AfterHandEmptied,


    // =========================================================
    // H111
    // 来源遗物：Bing Bong / Lucky Fysh / Darkstone Periapt
    //
    // Hook：
    // 一张牌进入你的牌组之后（克隆产生的牌不算）
    //
    // 原生：AfterCardChangedPiles(CardModel, PileType, AbstractModel?)
    // 反编译：AbstractModel.cs:359
    //        BingBong.cs:28 / LuckyFysh.cs:24 / DarkstonePeriapt.cs:16
    // =========================================================
    H111_CardEnteredDeck,


    // =========================================================
    // H112
    // 来源遗物：Bone Flute
    //
    // Hook：
    // 你（或你的宠物）发动一次攻击之后
    //
    // 原生：AfterAttack(PlayerChoiceContext, AttackCommand)
    // 反编译：AbstractModel.cs:240 / BoneFlute.cs:19
    // =========================================================
    H112_AfterAttackBoneFlute,


    // =========================================================
    // H113
    // 来源遗物：Maw Bank
    //
    // Hook：
    // 你在商店里花掉金币买下一件东西之后
    //
    // 原生：AfterItemPurchased(Player, MerchantEntry, int)
    // 反编译：AbstractModel.cs:815 / MawBank.cs:52
    // =========================================================
    H113_AfterItemPurchasedMawBank,


    // =========================================================
    // H114
    // 来源遗物：Petrified Toad
    //
    // Hook：
    // 战斗开始的最末阶段（起始手牌抽取之前）
    //
    // 原生：BeforeCombatStartLate()
    // 反编译：AbstractModel.cs:510 / PetrifiedToad.cs:16
    // =========================================================
    H114_BeforeCombatStartLatePetrifiedToad,


    // =========================================================
    // H115
    // 来源遗物：Bookmark
    //
    // Hook：
    // 回合结束、手牌刷新之后
    //
    // 原生：AfterFlush(PlayerChoiceContext, Player,
    //                IReadOnlyCollection<CardModel>,
    //                IReadOnlyCollection<CardModel>)
    // 反编译：AbstractModel.cs:758 / Bookmark.cs:18
    // =========================================================
    H115_AfterFlushBookmark,


    // =========================================================
    // H116
    // 来源遗物：Regal Pillow / Stone Humidifier
    //
    // Hook：
    // 你在营火休息回血之后
    //
    // 原生：AfterRestSiteHeal(Player, bool)
    // 反编译：AbstractModel.cs:1112
    //        RegalPillow.cs:28 / StoneHumidifier.cs:18
    // =========================================================
    H116_AfterRestSiteHealRegalPillow,


    // =========================================================
    // H117
    // 来源遗物：Seal of Gold
    //
    // Hook：
    // 你方回合开始的瞬间，且你当前金币不少于 3（每次都会检查）
    //
    // 原生：AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)
    // 反编译：AbstractModel.cs:1268 / SealOfGold.cs:25
    //
    // 注意：原版没有“只在第一回合”的限制，只要金币够就每回合都触发。
    // =========================================================
    H117_AfterSideTurnStartSealOfGold,


    // =========================================================
    // H118
    // 来源遗物：Power Cell
    //
    // Hook：
    // 第一回合、己方行动阶段开始之前
    //
    // 原生：BeforeSideTurnStart(PlayerChoiceContext, CombatSide,
    //                          IReadOnlyList<Creature>, ICombatState)
    // 反编译：AbstractModel.cs:1247 / PowerCell.cs:21
    // =========================================================
    H118_BeforeSideTurnStartPowerCell,


    // =========================================================
    // H119
    // 来源遗物：Delicate Frond
    //
    // Hook：
    // 战斗开始（起始手牌抽取之前）
    //
    // 原生：BeforeCombatStart()
    // 反编译：AbstractModel.cs:498 / DelicateFrond.cs:12
    // =========================================================
    H119_BeforeCombatStartDelicateFrond,


    // =========================================================
    // H120
    // 来源遗物：Bone Tea
    //
    // Hook：
    // 第一回合、你的回合开始之后
    //
    // 原生：AfterPlayerTurnStart(PlayerChoiceContext, Player)
    // 反编译：AbstractModel.cs:1320 / BoneTea.cs:50
    //
    // 注意：原版 Bone Tea 自己带“只生效一场战斗”的状态；
    // ERROR 的 H 片段是可复用片段，因此这里只保留时机条件。
    // =========================================================
    H120_FirstTurnAfterPlayerTurnStartBoneTea,


    // =========================================================
    // H121
    // 来源遗物：Art of War
    //
    // Hook：
    // 你的能量重置时，若上一回合没有打出过攻击牌，且不是第一回合
    //
    // 原生：AfterEnergyReset(Player)
    //      配合 AfterCardPlayed(记录) 与 AfterSideTurnEnd(结转)
    // 反编译：AbstractModel.cs:689 / ArtOfWar.cs:55,78,89
    // =========================================================
    H121_AfterEnergyResetNoAttackArtOfWar,


    // =========================================================
    // H122
    // 来源遗物：Self-Forming Clay
    //
    // Hook：
    // 你受到未格挡伤害之后
    //
    // 原生：AfterDamageReceived(...)，条件
    //      CombatManager.Instance.IsInProgress
    //      && target == Owner.Creature
    //      && result.UnblockedDamage > 0
    // 反编译：AbstractModel.cs:623 / SelfFormingClay.cs:25
    // =========================================================
    H122_AfterDamageReceivedUnblockedSelfFormingClay,


    // =========================================================
    // H123
    // 来源遗物：Emotion Chip
    //
    // Hook：
    // 你的回合开始，且上一回合失去过生命
    //
    // 原生：AfterDamageReceived 记录“失去过生命”，
    //      AfterPlayerTurnStart 用 CombatManager.History 判定
    //      HappenedLastPlayerTurn 后触发。
    //      反编译：EmotionChip.cs:21,40
    //      AbstractModel.cs:623 / 1320
    // =========================================================
    H123_AfterPlayerTurnStartLostHpEmotionChip,


    // =========================================================
    // H124
    // 来源遗物：Crossbow
    //
    // Hook：
    // 你方回合开始（原版没有回合数限制，每回合都给一张）
    //
    // 原生：AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)
    // 反编译：AbstractModel.cs:1268 / Crossbow.cs:17
    // =========================================================
    H124_AfterSideTurnStartCrossbow,


    // =========================================================
    // H125
    // 来源遗物：War Hammer（Ancient）
    //
    // Hook：战斗胜利，且该战斗是精英战
    // 原生：AfterCombatVictory(CombatRoom)，room.RoomType == RoomType.Elite
    // 反编译：AbstractModel.cs:556 / WarHammer.cs:15
    // =========================================================
    H125_AfterCombatVictoryEliteWarHammer,


    // =========================================================
    // H126
    // 来源遗物：Burning Blood（Starter）
    //
    // Hook：战斗胜利且自己没死
    // 原生：AfterCombatVictory(CombatRoom)，!Owner.Creature.IsDead
    // 反编译：AbstractModel.cs:556 / BurningBlood.cs:13
    // =========================================================
    H126_AfterCombatVictoryBurningBlood,


    // =========================================================
    // H127
    // 来源遗物：Black Blood（Starter）
    //
    // Hook：战斗胜利且自己没死
    // 原生：AfterCombatVictory(CombatRoom)，!Owner.Creature.IsDead
    // 反编译：AbstractModel.cs:556 / BlackBlood.cs:16
    // =========================================================
    H127_AfterCombatVictoryBlackBlood,


    // =========================================================
    // H128
    // 来源遗物：Lunar Pastry（Rare）
    //
    // Hook：己方回合结束
    // 原生：AfterSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)
    //       participants.Contains(Owner.Creature)
    // 反编译：AbstractModel.cs:1406 / LunarPastry.cs:15
    // =========================================================
    H128_AfterSideTurnEndLunarPastry,


    // =========================================================
    // H129
    // 来源遗物：Regalite（Uncommon）
    //
    // Hook：有卡牌为战斗生成，且造物主是自己
    // 原生：AfterCardGeneratedForCombat(CardModel card, Player? creator)
    //       creator == Owner，且每回合只触发一次
    // 反编译：AbstractModel.cs:1207 / Regalite.cs:26
    // =========================================================
    H129_AfterCardGeneratedForCombatRegalite,

    // =========================================================
    // H130
    // 来源遗物：Cloak Clasp（Rare）
    //
    // Hook：己方回合结束时，且手牌不为空
    // 原生：BeforeSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)
    //       participants.Contains(Owner.Creature) && 手牌数 != 0
    // 反编译：AbstractModel.cs:1388 / CloakClasp.cs:22
    // =========================================================
    H130_BeforeSideTurnEndCloakClasp,

    // =========================================================
    // H131
    // 来源遗物：Divine Right（Starter）
    //
    // Hook：进入战斗房间
    // 原生：AfterRoomEntered(AbstractRoom room)，room is CombatRoom
    // 反编译：AbstractModel.cs:1153 / DivineRight.cs:12
    // =========================================================
    H131_EnterCombatDivineRight,

    // =========================================================
    // H132
    // 来源遗物：Ember Tea（Event）
    //
    // Hook：进入战斗房间（原版还有"剩余 5 场战斗"的上限，片段不设上限）
    // 原生：AfterRoomEntered(AbstractRoom room)，room is CombatRoom
    // 反编译：AbstractModel.cs:1153 / EmberTea.cs:58
    // =========================================================
    H132_EnterCombatEmberTea,

    // =========================================================
    // H133
    // 来源遗物：Pael's Tears（Ancient）
    //
    // Hook：己方回合开始时，且上一回合结束时有剩余能量
    // 原生：BeforeSideTurnEnd 记录 `PlayerCombatState.Energy > 0`，
    //       AfterSideTurnStart 时若该记录为真则触发
    // 反编译：AbstractModel.cs:1388 / :1268 / PaelsTears.cs:39
    // =========================================================
    H133_AfterSideTurnStartLeftoverEnergyPaelsTears,

    // =========================================================
    // H134
    // 来源遗物：Infused Core（Starter）
    //
    // Hook：己方第一回合开始（TurnNumber <= 1）
    // 原生：AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)
    //       participants.Contains(Owner.Creature) && TurnNumber <= 1
    // 反编译：AbstractModel.cs:1268 / InfusedCore.cs:22
    // =========================================================
    H134_FirstTurnAfterSideTurnStartInfusedCore,

    // =========================================================
    // H135
    // 来源遗物：Bread（Shop）
    //
    // Hook：己方第一回合开始（TurnNumber == 1）
    // 原生：AfterSideTurnStart，participants.Contains(Owner.Creature)
    //       && Owner.PlayerCombatState.TurnNumber == 1
    // 反编译：AbstractModel.cs:1268 / Bread.cs:41
    // =========================================================
    H135_FirstTurnAfterSideTurnStartBread,

    // =========================================================
    // H136
    // 来源遗物：Booming Conch（Ancient）
    //
    // Hook：精英战的第一回合开始
    // 原生：AfterSideTurnStart，participants.Contains(Owner.Creature)
    //       && TurnNumber <= 1 && Owner.RunState.CurrentRoom?.RoomType == Elite
    // 反编译：AbstractModel.cs:1268 / BoomingConch.cs:44
    // =========================================================
    H136_EliteFirstTurnAfterSideTurnStartBoomingConch,

    // =========================================================
    // H137
    // 来源遗物：Blessed Antler（Ancient）
    //
    // Hook：第一回合抽牌前
    // 原生：BeforeHandDraw(Player, PlayerChoiceContext, ICombatState)
    //       player == Owner && TurnNumber == 1
    // 反编译：AbstractModel.cs:779 / BlessedAntler.cs:48
    // =========================================================
    H137_FirstTurnBeforeHandDrawBlessedAntler,

    // =========================================================
    // H138
    // 来源：无原版遗物使用该 Hook（原创组合时机）
    // Hook：领取奖励时
    // 原生：AfterRewardTaken(Player, Reward)
    // 反编译：AbstractModel.cs:1131 / Hook.cs:1104
    // =========================================================
    H138_AfterRewardTaken,

    // =========================================================
    // H139
    // 来源：无原版遗物使用该 Hook（原创组合时机）
    // Hook：在营火锻打（升级卡牌）时
    // 原生：AfterRestSiteSmith(Player)
    // 反编译：AbstractModel.cs:1121 / Hook.cs:1092
    // =========================================================
    H139_AfterRestSiteSmith,

    // =========================================================
    // H140
    // 来源：无原版遗物使用该 Hook（原创组合时机）
    // Hook：进入新的一幕时
    // 原生：AfterActEntered()
    // 反编译：AbstractModel.cs:208 / Hook.cs:68
    // =========================================================
    H140_AfterActEntered,

    // =========================================================
    // H141
    // 来源：无原版遗物使用该 Hook（原创组合时机）
    // Hook：获得格挡时
    // 原生：AfterBlockGained(Creature, decimal, ValueProp, CardModel?)
    // 反编译：AbstractModel.cs:330 / Hook.cs:143
    // =========================================================
    H141_AfterBlockGained,

    // =========================================================
    // H142
    // 来源：无原版遗物使用该 Hook（原创组合时机）
    // Hook：造成伤害时
    // 原生：AfterDamageGiven(PlayerChoiceContext, Creature?, DamageResult,
    //                       ValueProp, Creature, CardModel?)
    // 反编译：AbstractModel.cs:593 / Hook.cs:401
    // =========================================================
    H142_AfterDamageGiven,

    // =========================================================
    // H143
    // 来源遗物：Belt Buckle
    // Hook：获得药水时
    // 原生：AfterPotionProcured(PotionModel)
    // 反编译：AbstractModel.cs:1039 / Hook.cs:984
    // =========================================================
    H143_AfterPotionProcured,

    // =========================================================
    // H144 —— 已取消
    // 来源遗物：Ghost Seed（AfterCardEnteredCombat）
    // 取消原因（用户决定，2026-10-30）：这个时机"摸不着头脑"，
    // 触发点落在战斗初始化期间，玩家看不懂什么时候该触发，
    // 也容易引出 E109 这类选择器未就绪的崩溃。
    //
    // 保留枚举位而不删除，是为了不让 H145–H150 的底层数值整体前移：
    // 存档里的遗物是按 int 保存 HookId 的，前移会让老存档的遗物
    // 悄悄变成另一个时机。现在的行为是"永远不触发、不参与生成"。
    // =========================================================
    H144_ReservedCardEnteredCombat,

    // =========================================================
    // H145
    // 来源遗物：Philosophers Stone / Fur Coat
    // Hook：一个生物加入战斗时
    // 原生：AfterCreatureAddedToCombat(Creature)
    // 反编译：AbstractModel.cs:565 / Hook.cs:374
    // =========================================================
    H145_AfterCreatureAddedToCombat,

    // =========================================================
    // H146
    // 来源遗物：Lizard Tail
    // Hook：一次死亡被阻止时
    // 原生：AfterPreventingDeath(Creature)
    // 反编译：AbstractModel.cs:1097 / Hook.cs:1056
    // =========================================================
    H146_AfterPreventingDeath,

    // =========================================================
    // H147
    // 来源遗物：Pael's Eye
    // Hook：获得额外回合时
    // 原生：AfterTakingExtraTurn(Player)
    // 反编译：AbstractModel.cs:1220 / Hook.cs:1232
    // =========================================================
    H147_AfterTakingExtraTurn,
    // =========================================================
    // H148
    // 来源遗物：Belt Buckle
    // Hook：丢弃药水时
    // 原生：AfterPotionDiscarded(PotionModel)
    // 反编译：AbstractModel.cs:1029 / Hook.cs:972
    // =========================================================
    H148_AfterPotionDiscarded,

    // =========================================================
    // H149
    // 来源遗物：Lasting Candy
    // Hook：战斗奖励生成之前
    // 原生：BeforeCombatRewardOffered(RewardsSet, CombatRoom)
    // 反编译：AbstractModel.cs:532 / Hook.cs:340
    // =========================================================
    H149_BeforeCombatRewardOffered,

    // =========================================================
    // H150
    // 来源遗物：Meat on the Bone / Red Skull
    // Hook：当前生命值变化时
    // 原生：AfterCurrentHpChanged(Creature, decimal)
    // 反编译：AbstractModel.cs:578 / Hook.cs:386
    // =========================================================
    H150_AfterCurrentHpChanged
}
