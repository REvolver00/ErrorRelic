using ErrorRelics.ErrorRelicsCode.Config;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Runs;

[ModInitializer(nameof(Initialize))]
public static class NeowSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy, _started, _off;
    private static ErrorHookId _hook = ErrorHookId.H004_AfterObtained;
    private static int _ticks;
    private static Task? _obtain;
    private static CardModel[]? _selected;

    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-neow-smoke")) return;
        if (CommandLineHelper.HasArg("e102-before-combat")) _hook = ErrorHookId.H048_BeforeCombatStartAnchor;
        if (CommandLineHelper.HasArg("e102-enter-combat")) _hook = ErrorHookId.H001_EnterCombat;
        if (CommandLineHelper.HasArg("e102-turn-start")) _hook = ErrorHookId.H002_PlayerTurnStart;
        _off = CommandLineHelper.HasArg("pickaxe-off");
        ErrorRelicsConfig.StartWithRedPickaxe = !_off;
        _timer = new Godot.Timer { WaitTime = 0.5, Autostart = true };
        _timer.Timeout += Tick;
        ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, _timer);
    }

    private static void Check(bool ok, string reason)
    {
        if (!ok) throw new Exception(reason);
    }

    private static T? Find<T>(Node node) where T : Node
    {
        if (node is T found) return found;
        foreach (Node child in node.GetChildren())
        {
            var result = Find<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private static async void Tick()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            if (++_ticks > 120) throw new Exception("Neow smoke timed out");
            if (!_started)
            {
                var game = NGame.Instance;
                if (game?.MainMenu == null || !game.MainMenu.IsNodeReady()) return;
                _started = true;
                await game.StartNewSingleplayerRun(ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>(), false,
                    new ActModel[] { ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Overgrowth>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Hive>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Glory>() },
                    Array.Empty<ModifierModel>(), "ERRORNEOW", GameMode.Standard);
                return;
            }
            var run = RunManager.Instance.DebugOnlyGetState();
            if (run == null || run.CurrentRoomCount == 0) return;
            var me = LocalContext.GetMe(run)!;
            if (_obtain == null)
            {
                foreach (string ftue in new[] { "obtain_relic_ftue", "map_select_ftue" })
                    MegaCrit.Sts2.Core.Saves.SaveManager.Instance.MarkFtueAsComplete(ftue);
                var neow = (Neow)ModelDb.Event<Neow>().ToMutable();
                // Use a noninteractive first reward so the test can proceed to E102 UI.
                if (!_off) Traverse.Create(neow).Property("DebugOption").SetValue("GOLDEN_PEARL");
                GD.Print("ERROR_NEOW_BEGIN");
                await neow.BeginEvent(me, null, false);
                GD.Print("ERROR_NEOW_BEGUN");
                Check(me.Relics.Count(r => r is ErrorProofRelic) == (_off ? 0 : 1), "Starting proof count");
                var options = Traverse.Create(neow).Property("GeneratedOptions").GetValue<List<MegaCrit.Sts2.Core.Events.EventOption>>();
                Check(options.Count == 3, "Neow must retain three choices");
                Check((options[0].Relic is ErrorProofRelic) == _off, "First option must follow setting");
                Check(me.Relics.Count(r => r is ErrorProofRelic) == (_off ? 0 : 1), "Proof must not be duplicated");
                if (!_off)
                {
                    var first = options[0].Relic!;
                    GD.Print("ERROR_NEOW_FIRST " + first.Id);
                    await Traverse.Create(options[0]).Property("OnChosen").GetValue<Func<Task>>()();
                    Check(me.Relics.Any(r => r.Id == first.Id), "Chosen Neow relic must not be converted");
                }
                GD.Print("ERROR_NEOW_CHOICES_PASS off=" + _off);
                var relic = ErrorModeState.CreateMutableForRarity(RelicRarity.Common);
                relic.GeneratedHookId = _hook;
                relic.GeneratedEffectId = ErrorEffectId.E102_GnarledHammerEnchantUpTo3Sharp3;
                relic.DefinitionLocked = true;
                _obtain = RelicCmd.Obtain(relic, me);
                if (_hook != ErrorHookId.H004_AfterObtained)
                {
                    await _obtain;
                    MegaCrit.Sts2.Core.Saves.SaveManager.Instance.MarkFtueAsComplete("combat_rules_ftue");
                    var encounter = ModelDb.AllEncounters.First(e => e.RoomType == MegaCrit.Sts2.Core.Rooms.RoomType.Monster
                        && e.GetType().Name.Contains("Slime", StringComparison.OrdinalIgnoreCase));
                    GD.Print("ERROR_E102_COMBAT_ENTER " + _hook);
                    _obtain = RunManager.Instance.EnterRoomDebug(MegaCrit.Sts2.Core.Rooms.RoomType.Monster,
                        model: encounter.ToMutable(), showTransition: false);
                }
                return;
            }
            if (_obtain.IsFaulted) await _obtain;
            var screen = Find<NDeckEnchantSelectScreen>(((SceneTree)Engine.GetMainLoop()).Root);
            if (_selected == null && screen?.IsNodeReady() == true)
            {
                _selected = PileType.Deck.GetPile(me).Cards.Where(c => ModelDb.Enchantment<Sharp>().CanEnchant(c)).Take(3).ToArray();
                foreach (var card in _selected)
                    AccessTools.Method(screen.GetType(), "OnCardClicked").Invoke(screen, new object[] { card });
                AccessTools.Method(screen.GetType(), "ConfirmSelection").Invoke(screen, new object?[] { null });
            }
            if (!_obtain.IsCompleted || _selected == null) return;
            await _obtain;
            Check(_selected?.Length == 3 && _selected.All(c => c.Enchantment is Sharp && c.Enchantment.Amount == 3), "E102 must enchant three cards");
            GD.Print("ERROR_NEOW_E102_SMOKE_PASS off=" + _off + " hook=" + _hook);
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit();
        }
        catch (Exception error)
        {
            GD.Print("ERROR_NEOW_E102_SMOKE_FAIL " + error);
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit(1);
        }
        finally { _busy = false; }
    }
}
