using System.Collections.Generic;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// Elite / normal RelicReward.
//
// IMPORTANT:
// _relic stays the ORIGINAL vanilla relic at all times.
//
// This preserves:
// - reward serialization
// - reward synchronization
// - vanilla reward bookkeeping
//
// We only associate a locked ERROR view with that source.
// RelicCmd.Obtain swaps to the exact same ERROR at claim time.
// =============================================================

[HarmonyPatch(
    typeof(RelicReward),
    nameof(RelicReward.Populate)
)]
public static class RelicRewardPopulateErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance)
    {
        if (!ErrorModeState.IsEnabled(
                __instance.Player))
        {
            return;
        }

        RelicModel? source =
            __instance.Relic;

        if (source == null)
            return;

        if (ErrorModeState.IsErrorRelic(source)
            || ErrorModeState.IsProofRelic(source))
        {
            return;
        }

        ErrorDisplayReplacementState.GetOrCreate(
            source
        );
    }
}


// Description shown by reward UI.
[HarmonyPatch(
    typeof(RelicReward),
    "get_Description"
)]
public static class RelicRewardDescriptionErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance,
        ref LocString __result)
    {
        RelicModel? source =
            __instance.Relic;

        if (source == null)
            return;

        if (!ErrorDisplayReplacementState.TryGet(
                source,
                out var error)
            || error == null)
        {
            return;
        }

        __result =
            error.Title;
    }
}


// Hover text shown by reward UI.
[HarmonyPatch(
    typeof(RelicReward),
    "get_ExtraHoverTips"
)]
public static class RelicRewardHoverErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance,
        ref IEnumerable<IHoverTip> __result)
    {
        RelicModel? source =
            __instance.Relic;

        if (source == null)
            return;

        if (!ErrorDisplayReplacementState.TryGet(
                source,
                out var error)
            || error == null)
        {
            return;
        }

        __result =
            error.HoverTips;
    }
}


// Icon shown by reward UI.
[HarmonyPatch(
    typeof(RelicReward),
    "CreateIcon"
)]
public static class RelicRewardIconErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance,
        ref TextureRect __result)
    {
        RelicModel? source =
            __instance.Relic;

        if (source == null)
            return;

        if (!ErrorDisplayReplacementState.TryGet(
                source,
                out var error)
            || error == null)
        {
            return;
        }

        __result.Texture =
            error.BigIcon;

        error.UpdateTexture(
            __result
        );
    }
}
