using System;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorGenerator
{
    public static ErrorDefinition Generate()
    {
        var hooks = Enum.GetValues<ErrorHookId>();
        var effects = Enum.GetValues<ErrorEffectId>();

        var hook = hooks[Random.Shared.Next(hooks.Length)];
        var effect = effects[Random.Shared.Next(effects.Length)];

        return new ErrorDefinition(
            hook,
            effect
        );
    }
}