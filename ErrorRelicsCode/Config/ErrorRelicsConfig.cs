using BaseLib.Config;

namespace ErrorRelics.ErrorRelicsCode.Config;

public sealed class ErrorRelicsConfig : SimpleModConfig
{
    public static bool StartWithRedPickaxe { get; set; } = false;

    public static bool ShowFullEffects { get; set; } = true;

    public static bool PickupRandomRotation { get; set; } = true;

    public static bool RandomTriggerSfx { get; set; } = true;

    // Gameplay-affecting generation filter: when false, Effects whose source
    // relic is Ancient rarity are excluded from future ERROR generation.
    // Existing ERROR relics are never rewritten.
    public static bool AllowAncientSourceEffects { get; set; } = true;
}
