using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HarmonyLib;

using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// EVENT4 - EVENT EXECUTION CONTEXT
//
// Event options remain visually vanilla.
// Only relic obtains that happen while the option callback is
// executing are eligible for event conversion.
//
// AsyncLocal keeps this scoped to the current async flow.
// =============================================================

public static class EventRelicObtainContext
{
    private static readonly AsyncLocal<int> Depth = new();

    public static bool IsActive => Depth.Value > 0;

    public static Func<Task>? Wrap(Func<Task>? original)
    {
        if (original == null)
            return null;

        return async () =>
        {
            Depth.Value = Depth.Value + 1;
            try
            {
                await original();
            }
            finally
            {
                Depth.Value = Math.Max(0, Depth.Value - 1);
            }
        };
    }
}


[HarmonyPatch(
    typeof(EventOption),
    MethodType.Constructor,
    new Type[]
    {
        typeof(EventModel),
        typeof(Func<Task>),
        typeof(LocString),
        typeof(LocString),
        typeof(string),
        typeof(IEnumerable<IHoverTip>)
    }
)]
public static class EventOptionExplicitTextContextPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref Func<Task>? onChosen)
    {
        onChosen = EventRelicObtainContext.Wrap(onChosen);
    }
}


[HarmonyPatch(
    typeof(EventOption),
    MethodType.Constructor,
    new Type[]
    {
        typeof(EventModel),
        typeof(Func<Task>),
        typeof(string),
        typeof(IEnumerable<IHoverTip>)
    }
)]
public static class EventOptionTextKeyContextPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref Func<Task>? onChosen)
    {
        onChosen = EventRelicObtainContext.Wrap(onChosen);
    }
}
