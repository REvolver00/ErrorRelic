# 原版遗物与 ERROR 片段覆盖清单

核对日期：2026-09-28。对象：本机《杀戮尖塔 2》v0.111.0 与当前本地 ERROR 源码。

## 如何理解本清单

- Hook（H）指“什么时候触发”；Effect（E）指“触发后做什么”。
- 按枚举注释中的来源遗物统计：61 个 H、65 个 E，共涉及 62 件来源遗物；其中 60 件同时有 H/E 来源编号，骇人头盔只有 H003，剃刀牙只有 E002。
- 原版程序集内有 288 个参考遗物类（排除 Fake*、DeprecatedRelic 和非遗物辅助类）。其中 226 件未单列为片段来源；这不是 226 套独有能力缺失，也不表示这 288 件目前都能正常掉落。
- 来源编号缺失不等于能力缺失。例如骇人头盔的“获得 4 点格挡”已有 E038/E046；手里剑和苦无的“每回合每打出 3 张攻击牌”已有 H035/H036，力量与敏捷已有 E001/E026；草莓增加 7 点生命上限已有 E053。
- E002（剃刀牙：升级打出的牌）与 E007（工具箱：三选一无色牌）有代码，但被排除在随机池之外；不能归为完全未写。
- 大蘑菇 H032/E033 已实现拾取时增加 20 点生命上限，原版“每场战斗开始少抽 2 张牌”未成为片段。
- 摆动球原版通过修改抽牌数量实现；当前 H023/E024 已转成每 3 回合抽 1 张，不能仅凭未使用同一个原版方法就判为效果未实现。
- 下表分类依据原版类的方法签名初筛，不是逐件完整行为审计；部分事件方法仅用于动画或计数。表中的方法名供开发定位，不能直接当作适合新增 H 的结论。
- 原版描述去除了颜色标签；{…} 是游戏运行时填入数值或图标的占位符，本清单未将其猜成具体数字。

## 已有来源编号

| 原版名称 | 类名 | H | E |
| --- | --- | --- | --- |
| 赤牛 | Akabeko | H041 | E043 |
| 锚 | Anchor | H048 | E050 |
| 星盘 | Astrolabe | H008 | E008 |
| 弹珠袋 | BagOfMarbles | H039 | E041 |
| 大蘑菇 | BigMushroom | H032 | E033 |
| 小血瓶 | BloodVial | H047 | E049 |
| 硫磺 | Brimstone | H056 | E059、E060 |
| 烛台 | Candelabra | H028 | E029 |
| 舵盘 | CaptainsWheel | H027 | E028 |
| 百年积木 | CentennialPuzzle | H042 | E044 |
| 天选芝士 | ChosenCheese | H058 | E062 |
| 数据磁盘 | DataDisk | H049 | E051 |
| 卓越斗篷 | DistinguishedCape | H006 | E006、E010 |
| 节日拉炮 | FestivePopper | H050 | E052 |
| 棋子 | GamePiece | H054 | E057 |
| 地精之角 | GremlinHorn | H033 | E034、E035 |
| 开心小花 | HappyFlower | H021 | E022 |
| 船夹板 | HornCleat | H026 | E027 |
| 骇人头盔 | IntimidatingHelmet | H003 |  |
| 珠宝盒 | JewelryBox | H019 | E020 |
| 金纸 | JossPaper | H030 | E031 |
| 锁镰 | Kusarigama | H035 | E037 |
| 李家华夫饼 | LeesWaffle | H051 | E053、E054 |
| 开信刀 | LetterOpener | H034 | E036 |
| 餐券 | MealTicket | H029 | E030 |
| 带骨肉 | MeatOnTheBone | H059 | E063 |
| 水银沙漏 | MercuryHourglass | H002 | E003 |
| 干瘪之手 | MummifiedHand | H005 | E005 |
| 涅奥的护符 | NeowsTalisman | H015 | E016 |
| 涅奥的苦痛 | NeowsTorment | H017 | E018 |
| 忍术卷轴 | NinjaScroll | H057 | E061 |
| 双截棍 | Nunchaku | H037 | E039 |
| 营养汤 | NutritiousSoup | H010 | E011 |
| 意外光滑的石头 | OddlySmoothStone | H025 | E026 |
| 古钱币 | OldCoin | H004 | E004 |
| 奥利哈钢 | Orichalcum | H043 | E045 |
| 精致折扇 | OrnamentalFan | H036 | E038 |
| 佩尔之爪 | PaelsClaw | H011 | E012 |
| 佩尔之角 | PaelsHorn | H016 | E017 |
| 缩放仪 | Pantograph | H046 | E048 |
| 招架盾 | ParryingShield | H045 | E047 |
| 摆动球 | Pendulum | H023 | E024 |
| 活动星图 | Planisphere | H031 | E032 |
| 剃刀牙 | RazorTooth |  | E002 |
| 红面具 | RedMask | H055 | E058 |
| 爬行动物饰品 | ReptileTrinket | H060 | E064 |
| 波纹水盆 | RippleBasin | H044 | E046 |
| 王室猛毒 | RoyalPoison | H061 | E065 |
| 王室印章 | RoyalStamp | H009 | E009 |
| 沙堡 | SandCastle | H012 | E013 |
| 尖叫酒壶 | ScreamingFlagon | H053 | E056 |
| 闪亮口红 | SparklingRouge | H024 | E025 |
| 历石 | StoneCalendar | H022 | E023 |
| 故事书 | Storybook | H018 | E019 |
| 坦克斯的哨子 | TanxsWhistle | H020 | E021 |
| 算盘 | TheAbacus | H052 | E055 |
| 工具箱 | Toolbox | H007 | E007 |
| 音叉 | TuningFork | H038 | E040 |
| 扭曲漏斗 | TwistedFunnel | H040 | E042 |
| 金刚杵 | Vajra | H001 | E001 |
| 战纹涂料 | WarPaint | H013 | E014 |
| 磨刀石 | Whetstone | H014 | E015 |

