using ErrorRelics.ErrorRelicsCode.Config;
using System.Collections.Generic;
using System.Reflection;
using ErrorRelics.ErrorRelicsCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.Visuals;

[HarmonyPatch]
public static class ErrorRelicDescriptionPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.Title));
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicDescription));
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicEventDescription));
        yield return AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.Flavor));
    }

    [HarmonyPostfix]
    public static void Postfix(RelicModel __instance, ref LocString __result)
    {
        // This is a local presentation preference. Never change the definition,
        // saved properties, RNG or actual hook/effect execution when toggled.
        if (!ErrorRelicsConfig.ShowFullEffects && __instance is ErrorGeneratedRelic)
        {
            __result = new LocString("static_hover_tips", "ERRORRELICS-HIDDEN_EFFECT.description");
            return;
        }
        if (__instance is ErrorRandomTestRelic error)
        {
            __result.Add("ErrorTitle", ErrorGlitchName.For(error));
            __result.Add("ErrorDescription", error.FullDescription);
        }
    }
}
