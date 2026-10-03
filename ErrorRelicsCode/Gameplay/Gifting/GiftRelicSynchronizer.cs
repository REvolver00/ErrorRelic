using System;
using System.Threading.Tasks;
using ErrorRelics.ErrorRelicsCode.Gameplay.Gifting.Messages;
using ErrorRelics.ErrorRelicsCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting;

public sealed class GiftRelicSynchronizer : IDisposable
{
    public static GiftRelicSynchronizer? Instance { get; private set; }
    private readonly INetGameService _netService;
    private readonly IPlayerCollection _players;
    private readonly ulong _localPlayerId;

    public bool IsUsingService(INetGameService service) => ReferenceEquals(_netService, service);

    public GiftRelicSynchronizer(INetGameService netService, IPlayerCollection players, ulong localPlayerId)
    {
        _netService = netService;
        _players = players;
        _localPlayerId = localPlayerId;
        _netService.RegisterMessageHandler<GiftRelicMessage>(HandleGift);
        Instance = this;
        MainFile.Logger.Info("GiftRelicSynchronizer initialized");
    }

    public void Dispose()
    {
        try { _netService.UnregisterMessageHandler<GiftRelicMessage>(HandleGift); } catch { }
        if (Instance == this) Instance = null;
    }

    public async Task<bool> SendGift(Player source, int sourceRelicIndex, Player target)
    {
        if (source.NetId != _localPlayerId || source.NetId == target.NetId) return false;
        if (!CanGiftIndex(source, sourceRelicIndex)) return false;

        // Apply on the sender first, then broadcast the compact deterministic instruction.
        // Every machine has the same source inventory at this point, so the index identifies
        // the same relic. The relic is serialized locally before removal, preserving all
        // SavedProperty state (ERROR H/E identity and discovery state included).
        bool ok = await ApplyGift(source.NetId, target.NetId, sourceRelicIndex);
        if (!ok) return false;

        _netService.SendMessage(new GiftRelicMessage
        {
            TargetPlayerId = target.NetId,
            SourceRelicIndex = sourceRelicIndex
        });
        return true;
    }

    private void HandleGift(GiftRelicMessage message, ulong senderId)
    {
        if (senderId == _localPlayerId) return; // sender already applied it
        _ = ApplyGift(senderId, message.TargetPlayerId, message.SourceRelicIndex);
    }

    private async Task<bool> ApplyGift(ulong sourceId, ulong targetId, int index)
    {
        try
        {
            Player? source = _players.GetPlayer(sourceId);
            Player? target = _players.GetPlayer(targetId);
            if (source == null || target == null || source == target || !CanGiftIndex(source, index))
                return false;

            RelicModel relic = source.Relics[index];
            var serialized = relic.ToSerializable();
            MainFile.Logger.Info($"Gift relic: {relic.Id.Entry} {sourceId} -> {targetId}");

            await RelicCmd.Remove(relic);

            // Reconstruct the exact relic state, then use the real obtain command. This is
            // intentional: AfterObtained must fire on the recipient, including ERROR pickup
            // presentation and any obtain-time H/E behavior.
            RelicModel fresh = RelicModel.FromSerializable(serialized);
            await RelicCmd.Obtain(fresh, target);
            return true;
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Gift relic failed: {e}");
            return false;
        }
    }

    public static bool CanGiftIndex(Player source, int index)
    {
        if (index < 0 || index >= source.Relics.Count) return false;
        // Proof owns ERROR activation/discovery charges and is intentionally non-transferable.
        return source.Relics[index] is not ErrorProofRelic;
    }
}
