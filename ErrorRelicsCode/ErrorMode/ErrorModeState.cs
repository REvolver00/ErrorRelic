using System.Collections.Generic;
using System.Linq;

using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;

public static class ErrorModeState
{
    private static readonly object Gate = new();

    private static readonly Dictionary<ulong, ErrorProofRelic>
        NeowReplacementByPlayer = new();

    // Only the proof relic enables ERROR mode.
    public static bool IsEnabled(
        Player player)
    {
        return player.Relics.Any(
            relic => relic is ErrorProofRelic
        );
    }

    public static bool IsErrorRelic(
        RelicModel relic)
    {
        return relic is ErrorRandomTestRelic;
    }

    public static bool IsProofRelic(
        RelicModel relic)
    {
        return relic is ErrorProofRelic;
    }

    public static RelicModel GetCanonicalForRarity(
        RelicRarity rarity)
    {
        return rarity switch
        {
            RelicRarity.Uncommon
                => ModelDb.Relic<ErrorRandomUncommonRelic>(),

            RelicRarity.Rare
                => ModelDb.Relic<ErrorRandomRareRelic>(),

            RelicRarity.Shop
                => ModelDb.Relic<ErrorRandomShopRelic>(),

            _
                => ModelDb.Relic<ErrorRandomTestRelic>()
        };
    }

    public static ErrorRandomTestRelic CreateMutableForRarity(
        RelicRarity rarity)
    {
        RelicModel mutable =
            GetCanonicalForRarity(rarity)
                .ToMutable();

        if (mutable is not ErrorRandomTestRelic errorRelic)
        {
            throw new System.InvalidOperationException(
                "ERROR rarity shell did not create an ErrorRandomTestRelic."
            );
        }

        return errorRelic;
    }

    // Generate H/E now, while the reward/shop/chest is being shown.
    public static ErrorRandomTestRelic CreateLockedError(
        RelicRarity rarity)
    {
        ErrorRandomTestRelic relic =
            CreateMutableForRarity(rarity);

        ErrorDefinition generated =
            ErrorGenerator.Generate();

        relic.GeneratedHookId =
            generated.HookId;

        relic.GeneratedEffectId =
            generated.EffectId;

        relic.DefinitionLocked = true;

        return relic;
    }

    public static ErrorProofRelic CreateProof()
    {
        RelicModel mutable =
            ModelDb.Relic<ErrorProofRelic>()
                .ToMutable();

        if (mutable is not ErrorProofRelic proof)
        {
            throw new System.InvalidOperationException(
                "ERROR proof model did not create ErrorProofRelic."
            );
        }

        return proof;
    }

    public static void ArmNeowReplacement(
        Player player,
        ErrorProofRelic proof)
    {
        lock (Gate)
        {
            NeowReplacementByPlayer[player.NetId] =
                proof;
        }
    }

    public static bool TryConsumeNeowReplacement(
        Player player,
        out ErrorProofRelic? proof)
    {
        lock (Gate)
        {
            if (!NeowReplacementByPlayer.TryGetValue(
                    player.NetId,
                    out ErrorProofRelic? value))
            {
                proof = null;
                return false;
            }

            NeowReplacementByPlayer.Remove(
                player.NetId
            );

            proof = value;
            return true;
        }
    }

    public static void ClearNeowReplacement(
        Player player)
    {
        lock (Gate)
        {
            NeowReplacementByPlayer.Remove(
                player.NetId
            );
        }
    }
}
