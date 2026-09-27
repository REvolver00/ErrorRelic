using System.Threading;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// EVENT3 - GENERIC RelicCmd.Obtain<T> GUARD
//
// RelicCmd.Obtain<T> eventually does:
//
//   return (T) await RelicCmd.Obtain(RelicModel, Player, int);
//
// Replacing that inner RelicModel with ErrorRelic makes the final
// cast back to T fail.  LostWisp and SunkenStatue demonstrated this
// exact failure mode.
//
// This depth marker tells the final Obtain patch to leave the relic
// vanilla when it is serving a generic Obtain<T> call.
// =============================================================

public static class GenericRelicObtainContext
{
    private static readonly AsyncLocal<int> Depth = new();

    public static bool IsActive => Depth.Value > 0;

    public static void Enter()
    {
        Depth.Value = Depth.Value + 1;
    }

    public static void Exit()
    {
        if (Depth.Value > 0)
            Depth.Value = Depth.Value - 1;
    }
}
