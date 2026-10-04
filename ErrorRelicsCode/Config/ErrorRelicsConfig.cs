using BaseLib.Config;

namespace ErrorRelics.ErrorRelicsCode.Config;

public sealed class ErrorRelicsConfig : SimpleModConfig
{
    // ---------------- Generation ----------------
    [ConfigSection("GENERATION_SECTION")]
    public static bool StartWithRedPickaxe { get; set; } = false;

    // Gameplay-affecting generation filter: when false, Effects whose source
    // relic is Ancient rarity are excluded from future ERROR generation.
    // Existing ERROR relics are never rewritten.
    public static bool AllowAncientSourceEffects { get; set; } = true;

    // Gameplay-affecting generation filter: when false, Effects that open
    // blocking multiplayer reward/card-selection flows are excluded from
    // future ERROR generation. Existing ERROR relics are never rewritten.
    public static bool AllowMultiplayerImpactEffects { get; set; } = true;

    // ---------------- Display ----------------
    [ConfigSection("DISPLAY_SECTION")]
    public static bool ShowFullEffects { get; set; } = true;

    // Child mode of ShowFullEffects=false.  When full display is enabled this
    // value is intentionally ignored.  When full display is disabled:
    // false = hide H/E completely; true = Discovery Mode (E first, then H).
    public static bool DataCrackRevealMode { get; set; } = false;

    public static bool PickupRandomRotation { get; set; } = true;

    public static bool RandomTriggerSfx { get; set; } = true;
}
