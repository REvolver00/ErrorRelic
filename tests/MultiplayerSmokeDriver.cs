using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Relics;
using ErrorRelics.ErrorRelicsCode.Fragments;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

// This assembly is a separate test-only mod. Never ship it with ErrorRelics.
[ModInitializer(nameof(Initialize))]
public static class MultiplayerSmokeDriver
{
    private static Godot.Timer? _timer;
    private static MegaCrit.Sts2.Core.Entities.Relics.RelicRarity _chestRarity;
    private static bool _busy, _ready, _openedChest, _single;
    private static int _stage, _ticks, _initialGold, _initialMaxHp;

    public static void Initialize()
    {
        _single = CommandLineHelper.HasArg("errorrelic-single-smoke");
        if (!_single && !CommandLineHelper.HasArg("errorrelic-smoke")) return;
        // Isolate the chest conversion/ownership test from effects that need UI.
        // The production generator is checked independently on both live peers.
        new Harmony("ErrorRelics.SmokeFixture").Patch(AccessTools.Method(typeof(ErrorGenerator), nameof(ErrorGenerator.Generate),
            new[] { typeof(MegaCrit.Sts2.Core.Entities.Players.Player), typeof(ModelId), typeof(string), typeof(string) }),
            prefix: new HarmonyMethod(typeof(MultiplayerSmokeDriver), nameof(ChestFixture)));
        if (_single)
            new Harmony("ErrorRelics.SingleSmokeFixture").Patch(AccessTools.Method(typeof(ErrorGenerator), nameof(ErrorGenerator.Generate), Type.EmptyTypes),
                prefix: new HarmonyMethod(typeof(MultiplayerSmokeDriver), nameof(SingleChestFixture)));
        _timer = new Godot.Timer { WaitTime = 0.5, Autostart = true };
        _timer.Timeout += Tick;
        ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, _timer);
    }

    private static async void Tick()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            if (++_ticks > 240) throw new Exception($"Smoke test timed out in stage {_stage}");
            await Drive();
        }
        catch (Exception e)
        {
            GD.Print("ERROR_MP_SMOKE_FAIL " + e);
            _timer?.Stop();
        }
        finally { _busy = false; }
    }

    private static StartRunLobby? FindLobby(Node node)
    {
        foreach (var field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (field.GetValue(node) is StartRunLobby lobby) return lobby;
        foreach (Node child in node.GetChildren())
        {
            var found = FindLobby(child);
            if (found != null) return found;
        }
        return null;
    }

    private static T? FindNode<T>(Node node) where T : Node
    {
        if (node is T found) return found;
        foreach (Node child in node.GetChildren())
        {
            var candidate = FindNode<T>(child);
            if (candidate != null) return candidate;
        }
        return null;
    }

    private static bool SingleChestFixture(ref ErrorDefinition __result)
    {
        __result = new ErrorDefinition(ErrorHookId.H061_FirstTurnAfterPlayerTurnStartRoyalPoison, ErrorEffectId.E033_GainMaxHp20);
        return false;
    }

    private static bool ChestFixture(string sourceKey, ref ErrorDefinition __result)
    {
        if (sourceKey != "treasure") return true;
        __result = _single
            ? new ErrorDefinition(ErrorHookId.H061_FirstTurnAfterPlayerTurnStartRoyalPoison, ErrorEffectId.E033_GainMaxHp20)
            : new ErrorDefinition(ErrorHookId.H002_PlayerTurnStart, ErrorEffectId.E001_GainStrength1);
        return false;
    }

    private static async Task Drive()
    {
        if (!_ready && _single)
        {
            var game = MegaCrit.Sts2.Core.Nodes.NGame.Instance;
            if (game?.MainMenu == null || !game.MainMenu.IsNodeReady()) return;
            _ready = true;
            await game.StartNewSingleplayerRun(ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>(), false,
                new ActModel[] { ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Overgrowth>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Hive>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Glory>() },
                Array.Empty<ModifierModel>(), "ERRORSP", MegaCrit.Sts2.Core.Runs.GameMode.Standard);
            GD.Print($"ERROR_SP_STARTED mode={RunManager.Instance.NetService.Type}");
            return;
        }
        if (!_ready)
        {
            var lobby = FindLobby(((SceneTree)Engine.GetMainLoop()).Root);
            if (lobby == null || lobby.Players.Count != (_single ? 1 : 2)) return;
            _ready = true;
            lobby.SetReady(true);
            GD.Print("ERROR_MP_SMOKE_READY");
            return;
        }
        var manager = RunManager.Instance;
        var run = manager.DebugOnlyGetState();
        if (run == null || run.CurrentRoomCount == 0) return;
        var me = LocalContext.GetMe(run);
        if (me == null) return;
        if (_stage == 0)
        {
            if (CombatManager.Instance.IsInProgress) return;
            foreach (string ftue in new[] { "combat_rules_ftue", "obtain_relic_ftue", "map_select_ftue", "shuffle_ftue" })
                MegaCrit.Sts2.Core.Saves.SaveManager.Instance.MarkFtueAsComplete(ftue);
            await RelicCmd.Obtain(ErrorModeState.CreateProof(), me);
            var error = ErrorModeState.CreateMutableForRarity(MegaCrit.Sts2.Core.Entities.Relics.RelicRarity.Common);
            error.GeneratedHookId = ErrorHookId.H002_PlayerTurnStart;
            error.GeneratedEffectId = me.NetId == 1 ? ErrorEffectId.E004_GainGold300 : ErrorEffectId.E033_GainMaxHp20;
            error.DefinitionLocked = true;
            error.SetVisualSource(ModelDb.Relic<Vajra>());
            await RelicCmd.Obtain(error, me);
            var second = ErrorModeState.CreateMutableForRarity(MegaCrit.Sts2.Core.Entities.Relics.RelicRarity.Common);
            second.GeneratedHookId = ErrorHookId.H003_PlayCardEnergy2Plus;
            second.GeneratedEffectId = ErrorEffectId.E026_GainDexterity1;
            second.DefinitionLocked = true;
            await RelicCmd.Obtain(second, me);
            string description = error.DynamicDescription.GetFormattedText();
            if (description != error.FullDescription || description == second.DynamicDescription.GetFormattedText())
                throw new Exception("Native formatted tooltip mismatch");
            GD.Print($"ERROR_MP_DESCRIPTION peer={me.NetId} {description}");
            _initialGold = me.Gold;
            _initialMaxHp = me.Creature.MaxHp;
            _stage = 1;
            return;
        }
        if (_stage == 5)
        {
            _stage = 6;
            await manager.EnterRoomDebug(RoomType.Treasure, showTransition: false);
            return;
        }
        if (_stage == 6)
        {
            var roomNode = FindNode<MegaCrit.Sts2.Core.Nodes.Rooms.NTreasureRoom>(((SceneTree)Engine.GetMainLoop()).Root);
            if (roomNode == null || !roomNode.IsNodeReady()) return;
            if (!_openedChest)
            {
                _openedChest = true;
                AccessTools.Method(roomNode.GetType(), "OnChestButtonReleased").Invoke(roomNode, new object?[] { null });
                return;
            }
            if (!(bool)AccessTools.Field(roomNode.GetType(), "_hasChestBeenOpened").GetValue(roomNode)!) return;
            var choices = manager.TreasureRoomRelicSynchronizer.CurrentRelics;
            if (choices == null || choices.Count < (_single ? 1 : 2)) return;
            int index = me.NetId == 1 ? 0 : 1;
            _chestRarity = choices[index].Rarity;
            _stage = 7;
            if (_single)
            {
                var collection = FindNode<MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic.NTreasureRoomRelicCollection>(((SceneTree)Engine.GetMainLoop()).Root)!;
                var holder = collection.SingleplayerRelicHolder;
                GD.Print($"ERROR_SP_CLICK index={holder.Index} model={holder.Relic.Model.Id}");
                holder.EmitSignal(MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.SignalName.Released, holder);
            }
            else manager.TreasureRoomRelicSynchronizer.PickRelicLocally(index);
            return;
        }
        if (_stage == 7)
        {
            var treasure = FindNode<MegaCrit.Sts2.Core.Nodes.Rooms.NTreasureRoom>(((SceneTree)Engine.GetMainLoop()).Root);
            if (treasure == null || (bool)AccessTools.Field(treasure.GetType(), "_isRelicCollectionOpen").GetValue(treasure)!) return;
            var errors = me.Relics.OfType<ErrorRandomTestRelic>().ToList();
            if (errors.Count != 3) return;
            if (errors[2].Owner != me || errors[2].Rarity != _chestRarity
                || errors[2].GeneratedHookId != (_single ? ErrorHookId.H061_FirstTurnAfterPlayerTurnStartRoyalPoison : ErrorHookId.H002_PlayerTurnStart)
                || errors[2].GeneratedEffectId != (_single ? ErrorEffectId.E033_GainMaxHp20 : ErrorEffectId.E001_GainStrength1))
                throw new Exception("Treasure award lost owner, rarity or locked pair");
            GD.Print($"ERROR_MP_CHEST_PASS peer={me.NetId} rarity={errors[2].Rarity}");
            _initialGold = me.Gold - (me.NetId == 1 ? 600 : 0);
            _stage = 8;
            return;
        }
        if (_stage == 1 || _stage == 3 || _stage == 8)
        {
            int nextStage = _stage + 1;
            var encounter = ModelDb.AllEncounters.Where(e => e.RoomType == RoomType.Monster)
                .OrderBy(e => e.Id.Entry, StringComparer.Ordinal)
                .First(e => e.GetType().Name.Contains("Slime", StringComparison.OrdinalIgnoreCase));
            _stage = nextStage;
            GD.Print($"ERROR_MP_ENTER peer={me.NetId} encounter={encounter.Id} stage={_stage}");
            await manager.EnterRoomDebug(RoomType.Monster, model: encounter.ToMutable(), showTransition: false);
            return;
        }
        if (!CombatManager.Instance.IsInProgress || me.PlayerCombatState?.TurnNumber != 1
            || me.PlayerCombatState.Phase.ToString() != "Play") return;
        int combats = _stage == 2 ? 1 : _stage == 4 ? 2 : 3;
        int expectedGold = _initialGold + (me.NetId == 1 ? 300 * combats : 0);
        int expectedMaxHp = _initialMaxHp + (me.NetId == 1 ? 0 : 20 * combats) + (_single && combats == 3 ? 20 : 0);
        if (me.Gold != expectedGold || me.Creature.MaxHp != expectedMaxHp) return;
        foreach (var player in run.Players)
        {
            var relics = player.Relics.OfType<ErrorRandomTestRelic>().ToList();
            if (relics.Count != (combats == 3 ? 3 : 2) || relics.Any(r => r.Owner != player || !r.DefinitionLocked))
                throw new Exception("Remote inventory reconstruction lost relic data or ownership");
            var expectedEffect = player.NetId == 1 ? ErrorEffectId.E004_GainGold300 : ErrorEffectId.E033_GainMaxHp20;
            if (relics[0].GeneratedEffectId != expectedEffect) throw new Exception("Remote H/E mismatch");
        }
        GD.Print($"ERROR_MP_COMBAT_PASS peer={me.NetId} combat={combats} gold={me.Gold} maxHp={me.Creature.MaxHp}");
        if (_stage == 2) _stage = 3;
        else if (_stage == 4) _stage = 5;
        else
        {
            if (!_single && me.Creature.Powers.OfType<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>().Sum(p => p.Amount) != 1)
                throw new Exception($"Awarded chest relic must grant Strength exactly once; actual={me.Creature.Powers.OfType<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>().Sum(p => p.Amount)}");
            foreach (var player in run.Players)
            {
                var generated = ErrorGenerator.Generate(player, ModelDb.Relic<Vajra>().Id, "smoke-consistency");
                GD.Print($"ERROR_MP_GENERATION peer={me.NetId} owner={player.NetId} {generated.HookId}|{generated.EffectId}");
                foreach (var proof in player.Relics.OfType<ErrorProofRelic>().ToList())
                    player.RemoveRelicInternal(proof, silent: true);
                if (!ErrorModeState.IsEnabled(player)) throw new Exception("Removing Proof deactivated the multiplayer run");
                for (int i = 0; i < 8; i++)
                {
                    var fresh = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), player, "mp-circlet");
                    GD.Print($"ERROR_MP_CIRCLET peer={me.NetId} owner={player.NetId} identity={fresh.GenerationIdentity} pair={fresh.GeneratedHookId}/{fresh.GeneratedEffectId}");
                }
            }
            GD.Print($"ERROR_MP_SMOKE_PASS peer={me.NetId}");
            _timer?.Stop();
        }
    }
}
