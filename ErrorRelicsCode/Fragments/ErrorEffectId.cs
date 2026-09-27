namespace ErrorRelics.ErrorRelicsCode.Fragments;

public enum ErrorEffectId
{
    // =========================================================
    // E001
    // 来源遗物：Vajra
    //
    // Effect：
    // 获得 1 点 Strength
    // =========================================================
    E001_GainStrength1,


    // =========================================================
    // E002
    // 来源遗物：Razor Tooth
    //
    // Effect：
    // 升级当前打出的牌
    // =========================================================
    E002_UpgradePlayedCard,


    // =========================================================
    // E003
    // 来源遗物：Mercury Hourglass
    //
    // Effect：
    // 对所有敌人造成 3 点伤害
    // =========================================================
    E003_DamageAllEnemies3,


    // =========================================================
    // E004
    // 来源遗物：Old Coin
    //
    // Effect：
    // 获得 300 Gold
    // =========================================================
    E004_GainGold300,


    // =========================================================
    // E005
    // 来源遗物：Mummified Hand
    //
    // Effect：
    // 随机选择当前手牌中的一张牌
    // 使其本回合免费
    // =========================================================
    E005_RandomHandCardFreeThisTurn,


    // =========================================================
    // E006
    // 来源遗物：Distinguished Cape
    //
    // Effect A：
    // 向牌组加入 3 张 Apparition
    // =========================================================
    E006_Add3Apparitions,


    // =========================================================
    // E007
    // 来源遗物：Toolbox
    //
    // Effect：
    // 随机生成 3 张不同的无色牌
    // 玩家选择其中 1 张
    // 将选择的牌加入当前手牌
    // =========================================================
    E007_Choose1Of3ColorlessToHand,


    // =========================================================
    // E008
    // 来源遗物：Astrolabe
    //
    // Effect：
    // 从牌组选择 3 张牌。
    //
    // 对每张选择的牌：
    // 1. 随机 Transform
    // 2. 将 Transform 得到的新牌 Upgrade
    //
    // 忠于 Astrolabe 原版完整效果。
    // =========================================================
    E008_Transform3AndUpgrade,


    // =========================================================
    // E010
    // 来源遗物：Distinguished Cape
    //
    // Effect B：
    // 从当前可生成的 Curse 中
    // 随机选择 2 张不同的 Curse
    // 加入牌组
    // =========================================================
    E010_Add2RandomCurses,


    // =========================================================
    // E009
    // 来源遗物：Royal Stamp
    //
    // Effect：
    // 从牌组中选择 1 张可以被 RoyallyApproved 附魔的牌，
    // 对它施加 RoyallyApproved，
    // 并保留原版 NCardEnchantVfx。
    //
    // 注意：
    // E009 在源码位置上追加到 E010 后面，
    // 是为了不改变已经存在的 E010 枚举底层值。
    // Fragment ID 仍然是 E009。
    // =========================================================
    E009_Enchant1CardRoyallyApproved,


    // =========================================================
    // E011
    // 来源遗物：Nutritious Soup
    //
    // Effect：
    // 将牌组中的所有 Basic Strike
    // 附魔为 TezcatarasEmber。
    // =========================================================
    E011_EnchantBasicStrikesTezcatarasEmber,


    // =========================================================
    // E012
    // 来源遗物：Pael's Claw
    //
    // Effect：
    // 对牌组中所有 Goopy.CanEnchant(card) 的牌
    // 施加 Goopy。
    //
    // 原版描述对应：Enchant all Defends with Goopy.
    // =========================================================
    E012_EnchantAllGoopyEligible,


    // =========================================================
    // E013
    // 来源遗物：Sand Castle
    //
    // Effect：
    // 使用 Niche RNG 稳定洗牌后，
    // 随机 Upgrade 6 张可升级牌。
    // =========================================================
    E013_Upgrade6RandomCards,


    // =========================================================
    // E014
    // 来源遗物：War Paint
    //
    // Effect：
    // 使用 Niche RNG 稳定洗牌后，
    // 随机 Upgrade 2 张 Skill。
    // =========================================================
    E014_Upgrade2RandomSkills,


    // =========================================================
    // E015
    // 来源遗物：Whetstone
    //
    // Effect：
    // 使用 Niche RNG 稳定洗牌后，
    // 随机 Upgrade 2 张 Attack。
    // =========================================================
    E015_Upgrade2RandomAttacks,


