using System.Reflection;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

[ModInitializer(nameof(Initialize))]
public static class RoomEntryRewardsSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy, _started, _multiplayer, _skipped;
    private static int _ticks;
    private static ulong _ownerId;
    private static Task? _enter;

    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-room-rewards-smoke")) return;
        _multiplayer = CommandLineHelper.HasArg("rewards-mp");
        _ownerId = CommandLineHelper.HasArg("reward-owner-client") ? 1001UL : 1UL;
        _timer = new Godot.Timer { WaitTime = 0.25, Autostart = true };
        _timer.Timeout += Tick;
        ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, _timer);
    }

    private static T? Find<T>(Node node) where T : Node
    {
        if (node is T found) return found;
        foreach (Node child in node.GetChildren())
            if (Find<T>(child) is T result) return result;
        return null;
    }

    private static StartRunLobby? FindLobby(Node node)
    {
        foreach (var field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (field.GetValue(node) is StartRunLobby lobby) return lobby;
        foreach (Node child in node.GetChildren())
            if (FindLobby(child) is { } lobby) return lobby;
        return null;
    }

    private static async Task EnterCombat()
    {
        await RunManager.Instance.FadeOut();
        var encounter = ModelDb.AllEncounters
            .Where(encounter => encounter.RoomType == RoomType.Monster)
            .OrderBy(encounter => encounter.Id.Entry, StringComparer.Ordinal)
            .First(encounter => encounter.GetType().Name.Contains("Slime", StringComparison.OrdinalIgnoreCase));
        await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter.ToMutable());
    }

    private static async void Tick()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            if (_enter != null && ++_ticks > 240) throw new Exception("Room rewards smoke timed out");
            var root = ((SceneTree)Engine.GetMainLoop()).Root;
            if (!_started)
            {
                if (_multiplayer)
                {
                    var lobby = FindLobby(root);
                    if (lobby == null || lobby.Players.Count != 2) return;
                    _started = true;
                    lobby.SetReady(true);
                    return;
                }
                var game = NGame.Instance;
                if (game?.MainMenu == null || !game.MainMenu.IsNodeReady()) return;
                _started = true;
                await game.StartNewSingleplayerRun(ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>(), false,
                    new ActModel[] { ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Overgrowth>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Hive>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Glory>() },
                    Array.Empty<ModifierModel>(), "ERRORROOMREWARDS", GameMode.Standard);
                return;
            }
            var run = RunManager.Instance.DebugOnlyGetState();
            if (run == null || run.CurrentRoomCount == 0) return;
            var me = LocalContext.GetMe(run)!;
            if (_enter == null)
            {
                foreach (string ftue in new[] { "combat_rules_ftue", "obtain_relic_ftue", "map_select_ftue", "shuffle_ftue" })
                    SaveManager.Instance.MarkFtueAsComplete(ftue);
                SaveManager.Instance.PrefsSave.FastMode = FastModeType.Normal;
                var owner = _multiplayer ? run.Players.Single(player => player.NetId == _ownerId) : me;
                var relic = ErrorModeState.CreateMutableForRarity(RelicRarity.Common);
                relic.GeneratedHookId = ErrorHookId.H025_EnterCombatOddlySmoothStone;
                relic.GeneratedEffectId = ErrorEffectId.E106_OrreryOffer5CardRewards;
                relic.DefinitionLocked = true;
                owner.AddRelicInternal(relic, silent: true);
                GD.Print($"ERROR_ROOM_REWARDS_BEGIN peer={me.NetId} owner={owner.NetId}");
                _enter = EnterCombat();
                return;
            }
            if (_enter.IsFaulted) await _enter;
            var screen = Find<NRewardsScreen>(root);
            if (!_skipped && screen?.IsNodeReady() == true && screen.IsVisibleInTree())
            {
                var transition = NGame.Instance.Transition;
                var blackout = (Control)AccessTools.Field(typeof(NTransition), "_simpleTransition").GetValue(transition)!;
                GD.Print($"ERROR_ROOM_REWARDS_SCREEN peer={me.NetId} transition={transition.InTransition} alpha={blackout.Modulate.A} filter={transition.MouseFilter}");
                if (transition.InTransition || blackout.Modulate.A > 0.01f || transition.MouseFilter != Control.MouseFilterEnum.Ignore)
                    throw new Exception("Room-entry rewards are covered by the room transition");
                var rewards = (RewardsSet)AccessTools.Field(typeof(NRewardsScreen), "_rewardsSet").GetValue(screen)!;
                if (rewards.Player != me || rewards.Rewards.Count != 5 || rewards.Rewards.Any(reward => reward is not CardReward))
                    throw new Exception("Expected five card rewards for the local owner");
                _skipped = true;
                AccessTools.Method(typeof(NRewardsScreen), "OnProceedButtonPressed").Invoke(screen, new object?[] { null });
            }
            if (!_enter.IsCompleted || !CombatManager.Instance.IsInProgress || me.PlayerCombatState?.Phase.ToString() != "Play") return;
            await _enter;
            var currentOwner = _multiplayer ? run.Players.Single(player => player.NetId == _ownerId) : me;
            var executedRelic = currentOwner.Relics.OfType<ErrorRandomTestRelic>()
                .Single(relic => relic.GeneratedHookId == ErrorHookId.H025_EnterCombatOddlySmoothStone
                    && relic.GeneratedEffectId == ErrorEffectId.E106_OrreryOffer5CardRewards);
            if (executedRelic.EffectExecutions != 1 || (currentOwner == me) != _skipped)
                throw new Exception("Expected one effect and rewards only on the owner's peer");
            if (!RunManager.Instance.RunLocationTargetedBuffer.CurrentLocation.Equals(run.RunLocation))
                throw new Exception("Reward message buffer has not reached the current room");
            GD.Print($"ERROR_ROOM_REWARDS_PASS peer={me.NetId} owner={currentOwner.NetId} executions={executedRelic.EffectExecutions}");
            _timer!.Stop();
        }
        catch (Exception exception)
        {
            GD.Print("ERROR_ROOM_REWARDS_FAIL " + exception);
            _timer!.Stop();
        }
        finally { _busy = false; }
    }
}
