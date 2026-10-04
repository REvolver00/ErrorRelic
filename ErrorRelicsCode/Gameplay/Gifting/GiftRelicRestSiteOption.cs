using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting;

public sealed class GiftRelicRestSiteOption : RestSiteOption
{
    public override string OptionId => "ERRORRELICS_GIFT_RELIC";
    public override bool IsEnabled => Owner.RunState.Players.Count > 1 && HasGiftableRelic();
    public override LocString Description => new("rest_site_ui", IsEnabled
        ? "ERRORRELICS_OPTION_GIFT_RELIC.description"
        : "ERRORRELICS_OPTION_GIFT_RELIC.descriptionDisabled");

    public GiftRelicRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(Owner);
        if (!LocalContext.IsMe(Owner))
        {
            var remote = await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(Owner, choiceId);
            // The actual inventory mutation arrives through GiftRelicMessage. This result only
            // keeps the native rest-site choice counters symmetrical on every machine.
            // The owner machine is responsible for ending its own rest-site action.
            // Remote peers only mirror the successful option result; completion is
            // propagated by the native RestSiteSynchronizer.
            return remote.AsPlayerId().HasValue;
        }

        int? relicIndex = await NGiftRelicPicker.Pick(Owner);
        if (!relicIndex.HasValue)
        {
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromPlayerId(null));
            return false;
        }

        Player? target = await SelectTarget();
        if (target == null)
        {
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromPlayerId(null));
            return false;
        }

        RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromPlayerId(target.NetId));
        var sync = GiftRelicSynchronizer.Instance;
        if (sync == null) return false;
        bool succeeded = await sync.SendGift(Owner, relicIndex.Value, target);
        if (!succeeded) return false;

        // Gift is a real campfire action, not an extra menu. End the local player's
        // rest-site action through the game's native completion path, exactly the path
        // used when leaving a rest site with options remaining. This clears the local
        // options and synchronizes completion to every peer, so Gift cannot be followed
        // by another Gift/Smith/Rest in the same campfire visit.
        RunManager.Instance.RestSiteSynchronizer.BeforeLocalRestSiteExited();
        return true;
    }

    private bool HasGiftableRelic()
    {
        for (int i = 0; i < Owner.Relics.Count; i++)
            if (GiftRelicSynchronizer.CanGiftIndex(Owner, i)) return true;
        return false;
    }

    private async Task<Player?> SelectTarget()
    {
        var room = NRestSiteRoom.Instance;
        if (room == null) return null;
        var button = room.GetButtonForOption(this);
        if (button == null) return null;
        Vector2 start = button.GlobalPosition + button.Size / 2f;
        NTargetManager.Instance.StartTargeting(TargetType.AnyPlayer, start, TargetMode.ClickMouseToTarget,
            ShouldCancelTargeting, AllowTargetingNode);
        try { return NodeToPlayer(await NTargetManager.Instance.SelectionFinished()); }
        catch { return null; }
    }

    private static Player? NodeToPlayer(Node? node) => node is NRestSiteCharacter c ? c.Player : null;
    private static bool ShouldCancelTargeting() => NOverlayStack.Instance.ScreenCount > 0 || NCapstoneContainer.Instance.InUse;
    private static bool AllowTargetingNode(Node node)
    {
        Player? p = NodeToPlayer(node);
        return p != null && !LocalContext.IsMe(p);
    }
}
