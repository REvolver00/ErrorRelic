using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorEffectRegistry
{
    // =========================================================
    // E007 Choice UI Queue
    //
    // 来源问题：
    // Toolbox 的 Effect 会打开 Choose A Card 界面。
    //
    // 单个 E007 正常。
    //
    // 但是如果多个 ERROR 在同一个时机同时触发 E007，
    // 游戏可能同时尝试打开多个选牌界面，
    // 导致 Choice UI 状态冲突 / 黑屏。
    //
    // 所以 E007 使用一个 SemaphoreSlim：
    //
    // 第一个 E007
    // ↓
    // 打开选牌
    // ↓
    // 玩家完成选择
    // ↓
    // Release
    // ↓
    // 第二个 E007 才允许继续
    //
    // 注意：
    // 这里没有删除任何 E007。
    //
    // 多次触发仍然全部执行，
    // 只是从“同时执行”改成“排队执行”。
    // =========================================================

    private static readonly SemaphoreSlim E007ChoiceGate =
        new SemaphoreSlim(1, 1);


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
            // ERROR 特殊安全处理：
            //
            // 如果多个 E007 同时触发，
            // 不允许同时打开多个选择界面。
            //
            // 所有 E007 排队执行。
            //
            // 注意：
            // 不是 NoOp。
            // 每一次触发仍然都会执行。
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

            ErrorEffectId.E010_Add2RandomCurses
                => "add 2 different random Curses to your deck.",

            _ => "do nothing."
        };
    }
}