## 未单列来源：事件型候选（140 件）

| 原版名称 | 类名 | 原版描述 | 原版相关方法 |
| --- | --- | --- | --- |
| 炼金箱 | AlchemicalCoffer | 拾起时，获得{PotionSlots}个放有随机药水的药水栏位。 | AfterObtained |
| 奥术卷轴 | ArcaneScroll | 拾起时，将一张随机稀有牌加入你的牌组。 | AfterObtained |
| 古老牙齿 | ArchaicTooth | 拾起时，将{StarterCard.StringValue:cond:{StarterCard}变化为{AncientCard}／一张初始卡牌变化为先古版本}。 | AfterCloned, AfterObtained |
| 孙子兵法 | ArtOfWar | 如果你在本回合中没有打出过攻击牌，则在下一回合额外获得1点{Energy:energyIcons()}。 | AfterCardPlayed, AfterSideTurnEnd, AfterEnergyReset, AfterCombatEnd |
| 华美手镯 | BeautifulBracelet | 拾起时，为你牌组中的随机{Cards}张牌附魔：迅捷2。 | AfterObtained |
| 风箱 | Bellows | 你在每场战斗开始时的手牌，将被升级。 | AfterPlayerTurnStart |
| 腰带扣 | BeltBuckle | 当你没有药水时，你额外拥有{DexterityPower}点敏捷。 | AfterObtained, BeforeCombatStart, AfterCombatEnd, AfterPotionProcured, AfterPotionDiscarded, AfterPotionUsed, AfterCombatVictory |
| 大帽子 | BigHat | 在每场战斗开始时，将{Cards}张随机虚无牌加入你的手牌。 | AfterSideTurnStart |
| 大～抱抱 | BiiigHug | 拾起时，从你的牌组中移除{Cards}张牌。每当你的抽牌堆打乱洗牌时，将一张煤灰加入你的抽牌堆。 | AfterObtained, AfterShuffle |
| 宾邦 | BingBong | 每当你往牌组中增添卡牌时，都将额外添加一张相同的牌。 | AfterCardChangedPiles |
| 黑暗之血 | BlackBlood | 在战斗结束时，回复{Heal}点生命。 | AfterCombatVictory |
| 骨笛 | BoneFlute | 每当奥斯提攻击时，获得{Block}点格挡。 | AfterAttack |
| 骨茶 | BoneTea | 在你接下来的{Combats}场战斗开始时，升级你的初始手牌。 | AfterPlayerTurnStart |
| 书签 | Bookmark | 每回合结束时，随机一张被保留的牌在被打出前的耗能减少1。 | AfterFlush |
| 五轮书 | BookOfFiveRings | 你每将{Cards}张牌加入你的牌组时，回复{Heal}点生命。 | AfterCardChangedPiles |
| 修书小刀 | BookRepairKnife | 每当有一名不是“爪牙”的敌人死于灾厄时，回复{Heal}点生命。 | AfterDiedToDoom |
| 缚魂命匣 | BoundPhylactery | 在你的回合开始时，召唤{Summon} | BeforeCombatStart, AfterEnergyResetLate |
| 铜质鳞片 | BronzeScales | 在每场战斗开始时，获得{ThornsPower}点荆棘。 | AfterRoomEntered |
| 燃烧之血 | BurningBlood | 在战斗结束时，回复{Heal}点生命。 | AfterCombatVictory |
| 燃烧木棍 | BurningSticks | 每场战斗中你第一次消耗技能牌时，将那张牌的复制品加入你的手牌 | AfterRoomEntered, AfterCardExhausted, AfterCombatEnd |
| 异鸟宝宝 | Byrdpip | 拾起时，获得一张异鸟扑击。一只幼年异鸟会在战斗中陪伴你。 | AfterObtained, BeforeCombatStart |
| 召唤铃铛 | CallingBell | 拾起时，获得一个独特的诅咒和{Relics}件遗物。 | AfterObtained |
| 大锅 | Cauldron | 拾起时，制作{Potions}瓶随机药水。 | AfterObtained |
| 吊灯 | Chandelier | 在你的第三回合开始时，获得{Energy:energyIcons()}。 | AfterSideTurnStart |
| 卡戎之灰 | CharonsAshes | 每当你消耗一张牌，对所有敌人造成{Damage}点伤害。 | AfterCardExhausted |
| 选择悖论 | ChoicesParadox | 在每场战斗开始时，从{Cards}张随机牌中选择1张放入你的手牌。被选中的牌获得保留。 | AfterPlayerTurnStart |
| 利爪 | Claws | 拾起时，将至多{Cards}张牌变化为撕咬。 | AfterObtained |
| 斗篷扣 | CloakClasp | 在你的回合结束时，每有一张手牌，就获得{Block}点格挡 | BeforeSideTurnEnd |
| 破损核心 | CrackedCore | 在每场战斗开始时，生成{Lightning}个闪电充能球。 | BeforeSideTurnStart |
| 十字弓 | Crossbow | 在你的回合开始时，将一张随机攻击牌加入你的手牌。这张牌在本回合可以免费打出。 | AfterSideTurnStart |
| 诅咒珍珠 | CursedPearl | 拾起时，获得一张贪婪，获得{Gold}金币。 | AfterObtained |
| 黑石护符 | DarkstonePeriapt | 每当你获得一张诅咒，就将你的最大生命值提升{MaxHp}。 | AfterCardChangedPiles |
| 风的女儿 | DaughterOfTheWind | 每当你打出一张攻击牌时，获得{Block}点格挡。 | AfterCardPlayed |
| 娇嫩蕨草 | DelicateFrond | 在每场战斗开始时，用随机药水将你的空药水栏位填满。 | BeforeCombatStart |
| 恶魔之舌 | DemonTongue | 每回合第一次在回合内失去生命值时，回复等量的生命值。 | AfterDamageReceived, BeforeSideTurnStart |
| 钻石头冠 | DiamondDiadem | 战斗开始时获得{Block}点格挡。你的格挡不会在你的第2回合开始前被移除。 | AfterSideTurnStart |
| 天命所归 | DivineDestiny | 在每场战斗开始时，获得{Stars:starIcons()}。 | AfterSideTurnStart |
| 天赋君权 | DivineRight | 在每场战斗开始时，获得{Stars:starIcons()}。 | AfterRoomEntered |
| 多利之镜 | DollysMirror | 拾起时，从你的牌组中选择一张牌进行复制。 | AfterObtained |
| 寻龙尺 | DowsingRod | 拾起时，将1张探寻加入你的牌组。 | AfterObtained |
| 火龙果 | DragonFruit | 每当你获得金币时，提升{MaxHp}点你的最大生命值。 | AfterGoldGained |
| 尘封魔典 | DustyTome | 拾起时，获得一张{AncientCard.StringValue:cond:{}+／先古牌}。 | AfterObtained |
| 放电异虾 | ElectricShrymp | 拾起时，选择一张技能牌为它附魔：注能。 | AfterObtained |
| 余烬茶 | EmberTea | 在接下来的{Combats}场战斗开始时，获得{StrengthPower}点力量。 | AfterRoomEntered |
| 情感芯片 | EmotionChip | 在每回合开始时，如果你在之前回合受到过伤害，则触发所有充能球的被动效果。 | AfterDamageReceived, AfterPlayerTurnStart, AfterCombatEnd |
| 空鸟笼 | EmptyCage | 拾起时，选择移除牌组中的{Cards}张牌。 | AfterObtained |
| 永恒羽毛 | EternalFeather | 你的牌组中每有{Cards}张牌，当你进入休息处时就会回复{Heal}点生命。 | AfterRoomEntered |
| 击剑指南 | FencingManual | 在每场战斗开始时，铸造{Forge}。 | AfterSideTurnStart |
| 钓鱼竿 | FishingRod | 每{Combats}场普通战斗，随机升级你牌组中的一张牌。 | AfterCombatEnd |
| 遗忘之魂 | ForgottenSoul | 每当你消耗一张牌，随机对一名敌人造成{Damage}点伤害。 | AfterCardExhausted |
| 芳香蘑菇 | FragrantMushroom | 拾起时，失去{HpLoss}点生命，然后随机升级{Cards}张牌。 | AfterObtained |
| 葬礼面具 | FuneraryMask | 在每场战斗开始时，将{Cards}张灵魂加入你的抽牌堆。 | BeforeHandDraw |
| 星系尘埃 | GalacticDust | 每消耗{Stars}点{singleStarIcon}，就获得{Block}格挡。 | AfterStarsSpent |
| 赌博筹码 | GamblingChip | 在每场战斗开始时，丢弃任意张牌，然后抽相同数量张牌。 | AfterPlayerTurnStart |
| 幽灵种子 | GhostSeed | 打击和防御获得虚无。 | AfterCardEnteredCombat, AfterRoomEntered |
| 玻璃眼珠 | GlassEye | 拾起时，获得2组普通、2组罕见、和1组稀有卡牌奖励。 | AfterObtained |
| 扭曲锤子 | GnarledHammer | 拾起时，从你的牌组中选择至多{Cards}张攻击牌，附魔：锋利{SharpAmount:diff()}。 | AfterObtained |
| 金色珍珠 | GoldenPearl | 拾起时，获得{Gold}金币。 | AfterObtained |
| 护喉甲 | Gorget | 在每场战斗开始时，获得{PlatingPower}层覆甲。 | AfterRoomEntered |
| 手钻 | HandDrill | 每当你突破敌人的格挡时，给予其{VulnerablePower}层易伤。 | AfterBlockBroken |
| 沉重石板 | HeftyTablet | 拾起时，从{Cards}张稀有牌中选择1张加入你的牌组，同时将1张受伤加入你的牌组。 | AfterObtained |
| 螺线飞镖 | HelicalDart | 你每打出一张小刀，就在本回合获得{DexterityPower}点敏捷。 | AfterCardPlayed |
| 历史课 | HistoryCourse | 在你的回合开始时，打出一张你上一回合最后打出的攻击牌的复制品。 | AfterAutoPrePlayPhaseEntered |
| 铁棒 | IronClub | 你每打出{Cards}张牌，就抽1张牌。 | AfterCardPlayed |
| 象牙麻将牌 | IvoryTile | 每当你打出一张耗能大于等于{EnergyThreshold:energyIcons()}的牌时，获得{Energy:energyIcons()}。 | AfterCardPlayed |
| 宝石面具 | JeweledMask | 在每场战斗开始时，将一张随机能力牌从你的抽牌堆放入你的手牌，这张牌在本场战斗可以免费打出。 | BeforeHandDraw |
| 万花筒 | Kaleidoscope | 拾起时，获得{Cards}次来自其他角色的卡牌奖励。 | AfterObtained |
| 木札 | Kifuda | 拾起时，从你的牌组中选择至多{Cards}张牌，附魔：伶俐。 | AfterObtained |
| 苦无 | Kunai | 你每在同一回合内打出{Cards}张攻击牌，就获得{DexterityPower}点敏捷。 | BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd |
| 灯笼 | Lantern | 在每场战斗的第一回合获得{Energy:energyIcons()}。 | AfterSideTurnStart |
| 巨大扭蛋 | LargeCapsule | 拾起时，获得{Relics}件随机遗物。额外将一对打击和防御，加入你的牌组。 | AfterObtained |
| 铅制镇纸 | LeadPaperweight | 拾起时，从2张无色牌中选择1张加入你的牌组。 | AfterObtained |
| 树叶药膏 | LeafyPoultice | 拾起时，变化你的1张打击和1张防御，然后失去{MaxHp}点最大生命。 | AfterObtained |
| 布质果实 | LoomingFruit | 拾起时，将你的最大生命值提升{MaxHp}。 | AfterObtained |
| 领主阳伞 | LordsParasol | 当你遇见商人时，立刻获得他所出售的所有物品。 | AfterRoomEntered |
| 失物盒 | LostCoffer | 拾起时，获得1次卡牌奖励和1瓶随机药水。 | AfterObtained |
| 迷失鬼火 | LostWisp | 你每打出一张能力牌，就对所有敌人造成{Damage}点伤害。 | AfterCardPlayed |
| 招财异鱼 | LuckyFysh | 每当你将一张卡牌加入你的牌组时，获得{Gold}金币。 | AfterCardChangedPiles |
| 月亮糕点 | LunarPastry | 在你的回合结束时，获得{Stars:starIcons()}。 | AfterSideTurnEnd |
| 芒果 | Mango | 拾起时，将你的最大生命值提升{MaxHp}。 | AfterObtained |
| 巨大卷轴 | MassiveScroll | 拾起时，从3张多人游戏牌中选择1张加入你的牌组。 | AfterObtained |
| 巨口储蓄罐 | MawBank | 每攀爬一层楼层，就获得{Gold}金币。一旦在商店中花费金币就会使其失效。 | AfterRoomEntered, AfterItemPurchased |
| 节拍器 | Metronome | 每场战斗中你首次生成{OrbCount}个充能球时，对所有敌人造成{Damage}点伤害。 | AfterRoomEntered, AfterOrbChanneled, AfterCombatEnd |
| 迷你储君 | MiniRegent | 每回合你第一次花费{singleStarIcon}时，获得{StrengthPower}点力量。 | AfterStarsSpent, BeforeSideTurnStart, AfterCombatEnd |
| 抱抱先生 | MrStruggles | 在你的回合开始时，对所有敌人造成等量于当前回合数的伤害。 | AfterPlayerTurnStart |
| 音乐盒 | MusicBox | 将你每回合打出的第一张攻击牌的一张虚无复制品加入你的手牌。 | BeforeCardPlayed, AfterCardPlayed, BeforeSideTurnStart, AfterCombatEnd |
| 涅奥骨骰 | NeowsBones | 拾起时，获得{Relics}件随机涅奥{Relics:plural:遗物／遗物}。将{Curses}张随机{Curses:plural:诅咒／诅咒}加入你的牌组。 | AfterObtained |
| 涅奥的牺牲 | NeowsSacrifice | 拾起时，获得1瓶龙涎香，并将1张愧疚加入你的牌组。 | AfterObtained |
| 新叶 | NewLeaf | 拾起时，变化{Cards}张牌。 | AfterObtained |
| 营养牡蛎 | NutritiousOyster | 拾起时，将你的最大生命值提升{MaxHp}。 | AfterObtained |
| 橙色团块 | OrangeDough | 在每场战斗开始时，将{Cards}张随机无色牌加入你的手牌。 | AfterSideTurnStart |
| 星系仪 | Orrery | 拾起时，获得{Cards}次卡牌奖励 | AfterObtained |
| 佩尔之泪 | PaelsTears | 如果你在拥有未花费的{energyPrefix:energyIcons(1)}情况下结束回合，则下个回合额外获得{Energy:energyIcons()}。 | BeforeSideTurnEnd, AfterSideTurnStart, AfterCombatEnd |
| 佩尔之牙 | PaelsTooth | 拾起时，从你的牌组中选择{Cards}张牌移除。在每场战斗结束时，将其中随机1张牌升级然后返还。{CardTitles.StringValue:cond: 被吞噬的牌： {}／} | AfterCloned, AfterObtained, AfterCombatEnd |
| 潘多拉魔盒 | PandorasBox | 变化所有“打击”和“防御”。 | AfterObtained |
| 梨子 | Pear | 拾起时，将你的最大生命值提升{MaxHp}。 | AfterObtained |
| 永冻冰晶 | Permafrost | 当你在战斗中第一次打出能力牌时，获得{Block}点格挡。 | AfterRoomEntered, AfterCardPlayed |
| 石化蟾蜍 | PetrifiedToad | 在每场战斗开始时，获得一瓶药水形状的石头 | BeforeCombatStartLate |
| 药瓶皮套 | PhialHolster | 拾起时，获得{PotionSlots}个药水栏位并获得{Potions}瓶随机药水。 | AfterObtained |
| 无界命匣 | PhylacteryUnbound | 在每场战斗开始时，召唤{StartOfCombat}。在你的回合开始时，召唤{StartOfTurn}。 | BeforeCombatStart, AfterSideTurnStart |
| 橙型香盒 | Pomander | 拾起时，升级一张牌。 | AfterObtained |
| 药水腰带 | PotionBelt | 拾起时，获得{PotionSlots}个药水栏位。 | AfterObtained |
| 能量电池 | PowerCell | 在每场战斗开始时，将{Cards}张耗能为0的卡牌从你的抽牌堆放入你的手牌。 | BeforeSideTurnStart |
| 松动羊毛剪 | PrecariousShears | 拾起时，从你的牌组中移除{Cards}张牌并失去{Damage}点生命。 | AfterObtained |
| 精准剪刀 | PreciseScissors | 拾起时，从你的牌组中移除{Cards}张牌。 | AfterObtained |
| 腌制活雾 | PreservedFog | 拾起时，从你的牌组中移除{Cards}张牌。将一张愚行加入你的牌组。 | AfterObtained |
| 拳刃 | PunchDagger | 拾起时，选择一张攻击牌为它附魔：动量{Momentum}。 | AfterObtained |
| 发光珍珠 | RadiantPearl | 在每场战斗开始时，将{Cards}张冷光加入你的手牌。 | BeforeHandDraw |
| 彩虹戒指 | RainbowRing | 每回合，你第一次打出攻击牌、技能牌和能力牌各一张时，获得{StrengthPower}点力量和{DexterityPower}点敏捷。 | BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd |
| 红头骨 | RedSkull | 当你的生命值低于或等于{HpThreshold}%时，你额外获得{StrengthPower}点力量 | AfterRoomEntered, AfterCombatEnd, AfterCurrentHpChanged |
| 君王矿石 | Regalite | 每回合你第一次生成一张牌时，获得{Block}点格挡。 | AfterCardGeneratedForCombat, BeforeSideTurnStart, AfterCombatEnd |
| 符文电容器 | RunicCapacitor | 每场战斗开始时，获得{Repeat}个额外充能球栏位。 | AfterSideTurnStart |
| 钗 | Sai | 在你的回合开始时，获得{Block}点格挡。 | AfterSideTurnStart |
| 卷轴箱 | ScrollBoxes | 拾起时，从2个卡牌包中选择1包加入你的牌组。 | AfterObtained |
| 海玻璃 | SeaGlass | 查看{Cards}张来自{Character.StringValue:cond:{}／其他角色}的牌。从中选择任意数量的卡牌加入你的牌组。 | AfterObtained |
| 黄金印 | SealOfGold | 在你的回合开始时，花费{Gold}金币来获得{Energy:energyIcons()}。 | AfterSideTurnStart |
| 自成型黏土 | SelfFormingClay | 每当你在战斗中失去生命，就在下回合获得{BlockNextTurn}点格挡。 | AfterDamageReceived |
| 原初之爪 | SereTalon | 拾起时，失去9点最大生命值，将{Wishes}张许愿加入你的牌组。 | AfterObtained |
| 手里剑 | Shuriken | 你每在同一回合内打出{Cards}张攻击牌，获得{StrengthPower}点力量。 | BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd |
| 图章戒指 | SignetRing | 拾起时，获得{Gold}金币。 | AfterObtained |
| 勇气投石索 | SlingOfCourage | 在与精英敌人战斗时，获得{StrengthPower}点力量。 | AfterRoomEntered |
| 小型扭蛋 | SmallCapsule | 拾起时，获得一件随机遗物。 | AfterObtained |
| 碎石钻 | StoneCracker | 在每场战斗开始时，在本场战斗中随机升级你抽牌堆中的{Cards}张牌。 | AfterRoomEntered |
| 草莓 | Strawberry | 拾起时，将你的最大生命值提升{MaxHp}。 | AfterObtained |
| 玉之剑 | SwordOfJade | 在每场战斗开始时，获得{StrengthPower}点力量。 | AfterRoomEntered |
| 石之剑 | SwordOfStone | 在击败{Elites}名精英敌人之后将变化为一件强力遗物。 | AfterCombatVictory |
| 共生病毒 | SymbioticVirus | 在每场战斗开始时，生成{Dark}个黑暗充能球。 | AfterSideTurnStart |
| 无礼之茶 | TeaOfDiscourtesy | 在下一场战斗开始时，将{DazedCount}张晕眩放入你的抽牌堆。 | BeforeCombatStart |
| 铜钹 | Tingsha | 你每在你的回合丢弃一张牌，就对一名随机敌人造成{Damage}点伤害。 | AfterCardDiscarded |
| 烘焙手套 | ToastyMittens | 在你的回合开始时，消耗你手牌中的1张牌并获得{StrengthPower}点力量。 | AfterPlayerTurnStart |
| 欧洛巴斯之触 | TouchOfOrobas | 拾起时，将{StarterRelic.StringValue:cond:{StarterRelic}替换为{UpgradedRelic}／你的初始遗物替换为先古版本}。 | AfterCloned, AfterObtained |
| 结实绷带 | ToughBandages | 你每在你的回合丢弃一张牌，就获得{Block}点格挡。 | AfterCardDiscarded |
| 玩具盒 | ToyBox | 拾起时，获得{Relics}件蜡制遗物。每经过{Combats}场战斗，你最左侧的蜡制遗物将会融化。 | AfterObtained, AfterCombatEnd |
| 三刃回旋镖 | TriBoomerang | 从你的牌组中选择{Cards}张攻击牌。为这些牌附魔：本能。 | AfterObtained |
| 不休陀螺 | UnceasingTop | 在你的回合，当你没有手牌时，抽一张牌。 | AfterHandEmptied |
| 古茶具套装 | VenerableTeaSet | 到达休息处后的下一场战斗开始时额外获得{Energy:energyIcons()}。 | AfterRoomEntered, AfterEnergyReset |
| 烫嘴可可 | VeryHotCocoa | 在每场战斗的第一回合额外获得{Energy:energyIcons()}。 | AfterSideTurnStart |
| 烦人机关盒 | VexingPuzzlebox | 在每场战斗开始时，将一张随机卡牌加入你的手牌。这张牌在本回合可以免费打出。 | AfterPlayerTurnStart |
| 战锤 | WarHammer | 每当你击败一名精英敌人的时候，随机升级{Cards}张牌。 | AfterCombatVictory |
| 美味饼干 | YummyCookie | 拾起时，升级{Cards}张牌。 | AfterCloned, AfterObtained |

