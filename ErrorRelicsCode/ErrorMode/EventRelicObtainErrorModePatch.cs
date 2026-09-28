using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;

// Only obtains made while an event option is executing are converted.
// Both overload paths receive the original, type-compatible return token;
// only the ERROR is put in inventory. No runtime stack inspection is needed.
[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain),
    new Type[] { typeof(RelicModel), typeof(Player), typeof(int) })]
public static class EventRelicObtainErrorModePatch
{
    private static readonly AsyncLocal<int> BridgeDepth = new();

    [HarmonyPrefix]
    public static bool Prefix(RelicModel relic, Player player, int index, ref Task<RelicModel> __result)
    {
        if (BridgeDepth.Value > 0 || !EventRelicObtainContext.IsActive
            || !ErrorModeState.IsEnabled(player)
            || ErrorModeState.IsErrorRelic(relic) || ErrorModeState.IsProofRelic(relic))
            return true;

        __result = ObtainErrorAndReturnTypedToken(relic, player, index);
        return false;
    }

    private static async Task<RelicModel> ObtainErrorAndReturnTypedToken(RelicModel source, Player player, int index)
    {
        var error = ErrorModeState.CreateLockedError(source, player, "event-obtain");
        BridgeDepth.Value++;
        try
        {
            await RelicCmd.Obtain(error, player, index);
        }
        finally
        {
            BridgeDepth.Value--;
        }
        return source;
    }
}
