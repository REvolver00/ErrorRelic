using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorEffectRegistry
{
    public static async Task ExecuteAsync(
        ErrorEffectId effectId,
        ErrorContext context)
    {
        switch (effectId)
        {
            // =====================================================
            // E001
            // 来源遗物：Vajra
            //
            // Effect：
            // 获得 1 Strength
            // =====================================================

            case ErrorEffectId.E001_GainStrength1:

                await PowerCmd.Apply<StrengthPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    1,
                    context.Owner.Creature,
                    null
                );

                break;


            // =====================================================
            // E002
            // 来源遗物：Razor Tooth
            //
            // Effect：
            // 升级当前打出的牌
            //
            // 如果当前 Hook 没有 CardPlay，
            // 什么都不发生。
            // =====================================================

            case ErrorEffectId.E002_UpgradePlayedCard:
            {
                var cardPlay = context.CardPlay;

                if (cardPlay is null)
                    break;

                var card = cardPlay.Card;

                if (card.Owner != context.Owner)
                    break;

                if (card.Type != CardType.Attack
                    && card.Type != CardType.Skill)
                    break;

                if (!card.IsUpgradable)
                    break;

                CardCmd.Upgrade(card);

                break;
            }


            // =====================================================
            // E003
            // 来源遗物：Mercury Hourglass
            //
            // Effect：
            // 对所有敌人造成 3 点伤害
            // =====================================================

            case ErrorEffectId.E003_DamageAllEnemies3:

                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies,
                    context.Damage,
                    context.Owner.Creature
                );

                break;


            // =====================================================
            // E004
            // 来源遗物：Old Coin
            //
            // Effect：
            // 获得 300 Gold
            // =====================================================

            case ErrorEffectId.E004_GainGold300:

                await PlayerCmd.GainGold(
                    context.Gold.BaseValue,
                    context.Owner
                );

                break;


            // =====================================================
            // E005
            // 来源遗物：Mummified Hand
            //
            // Effect：
            // 从当前手牌随机选择一张牌
            // 使其本回合免费
            // =====================================================

            case ErrorEffectId.E005_RandomHandCardFreeThisTurn:
            {
                var combatCardSelection =
                    context.Owner.RunState.Rng.CombatCardSelection;

                IReadOnlyList<CardModel> cards =
                    PileType.Hand.GetPile(context.Owner).Cards;

                List<CardModel> paidCards =
                    cards
                        .Where(
                            card =>
                                card.EnergyCost.GetWithModifiers(
                                    CostModifiers.None
                                ) > 0
                                ||
                                card.BaseStarCost > 0
                        )
                        .ToList();

                (
                    combatCardSelection.NextItem<CardModel>(
                        paidCards.Where(
                            card => card.CostsEnergyOrStars(true)
                        )
                    )
                    ??
                    combatCardSelection.NextItem<CardModel>(
                        cards.Where(
                            card => card.CostsEnergyOrStars(true)
                        )
                    )
                    ??
                    combatCardSelection.NextItem<CardModel>(
                        paidCards
                    )
                    ??
                    combatCardSelection.NextItem<CardModel>(
                        cards
                    )
                )?.SetToFreeThisTurn();

                break;
            }


            // =====================================================
            // E006
            // 来源遗物：Distinguished Cape
            //
            // Effect A：
            // 向牌组加入 3 张 Apparition
            // =====================================================

            case ErrorEffectId.E006_Add3Apparitions:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                for (int i = 0; i < 3; ++i)
                {
                    CardModel card =
                        context.Owner.RunState.CreateCard<Apparition>(
                            context.Owner
                        );

                    results.Add(
                        await CardPileCmd.Add(
                            card,
                            PileType.Deck
                        )
                    );
                }

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }


            // =====================================================
            // E007
            // 来源遗物：Toolbox
            //
            // Effect：
            // 从 3 张不同的随机无色牌中选择 1 张，
            // 将选择的牌加入当前手牌。
            //
            // 原版流程：
            //
            // ColorlessCardPool
            // ↓
            // GetUnlockedCards
            // ↓
            // CardFactory.GetDistinctForCombat
            // ↓
            // CombatCardGeneration RNG
            // ↓
            // FromChooseACardScreen
            // ↓
            // AddGeneratedCardToCombat
            //
            // 重要：
            // 不额外使用 SemaphoreSlim。
            //
            // PlayerChoice 的暂停、排队与恢复
            // 交给游戏原生 PlayerChoiceContext /
            // HookPlayerChoiceContext 生命周期。
            // =====================================================

            case ErrorEffectId.E007_Choose1Of3ColorlessToHand:
            {
                List<CardModel> choices =
                    CardFactory.GetDistinctForCombat(
                            context.Owner,
                            ModelDb
                                .CardPool<ColorlessCardPool>()
                                .GetUnlockedCards(
                                    context.Owner.UnlockState,
                                    context.Owner.RunState.CardMultiplayerConstraint
                                ),
                            3,
                            context.Owner.RunState.Rng.CombatCardGeneration
                        )
                        .ToList<CardModel>();

                CardModel? card =
                    await CardSelectCmd.FromChooseACardScreen(
                        context.ChoiceContext,
                        choices,
                        context.Owner
                    );

                if (card == null)
                    break;

                await CardPileCmd.AddGeneratedCardToCombat(
                    card,
                    PileType.Hand,
                    context.Owner
                );

                break;
            }


            // =====================================================
            // E008
            // 来源遗物：Astrolabe
            //
            // 原版效果：
            //
            // 选择牌组中的 3 张牌。
            //
            // 对每一张选择的牌：
            //
            // CreateRandomCardForTransform(
            //     original,
            //     false,
            //     Owner.RunState.Rng.Niche)
            //
            // ↓
            //
            // Upgrade(newCard)
            //
            // ↓
            //
            // Transform(original, newCard)
            //
            // 注意：
            // 这里保持 Astrolabe 原版执行顺序：
            //
            // 先创建 Transform 目标
            // → Upgrade 新牌
            // → 再执行 Transform。
            // =====================================================

            case ErrorEffectId.E008_Transform3AndUpgrade:
            {
                CardSelectorPrefs prefs =
                    new CardSelectorPrefs(
                        CardSelectorPrefs.TransformSelectionPrompt,
                        3
                    );

                List<CardModel> selectedCards =
                    (
                        await CardSelectCmd.FromDeckForTransformation(
                            context.Owner,
                            prefs
                        )
                    )
                    .ToList<CardModel>();

                foreach (CardModel original in selectedCards)
                {
                    CardModel cardForTransform =
                        CardFactory.CreateRandomCardForTransform(
                            original,
                            false,
                            context.Owner.RunState.Rng.Niche
                        );

                    CardCmd.Upgrade(
                        cardForTransform
                    );

                    await CardCmd.Transform(
                        original,
                        cardForTransform
                    );
                }

                break;
            }


            // =====================================================
            // E009
            // 来源遗物：Royal Stamp
            //
            // 原版效果：
            //
            // 1. 获取 RoyallyApproved Enchantment
            // 2. 找出牌组中所有可以被它附魔的牌
            // 3. 使用 Niche RNG 执行 UnstableShuffle
            // 4. 弹出 EnchantSelectionPrompt，选择 1 张
            // 5. CardCmd.Enchant<RoyallyApproved>(card, 1M)
            // 6. 创建 NCardEnchantVfx
            // 7. 如果 NRun.Instance 存在，
            //    将 VFX 加入 GlobalUi.CardPreviewContainer
            //
            // 这里不删除原版 VFX，
            // 也不把选择效果改成随机效果。
            // =====================================================

            case ErrorEffectId.E009_Enchant1CardRoyallyApproved:
            {
                EnchantmentModel royalStamp =
                    ModelDb.Enchantment<RoyallyApproved>();

                List<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .Where(card => royalStamp.CanEnchant(card))
                        .ToList();

                CardSelectorPrefs prefs =
                    new CardSelectorPrefs(
                        CardSelectorPrefs.EnchantSelectionPrompt,
                        1
                    );

                CardModel? card =
                    (
                        await CardSelectCmd.FromDeckForEnchantment(
                            cards
                                .UnstableShuffle(
                                    context.Owner.RunState.Rng.Niche
                                )
                                .ToList(),
                            royalStamp,
                            1,
                            prefs
                        )
                    )
                    .FirstOrDefault();

                if (card == null)
                    break;

                CardCmd.Enchant<RoyallyApproved>(
                    card,
                    1M
                );

                NCardEnchantVfx? child =
                    NCardEnchantVfx.Create(card);

                if (child == null)
                    break;

                NRun? instance = NRun.Instance;

                if (instance == null)
                    break;

                instance
                    .GlobalUi
                    .CardPreviewContainer
                    .AddChildSafely(child);

                break;
            }


            // =====================================================
            // E010
            // 来源遗物：Distinguished Cape
            //
            // Effect B：
            // 随机加入 2 张不同的 Curse
            // =====================================================

            case ErrorEffectId.E010_Add2RandomCurses:
            {
                List<CardModel> availableCurses =
                    ModelDb
                        .CardPool<CurseCardPool>()
                        .GetUnlockedCards(
                            context.Owner.UnlockState,
                            context.Owner.RunState.CardMultiplayerConstraint
                        )
                        .Where(
                            card => card.CanBeGeneratedByModifiers
                        )
                        .OrderBy(
                            card => card.Id
                        )
                        .ToList();

                List<CardPileAddResult> curseResults =
                    new List<CardPileAddResult>();

                for (int i = 0; i < 2; ++i)
                {
                    CardModel canonicalCard =
                        context.Owner.RunState.Rng.Niche
                            .NextItem<CardModel>(
                                availableCurses
                            );

                    availableCurses.Remove(
                        canonicalCard
                    );

                    CardModel curse =
                        context.Owner.RunState.CreateCard(
                            canonicalCard,
                            context.Owner
                        );

                    curseResults.Add(
                        await CardPileCmd.Add(
                            curse,
                            PileType.Deck
                        )
                    );
                }

                CardCmd.PreviewCardPileAdd(
                    curseResults,
                    2f
                );

                break;
            }

            // =====================================================
            // E011
            // 来源遗物：Nutritious Soup
            //
            // 原版 AfterObtained：
            // 遍历当前牌组快照。
            // 对所有 Basic + Strike + 可被 TezcatarasEmber 附魔的牌：
            // Enchant<TezcatarasEmber>(card, 1M) + 原版附魔 VFX。
            // =====================================================

            case ErrorEffectId.E011_EnchantBasicStrikesTezcatarasEmber:
            {
                IEnumerable<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .ToList<CardModel>();

                foreach (CardModel card in cards)
                {
                    if (card.Rarity == CardRarity.Basic
                        && card.Tags.Contains(CardTag.Strike)
                        && ModelDb.Enchantment<TezcatarasEmber>()
                            .CanEnchant(card))
                    {
                        CardCmd.Enchant<TezcatarasEmber>(
                            card,
                            1M
                        );

                        NCardEnchantVfx? vfx =
                            NCardEnchantVfx.Create(card);

                        if (vfx != null)
                        {
                            NRun? instance = NRun.Instance;

                            if (instance != null)
                            {
                                instance
                                    .GlobalUi
                                    .CardPreviewContainer
                                    .AddChildSafely(vfx);
                            }
                        }
                    }
                }

                break;
            }


            // =====================================================
            // E012
            // 来源遗物：Pael's Claw
            //
            // 原版 AfterObtained：
            // 遍历牌组，对所有 Goopy.CanEnchant(card) 的牌
            // Enchant<Goopy>(card, 1M) + 原版附魔 VFX。
            //
            // 不自行追加 Defend 过滤，
            // 忠于原版让 Goopy.CanEnchant 决定目标资格。
            // =====================================================

            case ErrorEffectId.E012_EnchantAllGoopyEligible:
            {
                IEnumerable<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .ToList<CardModel>();

                foreach (CardModel card in cards)
                {
                    if (ModelDb.Enchantment<Goopy>()
                        .CanEnchant(card))
                    {
                        CardCmd.Enchant<Goopy>(
                            card,
                            1M
                        );

                        NRun? instance = NRun.Instance;

                        if (instance != null)
                        {
                            instance
                                .GlobalUi
                                .CardPreviewContainer
                                .AddChildSafely(
                                    NCardEnchantVfx.Create(card)
                                );
                        }
                    }
                }

                break;
            }


            // =====================================================
            // E013
            // 来源遗物：Sand Castle
            //
            // 原版 AfterObtained：
            // 可升级牌 -> StableShuffle(Niche) -> Take(6)
            // -> Grid preview 3 columns -> Upgrade(GridLayout)
            // =====================================================

            case ErrorEffectId.E013_Upgrade6RandomCards:
            {
                IEnumerable<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .Where(card =>
                            card != null
                            && card.IsUpgradable
                        )
                        .ToList<CardModel>()
                        .StableShuffle(
                            context.Owner.RunState.Rng.Niche
                        )
                        .Take(6);

                NRun? instance = NRun.Instance;

                if (instance != null)
                {
                    instance
                        .GlobalUi
                        .GridCardPreviewContainer
                        .ForceMaxColumnsUntilEmpty(3);
                }

                foreach (CardModel card in cards)
                {
                    CardCmd.Upgrade(
                        card,
                        CardPreviewStyle.GridLayout
                    );
                }

                break;
            }


            // =====================================================
            // E014
            // 来源遗物：War Paint
            //
            // 原版 AfterObtained：
            // 可升级 Skill -> StableShuffle(Niche) -> Take(2)
            // -> Upgrade(HorizontalLayout)
            // =====================================================

            case ErrorEffectId.E014_Upgrade2RandomSkills:
            {
                IEnumerable<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .Where(card =>
                            card != null
                            && card.Type == CardType.Skill
                            && card.IsUpgradable
                        )
                        .ToList<CardModel>()
                        .StableShuffle(
                            context.Owner.RunState.Rng.Niche
                        )
                        .Take(2);

                foreach (CardModel card in cards)
                {
                    CardCmd.Upgrade(
                        card,
                        CardPreviewStyle.HorizontalLayout
                    );
                }

                break;
            }


            // =====================================================
            // E015
            // 来源遗物：Whetstone
            //
            // 原版 AfterObtained：
            // 可升级 Attack -> StableShuffle(Niche) -> Take(2)
            // -> Upgrade(HorizontalLayout)
            // =====================================================

            case ErrorEffectId.E015_Upgrade2RandomAttacks:
            {
                IEnumerable<CardModel> cards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .Where(card =>
                            card != null
                            && card.Type == CardType.Attack
                            && card.IsUpgradable
                        )
                        .ToList<CardModel>()
                        .StableShuffle(
                            context.Owner.RunState.Rng.Niche
                        )
                        .Take(2);

                foreach (CardModel card in cards)
                {
                    CardCmd.Upgrade(
                        card,
                        CardPreviewStyle.HorizontalLayout
                    );
                }

                break;
            }


            // =====================================================
            // E016
            // 来源遗物：Neow's Talisman
            //
            // 原版源码：
            // Deck -> Basic cards
            // -> LastOrDefault(Strike)
            // -> LastOrDefault(Defend)
            // -> Upgrade(HorizontalLayout)
            //
            // 原版没有额外 IsUpgradable 判断，
            // 这里也不擅自添加。
            // =====================================================

            case ErrorEffectId.E016_UpgradeBasicStrikeAndDefend:
            {
                List<CardModel> basicCards =
                    PileType.Deck
                        .GetPile(context.Owner)
                        .Cards
                        .Where(card =>
                            card.Rarity == CardRarity.Basic
                        )
                        .ToList();

                CardModel? strike =
                    basicCards.LastOrDefault(
                        card => card.Tags.Contains(
                            CardTag.Strike
                        )
                    );

                CardModel? defend =
                    basicCards.LastOrDefault(
                        card => card.Tags.Contains(
                            CardTag.Defend
                        )
                    );

                if (strike != null)
                {
                    CardCmd.Upgrade(
                        strike,
                        CardPreviewStyle.HorizontalLayout
                    );
                }

                if (defend != null)
                {
                    CardCmd.Upgrade(
                        defend,
                        CardPreviewStyle.HorizontalLayout
                    );
                }

                break;
            }


            // =====================================================
            // E017
            // 来源遗物：Pael's Horn
            //
            // Effect：
            // 向牌组加入 2 张 Relax。
            //
            // 复用已经实机验证过的
            // Distinguished Cape / E006
            // CardPileCmd.Add + PreviewCardPileAdd 框架。
            // =====================================================

            case ErrorEffectId.E017_Add2Relax:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                for (int i = 0; i < 2; ++i)
                {
                    CardModel card =
                        context.Owner.RunState.CreateCard<Relax>(
                            context.Owner
                        );

                    results.Add(
                        await CardPileCmd.Add(
                            card,
                            PileType.Deck
                        )
                    );
                }

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }


            // =====================================================
            // E018
            // 来源遗物：Neow's Torment
            //
            // Effect：
            // 向牌组加入 1 张 Neow's Fury。
            // =====================================================

            case ErrorEffectId.E018_AddNeowsFury:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                CardModel card =
                    context.Owner.RunState.CreateCard<NeowsFury>(
                        context.Owner
                    );

                results.Add(
                    await CardPileCmd.Add(
                        card,
                        PileType.Deck
                    )
                );

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }


            // =====================================================
            // E019
            // 来源遗物：Storybook
            //
            // Effect：
            // 向牌组加入 1 张 Brightest Flame。
            // =====================================================

            case ErrorEffectId.E019_AddBrightestFlame:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                CardModel card =
                    context.Owner.RunState.CreateCard<BrightestFlame>(
                        context.Owner
                    );

                results.Add(
                    await CardPileCmd.Add(
                        card,
                        PileType.Deck
                    )
                );

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }


            // =====================================================
            // E020
            // 来源遗物：Jewelry Box
            //
            // Effect：
            // 向牌组加入 1 张 Apotheosis。
            // =====================================================

            case ErrorEffectId.E020_AddApotheosis:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                CardModel card =
                    context.Owner.RunState.CreateCard<Apotheosis>(
                        context.Owner
                    );

                results.Add(
                    await CardPileCmd.Add(
                        card,
                        PileType.Deck
                    )
                );

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }


            // =====================================================
            // E021
            // 来源遗物：Tanx's Whistle
            //
            // Effect：
            // 向牌组加入 1 张 Whistle。
            // =====================================================

            case ErrorEffectId.E021_AddWhistle:
            {
                List<CardPileAddResult> results =
                    new List<CardPileAddResult>();

                CardModel card =
                    context.Owner.RunState.CreateCard<Whistle>(
                        context.Owner
                    );

                results.Add(
                    await CardPileCmd.Add(
                        card,
                        PileType.Deck
                    )
                );

                CardCmd.PreviewCardPileAdd(
                    results,
                    2f
                );

                break;
            }



            // =====================================================
            // E022
            // 来源遗物：Happy Flower
            //
            // 原版效果：
            // 获得 1 Energy。
            // =====================================================

            case ErrorEffectId.E022_GainEnergy1:
            {
                await PlayerCmd.GainEnergy(
                    context.Vars["HappyFlowerEnergy"].BaseValue,
                    context.Owner
                );

                break;
            }


            // =====================================================
            // E023
            // 来源遗物：Stone Calendar
            //
            // 原版效果：
            // 对所有 HittableEnemies 造成 52 点
            // ValueProp.Unpowered 伤害。
            // =====================================================

            case ErrorEffectId.E023_DamageAllEnemies52:
            {
                var stoneCalendarDamage =
                    (DamageVar)context.Vars[
                        "StoneCalendarDamage"
                    ];

                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    context.Owner.Creature
                        .CombatState
                        .HittableEnemies,
                    stoneCalendarDamage,
                    context.Owner.Creature
                );

                break;
            }


            // =====================================================
            // E024
            // 来源遗物：Pendulum
            //
            // 原版效果：
            // 使用当前 Hook 的原生 PlayerChoiceContext
            // 抽 1 张牌。
            // =====================================================

            case ErrorEffectId.E024_Draw1:
            {
                await CardPileCmd.Draw(
                    context.ChoiceContext,
                    context.Vars["PendulumCards"].BaseValue,
                    context.Owner
                );

                break;
            }


            // =====================================================
            // E025
            // 来源遗物：Sparkling Rouge
            // 用户本机 sts2.dll 反编译源码：
            // 先 Apply<StrengthPower>(1)，再 Apply<DexterityPower>(1)。
            // =====================================================

            case ErrorEffectId.E025_GainStrength1Dexterity1:
            {
                await PowerCmd.Apply<StrengthPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["SparklingRougeStrength"].BaseValue,
                    context.Owner.Creature,
                    null
                );

                await PowerCmd.Apply<DexterityPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["SparklingRougeDexterity"].BaseValue,
                    context.Owner.Creature,
                    null
                );

                break;
            }


            // =====================================================
            // E026
            // 来源遗物：Oddly Smooth Stone
            // 用户本机 sts2.dll 反编译源码：Apply<DexterityPower>(1)。
            // =====================================================

            case ErrorEffectId.E026_GainDexterity1:
            {
                await PowerCmd.Apply<DexterityPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["OddlySmoothStoneDexterity"].BaseValue,
                    context.Owner.Creature,
                    null
                );

                break;
            }


            // =====================================================
            // E027
            // 来源遗物：Horn Cleat
            // 用户本机 sts2.dll 反编译源码：
            // GainBlock(... BlockVar(14, Unpowered), null)。
            // =====================================================

            case ErrorEffectId.E027_GainBlock14:
            {
                BlockVar block =
                    new BlockVar(
                        context.Vars["HornCleatBlock"].BaseValue,
                        ValueProp.Unpowered
                    );

                await CreatureCmd.GainBlock(
                    context.Owner.Creature,
                    block,
                    null
                );

                break;
            }


            // =====================================================
            // E028
            // 来源遗物：Captain's Wheel
            // 用户本机 sts2.dll 反编译源码：
            // GainBlock(... BlockVar(18, Unpowered), null)。
            // =====================================================

            case ErrorEffectId.E028_GainBlock18:
            {
                BlockVar block =
                    new BlockVar(
                        context.Vars["CaptainsWheelBlock"].BaseValue,
                        ValueProp.Unpowered
                    );

                await CreatureCmd.GainBlock(
                    context.Owner.Creature,
                    block,
                    null
                );

                break;
            }


            // =====================================================
            // E029
            // 来源遗物：Candelabra
            // 用户本机 sts2.dll 反编译源码：GainEnergy(2, Owner)。
            // =====================================================

            case ErrorEffectId.E029_GainEnergy2:
            {
                await PlayerCmd.GainEnergy(
                    context.Vars["CandelabraEnergy"].BaseValue,
                    context.Owner
                );

                break;
            }


            // =====================================================
            // E030
            // 来源遗物：Meal Ticket
            //
            // 用户本机 sts2.dll 反编译源码：
            // CreatureCmd.Heal(Owner.Creature, HealVar(15).BaseValue)
            // =====================================================

            case ErrorEffectId.E030_Heal15:
            {
                await CreatureCmd.Heal(
                    context.Owner.Creature,
                    context.Vars["MealTicketHeal"].BaseValue
                );

                break;
            }


            // =====================================================
            // E031
            // 来源遗物：Joss Paper
            //
            // 用户本机 sts2.dll 反编译源码：
            // CardPileCmd.Draw(choiceContext, 1, Owner)
            //
            // 与 E024 Pendulum 分开保留独立 Fragment ID。
            // =====================================================

            case ErrorEffectId.E031_Draw1JossPaper:
            {
                await CardPileCmd.Draw(
                    context.ChoiceContext,
                    context.Vars["JossPaperCards"].BaseValue,
                    context.Owner
                );

                break;
            }


            // =====================================================
            // E032
            // 来源遗物：Planisphere
            //
            // 用户本机 sts2.dll 反编译源码：
            // CreatureCmd.Heal(Owner.Creature, HealVar(5).BaseValue)
            // =====================================================

            case ErrorEffectId.E032_Heal5:
            {
                await CreatureCmd.Heal(
                    context.Owner.Creature,
                    context.Vars["PlanisphereHeal"].BaseValue
                );

                break;
            }

            case ErrorEffectId.E033_GainMaxHp20:
                await CreatureCmd.GainMaxHp(context.Owner.Creature, context.Vars["BigMushroomMaxHp"].BaseValue);
                break;

            case ErrorEffectId.E034_GainEnergy1GremlinHorn:
                await PlayerCmd.GainEnergy(1M, context.Owner);
                break;

            case ErrorEffectId.E035_Draw1GremlinHorn:
                await CardPileCmd.Draw(context.ChoiceContext, 1M, context.Owner);
                break;

            case ErrorEffectId.E036_DamageAllEnemies5:
                await CreatureCmd.Damage(context.ChoiceContext, context.Owner.Creature.CombatState.HittableEnemies,
                    new DamageVar(5M, ValueProp.Unpowered), context.Owner.Creature);
                break;

            case ErrorEffectId.E037_DamageRandomEnemy6Kusarigama:
            case ErrorEffectId.E047_DamageRandomEnemy6ParryingShield:
            {
                var target = context.Owner.RunState.Rng.CombatTargets.NextItem(context.Owner.Creature.CombatState.HittableEnemies);
                if (target != null)
                    await CreatureCmd.Damage(context.ChoiceContext, target,
                        new DamageVar(6M, ValueProp.Unpowered), context.Owner.Creature);
                break;
            }

            case ErrorEffectId.E038_GainBlock4OrnamentalFan:
            case ErrorEffectId.E046_GainBlock4RippleBasin:
                await CreatureCmd.GainBlock(context.Owner.Creature, new BlockVar(4M, ValueProp.Unpowered), null);
                break;

            case ErrorEffectId.E039_GainEnergy1Nunchaku:
                await PlayerCmd.GainEnergy(1M, context.Owner);
                break;

            case ErrorEffectId.E040_GainBlock7TuningFork:
                await CreatureCmd.GainBlock(context.Owner.Creature, new BlockVar(7M, ValueProp.Unpowered), null);
                break;

            case ErrorEffectId.E041_ApplyVulnerable1All:
                await PowerCmd.Apply<VulnerablePower>(context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies, 1M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E042_ApplyPoison4All:
                foreach (var enemy in context.Owner.Creature.CombatState.HittableEnemies)
                    await PowerCmd.Apply<PoisonPower>(context.ChoiceContext, enemy, 4M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E043_ApplyVigor8Self:
                await PowerCmd.Apply<VigorPower>(context.ChoiceContext, context.Owner.Creature, 8M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E044_Draw3:
                await CardPileCmd.Draw(context.ChoiceContext, 3M, context.Owner);
                break;

            case ErrorEffectId.E045_GainBlock6Orichalcum:
            case ErrorEffectId.E055_GainBlock6Abacus:
                await CreatureCmd.GainBlock(context.Owner.Creature, new BlockVar(6M, ValueProp.Unpowered), null);
                break;

            case ErrorEffectId.E048_Heal25:
                await CreatureCmd.Heal(context.Owner.Creature, 25M);
                break;

            case ErrorEffectId.E049_Heal2:
                await CreatureCmd.Heal(context.Owner.Creature, 2M);
                break;

            case ErrorEffectId.E050_GainBlock10:
                await CreatureCmd.GainBlock(context.Owner.Creature, new BlockVar(10M, ValueProp.Unpowered), null);
                break;

            case ErrorEffectId.E051_ApplyFocus1:
                await PowerCmd.Apply<FocusPower>(context.ChoiceContext, context.Owner.Creature, 1M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E052_DamageAllEnemies9:
                await CreatureCmd.Damage(context.ChoiceContext, context.Owner.Creature.CombatState.HittableEnemies,
                    new DamageVar(9M, ValueProp.Unpowered), context.Owner.Creature);
                break;

            case ErrorEffectId.E053_GainMaxHp7:
                await CreatureCmd.GainMaxHp(context.Owner.Creature, 7M);
                break;

            case ErrorEffectId.E054_HealToFull:
                await CreatureCmd.Heal(context.Owner.Creature,
                    context.Owner.Creature.MaxHp - context.Owner.Creature.CurrentHp);
                break;

            case ErrorEffectId.E056_DamageAllEnemies20:
                await CreatureCmd.Damage(context.ChoiceContext, context.Owner.Creature.CombatState.HittableEnemies,
                    new DamageVar(20M, ValueProp.Unpowered), context.Owner.Creature);
                break;

            case ErrorEffectId.E057_Draw1GamePiece:
                await CardPileCmd.Draw(context.ChoiceContext, 1M, context.Owner);
                break;

            case ErrorEffectId.E058_ApplyWeak1All:
                await PowerCmd.Apply<WeakPower>(context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies, 1M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E059_GainStrength2Self:
                await PowerCmd.Apply<StrengthPower>(context.ChoiceContext, context.Owner.Creature, 2M, context.Owner.Creature, null);
                break;

            case ErrorEffectId.E060_GainStrength1AllEnemies:
                await PowerCmd.Apply<StrengthPower>(context.ChoiceContext,
                    context.Owner.Creature.CombatState.GetOpponentsOf(context.Owner.Creature).Where(c => c.IsAlive),
                    1M, null, null);
                break;

            case ErrorEffectId.E061_Create3ShivsInHand:
                await Shiv.CreateInHand(context.Owner, 3, context.Owner.Creature.CombatState);
                break;

            case ErrorEffectId.E062_GainMaxHp1:
                await CreatureCmd.GainMaxHp(context.Owner.Creature, 1M);
                break;

            case ErrorEffectId.E063_Heal12:
                await CreatureCmd.Heal(context.Owner.Creature, 12M);
                break;

            case ErrorEffectId.E064_ApplyReptileTrinketPower3:
                await PowerCmd.Apply<ReptileTrinketPower>(context.ChoiceContext, context.Owner.Creature, 3M,
                    context.Owner.Creature, null);
                break;

            case ErrorEffectId.E065_SelfDamage4Unblockable:
                await CreatureCmd.Damage(context.ChoiceContext, context.Owner.Creature,
                    new DamageVar(4M, ValueProp.Unblockable | ValueProp.Unpowered),
                    null, null, null);
                break;

            case ErrorEffectId.E066_ShurikenGainStrength1:
                await PowerCmd.Apply<StrengthPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["ShurikenStrength"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E067_KunaiGainDexterity1:
                await PowerCmd.Apply<DexterityPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["KunaiDexterity"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E068_LanternGainEnergy1:
                await PlayerCmd.GainEnergy(
                    context.Vars["LanternEnergy"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E069_VeryHotCocoaGainEnergy4:
                await PlayerCmd.GainEnergy(
                    context.Vars["VeryHotCocoaEnergy"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E070_StrawberryGainMaxHp7:
                await CreatureCmd.GainMaxHp(
                    context.Owner.Creature,
                    context.Vars["StrawberryMaxHp"].BaseValue);
                return;

            case ErrorEffectId.E071_PearGainMaxHp10:
                await CreatureCmd.GainMaxHp(
                    context.Owner.Creature,
                    context.Vars["PearMaxHp"].BaseValue);
                return;

            case ErrorEffectId.E072_MangoGainMaxHp14:
                await CreatureCmd.GainMaxHp(
                    context.Owner.Creature,
                    context.Vars["MangoMaxHp"].BaseValue);
                return;

            case ErrorEffectId.E073_LoomingFruitGainMaxHp31:
                await CreatureCmd.GainMaxHp(
                    context.Owner.Creature,
                    context.Vars["LoomingFruitMaxHp"].BaseValue);
                return;

            case ErrorEffectId.E074_NutritiousOysterGainMaxHp11:
                await CreatureCmd.GainMaxHp(
                    context.Owner.Creature,
                    context.Vars["NutritiousOysterMaxHp"].BaseValue);
                return;

            case ErrorEffectId.E075_GoldenPearlGainGold150:
                await PlayerCmd.GainGold(
                    context.Vars["GoldenPearlGold"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E076_SignetRingGainGold888:
                await PlayerCmd.GainGold(
                    context.Vars["SignetRingGold"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E077_IvoryTileGainEnergy1:
                await PlayerCmd.GainEnergy(
                    context.Vars["IvoryTileEnergy"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E078_SaiGainBlock7:
                await CreatureCmd.GainBlock(
                    context.Owner.Creature,
                    new BlockVar(
                        context.Vars["SaiBlock"].BaseValue,
                        ValueProp.Unpowered),
                    null);
                return;

            case ErrorEffectId.E079_ChandelierGainEnergy3:
                await PlayerCmd.GainEnergy(
                    context.Vars["ChandelierEnergy"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E080_SwordOfJadeGainStrength3:
                await PowerCmd.Apply<StrengthPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["SwordOfJadeStrength"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E081_DaughterOfTheWindGainBlock4:
                await CreatureCmd.GainBlock(
                    context.Owner.Creature,
                    new BlockVar(
                        context.Vars["DaughterOfTheWindBlock"].BaseValue,
                        ValueProp.Unpowered),
                    null,
                    true);
                return;

            case ErrorEffectId.E082_LostWispDamageAllEnemies8:
                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies,
                    new DamageVar(
                        context.Vars["LostWispDamage"].BaseValue,
                        ValueProp.Unpowered),
                    context.Owner.Creature);
                return;

            case ErrorEffectId.E083_CharonsAshesDamageAllEnemies3:
                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies,
                    new DamageVar(
                        context.Vars["CharonsAshesDamage"].BaseValue,
                        ValueProp.Unpowered),
                    context.Owner.Creature);
                return;

            case ErrorEffectId.E084_ForgottenSoulDamageRandomEnemy4WithBluntVfx:
            {
                var target = context.Owner.RunState.Rng.CombatTargets.NextItem(
                    context.Owner.Creature.CombatState.HittableEnemies);

                if (target is null)
                    return;

                VfxCmd.PlayOnCreatureCenter(
                    target,
                    "vfx/vfx_attack_blunt");

                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    target,
                    new DamageVar(
                        context.Vars["ForgottenSoulDamage"].BaseValue,
                        ValueProp.Unpowered),
                    context.Owner.Creature);
                return;
            }

            case ErrorEffectId.E085_IronClubDraw1:
                await CardPileCmd.Draw(
                    context.ChoiceContext,
                    context.Vars["IronClubDraw"].BaseValue,
                    context.Owner);
                return;

            case ErrorEffectId.E086_BronzeScalesApplyThorns3:
                await PowerCmd.Apply<ThornsPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["BronzeScalesThorns"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E087_GorgetApplyPlating4:
                await PowerCmd.Apply<PlatingPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["GorgetPlating"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E088_HelicalDartApplyHelicalDartPower1:
                await PowerCmd.Apply<HelicalDartPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    context.Vars["HelicalDartDexterity"].BaseValue,
                    context.Owner.Creature,
                    null);
                return;

            case ErrorEffectId.E089_PermafrostGainBlock7:
                await CreatureCmd.GainBlock(
                    context.Owner.Creature,
                    new BlockVar(
                        context.Vars["PermafrostBlock"].BaseValue,
                        ValueProp.Unpowered),
                    null);
                return;

            case ErrorEffectId.E090_EmptyCageRemove2FromDeck:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.RemoveSelectionPrompt,
                    context.Vars["EmptyCageCards"].IntValue);

                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForRemoval(
                        context.Owner,
                        prefs,
                        null);

                foreach (CardModel card in selected)
                    await CardPileCmd.RemoveFromDeck(card, true);

                return;
            }

            case ErrorEffectId.E091_PomanderUpgrade1FromDeck:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.UpgradeSelectionPrompt,
                    context.Vars["PomanderCards"].IntValue);

                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForUpgrade(
                        context.Owner,
                        prefs);

                foreach (CardModel card in selected.ToList())
                    CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);

                return;
            }

            case ErrorEffectId.E092_BigHatAdd2RandomEtherealToHand:
            {
                IEnumerable<CardModel> candidates =
                    context.Owner.Character.CardPool
                        .GetUnlockedCards(
                            context.Owner.UnlockState,
                            context.Owner.RunState.CardMultiplayerConstraint)
                        .Where(card => card.Keywords.Contains(CardKeyword.Ethereal));

                if (!candidates.Any())
                    return;

                IEnumerable<CardModel> cards =
                    CardFactory.GetDistinctForCombat(
                        context.Owner,
                        candidates,
                        context.Vars["BigHatCards"].IntValue,
                        context.Owner.RunState.Rng.CombatCardGeneration);

                await CardPileCmd.AddGeneratedCardsToCombat(
                    cards,
                    PileType.Hand,
                    context.Owner,
                    CardPilePosition.Bottom);
                return;
            }

            case ErrorEffectId.E093_OrangeDoughAdd2RandomColorlessToHand:
            {
                IEnumerable<CardModel> cards =
                    CardFactory.GetDistinctForCombat(
                        context.Owner,
                        ModelDb.CardPool<ColorlessCardPool>()
                            .GetUnlockedCards(
                                context.Owner.UnlockState,
                                context.Owner.RunState.CardMultiplayerConstraint),
                        context.Vars["OrangeDoughCards"].IntValue,
                        context.Owner.RunState.Rng.CombatCardGeneration);

                await CardPileCmd.AddGeneratedCardsToCombat(
                    cards,
                    PileType.Hand,
                    context.Owner,
                    CardPilePosition.Bottom);
                return;
            }

            case ErrorEffectId.E094_RadiantPearlAdd1LuminesceToHand:
            {
                List<CardModel> cards = new();

                for (int i = 0; i < context.Vars["RadiantPearlCards"].IntValue; ++i)
                {
                    cards.Add(
                        context.Owner.Creature.CombatState.CreateCard<Luminesce>(
                            context.Owner));
                }

                await CardPileCmd.AddGeneratedCardsToCombat(
                    cards,
                    PileType.Hand,
                    context.Owner,
                    CardPilePosition.Bottom);
                return;
            }

            case ErrorEffectId.E095_CrackedCoreChannel1Lightning:
            {
                int amount = context.Vars["CrackedCoreLightning"].IntValue;
                for (int i = 0; i < amount; ++i)
                {
                    await OrbCmd.Channel<LightningOrb>(
                        new BlockingPlayerChoiceContext(),
                        context.Owner);
                }
                return;
            }

            case ErrorEffectId.E096_SymbioticVirusChannel1Dark:
            {
                int amount = context.Vars["SymbioticVirusDark"].IntValue;
                for (int i = 0; i < amount; ++i)
                {
                    await OrbCmd.Channel<DarkOrb>(
                        new BlockingPlayerChoiceContext(),
                        context.Owner);
                }
                return;
            }

            case ErrorEffectId.E097_CallingBellAddCurseOfTheBell:
                await CardPileCmd.AddCurseToDeck<CurseOfTheBell>(
                    context.Owner);
                return;

            case ErrorEffectId.E098_CallingBellOfferThreeRelicRewards:
            {
                List<Reward> rewards = new()
                {
                    new RelicReward(RelicRarity.Common, context.Owner),
                    new RelicReward(RelicRarity.Uncommon, context.Owner),
                    new RelicReward(RelicRarity.Rare, context.Owner)
                };

                await RewardsCmd.OfferCustom(
                    context.Owner,
                    rewards);
                return;
            }

            case ErrorEffectId.E099_ToyBoxOfferFiveWaxRelics:
            {
                List<Reward> rewards = new();

                int amount = context.Vars["ToyBoxRelics"].IntValue;
                for (int i = 0; i < amount; ++i)
                {
                    RelicModel relic =
                        RelicFactory.PullNextRelicFromFront(context.Owner)
                            .ToMutable();

                    relic.IsWax = true;
                    rewards.Add(new RelicReward(relic, context.Owner));
                }

                await RewardsCmd.OfferCustom(
                    context.Owner,
                    rewards);
                return;
            }

            case ErrorEffectId.E100_ToyBoxMeltLeftmostWax:
            {
                RelicModel? relic = context.Owner.Relics.FirstOrDefault(
                    candidate => candidate.IsWax && !candidate.IsMelted);

                if (relic is null)
                    return;

                await RelicCmd.Melt(relic);

                // Toy Box performs this scaled wait after Melt in the target DLL.
                await Cmd.CustomScaledWait(
                    0.5f,
                    0.75f,
                    false,
                    CancellationToken.None);
                return;
            }


            case ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard:
            {
                CardSelectorPrefs prefs = new(
                    new LocString(
                        "relics",
                        "DOLLYS_MIRROR.selectionScreenPrompt"),
                    context.Vars["DollysMirrorCards"].IntValue);

                CardModel? selected = (
                    await CardSelectCmd.FromDeckGeneric(
                        context.Owner,
                        prefs,
                        card => card.Type != CardType.Quest,
                        null))
                    .FirstOrDefault();

                if (selected is null)
                    return;

                CardModel clone = context.Owner.RunState.CloneCard(selected);
                CardPileAddResult addResult = await CardPileCmd.Add(
                    clone,
                    PileType.Deck,
                    CardPilePosition.Bottom,
                    null,
                    false);

                CardCmd.PreviewCardPileAdd(
                    addResult,
                    1.2f,
                    CardPreviewStyle.HorizontalLayout);
                return;
            }

            case ErrorEffectId.E102_GnarledHammerEnchantUpTo3Sharp3:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.EnchantSelectionPrompt,
                    0,
                    context.Vars["GnarledHammerCards"].IntValue)
                {
                    Cancelable = false,
                    RequireManualConfirmation = true
                };

                EnchantmentModel sharp = ModelDb.Enchantment<Sharp>();
                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForEnchantment(
                        context.Owner,
                        sharp,
                        context.Vars["GnarledHammerSharpAmount"].IntValue,
                        prefs);

                foreach (CardModel card in selected)
                {
                    CardCmd.Enchant(
                        sharp.ToMutable(),
                        card,
                        context.Vars["GnarledHammerSharpAmount"].BaseValue);
                    CardCmd.Preview(card, 1.2f, CardPreviewStyle.HorizontalLayout);
                }

                return;
            }

            case ErrorEffectId.E103_KifudaEnchantUpTo3Adroit3:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.EnchantSelectionPrompt,
                    0,
                    context.Vars["KifudaCards"].IntValue)
                {
                    Cancelable = false,
                    RequireManualConfirmation = true
                };

                EnchantmentModel adroit = ModelDb.Enchantment<Adroit>();
                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForEnchantment(
                        context.Owner,
                        adroit,
                        context.Vars["KifudaAdroitAmount"].IntValue,
                        prefs);

                foreach (CardModel card in selected)
                {
                    CardCmd.Enchant(
                        adroit.ToMutable(),
                        card,
                        context.Vars["KifudaAdroitAmount"].BaseValue);
                    CardCmd.Preview(card, 1.2f, CardPreviewStyle.HorizontalLayout);
                }

                return;
            }

            case ErrorEffectId.E104_PunchDaggerEnchant1Momentum5:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.EnchantSelectionPrompt,
                    context.Vars["PunchDaggerCards"].IntValue);

                EnchantmentModel momentum = ModelDb.Enchantment<Momentum>();
                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForEnchantment(
                        context.Owner,
                        momentum,
                        context.Vars["PunchDaggerMomentum"].IntValue,
                        prefs);

                foreach (CardModel card in selected)
                {
                    CardCmd.Enchant(
                        momentum.ToMutable(),
                        card,
                        context.Vars["PunchDaggerMomentum"].BaseValue);
                    CardCmd.Preview(card, 1.2f, CardPreviewStyle.HorizontalLayout);
                }

                return;
            }

            case ErrorEffectId.E105_TriBoomerangEnchant3Instinct1:
            {
                CardSelectorPrefs prefs = new(
                    CardSelectorPrefs.EnchantSelectionPrompt,
                    context.Vars["TriBoomerangCards"].IntValue);

                IEnumerable<CardModel> selected =
                    await CardSelectCmd.FromDeckForEnchantment(
                        context.Owner,
                        ModelDb.Enchantment<Instinct>(),
                        context.Vars["TriBoomerangInstinct"].IntValue,
                        prefs);

                foreach (CardModel card in selected)
                {
                    CardCmd.Enchant<Instinct>(
                        card,
                        context.Vars["TriBoomerangInstinct"].BaseValue);

                    // Target TriBoomerang explicitly creates this enchant VFX
                    // instead of using CardCmd.Preview.
                    NCardEnchantVfx? vfx = NCardEnchantVfx.Create(card);
                    if (vfx is not null && NRun.Instance is not null)
                    {
                        NRun.Instance.GlobalUi.CardPreviewContainer
                            .AddChildSafely(vfx);
                    }
                }

                return;
            }

            case ErrorEffectId.E106_OrreryOffer5CardRewards:
            {
                List<Reward> rewards = new();
                CardCreationOptions options = new(
                    new[] { context.Owner.Character.CardPool },
                    CardCreationSource.Other,
                    CardRarityOddsType.RegularEncounter,
                    null);

                int rewardCount = context.Vars["OrreryRewards"].IntValue;
                int choices = context.Vars["OrreryChoicesPerReward"].IntValue;
                for (int i = 0; i < rewardCount; ++i)
                {
                    rewards.Add(new CardReward(options, choices, context.Owner, null));
                }

                await RewardsCmd.OfferCustom(context.Owner, rewards);
                return;
            }

            case ErrorEffectId.E107_GlassEyeOffer5RarityCardRewards:
            {
                // Target field RVA decodes to Common, Common,
                // Uncommon, Uncommon, Rare in this exact order.
                CardRarity[] rarities =
                {
                    CardRarity.Common,
                    CardRarity.Common,
                    CardRarity.Uncommon,
                    CardRarity.Uncommon,
                    CardRarity.Rare
                };

                List<Reward> rewards = new();
                int choices = context.Vars["GlassEyeChoicesPerReward"].IntValue;

                foreach (CardRarity rarity in rarities)
                {
                    CardCreationOptions options =
                        CardCreationOptions.ForNonCombatWithUniformOdds(
                                new[] { context.Owner.Character.CardPool },
                                card => card.Rarity == rarity)
                            .WithFlags(CardCreationFlags.NoRarityModification);

                    rewards.Add(new CardReward(options, choices, context.Owner, null));
                }

                await RewardsCmd.OfferCustom(context.Owner, rewards);
                return;
            }

            case ErrorEffectId.E108_SmallCapsuleOffer1RelicReward:
                await RewardsCmd.OfferCustom(
                    context.Owner,
                    new List<Reward> { new RelicReward(context.Owner) });
                return;

            case ErrorEffectId.E109_ChoicesParadoxChoose1Of5RetainToHand:
            {
                // Some ERROR Hooks intentionally have no PlayerChoiceContext
                // and use ThrowingPlayerChoiceContext. Do not defer or invent a
                // later timing; in that invalid context this combination is a
                // safe no-op rather than a crash/desync risk.
                if (context.ChoiceContext is ThrowingPlayerChoiceContext)
                    return;

                List<CardModel> cards = CardFactory.GetDistinctForCombat(
                        context.Owner,
                        context.Owner.Character.CardPool.GetUnlockedCards(
                            context.Owner.UnlockState,
                            context.Owner.RunState.CardMultiplayerConstraint),
                        context.Vars["ChoicesParadoxCards"].IntValue,
                        context.Owner.RunState.Rng.CombatCardGeneration)
                    .ToList();

                // Vanilla has the same early-return intent: if no cards are
                // generated, it reports a SoftlockException and returns.
                if (cards.Count == 0)
                    return;

                foreach (CardModel card in cards)
                    CardCmd.ApplyKeyword(card, new[] { CardKeyword.Retain });

                LocString prompt = new(
                    "relics",
                    "CHOICES_PARADOX.selectionScreenPrompt");

                IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
                    context.ChoiceContext,
                    cards,
                    context.Owner,
                    new CardSelectorPrefs(
                        prompt,
                        context.Vars["ChoicesParadoxChoose"].IntValue));

                foreach (CardModel card in selected)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(
                        card,
                        PileType.Hand,
                        context.Owner,
                        CardPilePosition.Bottom);
                }

                return;
            }

            default:
                break;
        }
    }


    // =========================================================
    // 自动描述
    // =========================================================

    public static string GetText(
        ErrorEffectId effectId)
    {
        return effectId switch
        {
            ErrorEffectId.E001_GainStrength1
                => "gain 1 Strength.",

            ErrorEffectId.E002_UpgradePlayedCard
                => "upgrade the played card.",

            ErrorEffectId.E003_DamageAllEnemies3
                => "deal 3 damage to ALL enemies.",

            ErrorEffectId.E004_GainGold300
                => "gain 300 Gold.",

            ErrorEffectId.E005_RandomHandCardFreeThisTurn
                => "make a random card in your hand free this turn.",

            ErrorEffectId.E006_Add3Apparitions
                => "add 3 Apparitions to your deck.",

            ErrorEffectId.E007_Choose1Of3ColorlessToHand
                => "choose 1 of 3 random Colorless cards and add it to your hand.",

            ErrorEffectId.E008_Transform3AndUpgrade
                => "transform 3 cards, then upgrade the transformed cards.",

            ErrorEffectId.E009_Enchant1CardRoyallyApproved
                => "enchant 1 card with Royally Approved.",

            ErrorEffectId.E010_Add2RandomCurses
                => "add 2 different random Curses to your deck.",

            ErrorEffectId.E011_EnchantBasicStrikesTezcatarasEmber
                => "enchant all Basic Strikes with Tezcatara's Ember.",

            ErrorEffectId.E012_EnchantAllGoopyEligible
                => "enchant all Defends with Goopy.",

            ErrorEffectId.E013_Upgrade6RandomCards
                => "upgrade 6 random cards.",

            ErrorEffectId.E014_Upgrade2RandomSkills
                => "upgrade 2 random Skills.",

            ErrorEffectId.E015_Upgrade2RandomAttacks
                => "upgrade 2 random Attacks.",

            ErrorEffectId.E016_UpgradeBasicStrikeAndDefend
                => "upgrade 1 Basic Strike and 1 Basic Defend.",

            ErrorEffectId.E017_Add2Relax
                => "add 2 Relax to your deck.",

            ErrorEffectId.E018_AddNeowsFury
                => "add 1 Neow's Fury to your deck.",

            ErrorEffectId.E019_AddBrightestFlame
                => "add 1 Brightest Flame to your deck.",

            ErrorEffectId.E020_AddApotheosis
                => "add 1 Apotheosis to your deck.",

            ErrorEffectId.E021_AddWhistle
                => "add 1 Whistle to your deck.",

            ErrorEffectId.E022_GainEnergy1
                => "gain 1 Energy.",

            ErrorEffectId.E023_DamageAllEnemies52
                => "deal 52 damage to ALL enemies.",

            ErrorEffectId.E024_Draw1
                => "draw 1 card.",

            ErrorEffectId.E025_GainStrength1Dexterity1
                => "gain 1 Strength, then gain 1 Dexterity.",

            ErrorEffectId.E026_GainDexterity1
                => "gain 1 Dexterity.",

            ErrorEffectId.E027_GainBlock14
                => "gain 14 Block.",

            ErrorEffectId.E028_GainBlock18
                => "gain 18 Block.",

            ErrorEffectId.E029_GainEnergy2
                => "gain 2 Energy.",

            ErrorEffectId.E030_Heal15
                => "heal 15 HP.",

            ErrorEffectId.E031_Draw1JossPaper
                => "draw 1 card.",

            ErrorEffectId.E032_Heal5
                => "heal 5 HP.",

            ErrorEffectId.E033_GainMaxHp20 => "gain 20 Max HP.",
            ErrorEffectId.E034_GainEnergy1GremlinHorn => "gain 1 Energy.",
            ErrorEffectId.E035_Draw1GremlinHorn => "draw 1 card.",
            ErrorEffectId.E036_DamageAllEnemies5 => "deal 5 damage to ALL enemies.",
            ErrorEffectId.E037_DamageRandomEnemy6Kusarigama => "deal 6 damage to a random enemy.",
            ErrorEffectId.E038_GainBlock4OrnamentalFan => "gain 4 Block.",
            ErrorEffectId.E039_GainEnergy1Nunchaku => "gain 1 Energy.",
            ErrorEffectId.E040_GainBlock7TuningFork => "gain 7 Block.",
            ErrorEffectId.E041_ApplyVulnerable1All => "apply 1 Vulnerable to ALL enemies.",
            ErrorEffectId.E042_ApplyPoison4All => "apply 4 Poison to ALL enemies.",
            ErrorEffectId.E043_ApplyVigor8Self => "gain 8 Vigor.",
            ErrorEffectId.E044_Draw3 => "draw 3 cards.",
            ErrorEffectId.E045_GainBlock6Orichalcum => "gain 6 Block.",
            ErrorEffectId.E046_GainBlock4RippleBasin => "gain 4 Block.",
            ErrorEffectId.E047_DamageRandomEnemy6ParryingShield => "deal 6 damage to a random enemy.",
            ErrorEffectId.E048_Heal25 => "heal 25 HP.",
            ErrorEffectId.E049_Heal2 => "heal 2 HP.",
            ErrorEffectId.E050_GainBlock10 => "gain 10 Block.",
            ErrorEffectId.E051_ApplyFocus1 => "gain 1 Focus.",
            ErrorEffectId.E052_DamageAllEnemies9 => "deal 9 damage to ALL enemies.",
            ErrorEffectId.E053_GainMaxHp7 => "gain 7 Max HP.",
            ErrorEffectId.E054_HealToFull => "heal to full HP.",
            ErrorEffectId.E055_GainBlock6Abacus => "gain 6 Block.",
            ErrorEffectId.E056_DamageAllEnemies20 => "deal 20 damage to ALL enemies.",
            ErrorEffectId.E057_Draw1GamePiece => "draw 1 card.",
            ErrorEffectId.E058_ApplyWeak1All => "apply 1 Weak to ALL enemies.",
            ErrorEffectId.E059_GainStrength2Self => "gain 2 Strength.",
            ErrorEffectId.E060_GainStrength1AllEnemies => "give ALL living enemies 1 Strength.",
            ErrorEffectId.E061_Create3ShivsInHand => "create 3 Shivs in your hand.",
            ErrorEffectId.E062_GainMaxHp1 => "gain 1 Max HP.",
            ErrorEffectId.E063_Heal12 => "heal 12 HP.",
            ErrorEffectId.E064_ApplyReptileTrinketPower3 => "gain Reptile Trinket's 3 Strength effect.",
            ErrorEffectId.E065_SelfDamage4Unblockable => "take 4 unblockable damage.",

            ErrorEffectId.E066_ShurikenGainStrength1 => "gain 1 Strength.",
            ErrorEffectId.E067_KunaiGainDexterity1 => "gain 1 Dexterity.",
            ErrorEffectId.E068_LanternGainEnergy1 => "gain 1 Energy.",
            ErrorEffectId.E069_VeryHotCocoaGainEnergy4 => "gain 4 Energy.",
            ErrorEffectId.E070_StrawberryGainMaxHp7 => "gain 7 Max HP.",
            ErrorEffectId.E071_PearGainMaxHp10 => "gain 10 Max HP.",
            ErrorEffectId.E072_MangoGainMaxHp14 => "gain 14 Max HP.",
            ErrorEffectId.E073_LoomingFruitGainMaxHp31 => "gain 31 Max HP.",
            ErrorEffectId.E074_NutritiousOysterGainMaxHp11 => "gain 11 Max HP.",
            ErrorEffectId.E075_GoldenPearlGainGold150 => "gain 150 Gold.",
            ErrorEffectId.E076_SignetRingGainGold888 => "gain 888 Gold.",
            ErrorEffectId.E077_IvoryTileGainEnergy1 => "gain 1 Energy.",
            ErrorEffectId.E078_SaiGainBlock7 => "gain 7 Block.",
            ErrorEffectId.E079_ChandelierGainEnergy3 => "gain 3 Energy.",
            ErrorEffectId.E080_SwordOfJadeGainStrength3 => "gain 3 Strength.",
            ErrorEffectId.E081_DaughterOfTheWindGainBlock4 => "gain 4 Block.",
            ErrorEffectId.E082_LostWispDamageAllEnemies8 => "deal 8 damage to ALL enemies.",
            ErrorEffectId.E083_CharonsAshesDamageAllEnemies3 => "deal 3 damage to ALL enemies.",
            ErrorEffectId.E084_ForgottenSoulDamageRandomEnemy4WithBluntVfx => "play the blunt-hit VFX and deal 4 damage to a random enemy.",
            ErrorEffectId.E085_IronClubDraw1 => "draw 1 card.",
            ErrorEffectId.E086_BronzeScalesApplyThorns3 => "gain 3 Thorns.",
            ErrorEffectId.E087_GorgetApplyPlating4 => "gain 4 Plating.",
            ErrorEffectId.E088_HelicalDartApplyHelicalDartPower1 => "gain 1 Helical Dart temporary Dexterity.",
            ErrorEffectId.E089_PermafrostGainBlock7 => "gain 7 Block.",
            ErrorEffectId.E090_EmptyCageRemove2FromDeck => "choose 2 cards from your deck and remove them.",
            ErrorEffectId.E091_PomanderUpgrade1FromDeck => "choose 1 card from your deck and upgrade it.",
            ErrorEffectId.E092_BigHatAdd2RandomEtherealToHand => "create 2 different random Ethereal cards in your hand.",
            ErrorEffectId.E093_OrangeDoughAdd2RandomColorlessToHand => "create 2 different random Colorless cards in your hand.",
            ErrorEffectId.E094_RadiantPearlAdd1LuminesceToHand => "create 1 Luminesce in your hand.",
            ErrorEffectId.E095_CrackedCoreChannel1Lightning => "Channel 1 Lightning.",
            ErrorEffectId.E096_SymbioticVirusChannel1Dark => "Channel 1 Dark.",
            ErrorEffectId.E097_CallingBellAddCurseOfTheBell => "add 1 Curse of the Bell to your deck.",
            ErrorEffectId.E098_CallingBellOfferThreeRelicRewards => "offer a Common, Uncommon, and Rare relic reward.",
            ErrorEffectId.E099_ToyBoxOfferFiveWaxRelics => "offer 5 Wax relics.",
            ErrorEffectId.E100_ToyBoxMeltLeftmostWax => "melt the leftmost unmelted Wax relic.",

            ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard => "choose 1 non-Quest card in your deck, duplicate it, and add the copy to your deck.",
            ErrorEffectId.E102_GnarledHammerEnchantUpTo3Sharp3 => "choose up to 3 cards and Enchant each with 3 Sharp.",
            ErrorEffectId.E103_KifudaEnchantUpTo3Adroit3 => "choose up to 3 cards and Enchant each with 3 Adroit.",
            ErrorEffectId.E104_PunchDaggerEnchant1Momentum5 => "choose 1 card and Enchant it with 5 Momentum.",
            ErrorEffectId.E105_TriBoomerangEnchant3Instinct1 => "choose 3 cards and Enchant each with 1 Instinct.",
            ErrorEffectId.E106_OrreryOffer5CardRewards => "offer 5 card rewards with 3 choices each.",
            ErrorEffectId.E107_GlassEyeOffer5RarityCardRewards => "offer 5 card rewards: 2 Common, 2 Uncommon, and 1 Rare.",
            ErrorEffectId.E108_SmallCapsuleOffer1RelicReward => "offer 1 random relic reward.",
            ErrorEffectId.E109_ChoicesParadoxChoose1Of5RetainToHand => "generate 5 random character cards with Retain, choose 1, and add it to your hand.",
            _ => "do nothing."
        };
    }
}