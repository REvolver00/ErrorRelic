using System.Linq;

using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// SHOP1 CLEAN
//
// Stable mode switch:
// player owns ErrorProofRelic => ERROR Mode ON.
//
// This file contains only state/helper methods.
// It does NOT patch rewards, treasure, RelicCmd, or RelicFactory.
// =============================================================

public static class ErrorModeState
{
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
                "ERROR rarity shell did not create ErrorRandomTestRelic."
            );
        }

        return errorRelic;
    }


    // Randomize once at shop-generation time, then lock.
    public static ErrorRandomTestRelic CreateLockedError(
        RelicRarity rarity)
    {
        ErrorRandomTestRelic relic =
            CreateMutableForRarity(
                rarity
            );

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
}