    // =========================================================
    // E016
    // 来源遗物：Neow's Talisman
    //
    // Effect：
    // 从牌组中的 Basic 牌里，
    // 分别取最后一张 Strike 和最后一张 Defend，
    // 将它们 Upgrade。
    // =========================================================
    E016_UpgradeBasicStrikeAndDefend,


    // =========================================================
    // E017
    // 来源遗物：Pael's Horn
    //
    // Effect：
    // 向牌组加入 2 张 Relax。
    // =========================================================
    E017_Add2Relax,


    // =========================================================
    // E018
    // 来源遗物：Neow's Torment
    //
    // Effect：
    // 向牌组加入 1 张 Neow's Fury。
    // =========================================================
    E018_AddNeowsFury,


    // =========================================================
    // E019
    // 来源遗物：Storybook
    //
    // Effect：
    // 向牌组加入 1 张 Brightest Flame。
    // =========================================================
    E019_AddBrightestFlame,


    // =========================================================
    // E020
    // 来源遗物：Jewelry Box
    //
    // Effect：
    // 向牌组加入 1 张 Apotheosis。
    // =========================================================
    E020_AddApotheosis,


    // =========================================================
    // E021
    // 来源遗物：Tanx's Whistle
    //
    // Effect：
    // 向牌组加入 1 张 Whistle。
    // =========================================================
    E021_AddWhistle,


    // =========================================================
    // E022
    // 来源遗物：Happy Flower
    //
    // Effect：
    // 获得 1 Energy。
    // =========================================================
    E022_GainEnergy1,


    // =========================================================
    // E023
    // 来源遗物：Stone Calendar
    //
    // Effect：
    // 对所有敌人造成 52 点伤害。
    // =========================================================
    E023_DamageAllEnemies52,


    // =========================================================
    // E024
    // 来源遗物：Pendulum
    //
    // Effect：
    // 抽 1 张牌。
    // =========================================================
    E024_Draw1,


    // E025 来源遗物：Sparkling Rouge
    // 先获得 1 Strength，再获得 1 Dexterity。
    E025_GainStrength1Dexterity1,


    // E026 来源遗物：Oddly Smooth Stone
    // 获得 1 Dexterity。
    E026_GainDexterity1,


    // E027 来源遗物：Horn Cleat
    // 获得 14 Block。
    E027_GainBlock14,


    // E028 来源遗物：Captain's Wheel
    // 获得 18 Block。
    E028_GainBlock18,


    // E029 来源遗物：Candelabra
    // 获得 2 Energy。
    E029_GainEnergy2,


    // =========================================================
    // E030
    // 来源遗物：Meal Ticket
    // Effect：回复 15 HP。
    // =========================================================
    E030_Heal15,


    // =========================================================
    // E031
    // 来源遗物：Joss Paper
    // Effect：抽 1 张牌。
    //
    // 与 E024 Pendulum 都是抽 1，
    // 但来源遗物不同，所以保留独立 E ID / 随机权重。
    // =========================================================
    E031_Draw1JossPaper,


    // =========================================================
    // E032
    // 来源遗物：Planisphere
    // Effect：回复 5 HP。
    // =========================================================
    E032_Heal5
,
    E033_GainMaxHp20,
    E034_GainEnergy1GremlinHorn,
    E035_Draw1GremlinHorn,
    E036_DamageAllEnemies5,
    E037_DamageRandomEnemy6Kusarigama,
    E038_GainBlock4OrnamentalFan,
    E039_GainEnergy1Nunchaku,
    E040_GainBlock7TuningFork,
    E041_ApplyVulnerable1All,
    E042_ApplyPoison4All,
    E043_ApplyVigor8Self,
    E044_Draw3,
    E045_GainBlock6Orichalcum,
    E046_GainBlock4RippleBasin,
    E047_DamageRandomEnemy6ParryingShield,
    E048_Heal25,
    E049_Heal2,
    E050_GainBlock10,
    E051_ApplyFocus1,
    E052_DamageAllEnemies9,
    E053_GainMaxHp7,
    E054_HealToFull,
    E055_GainBlock6Abacus,
    E056_DamageAllEnemies20,
    E057_Draw1GamePiece,
    E058_ApplyWeak1All,
    E059_GainStrength2Self,
    E060_GainStrength1AllEnemies,
    E061_Create3ShivsInHand,
    E062_GainMaxHp1,
    E063_Heal12,
    E064_ApplyReptileTrinketPower3,
    E065_SelfDamage4Unblockable
}
