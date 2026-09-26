using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// ELITE1 - RANDOM RELIC REWARDS ONLY
//
// Vanilla RelicReward.Populate() first completes its normal flow:
// - rolls / uses the intended rarity
// - PullNextRelicFromFront(...)
// - removes the source relic from the player's RelicGrabBag
// - removes it from SharedRelicGrabBag
// - ToMutable()
//
// Only AFTER all of that finishes do we replace the final _relic
// with one locked ERROR relic.
//
// We intentionally do NOT patch:
// - RelicFactory
// - RelicCmd
// - reward UI getters
// - predetermined relic rewards
//
// Predetermined rewards are left vanilla in ELITE1.
// This keeps the test focused on Elite/random relic rewards.
// =============================================================

[HarmonyPatch(
    typeof(RelicReward),
    nameof(RelicReward.Populate)
)]
public static class RelicRewardEliteErrorModePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance)
    {
        Player player =
            __instance.Player;

        if (player == null
            || !ErrorModeState.IsEnabled(player))
        {
            return;
        }

        // ELITE1 is intentionally singleplayer-only.
        if (player.RunState.Players.Count != 1)
            return;

        // Leave predetermined relic rewards untouched.
        RelicModel? predetermined =
            Traverse.Create(__instance)
                .Field("_predeterminedRelic")
                .GetValue<RelicModel>();

        if (predetermined != null)
            return;

        RelicModel? source =
            __instance.Relic;

        if (source == null)
            return;

        if (ErrorModeState.IsErrorRelic(source)
            || ErrorModeState.IsProofRelic(source))
        {
            return;
        }

        RelicModel replacement =
            ErrorModeState.CreateLockedError(
                source
            );

        // RelicReward.OnSelect later passes _relic directly to
        // RelicCmd.Obtain, so keep this as the mutable ERROR made
        // by CreateLockedError.
        Traverse.Create(__instance)
            .Field("_relic")
            .SetValue(replacement);
    }
}
