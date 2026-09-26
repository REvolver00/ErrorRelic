using System.Collections.Generic;

using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;

using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// CHEST2 - SINGLEPLAYER TREASURE
//
// CHEST1 tried to patch the tiny CurrentRelics getter.
// In the user's test the chest still displayed the vanilla relic,
// so CHEST2 removes that approach completely.
//
// Vanilla BeginRelicPicking already:
// 1. rolls rarity;
// 2. pulls the vanilla source relic from SharedRelicGrabBag;
// 3. stores it in _currentRelics.
//
// CHEST2 changes ONLY step 3's final stored choice, in a postfix:
// source relic -> canonical ERROR shell with one locked H/E.
//
// This means every later vanilla treasure step sees the SAME
// canonical ERROR object:
// - InitializeRelics display
// - voting by index
// - AwardRelics result.relic
// - holder lookup
// - result.relic.ToMutable()
// - RelicCmd.Obtain
//
// We remember the vanilla source only so that, when the player
// actually takes the ERROR, we also remove that source relic from
// the player's personal RelicGrabBag. Vanilla already removed it
// from the shared bag when the chest was generated.
//
// No RelicFactory patch.
// No RelicCmd patch.
// No RelicReward patch.
// No CurrentRelics getter patch.
// =============================================================


internal static class TreasureRoomErrorModeState
{
    // Singleplayer CHEST2 only has one active treasure choice,
    // but using a dictionary keeps the mapping explicit.
    private static readonly Dictionary<
        RelicModel,
        RelicModel
    > SourceByError = new();


    public static void Reset()
    {
        SourceByError.Clear();
    }


    public static void Remember(
        RelicModel errorRelic,
        RelicModel sourceRelic)
    {
        SourceByError[errorRelic] =
            sourceRelic;
    }


    public static bool TryGetSource(
        RelicModel errorRelic,
        out RelicModel sourceRelic)
    {
        return SourceByError.TryGetValue(
            errorRelic,
            out sourceRelic!
        );
    }
}


// -------------------------------------------------------------
// Generate the treasure normally first, then replace only the
// finished singleplayer treasure choice stored in _currentRelics.
//
// We deliberately use the canonical ERROR shell here because
// vanilla AnimateRelicAwards later calls result.relic.ToMutable().
// Canonical -> ToMutable is the game's normal relic flow.
//
// GeneratedHookId / GeneratedEffectId / DefinitionLocked are our
// own properties and are copied into the mutable relic by the
// normal model clone.
// -------------------------------------------------------------

[HarmonyPatch(
    typeof(TreasureRoomRelicSynchronizer),
    nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking)
)]
public static class TreasureRoomBeginErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        TreasureRoomRelicSynchronizer __instance)
    {
        Player localPlayer =
            Traverse.Create(__instance)
                .Property("LocalPlayer")
                .GetValue<Player>();

        if (localPlayer == null
            || !ErrorModeState.IsEnabled(localPlayer))
        {
            return;
        }

        // CHEST2 intentionally proves singleplayer first.
        if (localPlayer.RunState.Players.Count != 1)
            return;

        List<RelicModel>? currentRelics =
            Traverse.Create(__instance)
                .Field("_currentRelics")
                .GetValue<List<RelicModel>>();

        if (currentRelics == null
            || currentRelics.Count != 1)
        {
            return;
        }

        RelicModel source =
            currentRelics[0];

        if (ErrorModeState.IsErrorRelic(source)
            || ErrorModeState.IsProofRelic(source))
        {
            return;
        }

        RelicModel canonical =
            ErrorModeState.GetCanonicalForRarity(
                source.Rarity
            );

        if (canonical
            is not ErrorRandomTestRelic errorRelic)
        {
            return;
        }

        ErrorDefinition generated =
            ErrorGenerator.Generate();

        errorRelic.GeneratedHookId =
            generated.HookId;

        errorRelic.GeneratedEffectId =
            generated.EffectId;

        errorRelic.DefinitionLocked =
            true;

        TreasureRoomErrorModeState.Reset();

        TreasureRoomErrorModeState.Remember(
            errorRelic,
            source
        );

        // Critical CHEST2 change:
        // from this point onward the vanilla treasure system itself
        // sees ERROR, instead of only changing the UI.
        currentRelics[0] =
            errorRelic;
    }
}


// -------------------------------------------------------------
// Vanilla chest generation removed the source relic from the
// SHARED grab bag. Normally RelicCmd.Obtain(source) would also
// remove that source from the player's PERSONAL grab bag.
//
// We now obtain ERROR instead, so do that one missing bookkeeping
// step immediately before the vanilla award animation starts.
//
// We do NOT replace result.relic here. BeginRelicPicking already
// made result.relic the same ERROR object shown by the holder.
// -------------------------------------------------------------

[HarmonyPatch(
    typeof(NTreasureRoomRelicCollection),
    "OnRelicsAwarded"
)]
public static class TreasureRoomAwardBookkeepingPatch
{
    [HarmonyPrefix]
    public static void Prefix(
        NTreasureRoomRelicCollection __instance,
        List<RelicPickingResult> results)
    {
        foreach (RelicPickingResult result
                 in results)
        {
            if (result.type
                    == RelicPickingResultType.Skipped
                || result.player == null)
            {
                continue;
            }

            if (!ErrorModeState.IsEnabled(
                    result.player))
            {
                continue;
            }

            if (!TreasureRoomErrorModeState.TryGetSource(
                    result.relic,
                    out RelicModel source))
            {
                continue;
            }

            // RelicGrabBag.Remove is safe if the relic is already
            // absent. This mirrors the bookkeeping that vanilla
            // RelicCmd.Obtain(source) would have done.
            result.player.RelicGrabBag.Remove(
                source
            );
        }
    }
}
