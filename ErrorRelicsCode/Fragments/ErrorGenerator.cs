using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorGenerator
{
    private static readonly ErrorHookId[] Hooks = Enum.GetValues<ErrorHookId>();
    private static readonly ErrorEffectId[] Effects = Enum.GetValues<ErrorEffectId>()
        .Where(e => e != ErrorEffectId.E002_UpgradePlayedCard
                 && e != ErrorEffectId.E007_Choose1Of3ColorlessToHand).ToArray();

    // Ownerless generation is reserved for debug callers.
    public static ErrorDefinition Generate() => new(
        Hooks[Random.Shared.Next(Hooks.Length)],
        Effects[Random.Shared.Next(Effects.Length)]);

    public static ErrorDefinition Generate(Player owner, ModelId sourceId, string sourceKey)
    {
        // Singleplayer and multiplayer use the same replayable content RNG.
        // Preview rebuilds must not advance shared combat/reward RNG streams.
        // The source key distinguishes the room and obtaining path on every peer.
        string key = FormattableString.Invariant($"ErrorRelics|{owner.RunState.TotalFloor}|{sourceKey}");
        var rng = new Rng(owner, sourceId, StringHelper.GetDeterministicHashCode(key));
        return Generate(rng);
    }

    public static ErrorDefinition Generate(Rng rng) => new(
        Hooks[rng.NextInt(Hooks.Length)],
        Effects[rng.NextInt(Effects.Length)]);
}
