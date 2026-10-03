using ErrorRelics.ErrorRelicsCode.Config;
using System.Collections.Generic;
using System.Reflection;
using ErrorRelics.ErrorRelicsCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;

namespace ErrorRelics.ErrorRelicsCode.Visuals;

[HarmonyPatch]
public static class ErrorRelicDescriptionPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicDescription));
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicEventDescription));
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.Flavor));
    }

    [HarmonyPostfix]
    public static void Postfix(RelicModel __instance, ref LocString __result)
    {
        // Visibility is presentation-only. Gameplay always keeps the real H/E.
        if (__instance is ErrorRandomTestRelic generated)
        {
            // In multiplayer, other players' ERRORs are always fully readable.
            // Discovery restrictions apply only to the local owner's own relics.
            bool viewingOtherPlayer = generated.Owner != null
                && generated.Owner.NetId != LocalContext.NetId;

            if (ErrorRelicsConfig.ShowFullEffects || viewingOtherPlayer)
            {
                __result.Add("ErrorDescription", generated.FullDescription);
                return;
            }

            if (ErrorRelicsConfig.DataCrackRevealMode)
            {
                string text = !generated.EffectRevealed
                    ? ""
                    : !generated.HookRevealed
                        ? generated.EffectOnlyDescription
                        : generated.FullDescription;
                if (text.Length == 0)
                    __result = new LocString("static_hover_tips", "ERRORRELICS-HIDDEN_EFFECT.description");
                else
                    __result.Add("ErrorDescription", text);
                return;
            }

            __result = new LocString("static_hover_tips", "ERRORRELICS-HIDDEN_EFFECT.description");
            return;
        }

        if (!ErrorRelicsConfig.ShowFullEffects && __instance is ErrorGeneratedRelic)
        {
            __result = new LocString("static_hover_tips", "ERRORRELICS-HIDDEN_EFFECT.description");
            return;
        }
    }
}

// Titles must remain visible when effect descriptions are hidden, and reading a
// title (including a canonical history entry) must not evaluate a description.
[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Title), MethodType.Getter)]
public static class ErrorRelicTitlePatch
{
    [HarmonyPostfix]
    public static void Postfix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is ErrorRandomTestRelic error)
            __result.Add("ErrorTitle", ErrorGlitchName.For(error));
    }
}
