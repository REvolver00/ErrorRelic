using System.Collections.Generic;

using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// Treasure room.
//
// _currentRelics stays VANILLA.
// This is important because TreasureRoomRelicSynchronizer uses
// that list for voting / awarding / synchronization.
//
// We only create ERROR display mappings after vanilla generation.
// =============================================================

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

        if (!ErrorModeState.IsEnabled(
                localPlayer))
        {
            return;
        }

        List<RelicModel>? vanillaRelics =
            Traverse.Create(__instance)
                .Field("_currentRelics")
                .GetValue<List<RelicModel>>();

        if (vanillaRelics == null)
            return;

        foreach (RelicModel source in vanillaRelics)
        {
            if (ErrorModeState.IsErrorRelic(source)
                || ErrorModeState.IsProofRelic(source))
            {
                continue;
            }

            ErrorDisplayReplacementState.GetOrCreate(
                source
            );
        }
    }
}


// =============================================================
// UI asks CurrentRelics.
// Return ERROR views with identical list indices.
//
// Internal OnPicked / AwardRelics use the private _currentRelics
// field directly, so they continue operating on vanilla relics.
// =============================================================

[HarmonyPatch(
    typeof(TreasureRoomRelicSynchronizer),
    "get_CurrentRelics"
)]
public static class TreasureRoomCurrentRelicsErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        TreasureRoomRelicSynchronizer __instance,
        ref IReadOnlyList<RelicModel>? __result)
    {
        if (__result == null)
            return;

        Player localPlayer =
            Traverse.Create(__instance)
                .Property("LocalPlayer")
                .GetValue<Player>();

        if (!ErrorModeState.IsEnabled(
                localPlayer))
        {
            return;
        }

        List<RelicModel> display =
            new(__result.Count);

        foreach (RelicModel source in __result)
        {
            if (ErrorModeState.IsErrorRelic(source)
                || ErrorModeState.IsProofRelic(source))
            {
                display.Add(source);
                continue;
            }

            display.Add(
                ErrorDisplayReplacementState.GetOrCreate(
                    source
                )
            );
        }

        __result = display;
    }
}
