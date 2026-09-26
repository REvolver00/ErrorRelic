using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// SHOP1 ONLY
//
// This is the only new gameplay patch compared with DIAG8.
//
// Do NOT patch RelicFactory.
// Do NOT patch RelicCmd.
// Do NOT patch treasure.
// Do NOT patch RelicReward.
// =============================================================

[HarmonyPatch(
    typeof(MerchantRelicEntry),
    "FillSlot"
)]
public static class MerchantRelicEntryErrorModePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        MerchantRelicEntry __instance)
    {
        Player player =
            Traverse.Create(__instance)
                .Field("_player")
                .GetValue<Player>();

        if (!ErrorModeState.IsEnabled(
                player))
        {
            return;
        }

        RelicModel? source =
            __instance.Model;

        if (source == null)
            return;

        if (ErrorModeState.IsErrorRelic(source)
            || ErrorModeState.IsProofRelic(source))
        {
            return;
        }

        RelicModel replacement =
            ErrorModeState.CreateLockedError(
                source.Rarity
            );

        // Model has a private setter.
        // Vanilla already calculated the shop price.
        // Keep that price and only replace the displayed/purchased relic.
        Traverse.Create(__instance)
            .Property("Model")
            .SetValue(replacement);
    }
}
