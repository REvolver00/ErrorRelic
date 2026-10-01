using System.Reflection;
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
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

[ModInitializer(nameof(Initialize))]
public static class MerchantSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy, _started, _multiplayer, _firstBattle, _chosen;
    private static Task? _enter, _neowEnter;
    private static int _ticks;
    private static ErrorEffectId _effect;
    private static CardModel? _selected;
    private static CardModel[]? _automaticTargets;

    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-merchant-smoke")) return;
        _multiplayer = CommandLineHelper.HasArg("merchant-mp");
        _firstBattle = CommandLineHelper.HasArg("first-battle");
        ErrorRelics.ErrorRelicsCode.Config.ErrorRelicsConfig.StartWithRedPickaxe = CommandLineHelper.HasArg("pickaxe-on");
        if (_firstBattle)
            new Harmony("ErrorRelics.FirstBattleFixture").Patch(AccessTools.Method(typeof(MegaCrit.Sts2.Core.Models.Events.Neow), "GenerateInitialOptions"),
                prefix: new HarmonyMethod(typeof(MerchantSmokeDriver), nameof(NeowFixture)));
        _effect = CommandLineHelper.HasArg("goopy") ? ErrorEffectId.E012_EnchantAllGoopyEligible :
            CommandLineHelper.HasArg("auto-enchant")
            ? ErrorEffectId.E011_EnchantBasicStrikesTezcatarasEmber
            : ErrorEffectId.E009_Enchant1CardRoyallyApproved;
        _timer = new Godot.Timer { WaitTime = 0.5, Autostart = true };
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

    private static MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.StartRunLobby? FindLobby(Node node)
    {
        foreach (var field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (field.GetValue(node) is MegaCrit.Sts2.Core.Multiplayer.Game.Lobby.StartRunLobby lobby) return lobby;
        foreach (Node child in node.GetChildren())
            if (FindLobby(child) is { } lobby) return lobby;
        return null;
    }

    private static void NeowFixture(MegaCrit.Sts2.Core.Models.Events.Neow __instance)
        => Traverse.Create(__instance).Property("DebugOption").SetValue("GOLDEN_PEARL");

    private static async void Tick()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            if (++_ticks > 120) throw new Exception("Merchant smoke timed out");
            if (!_started)
            {
                if (_multiplayer)
                {
                    var lobby = FindLobby(((SceneTree)Engine.GetMainLoop()).Root);
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
                    Array.Empty<ModifierModel>(), "ERRORSHOP", GameMode.Standard);
                return;
            }
            var run = RunManager.Instance.DebugOnlyGetState();
            if (run == null || run.CurrentRoomCount == 0) return;
            var me = LocalContext.GetMe(run)!;
            if (_firstBattle)
            {
                var manager = RunManager.Instance;
                if (_neowEnter == null)
                {
                    _neowEnter = manager.EnterRoomDebug(RoomType.Event, model: ModelDb.Event<MegaCrit.Sts2.Core.Models.Events.Neow>(), showTransition: false);
                    return;
                }
                if (_neowEnter.IsFaulted) await _neowEnter;
                if (!_chosen)
                {
                    if (manager.EventSynchronizer.Events.Count != 2 || manager.EventSynchronizer.GetLocalEvent().CurrentOptions.Count == 0) return;
                    if (run.Players.Any(p => ErrorModeState.IsEnabled(p) != (p.NetId == 1))) return;
                    foreach (var player in run.Players)
                    {
                        bool hasProof = ErrorModeState.IsEnabled(player);
                        if (hasProof != (player.NetId == 1)) throw new Exception("Opening config was not synchronized per owner");
                        GD.Print($"ERROR_START_PROOF peer={me.NetId} owner={player.NetId} enabled={hasProof}");
                    }
                    _chosen = true;
                    manager.EventSynchronizer.ChooseLocalOption(0);
                    return;
                }
                if (_enter == null)
                {
                    if (manager.EventSynchronizer.Events.Any(e => !e.IsFinished)) return;
                    foreach (string ftue in new[] { "combat_rules_ftue", "obtain_relic_ftue", "map_select_ftue", "shuffle_ftue" })
                        MegaCrit.Sts2.Core.Saves.SaveManager.Instance.MarkFtueAsComplete(ftue);
                    var encounter = ModelDb.AllEncounters.Where(e => e.RoomType == RoomType.Monster)
                        .OrderBy(e => e.Id.Entry, StringComparer.Ordinal)
                        .First(e => e.GetType().Name.Contains("Slime", StringComparison.OrdinalIgnoreCase));
                    _enter = manager.EnterRoomDebug(RoomType.Monster, model: encounter.ToMutable(), showTransition: false);
                    return;
                }
                if (_enter.IsFaulted) await _enter;
                if (!MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress || me.PlayerCombatState?.Phase.ToString() != "Play") return;
                GD.Print($"ERROR_FIRST_BATTLE_PASS peer={me.NetId}");
                _timer!.Stop();
                return;
            }
            if (_enter == null)
            {
                foreach (string ftue in new[] { "obtain_relic_ftue", "map_select_ftue" })
                    MegaCrit.Sts2.Core.Saves.SaveManager.Instance.MarkFtueAsComplete(ftue);
                foreach (var player in run.Players)
                {
                    var relic = ErrorModeState.CreateMutableForRarity(RelicRarity.Common);
                    relic.GeneratedHookId = ErrorHookId.H029_EnterMerchantMealTicket;
                    relic.GeneratedEffectId = _effect;
                    relic.DefinitionLocked = true;
                    player.AddRelicInternal(relic, silent: true);
                    var createContext = AccessTools.Method(typeof(ErrorGeneratedRelic), "CreateErrorContext");
                    var first = (ErrorContext)createContext.Invoke(relic, new object?[] {
                        player, new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), relic.DynamicVars, null })!;
                    int[] expected = Enumerable.Range(0, 20).Select(_ => first.NicheRng!.NextInt()).ToArray();
                    // Unrelated effects/shared draws must not change this relic's replay.
                    var sharedSnapshot = run.Rng.Niche.ToSerializable();
                    for (int i = 0; i < 37; i++) run.Rng.Niche.NextInt();
                    relic.EffectExecutions = 0;
                    var replay = (ErrorContext)createContext.Invoke(relic, new object?[] {
                        player, new MegaCrit.Sts2.Core.GameActions.Multiplayer.ThrowingPlayerChoiceContext(), relic.DynamicVars, null })!;
                    if (!expected.SequenceEqual(Enumerable.Range(0, 20).Select(_ => replay.NicheRng!.NextInt())))
                        throw new Exception("Effect RNG replay changed");
                    run.Rng.Niche.LoadFromSerializable(sharedSnapshot);
                    GD.Print($"ERROR_EFFECT_RNG peer={me.NetId} owner={player.NetId} " + string.Join(",", expected));
                    relic.EffectExecutions = 0;
                    // Exercise real production source generation without advancing run RNG.
                    var generated = ErrorGenerator.Generate(player, ModelDb.Relic<MegaCrit.Sts2.Core.Models.Relics.Vajra>().Id, "merchant");
                    var rebuilt = ErrorGenerator.Generate(player, ModelDb.Relic<MegaCrit.Sts2.Core.Models.Relics.Vajra>().Id, "merchant");
                    if (generated.HookId != rebuilt.HookId || generated.EffectId != rebuilt.EffectId)
                        throw new Exception("Rebuilding a source must preserve H/E");
                    GD.Print($"ERROR_SOURCE peer={me.NetId} owner={player.NetId} {generated.HookId}/{generated.EffectId}");
                }
                _automaticTargets = me.Deck.Cards.Where(c => _effect == ErrorEffectId.E012_EnchantAllGoopyEligible
                    ? ModelDb.Enchantment<Goopy>().CanEnchant(c)
                    : c.Tags.Contains(CardTag.Strike) && ModelDb.Enchantment<TezcatarasEmber>().CanEnchant(c)).ToArray();
                GD.Print("ERROR_MERCHANT_BEGIN " + _effect);
                _enter = RunManager.Instance.EnterRoomDebug(RoomType.Shop, showTransition: false);
                return;
            }
            if (_enter.IsFaulted) await _enter;
            var screen = Find<NDeckEnchantSelectScreen>(((SceneTree)Engine.GetMainLoop()).Root);
            if (_selected == null && screen?.IsNodeReady() == true)
            {
                _selected = me.Deck.Cards.First(c => ModelDb.Enchantment<RoyallyApproved>().CanEnchant(c));
                AccessTools.Method(screen.GetType(), "OnCardClicked").Invoke(screen, new object[] { _selected });
                // Single selection has a separate enchantment-preview confirmation.
                AccessTools.Method(screen.GetType(), "ConfirmSelection").Invoke(screen, new object?[] { null });
            }
            if (!_enter.IsCompleted) return;
            await _enter;
            if (_effect == ErrorEffectId.E009_Enchant1CardRoyallyApproved && _selected?.Enchantment is not RoyallyApproved)
                throw new Exception("RoyalStamp enchantment missing");
            if (_effect == ErrorEffectId.E011_EnchantBasicStrikesTezcatarasEmber
                && !_automaticTargets!.All(c => c.Enchantment is TezcatarasEmber))
                throw new Exception("Automatic enchantment missing");
            if (_effect == ErrorEffectId.E012_EnchantAllGoopyEligible && !_automaticTargets!.All(c => c.Enchantment is Goopy))
                throw new Exception("Goopy enchantment missing");
            foreach (var player in run.Players)
            {
                var relic = player.Relics.OfType<ErrorRandomTestRelic>().Single();
                var saved = (ErrorRandomTestRelic)RelicModel.FromSerializable(relic.ToSerializable());
                if (saved.EffectExecutions != relic.EffectExecutions || saved.GeneratedEffectId != relic.GeneratedEffectId)
                    throw new Exception("Saved relic lost effect or RNG ordinal");
                GD.Print($"ERROR_DECK peer={me.NetId} owner={player.NetId} " + string.Join("|", player.Deck.Cards.Select(c => $"{c.Id}:{c.Enchantment?.Id}:{c.Enchantment?.Amount}")));
            }
            GD.Print("ERROR_MERCHANT_PASS peer=" + me.NetId + " " + _effect);
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit();
        }
        catch (Exception e)
        {
            GD.Print("ERROR_MERCHANT_FAIL " + e);
            _timer!.Stop();
            ((SceneTree)Engine.GetMainLoop()).Quit(1);
        }
        finally { _busy = false; }
    }
}
