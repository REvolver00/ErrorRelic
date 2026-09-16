using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
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
            // 来源：Vajra
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
            // 来源：Razor Tooth
            //
            // 升级“当前打出的牌”
            //
            // 原版限制：
            // - 必须是自己的牌
            // - Attack / Skill
            // - 必须可以升级
            //
            // ERROR 规则：
            // 如果当前 Hook 根本没有 CardPlay，
            // 什么都不发生。
            // 不禁止这个组合。
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
            // 来源：Mercury Hourglass
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
            // 来源：Old Coin
            // 获得 300 金币
            // =====================================================

            case ErrorEffectId.E004_GainGold300:

                await PlayerCmd.GainGold(
                    context.Gold.BaseValue,
                    context.Owner
                );

                break;


            // =====================================================
            // E005
            // 来源：Mummified Hand
            //
            // 从当前手牌随机选择一张牌，
            // 使其本回合免费。
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
        }
    }


    public static string GetText(ErrorEffectId effectId)
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

            _ => "do nothing."
        };
    }
}
