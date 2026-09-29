using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Relics;
using ErrorRelics.ErrorRelicsCode.Config;
using MegaCrit.Sts2.Core.Models;

using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Events;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;

[HarmonyPatch(
    typeof(Neow),
    "GenerateInitialOptions"
)]
public static class NeowErrorModePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        Neow __instance,
        ref IReadOnlyList<EventOption> __result)
    {
        if (ErrorModeState.IsEnabled(
                __instance.Owner))
        {
            return;
        }

        List<EventOption> options =
            __result.ToList();

        int targetIndex =
            options.FindIndex(
                option => option.Relic != null
            );

        if (targetIndex < 0)
            return;

        ErrorProofRelic proof =
            ErrorModeState.CreateProof();

        async Task OnProofChosen()
        {
            await RelicCmd.Obtain(
                proof,
                __instance.Owner
            );

            Traverse.Create(__instance)
                .Method("Done")
                .GetValue();
        }

        EventOption proofOption =
            new EventOption(
                __instance,
                OnProofChosen,
                proof.Title,
                proof.DynamicEventDescription,
                "ERROR_PROOF",
                proof.HoverTipsExcludingRelic
            )
            .WithRelic(proof);

        options[targetIndex] =
            proofOption;

        __result = options;
    }
}

// Await the grant before generating choices, on every simulated player.
[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted")]
public static class NeowStartingProofPatch
{
    [HarmonyPostfix]
    public static void Postfix(AncientEventModel __instance, bool isPreFinished, ref Task __result)
    {
        if (__instance is Neow && !isPreFinished && ErrorRelicsConfig.StartWithRedPickaxe)
            __result = GrantProof(__result, __instance);
    }

    private static async Task GrantProof(Task original, AncientEventModel ancient)
    {
        await original;
        var owner = ancient.Owner;
        if (owner != null && !ErrorModeState.IsEnabled(owner))
            await RelicCmd.Obtain(ErrorModeState.CreateProof(), owner);
    }
}
