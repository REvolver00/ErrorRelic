using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Relics;

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
