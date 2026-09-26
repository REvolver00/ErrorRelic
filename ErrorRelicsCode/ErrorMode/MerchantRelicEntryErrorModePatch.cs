using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// Shop ERROR replacement.
//
// Vanilla MerchantRelicEntry.FillSlot first finishes all of this:
// - PullNextRelicFromBack
// - vanilla blacklist / IsAllowedInShops checks
// - ToMutable
// - SetModel
// - CalcCost
// - MarkRelicAsSeen
//
// Only after that has completed do we swap the displayed/purchased
// Model to an already-randomized, locked ERROR relic.
//
// Important:
// We DO NOT call CalcCost again.
// This preserves the price vanilla already generated and avoids
// consuming PlayerRng.Shops a second time.
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

        if (!ErrorModeState.IsEnabled(player))
            return;

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
        // Set only the model itself; keep vanilla's already-generated cost.
        Traverse.Create(__instance)
            .Property("Model")
            .SetValue(replacement);
    }
}
