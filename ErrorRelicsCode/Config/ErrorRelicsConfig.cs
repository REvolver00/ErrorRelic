using BaseLib.Config;

namespace ErrorRelics.ErrorRelicsCode.Config;

public sealed class ErrorRelicsConfig : SimpleModConfig
{
    public static bool StartWithRedPickaxe { get; set; } = false;

    public static bool ShowFullEffects { get; set; } = true;

    public static bool PickupRandomRotation { get; set; } = true;

    public static bool RandomTriggerSfx { get; set; } = true;
}
