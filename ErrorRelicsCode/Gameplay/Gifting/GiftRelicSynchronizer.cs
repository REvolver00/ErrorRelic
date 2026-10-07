using System.Threading.Tasks;
using ErrorRelics.ErrorRelicsCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting;

// The native PlayerChoiceSynchronizer carries the selection. Both peers execute
// and await this transfer within their own copy of the same rest-site action.
public static class GiftRelicSynchronizer
{
    public static async Task<bool> ApplyGift(Player source, Player target, int index)
    {
        if (source == target || !CanGiftIndex(source, index)) return false;

        RelicModel relic = source.Relics[index];
        // Preserve SavedProperty state (including ERROR H/E and discovery state).
        // Reconstruct before removal so a serialization failure cannot lose a relic.
        RelicModel fresh = RelicModel.FromSerializable(relic.ToSerializable());
        MainFile.Logger.Info($"Gift relic: {relic.Id.Entry} {source.NetId} -> {target.NetId}");
        await RelicCmd.Remove(relic);
        // AfterObtained, including any nested player choice, must finish before
        // this action returns success and the native rest-site lifecycle continues.
        await RelicCmd.Obtain(fresh, target);
        return true;
    }

    public static bool CanGiftIndex(Player source, int index)
    {
        if (index < 0 || index >= source.Relics.Count) return false;
        // Proof owns ERROR activation/discovery charges and is non-transferable.
        return source.Relics[index] is not ErrorProofRelic;
    }
}
