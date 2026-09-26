using System.Runtime.CompilerServices;

using ErrorRelics.ErrorRelicsCode.Relics;

using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// Display-only ERROR mapping.
//
// The key is the ORIGINAL vanilla relic object owned by the
// reward/chest system.
//
// The value is the already-randomized, locked ERROR relic shown
// to the player and later handed to RelicCmd.Obtain.
//
// ConditionalWeakTable uses object identity and does not keep the
// vanilla source alive after its reward/session is gone.
// =============================================================

public static class ErrorDisplayReplacementState
{
    private static readonly ConditionalWeakTable<
        RelicModel,
        ErrorRandomTestRelic
    > Replacements = new();


    public static ErrorRandomTestRelic GetOrCreate(
        RelicModel source)
    {
        if (source is ErrorRandomTestRelic error)
            return error;

        return Replacements.GetValue(
            source,
            relic =>
                ErrorModeState.CreateLockedError(
                    relic.Rarity
                )
        );
    }


    public static bool TryGet(
        RelicModel source,
        out ErrorRandomTestRelic? replacement)
    {
        return Replacements.TryGetValue(
            source,
            out replacement
        );
    }


    public static bool TryConsume(
        RelicModel source,
        out ErrorRandomTestRelic? replacement)
    {
        if (!Replacements.TryGetValue(
                source,
                out replacement))
        {
            return false;
        }

        Replacements.Remove(source);
        return true;
    }
}
