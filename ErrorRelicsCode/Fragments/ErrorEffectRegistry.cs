using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

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
            // 获得 1 力量
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
            // 则什么都不发生。
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
            //
            // 原版触发条件 AfterObtained 属于 Hook，
            // 所以 Effect 本身不检查触发时机。
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
            // E010
            // 来源遗物：Distinguished Cape
            //
            // Effect B：
            // 随机加入 2 张不同的 Curse。
            //
            // 原版流程：
            //
            // 1. 从 CurseCardPool 取得当前已解锁的 Curse
            // 2. 只保留 CanBeGeneratedByModifiers 的卡
            // 3. 按 ModelId 排序
            // 4. 使用 RunState.Rng.Niche 随机抽取
            // 5. 抽到以后从候选池移除
            // 6. 因此两张 Curse 不会重复
            // 7. 将两张 Curse 加入 Deck
            //
            // AfterObtained 属于 Hook，
            // 所以这里不检查是不是刚获得遗物。
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

                    // 原版就是不放回抽取。
                    // 抽中以后从候选列表删除，
                    // 所以下一张不会和上一张重复。
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

            ErrorEffectId.E010_Add2RandomCurses
                => "add 2 different random Curses to your deck.",

            _ => "do nothing."
        };
    }
}