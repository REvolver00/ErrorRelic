using System.Collections.Generic;

using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;

using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// Singleplayer preview and award share one mutable ERROR instance.
// MultiplayerTreasureAwardPatch handles the award clone boundary for both modes.

internal static class TreasureRoomErrorModeState
{
    // Singleplayer CHEST2 only has one active treasure choice,
    // but using a dictionary keeps the mapping explicit.
    private static readonly Dictionary<
        RelicModel,
        RelicModel
    > SourceByError = new();


    public static void Reset()
    {
        SourceByError.Clear();
    }


    public static void Remember(
        RelicModel errorRelic,
        RelicModel sourceRelic)
    {
        SourceByError[errorRelic] =
            sourceRelic;
    }


    public static bool TryGetSource(
        RelicModel errorRelic,
        out RelicModel sourceRelic)
    {
        return SourceByError.TryGetValue(
            errorRelic,
            out sourceRelic!
        );
    }
}


// Replace only the finished singleplayer choice, after vanilla generation.
[HarmonyPatch(
    typeof(TreasureRoomRelicSynchronizer),
    nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking)
)]
public static class TreasureRoomBeginErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        TreasureRoomRelicSynchronizer __instance)
    {
        Player localPlayer =
            Traverse.Create(__instance)
                .Property("LocalPlayer")
                .GetValue<Player>();

        if (localPlayer == null
            || !ErrorModeState.IsEnabled(localPlayer))
        {
            return;
        }

        // CHEST2 intentionally proves singleplayer first.
        if (localPlayer.RunState.Players.Count != 1)
            return;

        List<RelicModel>? currentRelics =
            Traverse.Create(__instance)
                .Field("_currentRelics")
                .GetValue<List<RelicModel>>();

        if (currentRelics == null
            || currentRelics.Count != 1)
        {
            return;
        }

        RelicModel source =
            currentRelics[0];

        if (ErrorModeState.IsErrorRelic(source)
            || ErrorModeState.IsProofRelic(source))
        {
            return;
        }

        // A preview owns its data; never write H/E or identity into ModelDb.
        ErrorRandomTestRelic errorRelic = ErrorModeState.CreateLockedError(
            source, localPlayer, "treasure");

        TreasureRoomErrorModeState.Reset();

        TreasureRoomErrorModeState.Remember(
            errorRelic,
            source
        );

        // Critical CHEST2 change:
        // from this point onward the vanilla treasure system itself
        // sees ERROR, instead of only changing the UI.
        currentRelics[0] =
            errorRelic;
        MainFile.Logger.Info($"ERROR chest shown: source={source.Id}, pair={errorRelic.GeneratedHookId}/{errorRelic.GeneratedEffectId}");
    }
}


// -------------------------------------------------------------
// Vanilla chest generation removed the source relic from the
// SHARED grab bag. Normally RelicCmd.Obtain(source) would also
// remove that source from the player's PERSONAL grab bag.
//
// We now obtain ERROR instead, so do that one missing bookkeeping
// step immediately before the vanilla award animation starts.
//
// We do NOT replace result.relic here. BeginRelicPicking already
// made result.relic the same ERROR object shown by the holder.
// -------------------------------------------------------------

[HarmonyPatch(
    typeof(NTreasureRoomRelicCollection),
    "OnRelicsAwarded"
)]
public static class TreasureRoomAwardBookkeepingPatch
{
    [HarmonyPrefix]
    public static void Prefix(
        NTreasureRoomRelicCollection __instance,
        List<RelicPickingResult> results)
    {
        foreach (RelicPickingResult result
                 in results)
        {
            if (result.type
                    == RelicPickingResultType.Skipped
                || result.player == null)
            {
                continue;
            }

            if (!ErrorModeState.IsEnabled(
                    result.player))
            {
                continue;
            }

            if (!TreasureRoomErrorModeState.TryGetSource(
                    result.relic,
                    out RelicModel source))
            {
                continue;
            }

            // RelicGrabBag.Remove is safe if the relic is already
            // absent. This mirrors the bookkeeping that vanilla
            // RelicCmd.Obtain(source) would have done.
            result.player.RelicGrabBag.Remove(
                source
            );
        }
    }
}

// A failed mouse release and a failed obtain look the same to the player.
// Log only ERROR chest choices so the next report identifies that boundary.
[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "PickRelic")]
public static class ErrorChestClickTracePatch
{
    [HarmonyPrefix]
    public static void Prefix(NTreasureRoomRelicHolder holder)
    {
        if (holder.Relic.Model is ErrorRandomTestRelic error)
            MainFile.Logger.Info($"ERROR chest click received: index={holder.Index}, pair={error.GeneratedHookId}/{error.GeneratedEffectId}");
    }
}

[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.SkipRelicLocally))]
public static class ErrorChestSkipTracePatch
{
    [HarmonyPrefix]
    public static void Prefix(TreasureRoomRelicSynchronizer __instance)
    {
        if (__instance.CurrentRelics == null) return;
        foreach (var relic in __instance.CurrentRelics)
            if (relic is ErrorRandomTestRelic)
            {
                MainFile.Logger.Info("ERROR chest skipped before obtaining the relic.");
                return;
            }
    }
}
