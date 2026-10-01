using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

[ModInitializer(nameof(Initialize))]
public static class UnknownTransformSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy, _started, _selected;
    private static Task? _enter;
    private static int _ticks;
    private static CardModel[]? _originals;
    private static int _deckCount;
    private static ErrorGeneratedRelic? _relic;

    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-unknown-transform-smoke")) return;
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

    private static async Task EnterUnknown(RunState run)
    {
        // Exercise the actual black room transition, not just the hook or selector.
        await RunManager.Instance.FadeOut();
        var point = run.Map.GetAllMapPoints().First(p => p.PointType == MapPointType.Unknown);
        await RunManager.Instance.EnterMapCoordDebug(point.coord, RoomType.Event,
            MapPointType.Unknown);
    }

    private static async void Tick()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            if (++_ticks > 240) throw new Exception("Unknown transform smoke timed out");
            if (!_started)
            {
                var game = NGame.Instance;
                if (game?.MainMenu == null || !game.MainMenu.IsNodeReady()) return;
                _started = true;
                await game.StartNewSingleplayerRun(ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>(), false,
                    new ActModel[] { ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Overgrowth>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Hive>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Glory>() },
                    Array.Empty<ModifierModel>(), "ERRORUNKNOWN", GameMode.Standard);
                return;
            }
            var run = RunManager.Instance.DebugOnlyGetState();
            if (run == null || run.CurrentRoomCount == 0) return;
            var me = LocalContext.GetMe(run)!;
            if (_enter == null)
            {
                foreach (string ftue in new[] { "obtain_relic_ftue", "map_select_ftue" })
                    SaveManager.Instance.MarkFtueAsComplete(ftue);
                // Fast/instant modes can hide the reported normal-mode blackout.
                SaveManager.Instance.PrefsSave.FastMode = FastModeType.Normal;
                var relic = ErrorModeState.CreateMutableForRarity(RelicRarity.Common);
                relic.GeneratedHookId = ErrorHookId.H031_EnterFirstUnknownRoomPlanisphere;
                relic.GeneratedEffectId = ErrorEffectId.E008_Transform3AndUpgrade;
                relic.DefinitionLocked = true;
                me.AddRelicInternal(relic, silent: true);
                _relic = relic;
                _deckCount = me.Deck.Cards.Count;
                GD.Print("ERROR_UNKNOWN_TRANSFORM_BEGIN");
                _enter = EnterUnknown(run);
                return;
            }
            if (_enter.IsFaulted) await _enter;
            var screen = Find<NDeckTransformSelectScreen>(((SceneTree)Engine.GetMainLoop()).Root);
            if (!_selected && screen?.IsNodeReady() == true)
            {
                var transition = NGame.Instance.Transition;
                var blackout = (Control)AccessTools.Field(typeof(NTransition), "_simpleTransition").GetValue(transition)!;
                GD.Print($"ERROR_UNKNOWN_TRANSFORM_SCREEN transition={transition.InTransition} alpha={blackout.Modulate.A} filter={transition.MouseFilter}");
                if (transition.InTransition || blackout.Modulate.A > 0.01f || transition.MouseFilter != Control.MouseFilterEnum.Ignore)
                    throw new Exception("Transform selection is covered by the room transition");
                _originals = me.Deck.Cards.Where(c => c.Type != CardType.Quest && c.IsTransformable).Take(3).ToArray();
                if (_originals.Length != 3) throw new Exception("Expected three transformable cards");
                foreach (var card in _originals)
                    AccessTools.Method(screen.GetType(), "OnCardClicked").Invoke(screen, new object[] { card });
                AccessTools.Method(screen.GetType(), "CompleteSelection").Invoke(screen, new object?[] { null });
                _selected = true;
            }
            if (!_enter.IsCompleted) return;
            await _enter;
            if (!_selected || me.Deck.Cards.Count != _deckCount || _originals!.Any(me.Deck.Cards.Contains))
                throw new Exception("Three original cards were not replaced");
            var replacements = run.CurrentMapPointHistoryEntry!.GetEntry(me.NetId).CardsTransformed;
            if (replacements.Count != 3 || _relic!.EffectExecutions != 1)
                throw new Exception("Expected exactly one effect and three transformations");
            if (me.Deck.Cards.Count(c => c.IsUpgraded) < 3)
                throw new Exception("Transformed cards were not upgraded");
            GD.Print("ERROR_UNKNOWN_TRANSFORM_PASS");
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit();
        }
        catch (Exception e)
        {
            GD.Print("ERROR_UNKNOWN_TRANSFORM_FAIL " + e);
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit(1);
        }
        finally { _busy = false; }
    }
}
