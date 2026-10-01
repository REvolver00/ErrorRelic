using System.Reflection;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

[ModInitializer(nameof(Initialize))]
public static class CombatDuplicateSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy, _started, _multiplayer, _selected;
    private static int _ticks;
    private static ulong _ownerId;
    private static Task? _enter;
    private static CardModel[] _originals = Array.Empty<CardModel>();
    private static ErrorHookId _hook;

    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-combat-duplicate-smoke")) return;
        _hook = CommandLineHelper.HasArg("duplicate-before") ? ErrorHookId.H048_BeforeCombatStartAnchor
            : CommandLineHelper.HasArg("duplicate-turn") ? ErrorHookId.H007_FirstTurnBeforeHandDraw
            : ErrorHookId.H001_EnterCombat;
        _multiplayer = CommandLineHelper.HasArg("duplicate-mp");
        _ownerId = CommandLineHelper.HasArg("duplicate-owner-client") ? 1001UL : 1UL;
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

    // Remote cards are reconstructed at combat entry; compare their contents
    // rather than the pre-entry object references.
    private static string CardSignature(CardModel card)
        => $"{card.Id}:{card.CurrentUpgradeLevel}:{card.Enchantment?.Id}:{card.Enchantment?.Amount}";

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
            if (_enter != null && ++_ticks > 240) throw new Exception("Combat duplicate smoke timed out");
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
                    Array.Empty<ModifierModel>(), "ERRORDUPLICATE", GameMode.Standard);
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
                relic.GeneratedHookId = _hook;
                relic.GeneratedEffectId = ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard;
                relic.DefinitionLocked = true;
                owner.AddRelicInternal(relic, silent: true);
                _originals = owner.Deck.Cards.ToArray();
                GD.Print($"ERROR_COMBAT_DUPLICATE_BEGIN peer={me.NetId} owner={owner.NetId}");
                _enter = EnterCombat();
                return;
            }
            if (_enter.IsFaulted) await _enter;
            var screen = Find<NDeckCardSelectScreen>(root);
            if (!_selected && screen?.IsNodeReady() == true && screen.IsVisibleInTree())
            {
                var transition = NGame.Instance.Transition;
                var blackout = (Control)AccessTools.Field(typeof(NTransition), "_simpleTransition").GetValue(transition)!;
                // Combat-loop hooks can show the overlay during the normal
                // fade-in. Allow that independent transition to finish.
                if (_hook != ErrorHookId.H001_EnterCombat
                    && (transition.InTransition || blackout.Modulate.A > 0.01f)) return;
                GD.Print($"ERROR_COMBAT_DUPLICATE_SCREEN peer={me.NetId} transition={transition.InTransition} alpha={blackout.Modulate.A} filter={transition.MouseFilter}");
                if (transition.InTransition || blackout.Modulate.A > 0.01f || transition.MouseFilter != Control.MouseFilterEnum.Ignore)
                    throw new Exception("Deck selection is covered by the room transition");
                if (me.NetId != _ownerId) throw new Exception("Selection must only appear on the owner's peer");
                var card = me.Deck.Cards.First(card => card.Type != CardType.Quest);
                AccessTools.Method(screen.GetType(), "OnCardClicked").Invoke(screen, new object[] { card });
                AccessTools.Method(screen.GetType(), "ConfirmSelection").Invoke(screen, new object?[] { null });
                _selected = true;
            }
            if (!_enter.IsCompleted || !CombatManager.Instance.IsInProgress || me.PlayerCombatState?.Phase.ToString() != "Play") return;
            await _enter;
            var currentOwner = _multiplayer ? run.Players.Single(player => player.NetId == _ownerId) : me;
            var executedRelic = currentOwner.Relics.OfType<ErrorRandomTestRelic>()
                .Single(relic => relic.GeneratedHookId == _hook
                    && relic.GeneratedEffectId == ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard);
            if (executedRelic.EffectExecutions != 1 || (currentOwner == me) != _selected)
                throw new Exception("Expected one effect and selection only on the owner's peer");
            var copy = currentOwner.Deck.Cards.Last();
            var original = _originals.First(card => card.Type != CardType.Quest);
            if (currentOwner.Deck.Cards.Count != _originals.Length + 1 || !currentOwner.Deck.Cards.Take(_originals.Length).Select(CardSignature).SequenceEqual(_originals.Select(CardSignature))
                || copy.Id != original.Id || CardSignature(copy) != CardSignature(original) || copy.Owner != currentOwner)
                throw new Exception("Expected one permanent copy with the original card preserved");
            GD.Print($"ERROR_COMBAT_DUPLICATE_PASS peer={me.NetId} owner={currentOwner.NetId} hook={_hook} executions={executedRelic.EffectExecutions} deck={currentOwner.Deck.Cards.Count} copy={copy.Id}");
            _timer!.Stop();
        }
        catch (Exception exception)
        {
            GD.Print("ERROR_COMBAT_DUPLICATE_FAIL " + exception);
            _timer!.Stop();
        }
        finally { _busy = false; }
    }
}
