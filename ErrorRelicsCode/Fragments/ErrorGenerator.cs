using System;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorGenerator
{
    private static readonly ErrorHookId[] Hooks =
    {
        ErrorHookId.H001_EnterCombat,
        ErrorHookId.H002_PlayerTurnStart
    };

    private static readonly ErrorEffectId[] Effects =
    {
        ErrorEffectId.E001_GainStrength1,
        ErrorEffectId.E003_DamageAllEnemies3
    };

    public static ErrorDefinition Generate()
    {
        var hook = Hooks[Random.Shared.Next(Hooks.Length)];
        var effect = Effects[Random.Shared.Next(Effects.Length)];

        return new ErrorDefinition(
            hook,
            effect
        );
    }
}