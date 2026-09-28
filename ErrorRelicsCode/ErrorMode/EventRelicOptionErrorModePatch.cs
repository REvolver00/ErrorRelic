using System;
using System.Threading.Tasks;

using HarmonyLib;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// EVENT1 - ALL ORDINARY EVENT RELIC OPTIONS
//
// EventModel has one shared non-generic RelicOption:
//
//   RelicOption(RelicModel relic, Func<Task>? onChosen,
//               string pageName = "INITIAL")
//
// The generic RelicOption<T>() funnels into that method.
//
// Vanilla is allowed to decide which relic the event wants first.
// At this final EventModel option-building layer, we replace that
// relic with one locked ERROR. Therefore the event UI/hover and
// the relic ultimately obtained both see the same ERROR.
//
// AncientEventModel is explicitly excluded. Ancient relic options
// keep using the already-tested Ancient-specific patch.
// Neow therefore also remains on its existing Proof flow.
//
// Do NOT replace this with a global RelicCmd/RelicFactory patch.
// =============================================================

[HarmonyPatch(
    typeof(EventModel),
    "RelicOption",
    new Type[]
    {
        typeof(RelicModel),
        typeof(Func<Task>),
        typeof(string)
    }
)]
public static class EventRelicOptionErrorModePatch
{
    [HarmonyPrefix]
    public static void Prefix(
        EventModel __instance,
        ref RelicModel relic, string pageName)
    {
        // Ancient events have their own stable ERROR path.
        if (__instance is AncientEventModel)
            return;

        if (__instance.Owner == null
            || !ErrorModeState.IsEnabled(__instance.Owner))
        {
            return;
        }

        if (ErrorModeState.IsErrorRelic(relic)
            || ErrorModeState.IsProofRelic(relic))
        {
            return;
        }

        // Vanilla already selected the event's intended source relic.
        // Preserve that source's rarity/visual identity, but make the
        // actual option a newly locked ERROR.
        relic =
            ErrorModeState.CreateLockedError(
                relic, __instance.Owner, $"event:{__instance.Id.Entry}:{pageName}"
            );
    }
}
