using System;
using System.Linq;
using BaseLib.Patches.Saves;
using BaseLib.Utils;
using ErrorRelics.ErrorRelicsCode.Relics;
using ErrorRelics.ErrorRelicsCode.Config;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;

// State belongs to the run/player, never to the removable Proof object.
public static class ErrorRunState
{
    private static readonly SpireField<IRunState, bool> Activated = new(() => false);
    private static readonly SpireField<Player, int> GenerationCount = new(() => 0);
    private static bool _registered;

    public static void RegisterSaves()
    {
        if (_registered) return;
        _registered = true;
        ExtendedSaveTypes.RegisterSavedValue<IRunState, bool>(
            "ErrorRelics.Activated", IsActivated,
            (run, value) => { if (value) Activated.Set(run, true); },
            (value, writer) => writer.WriteBool(value), reader => reader.ReadBool());
        ExtendedSaveTypes.RegisterSavedValue<Player, int>(
            "ErrorRelics.GenerationCount", GenerationCount.Get, GenerationCount.Set,
            (value, writer) => writer.WriteInt(value), reader => reader.ReadInt());
        ExtendedSaveTypes.RegisterSavedValue<IRunState, int>(
            "ErrorRelics.GenerationFilters", ErrorGenerationSettings.GetSavedFilters, ErrorGenerationSettings.SetSavedFilters,
            (value, writer) => writer.WriteInt(value), reader => reader.ReadInt());
        // Extra combat rewards may already be populated when their room is saved.
        // Preserve the whole chosen model rather than assigning a new identity on load.
        ExtendedSaveTypes.RegisterSavedValue<RelicReward, SerializableRelic>(
            "ErrorRelics.GeneratedReward",
            reward => reward.Relic is ErrorRandomTestRelic error ? error.ToSerializable() : null,
            (reward, saved) =>
            {
                if (saved != null)
                    AccessTools.Field(typeof(RelicReward), "_relic").SetValue(reward, RelicModel.FromSerializable(saved));
            },
            (value, writer) => value.Serialize(writer),
            reader => { var value = new SerializableRelic(); value.Deserialize(reader); return value; });
    }

    public static bool IsActivated(IRunState run)
    {
        if (run is NullRunState) return false;
        if (Activated.Get(run)) return true;
        // Backfill old saves and mods which bypass the ordinary obtain command.
        // Evaluated only at semantic use/save boundaries, never every frame.
        if (!run.Players.Any(p => p.Relics.Any(r => r is ErrorProofRelic))) return false;
        Activated.Set(run, true);
        return true;
    }

    public static void ObserveInventory(Player player, RelicModel relic)
    {
        if (relic is ErrorProofRelic && player.RunState is RunState run)
            Activated.Set(run, true);
        if (relic is ErrorRandomTestRelic error && error.GenerationSequence > GenerationCount.Get(player))
            GenerationCount.Set(player, error.GenerationSequence);
    }

    public static void AssignIdentity(ErrorRandomTestRelic relic, Player player, string sourceKey)
    {
        if (relic.GenerationIdentity.Length != 0) return;
        if (sourceKey == "merchant")
        {
            // FillSlot has already rolled this item's price using Shops. Reading
            // its saved counter costs no RNG. Restocks can be local-only, so they
            // must not advance the shared event/reward generation sequence.
            relic.GenerationIdentity = FormattableString.Invariant(
                $"shop:{player.RunState.TotalFloor}:{player.PlayerRng.Shops.ToSerializable().counter}");
        }
        else
        {
            int next = checked(GenerationCount.Get(player) + 1);
            GenerationCount.Set(player, next);
            relic.GenerationSequence = next;
            relic.GenerationIdentity = FormattableString.Invariant($"generated:{next}");
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.AddRelicInternal))]
internal static class ErrorInventoryStatePatch
{
    [HarmonyPostfix]
    private static void Postfix(Player __instance, RelicModel relic)
        => ErrorRunState.ObserveInventory(__instance, relic);
}

[HarmonyPatch(typeof(RunState), "CreateShared")]
internal static class ErrorRunInventoryBackfillPatch
{
    [HarmonyPostfix]
    private static void Postfix(RunState __result) => ErrorRunState.IsActivated(__result);
}
