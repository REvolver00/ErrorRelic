using System;

using HarmonyLib;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// ANCIENT1 - ALL NON-NEOW ANCIENT RELIC OPTIONS
//
// AncientEventModel has one shared non-generic RelicOption:
//
//   RelicOption(RelicModel relic, string pageName,
//               string? customDonePage)
//
// The generic RelicOption<T>() funnels into that method.
//
// Therefore we do NOT patch each Ancient or each Ancient relic.
// Every non-Neow Ancient relic option passes through this one
// common point.
//
// Prefix replacement is important:
// - the EventOption is built from ERROR;
// - the hover/display is ERROR;
// - the original OnChosen closure captures ERROR;
// - vanilla RelicCmd.Obtain obtains that same ERROR;
// - vanilla Ancient Done()/history flow remains untouched.
//
// Neow is explicitly excluded. Its already-stable Proof flow
// stays exactly as it is.
// =============================================================

[HarmonyPatch(
    typeof(AncientEventModel),
    "RelicOption",
    new Type[]
    {
        typeof(RelicModel),
        typeof(string),
        typeof(string)
    }
)]
public static class AncientRelicOptionErrorModePatch
{
    [HarmonyPrefix]
    public static void Prefix(
        AncientEventModel __instance,
        ref RelicModel relic)
    {
        // Keep the already-tested Neow Proof implementation.
        if (__instance is Neow)
            return;

        if (!ErrorModeState.IsEnabled(
                __instance.Owner))
        {
            return;
        }

        if (ErrorModeState.IsErrorRelic(relic)
            || ErrorModeState.IsProofRelic(relic))
        {
            return;
        }

        // Each RelicOption call gets its own mutable ERROR and its
        // own locked H/E pair. Three Ancient relic choices therefore
        // become three independently generated ERROR choices.
        relic =
            ErrorModeState.CreateLockedError(
                relic.Rarity
            );
    }
}
