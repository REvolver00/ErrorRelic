using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting;

[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Generate))]
public static class AddGiftRelicOptionPatch
{
    [HarmonyPostfix]
    public static void Postfix(Player player, List<RestSiteOption> __result)
    {
        // Keep option indices identical on all peers: in co-op every player's list gets
        // the option, and IsEnabled decides whether it is currently usable.
        if (player.RunState.Players.Count > 1)
            __result.Add(new GiftRelicRestSiteOption(player));
    }
}

[HarmonyPatch(typeof(RestSiteSynchronizer), nameof(RestSiteSynchronizer.BeginRestSite))]
public static class InitGiftRelicSynchronizerPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        var net = RunManager.Instance.NetService;
        var state = RunManager.Instance.DebugOnlyGetState();
        if (net == null || state == null || state.Players.Count <= 1 || !LocalContext.NetId.HasValue) return;
        var existing = GiftRelicSynchronizer.Instance;
        if (existing != null)
        {
            if (existing.IsUsingService(net)) return;
            existing.Dispose();
        }
        new GiftRelicSynchronizer(net, state, LocalContext.NetId.Value);
    }
}
