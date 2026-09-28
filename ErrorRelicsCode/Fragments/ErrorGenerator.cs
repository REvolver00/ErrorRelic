using System;
using System.Linq;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorGenerator
{
    public static ErrorDefinition Generate()
    {
        var hooks = Enum.GetValues<ErrorHookId>();
        var effects = Enum.GetValues<ErrorEffectId>().Where(e => e != ErrorEffectId.E002_UpgradePlayedCard && e != ErrorEffectId.E007_Choose1Of3ColorlessToHand).ToArray();

        var hook = hooks[Random.Shared.Next(hooks.Length)];
        var effect = effects[Random.Shared.Next(effects.Length)];

        return new ErrorDefinition(
            hook,
            effect
        );
    }
}