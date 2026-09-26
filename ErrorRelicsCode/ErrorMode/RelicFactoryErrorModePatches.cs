using System;

using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


// =============================================================
// FRONT
// Vanilla source comment:
// used by most relic reward sources (elite rewards, etc.).
// =============================================================

[HarmonyPatch(
    typeof(RelicFactory),
    nameof(RelicFactory.PullNextRelicFromFront),
    new Type[]
    {
        typeof(Player),
        typeof(RelicRarity),
        typeof(Func<RelicModel, bool>)
    }
)]
public static class RelicFactoryFrontErrorPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        Player player,
        ref RelicModel __result)
    {
        ReplaceResultIfNeeded(
            player,
            ref __result
        );
    }


    internal static void ReplaceResultIfNeeded(
        Player player,
        ref RelicModel result)
    {
        if (!ErrorModeState.IsEnabled(player))
            return;

        if (ErrorModeState.IsErrorRelic(result))
            return;

        // 原版方法已经：
        // - 从 Player RelicGrabBag 移除 source
        // - 从 SharedRelicGrabBag 移除 source
        //
        // 所以这里只替换返回给 UI / Reward 的 Model。
        result =
            ErrorModeState.CreateLockedError(
                result.Rarity
            );
    }
}
