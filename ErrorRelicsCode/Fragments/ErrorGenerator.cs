using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using ErrorRelics.ErrorRelicsCode.Config;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorGenerator
{
    private static readonly ErrorHookId[] Hooks = Enum.GetValues<ErrorHookId>();
    private static readonly ErrorEffectId[] AllRandomEffects = Enum.GetValues<ErrorEffectId>()
        .Where(e => e != ErrorEffectId.E002_UpgradePlayedCard
                 && e != ErrorEffectId.E007_Choose1Of3ColorlessToHand).ToArray();

    private static ErrorEffectId[] ActiveEffects(int filters) => AllRandomEffects
        .Where(e => (filters & 1) != 0
                 || !ErrorEffectSourceMetadata.IsAncientSource(e))
        .Where(e => (filters & 2) != 0
                 || !ErrorEffectGenerationMetadata.MayAffectMultiplayerFlow(e))
        .ToArray();

    // Ownerless generation is reserved for debug callers.
    public static ErrorDefinition Generate()
    {
        ErrorEffectId[] effects = ActiveEffects(ErrorGenerationSettings.LocalFilters);
        return new ErrorDefinition(Hooks[Random.Shared.Next(Hooks.Length)], effects[Random.Shared.Next(effects.Length)]);
    }

    public static ErrorDefinition Generate(Player owner, ModelId sourceId, string sourceKey, string generationIdentity = "")
    {
        // Singleplayer and multiplayer use the same replayable content RNG.
        // Preview rebuilds must not advance shared combat/reward RNG streams.
        // The source key distinguishes the room and obtaining path on every peer.
        string key = FormattableString.Invariant($"ErrorRelics|{owner.RunState.TotalFloor}|{sourceKey}");
        if (generationIdentity.Length != 0) key += "|" + generationIdentity;
        var rng = new Rng(owner, sourceId, StringHelper.GetDeterministicHashCode(key));
        return Generate(rng, ErrorGenerationSettings.ForRun(owner.RunState));
    }

    public static ErrorDefinition Generate(Rng rng) => Generate(rng, ErrorGenerationSettings.LocalFilters);

    private static ErrorDefinition Generate(Rng rng, int filters)
    {
        ErrorEffectId[] effects = ActiveEffects(filters);
        return new ErrorDefinition(
            Hooks[rng.NextInt(Hooks.Length)],
            effects[rng.NextInt(effects.Length)]);
    }
}
