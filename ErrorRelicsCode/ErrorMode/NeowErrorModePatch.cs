using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Relics;

using HarmonyLib;

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

        EventOption originalOption =
            options[targetIndex];

        ErrorProofRelic proof =
            ErrorModeState.CreateProof();

        async Task OnProofChosen()
        {
            ErrorModeState.ArmNeowReplacement(
                __instance.Owner,
                proof
            );

            try
            {
                await originalOption.Chosen();
            }
            finally
            {
                ErrorModeState.ClearNeowReplacement(
                    __instance.Owner
                );
            }
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