## 未单列来源：含事件与规则修正（45 件）

| 原版名称 | 类名 | 原版描述 | 原版相关方法 |
| --- | --- | --- | --- |
| 紫水晶茄子 | AmethystAubergine | 敌人额外掉落{Gold}金币。 | AfterModifyingRewards; TryModifyRewards |
| 律动残余 | BeatingRemnant | 你在一回合内失去的生命值不会超过20点。 | AfterModifyingHpLostAfterOsty, AfterDamageReceived, BeforeSideTurnStart; ModifyHpLostAfterOsty |
| 赐福鹿角 | BlessedAntler | 在每回合开始时获得{Energy:energyIcons()}。在战斗开始时，将{Cards}张晕眩放入你的抽牌堆。 | BeforeHandDraw; ModifyMaxEnergy |
| 血染玫瑰 | BloodSoakedRose | 拾起时，将1张执迷加入你的牌组。在回合开始时获得{Energy:energyIcons()}。 | AfterObtained; ModifyMaxEnergy |
| 轰鸣海螺 | BoomingConch | 在精英战的战斗开始时，额外抽{Cards}张牌并获得{Energy:energyIcons()}。 | AfterSideTurnStart; ModifyHandDraw |
| 圆顶礼帽 | BowlerHat | 额外获得{GoldIncrease:percentMore()}%的金币。 | AfterModifyingGoldGained; ModifyGoldGained |
| 面包 | Bread | 在你的第一个回合开始时，失去{LoseEnergy:energyIcons()}。在其余的回合开始时，获得{GainEnergy:energyIcons()}。 | AfterSideTurnStart; ModifyMaxEnergy |
| 艳丽围巾 | BrilliantScarf | 你每回合从你的手牌打出的第5张牌可以被免费打出。 | BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd; TryModifyEnergyCostInCombatLate, TryModifyStarCost |
| 化学物X | ChemicalX | 耗能为X的牌的效果数值增加{Increase}点。 | BeforeCardPlayed; ModifyXValue |
| 灵体外质 | Ectoplasm | 你不能再获得任何金币。在回合开始时获得{Energy:energyIcons()} | AfterModifyingGoldGained; ModifyGoldGained, ModifyMaxEnergy |
| 小提琴 | Fiddle | 在每个回合开始时，额外抽{Cards}张牌。你在回合进行中不再能抽任何牌。 | AfterPreventingDraw; ModifyHandDraw, ShouldDraw |
| 皮草大衣 | FurCoat | 拾起时，随机标记{Combats}处战斗。这些战斗中的敌人将只有1点生命。 | AfterObtained, BeforeCombatStart, AfterCreatureAddedToCombat; ModifyGeneratedMapLate |
| 壶铃 | Girya | 你现在能在休息处获得力量。（最多3次） | AfterRoomEntered; TryModifyRestSiteOptions |
| 黄金罗盘 | GoldenCompass | 拾起时，将第2阶段的地图替换为一条特殊的直道。 | AfterObtained; ModifyGeneratedMap, ModifyUnknownMapPointRoomTypes |
| 镀金缆线 | GoldPlatedCables | 你最右侧的充能球会额外触发一次被动效果。 | AfterModifyingOrbPassiveTriggerCount; ModifyOrbPassiveTriggerCounts |
| 注能核心 | InfusedCore | 在每场战斗开始时，生成{Lightning}个闪电充能球。闪电充能球额外造成{ExtraDamage}点伤害。 | AfterSideTurnStart; ModifyOrbValue |
| 吃不完的糖 | LastingCandy | 每两场战斗，你的卡牌奖励就会额外包含一张能力牌。 | BeforeCombatRewardOffered; TryModifyCardRewardOptions |
| 熔岩灯 | LavaLamp | 在战斗结束时，如果你没有受到伤害，则升级你的所有卡牌奖励。 | AfterRoomEntered, AfterDamageReceived; TryModifyCardRewardOptionsLate |
| 蜥蜴尾巴 | LizardTail | 当你的生命值将要降低至0或以下时，回复到最大生命值的{Heal}%（仅能起效一次）。 | AfterPreventingDeath; ShouldDieLate |
| 佩尔之眼 | PaelsEye | 你在每场战斗中第一次没有打出任何牌就结束回合时，消耗所有手牌然后进行一个额外回合。 | AfterObtained, BeforeCardPlayed, AfterSideTurnStart, BeforeSideTurnEndEarly, AfterTakingExtraTurn, AfterCombatEnd; ShouldTakeExtraTurn |
| 佩尔之肉 | PaelsFlesh | 从你的第3回合开始，在回合开始时额外获得{Energy:energyIcons()}。 | BeforeCombatStart, BeforeSideTurnStart, AfterSideTurnStart, AfterCombatEnd; ModifyMaxEnergy |
| 佩尔的增生组织 | PaelsGrowth | 拾起时，从牌组中选择一张牌，为其附魔：{EnchantmentName}。 | AfterObtained; TryModifyRestSiteOptions |
| 佩尔的士兵 | PaelsLegion | 将你从一张卡牌中获得的格挡翻倍，然后此遗物会休眠{Turns}回合。 | AfterObtained, BeforeCombatStart, AfterModifyingBlockAmount, AfterCardPlayed, AfterSideTurnStart, AfterCombatEnd; ModifyBlockMultiplicative |
| 钢笔尖 | PenNib | 你每打出的第10张攻击牌将会造成双倍伤害。 | BeforeCardPlayed, AfterCardPlayed; ModifyDamageMultiplicative |
| 贤者之石 | PhilosophersStone | 在每回合开始时获得{Energy:energyIcons()}。所有敌人初始获得{StrengthPower}点力量。 | AfterCreatureAddedToCombat, AfterRoomEntered; ModifyMaxEnergy |
| 怀表 | Pocketwatch | 当你在本回合打出的牌少于等于{CardThreshold}张时，则在你的下个回合开始时额外抽{Cards}张牌。 | AfterCardPlayed, AfterModifyingHandDraw, BeforeSideTurnStart, AfterSideTurnStart, AfterCombatEnd; ModifyHandDraw |
| 花粉核心 | PollinousCore | 每{Turns}个回合，额外抽{Cards}张牌。 | BeforeHandDraw, AfterCombatEnd, AfterModifyingHandDraw; ModifyHandDraw |
| 南瓜蜡烛 | PumpkinCandle | 在每个回合开始时获得{Energy:energyIcons()}。这件遗物会在{CombatCount}场战斗后熄灭。可以在休息处为其添火。 | AfterObtained, AfterCombatEnd; ModifyMaxEnergy, TryModifyRestSiteOptions |
| 皇家枕头 | RegalPillow | 在休息时，额外回复{Heal}点生命。 | AfterRestSiteHeal, AfterRoomEntered; ModifyRestSiteHealAmount, ModifyExtraRestSiteHealText |
| 损毁头盔 | RuinedHelmet | 你在每场战斗中第一次获得的力量值翻倍。 | AfterModifyingPowerAmountReceived, AfterCombatEnd; TryModifyPowerAmountReceived |
| 华美发束 | SilkenTress | 拾起时，失去所有金币。为你的第一次卡牌奖励中的所有牌附魔：华彩。 | AfterObtained, AfterModifyingCardRewardOptions; TryModifyCardRewardOptionsLate |
| 白银熔炉 | SilverCrucible | 你遇到的前{Cards}次卡牌奖励将是被升级过的。你打开的第一个宝箱将是空的 | AfterModifyingCardRewardOptions, AfterRoomEntered; TryModifyCardRewardOptionsLate, ShouldGenerateTreasure |
| 异蛇之眼 | SneckoEye | 每回合多抽{Cards}张牌。每场战斗开始时获得混乱效果。 | AfterObtained, BeforeCombatStart; ModifyHandDraw |
| 异蛇头骨 | SneckoSkull | 每当你给予敌人中毒时，所给予的中毒层数增加{PoisonPower}层。 | AfterModifyingPowerAmountGiven; ModifyPowerAmountGivenAdditive |
| 石炉加湿器 | StoneHumidifier | 每当你在休息处休息时，将你的最大生命值提升{MaxHp}点。 | AfterRestSiteHeal; ModifyExtraRestSiteHealText |
| 坚固钳子 | SturdyClamp | 可以跨回合保留最多{Block}点格挡。 | AfterPreventingBlockClear; ShouldClearBlock |
| 发条靴 | TheBoot | 每当你造成小于等于{DamageThreshold}点未被格挡的攻击伤害时，将伤害提升为{DamageMinimum}。 | AfterModifyingHpLostAfterOsty; ModifyHpLostAfterOstyLate |
| 投斧 | ThrowingAxe | 你在每场战斗中打出的第一张牌会多打出一次。 | AfterRoomEntered, AfterModifyingCardPlayCount, AfterCombatEnd; ModifyCardPlayCount |
| 钨合金棍 | TungstenRod | 你每次失去生命时，减少失去的生命值{HpLossReduction}点。 | AfterModifyingHpLostAfterOsty; ModifyHpLostAfterOsty |
| 不安油灯 | UnsettlingLamp | 你在每场战斗中第一次打出能给予敌人负面状态的牌时，将其效果翻倍。 | BeforeCombatStart, BeforePowerAmountChanged, AfterCardPlayed, AfterCombatEnd; ModifyPowerAmountGivenMultiplicative |
| 臂甲 | Vambrace | 每场战斗中，你第一次从卡牌中获得的格挡值翻倍。 | BeforeCombatStart, AfterModifyingBlockAmount, AfterCardPlayed, AfterCombatEnd; ModifyBlockMultiplicative |
| 天鹅绒颈圈 | VelvetChoker | 在每回合开始时获得{Energy:energyIcons()}。你每回合不能打出超过{Cards}张牌。 | AfterCardPlayed, AfterRoomEntered, AfterCombatEnd, BeforeSideTurnStart; ModifyMaxEnergy, ShouldPlay |
| 低语耳环 | WhisperingEarring | 在每个回合开始时，获得{Energy:energyIcons()}。瓦库将接管你的第一回合。 | AfterAutoPrePlayPhaseEnteredLate; ModifyMaxEnergy |
| 羽翼之靴 | WingedBoots | 你在选择下一层的房间时有{Rooms}次机会可以无视当前的路线。 | AfterRoomEntered; ShouldAllowFreeTravel |
| 旺购神秘券 | WongosMysteryTicket | 在{RemainingCombats}场战斗后，获得随机{Repeat}件遗物。 | AfterCombatEnd, AfterModifyingRewards; TryModifyRewards |

