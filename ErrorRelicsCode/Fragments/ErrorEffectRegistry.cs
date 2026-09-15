using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
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
            // E001
            // 来源：Vajra
            case ErrorEffectId.E001_GainStrength1:

                await PowerCmd.Apply<StrengthPower>(
                    context.ChoiceContext,
                    context.Owner.Creature,
                    1,
                    context.Owner.Creature,
                    null
                );

                break;


            // E003
            // 来源：Mercury Hourglass
            case ErrorEffectId.E003_DamageAllEnemies3:

                await CreatureCmd.Damage(
                    context.ChoiceContext,
                    context.Owner.Creature.CombatState.HittableEnemies,
                    context.Damage,
                    context.Owner.Creature
                );

                break;
        }
    }

    public static string GetText(ErrorEffectId effectId)
    {
        return effectId switch
        {
            ErrorEffectId.E001_GainStrength1
                => "gain 1 Strength.",

            ErrorEffectId.E003_DamageAllEnemies3
                => "deal 3 damage to ALL enemies.",

            _ => "do nothing."
        };
    }
}