using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

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