## 未单列来源：规则修正/流程修改（39 件）

| 原版名称 | 类名 | 原版描述 | 原版相关方法 |
| --- | --- | --- | --- |
| 准备背包 | BagOfPreparation | 在每场战斗开始时，额外抽{Cards}张牌。 | ModifyHandDraw |
| 黑星 | BlackStar | 精英敌人在被打败时多掉落一件遗物。 | TryModifyRewards |
| 肮脏地毯 | DingyRug | 卡牌奖励现在会包括无色牌。 | ModifyCardRewardCreationOptions |
| 捕梦网 | DreamCatcher | 每当你休息时，可以添加一张牌到你的牌组。 | TryModifyRestSiteHealRewards, ModifyExtraRestSiteHealText |
| 浮木 | Driftwood | 你可以在每一个卡牌奖励中重掷一次。 | TryModifyRewardsLate |
| 菲涅耳透镜 | FresnelLens | 每当你将一张带有格挡的牌加入你的牌组时，为那张牌附魔：灵巧{NimbleAmount} | TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck |
| 冻结之蛋 | FrozenEgg | 每当你获得能力牌时，将其升级。 | TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck |
| 亮片 | Glitter | 为之后的所有卡牌奖励附魔：华彩。 | TryModifyCardRewardOptionsLate |
| 冰淇淋 | IceCream | 多余的能量可以留到下一回合。 | ShouldPlayerResetEnergy |
| 佛珠手链 | JuzuBracelet | 你在?房间中不会再遭遇常规战斗。 | ModifyUnknownMapPointRoomTypes |
| 熔岩石 | LavaRock | 第一阶段的Boss敌人额外掉落{Relics}件遗物。 | TryModifyRewards |
| 切肉刀 | MeatCleaver | 你可以在休息处进行烹饪。 | TryModifyRestSiteOptions |
| 会员卡 | MembershipCard | 所有商品打折{Discount}%！ | ModifyMerchantPrice |
| 微型大炮 | MiniatureCannon | 升级的攻击牌额外造成{ExtraDamage}点伤害。 | ModifyDamageAdditive |
| 微型帐篷 | MiniatureTent | 你可以在休息处选择任意数量的选项。 | ShouldDisableRemainingRestSiteOptions |
| 熔火之蛋 | MoltenEgg | 每当你获得攻击牌时，将其升级。 | TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck |
| 神秘打火机 | MysticLighter | 有附魔的攻击牌额外造成{Damage}点伤害。 | ModifyDamageAdditive |
| 佩尔之血 | PaelsBlood | 在你的回合开始时，额外抽{Cards}张牌 | ModifyHandDraw |
| 佩尔之翼 | PaelsWing | 你可以将你的卡牌奖励献祭给佩尔。每献祭{Sacrifices}次，就能获得一件遗物。 | TryModifyCardRewardAlternatives |
| 纸鹤 | PaperKrane | 有虚弱状态的敌人造成的伤害降低40%而非25%。 |  |
| 纸蛙 | PaperPhrog | 有易伤状态的敌人受到的伤害增加75%而非50%。 |  |
| 转经轮 | PrayerWheel | 普通敌人额外掉落一次卡牌奖励。 | TryModifyRewards |
| 棱彩宝石 | PrismaticGem | 在每个回合开始时获得{Energy:energyIcons()}。卡牌奖励现在会包含其他颜色的卡牌。 | ModifyMaxEnergy, ModifyCardRewardCreationOptions |
| 三角铃鼓 | RingingTriangle | 在每场战斗的第一回合保留你的手牌。 | ShouldFlush |
| 长蛇戒指 | RingOfTheDrake | 在战斗开始时的前{Turns}个回合，你额外抽{Cards}张牌。 | ModifyHandDraw |
| 蛇之戒指 | RingOfTheSnake | 在每场战斗开始时，额外抽{Cards}张牌。 | ModifyHandDraw |
| 符文金字塔 | RunicPyramid | 你在回合结束时不再自动丢弃所有手牌。 | ShouldFlush |
| 铲子 | Shovel | 现在你可以在休息处挖掘遗物。 | TryModifyRestSiteOptions |
| 添水 | Sozu | 在每回合开始时获得{Energy:energyIcons()}。你无法再获得药水。 | ShouldProcurePotion, ModifyMaxEnergy |
| 带刺手甲 | SpikedGauntlets | 在每回合开始时获得{Energy:energyIcons()}。能力牌的耗能增加1{energyPrefix:energyIcons(1)}。 | ModifyMaxEnergy, TryModifyEnergyCostInCombat |
| 打击木偶 | StrikeDummy | 名字中有“打击”的卡牌造成{ExtraDamage}点额外伤害。 | ModifyDamageAdditive |
| 送货员 | TheCourier | 商人的卡牌、遗物和药水不再会卖光，并且所有商品打折{Discount}%。 | ModifyMerchantPrice, ShouldRefillMerchantEntry |
| 小邮箱 | TinyMailbox | 每当你休息时，获得2瓶随机药水。 | TryModifyRestSiteHealRewards, ModifyExtraRestSiteHealText |
| 毒素之蛋 | ToxicEgg | 每当你获得技能牌时，将其升级。 | TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck |
| 不死符文 | UndyingSigil | 当敌人的灾厄层数大于等于其生命值时，它造成的伤害降低50%。 | ModifyDamageMultiplicative |
| 维特鲁威仆从 | VitruvianMinion | 名字中有“仆从”的卡牌造成双倍的伤害与格挡。 | ModifyDamageMultiplicative, ModifyBlockMultiplicative |
| 白兽雕像 | WhiteBeastStatue | 战斗结束后必定掉落药水。 | ShouldForcePotionReward |
| 白星 | WhiteStar | 精英敌人额外掉落一次稀有卡牌奖励。 | TryModifyRewards |
| 羽翼护符 | WingCharm | 每次卡牌奖励中，都会有随机一张牌被附魔：迅捷{SwiftAmount}。 | TryModifyCardRewardOptionsLate |

## 未单列来源：特殊/无事件回调（2 件）

| 原版名称 | 类名 | 原版描述 | 原版相关方法 |
| --- | --- | --- | --- |
| 头环 | Circlet | 这是一个头环。 |  |
| 旺购客户感恩徽章 | WongoCustomerAppreciationBadge | 没有任何作用。 |  |

## 核对依据

- ErrorRelicsCode/Fragments/ErrorHookId.cs、ErrorEffectId.cs、ErrorHookRegistry.cs、ErrorEffectRegistry.cs、ErrorGenerator.cs。
- 本机 sts2.dll 的遗物类和方法元数据，以及游戏自带简体中文遗物描述。
- 针对骇人头盔、大蘑菇、摆动球、卓越斗篷、灯笼、草莓、梨子、芒果另外核对了原版方法体。
