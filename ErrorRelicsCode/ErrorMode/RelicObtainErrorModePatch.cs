using System;

using ErrorRelics.ErrorRelicsCode.Relics;

using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;


[HarmonyPatch(
    typeof(RelicCmd),
    nameof(RelicCmd.Obtain),
    new Type[]
    {
        typeof(RelicModel),
        typeof(Player),
        typeof(int)
    }
)]
public static class RelicObtainErrorModePatch
{
    [HarmonyPrefix]
    public static void Prefix(
        ref RelicModel relic,
        Player player,
        int index)
    {
        // =====================================================
        // Neow 的一次性替换。
        //
        // 这是 ERROR Mode 从 OFF -> ON 的入口，
        // 所以此时 player.Relics 里还没有 ERROR。
        // =====================================================

        if (ErrorModeState.TryConsumeNeowReplacement(
                player,
                out ErrorProofRelic? proof)
            && proof != null)
        {
            PreserveOriginalGrabBagConsumption(
                relic,
                player
            );

            relic = proof;
            return;
        }


        // =====================================================
        // Reward / Treasure display mapping.
        //
        // The surrounding vanilla system still holds the original
        // relic object. Only at the final Obtain call do we swap
        // to the exact ERROR instance the player already saw.
        // =====================================================

        if (ErrorDisplayReplacementState.TryConsume(
                relic,
                out ErrorRandomTestRelic? displayedError)
            && displayedError != null)
        {
            PreserveOriginalGrabBagConsumption(
                relic,
                player
            );

            relic = displayedError;
            return;
        }


        // =====================================================
        // 已经是 ERROR / Proof：
        // 不允许再次套娃替换。
        // =====================================================

        if (ErrorModeState.IsErrorRelic(relic)
            || ErrorModeState.IsProofRelic(relic))
        {
            return;
        }


        // =====================================================
        // ERROR Mode 还没开启：
        // 正常 Run 完全不动。
        // =====================================================

        if (!ErrorModeState.IsEnabled(player))
            return;


        // =====================================================
        // 最后保险：
        //
        // 某些事件 / 特殊逻辑可能不经过 RelicFactory，
        // 直接 RelicCmd.Obtain(original, player)。
        //
        // 这种情况下也必须最终获得 ERROR。
        // =====================================================

        RelicRarity sourceRarity =
            relic.Rarity;

        PreserveOriginalGrabBagConsumption(
            relic,
            player
        );

        relic =
            ErrorModeState.CreateLockedError(
                sourceRarity
            );
    }


    // =========================================================
    // RelicCmd.Obtain 原版在获得非 Stackable relic 时会：
    //
    // player.RelicGrabBag.Remove(relic)
    // SharedRelicGrabBag.Remove(relic)
    //
    // Prefix 如果把 source 替换掉，
    // 原版之后只能看到 ERROR。
    //
    // 所以这里先替 source 做一次相同 Remove，
    // 保证直接 Obtain 的原版 relic 也不会重新回到池里。
    //
    // Factory 路径已经移除过 source；
    // 再 Remove 一次是安全 no-op。
    // =========================================================

    private static void PreserveOriginalGrabBagConsumption(
        RelicModel source,
        Player player)
    {
        if (source.IsStackable)
            return;

        player.RelicGrabBag.Remove(
            source
        );

        player.RunState
            .SharedRelicGrabBag
            .Remove(source);
    }
}
