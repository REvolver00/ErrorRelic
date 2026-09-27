using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// EVENT4 - FINAL OBTAIN BRIDGE
//
// Direct event call:
//   Obtain(RelicModel, Player, int)
//     -> replace argument with ERROR and let vanilla Obtain run.
//
// Generic event call:
//   Obtain<T>(Player)
//     -> vanilla internally calls the final overload and later casts
//        its returned RelicModel back to T.
//
// EVENT2 replaced T with ErrorRelic and therefore broke that cast.
// EVENT3 avoided the crash by leaving generic rewards vanilla.
//
// EVENT4 bridges the generic case:
//   - skip the inner final Obtain;
//   - obtain the ERROR through the real vanilla final Obtain;
//   - return the original T object only as the generic method's
//     type-compatible return token.
//
// Thus the inventory receives ERROR while `(T)` still succeeds.
// =============================================================

[HarmonyPatch(
    typeof(RelicCmd),
    nameof(RelicCmd.Obtain),
    new Type[]
    {
        typeof(RelicModel),
        typeof(Player),
        typeof(int)
    }
)]
public static class EventRelicObtainErrorModePatch
{
    private static readonly AsyncLocal<int> BridgeDepth = new();

    [HarmonyPrefix]
    public static bool Prefix(
        ref RelicModel relic,
        Player player,
        int index,
        ref Task<RelicModel> __result)
    {
        // Recursive Obtain(ERROR) performed by the bridge must be
        // completely vanilla.
        if (BridgeDepth.Value > 0)
            return true;

        if (!EventRelicObtainContext.IsActive)
            return true;

        if (!ErrorModeState.IsEnabled(player))
            return true;

        if (ErrorModeState.IsErrorRelic(relic)
            || ErrorModeState.IsProofRelic(relic))
        {
            return true;
        }

        if (IsCalledFromGenericObtain())
        {
            RelicModel originalTypedRelic = relic;
            __result = ObtainErrorAndReturnTypedToken(
                originalTypedRelic,
                player,
                index
            );
            return false;
        }

        // Non-generic event obtains (for example DollRoom) are safe:
        // the caller expects RelicModel, so vanilla can return ERROR.
        relic = ErrorModeState.CreateLockedError(relic);
        return true;
    }


    private static async Task<RelicModel> ObtainErrorAndReturnTypedToken(
        RelicModel source,
        Player player,
        int index)
    {
        RelicModel error =
            ErrorModeState.CreateLockedError(source);

        BridgeDepth.Value = BridgeDepth.Value + 1;

        try
        {
            await RelicCmd.Obtain(
                error,
                player,
                index
            );
        }
        finally
        {
            BridgeDepth.Value =
                Math.Max(0, BridgeDepth.Value - 1);
        }

        // RelicCmd.Obtain<T> immediately casts this back to T.
        // Returning the original source object preserves that API
        // contract without putting the source relic in inventory.
        return source;
    }


    private static bool IsCalledFromGenericObtain()
    {
        StackTrace trace = new StackTrace();

        foreach (StackFrame frame in trace.GetFrames())
        {
            MethodBase? method = frame.GetMethod();

            if (method == null)
                continue;

            if (method.DeclaringType != typeof(RelicCmd))
                continue;

            if (method.Name != nameof(RelicCmd.Obtain))
                continue;

            if (method.IsGenericMethod)
                return true;
        }

        return false;
    }
}
