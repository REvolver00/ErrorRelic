using ErrorRelics.ErrorRelicsCode.Config;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Context;

namespace ErrorRelics.ErrorRelicsCode.Gameplay;

/// <summary>
/// Optional anti-save-load gameplay inspired by the merged TMTRAINER Data Crack:
/// new ERROR effects are hidden, the Proof gains one charge at each combat start
/// (max four), and right-clicking an unrevealed ERROR spends one Proof charge to
/// reveal that relic permanently. No gameplay H/E data is removed or rerolled.
/// </summary>
public static class ErrorDataCrackService
{
    public static bool IsDiscoveryActive =>
        !ErrorRelicsConfig.ShowFullEffects && ErrorRelicsConfig.DataCrackRevealMode;

    public static bool TryReveal(ErrorRandomTestRelic relic)
    {
        if (!IsDiscoveryActive || (relic.EffectRevealed && relic.HookRevealed))
            return false;

        var owner = relic.Owner;
        if (owner == null || owner.NetId != LocalContext.NetId || !owner.Relics.Contains(relic))
            return false;

        ErrorProofRelic? proof = owner.Relics.OfType<ErrorProofRelic>().FirstOrDefault();
        if (proof == null || !proof.TrySpendDataCrackCharge())
            return false;

        string part;
        if (!relic.EffectRevealed)
        {
            relic.RevealEffect();
            part = "E";
        }
        else
        {
            relic.RevealHook();
            part = "H";
        }

        MainFile.Logger.Info($"ERROR discovery reveal: owner={owner.NetId}, part={part}, pair={relic.GeneratedHookId}/{relic.GeneratedEffectId}, charge={proof.DataCrackCharge}");
        return true;
    }
}

/// <summary>
/// Same low-impact interaction surface used by TMTRAINER: right-click the relic
/// icon. Left-click/hover behavior is left to vanilla.
/// </summary>
[HarmonyPatch(typeof(NRelicInventoryHolder), "_Ready")]
public static class ErrorDataCrackRightClickPatch
{
    [HarmonyPostfix]
    private static void Postfix(NRelicInventoryHolder __instance)
    {
        __instance.MousePressed += inputEvent =>
        {
            if (inputEvent is not InputEventMouseButton mouse
                || !mouse.Pressed
                || mouse.ButtonIndex != MouseButton.Right)
                return;

            if (__instance.Relic?.Model is ErrorRandomTestRelic relic)
                ErrorDataCrackService.TryReveal(relic);
        };
    }
}
