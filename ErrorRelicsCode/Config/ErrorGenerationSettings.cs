using System;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace ErrorRelics.ErrorRelicsCode.Config;

// The co-op pool is run state, not a peer's mutable preferences. -1 means
// a new/legacy run has not received its host's settings yet; 0..3 are snapshots.
public static class ErrorGenerationSettings
{
    private static readonly SpireField<IRunState, int> SavedFilters = new(() => -1);
    private static readonly SpireField<IRunState, Task?> Pending = new(() => null);

    public static int LocalFilters => (ErrorRelicsConfig.AllowAncientSourceEffects ? 1 : 0)
                                   | (ErrorRelicsConfig.AllowMultiplayerImpactEffects ? 2 : 0);

    public static int GetSavedFilters(IRunState run) => SavedFilters.Get(run);

    public static void SetSavedFilters(IRunState run, int value)
    {
        if (value < -1 || value > 3) throw new ArgumentOutOfRangeException(nameof(value));
        SavedFilters.Set(run, value);
    }

    public static int ForRun(IRunState run)
    {
        if (run.Players.Count <= 1) return LocalFilters;
        int value = GetSavedFilters(run);
        if (value < 0)
            throw new InvalidOperationException("ERROR generation filters have not been synchronized with the host.");
        return value;
    }

    public static Task EnsureSynchronized(IRunState run, INetGameService net, PlayerChoiceSynchronizer choices)
    {
        if (run.Players.Count <= 1 || GetSavedFilters(run) >= 0) return Task.CompletedTask;
        Task? pending = Pending.Get(run);
        if (pending != null) return pending;
        pending = Synchronize(run, net, choices);
        Pending.Set(run, pending);
        return pending;
    }

    private static async Task Synchronize(IRunState run, INetGameService net, PlayerChoiceSynchronizer choices)
    {
        // Use the actual host identity, including resumed lobbies; do not assume
        // that player slot zero is the host or overwrite either peer's config file.
        ulong hostId = net.Type == NetGameType.Host
            ? net.NetId
            : ((INetClientGameService)net).NetClient!.HostNetId;
        var host = run.GetPlayer(hostId) ?? throw new InvalidOperationException("ERROR generation host is absent from the run.");
        uint choiceId = choices.ReserveChoiceId(host);
        int filters;
        if (net.Type == NetGameType.Host)
        {
            filters = LocalFilters;
            choices.SyncLocalChoice(host, choiceId, PlayerChoiceResult.FromIndex(filters));
        }
        else
        {
            filters = (await choices.WaitForRemoteChoice(host, choiceId)).AsIndex();
        }
        SetSavedFilters(run, filters);
        MainFile.Logger.Info($"ERROR generation filters locked for run: host={hostId}, ancient={(filters & 1) != 0}, multiplayerImpact={(filters & 2) != 0}");
    }
}

// Both new and loaded runs generate a map after Launch unbuffers network
// messages, before Neow/room choices. Await settings before running map hooks.
[HarmonyPatch(typeof(RunManager), nameof(RunManager.GenerateMap))]
internal static class ErrorGenerationSettingsBeforeMapPatch
{
    [HarmonyPrefix]
    private static bool Prefix(RunManager __instance, ref Task __result)
    {
        var run = __instance.DebugOnlyGetState();
        if (run == null || run.Players.Count <= 1 || ErrorGenerationSettings.GetSavedFilters(run) >= 0)
            return true;
        __result = GenerateAfterSynchronization(__instance, run);
        return false;
    }

    private static async Task GenerateAfterSynchronization(RunManager manager, IRunState run)
    {
        await ErrorGenerationSettings.EnsureSynchronized(run, manager.NetService, manager.PlayerChoiceSynchronizer);
        // The snapshot now exists, so the prefix allows the original method.
        await manager.GenerateMap();
    }
}
