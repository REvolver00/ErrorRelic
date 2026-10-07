using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
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
        var choices = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = choices.ReserveChoiceId(Owner);
        PlayerChoiceResult choice;
        if (LocalContext.IsMe(Owner))
        {
            // One native choice contains the whole selection. An empty selection is
            // cancellation, including failure/teardown of the local picker.
            choice = PlayerChoiceResult.FromIndexes(new List<int>());
            try
            {
                int? relicIndex = await NGiftRelicPicker.Pick(Owner);
                if (relicIndex.HasValue && GiftRelicSynchronizer.CanGiftIndex(Owner, relicIndex.Value))
                {
                    var relic = Owner.Relics[relicIndex.Value];
                    Player? target = await SelectTarget();
                    int currentIndex = Owner.Relics.ToList().IndexOf(relic);
                    if (target != null && currentIndex >= 0)
                    {
                        choice = PlayerChoiceResult.FromIndexes(new List<int>
                        {
                            currentIndex, Owner.RunState.GetPlayerSlotIndex(target)
                        });
                    }
                }
            }
            finally
            {
                choices.SyncLocalChoice(Owner, choiceId, choice);
            }
        }
        else
        {
            choice = await choices.WaitForRemoteChoice(Owner, choiceId);
        }

        var selection = choice.AsIndexes();
        if (selection.Count != 2) return false;
        int targetSlot = selection[1];
        if (targetSlot < 0 || targetSlot >= Owner.RunState.Players.Count) return false;

        // Like Mend/Smith, every peer awaits the same commands inside OnSelect.
        // Returning success lets ChooseOption notify the UI, consume this player's
        // options and complete their rest site in the native order. Never skip/exit
        // the room here: the button still awaits ChooseOption to start its cleanup.
        return await GiftRelicSynchronizer.ApplyGift(Owner, Owner.RunState.Players[targetSlot], selection[0]);
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
