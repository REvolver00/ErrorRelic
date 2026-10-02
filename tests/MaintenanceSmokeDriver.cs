using System.Text.Json;
using ErrorRelics.ErrorRelicsCode.Config;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

[ModInitializer(nameof(Initialize))]
public static class MaintenanceSmokeDriver
{
    private static Godot.Timer? _timer;
    private static bool _busy;
    private static int _checks;
    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-maintenance-smoke")) return;
        _timer = new Godot.Timer { WaitTime = .5, Autostart = true };
        _timer.Timeout += Run;
        ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, _timer);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        _checks++;
    }
    private static bool AlwaysRotate(ref bool __result) { __result = true; return false; }
    private static bool QuarterTurn(ref int __result) { __result = 1; return false; }
    private static async void Run()
    {
        if (_busy || NGame.Instance?.MainMenu?.IsNodeReady() != true) return;
        _busy = true;
        _timer!.Stop();
        int exit = 0;
        try
        {
            ErrorRelicsConfig.StartWithRedPickaxe = false;
            var run = await NGame.Instance.StartNewSingleplayerRun(
                ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>(), false,
                new ActModel[] { ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Overgrowth>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Hive>(), ModelDb.Act<MegaCrit.Sts2.Core.Models.Acts.Glory>() },
                Array.Empty<ModifierModel>(), "ERROR093FIX", GameMode.Standard);
            var player = LocalContext.GetMe(run)!;
            Check(!ErrorModeState.IsEnabled(player), "Unactivated run");
            var proof = ErrorModeState.CreateProof();
            Check(!ErrorModeState.IsEnabled(player), "Creating a preview must not activate the run");
            player.AddRelicInternal(proof, silent: true);
            Check(ErrorModeState.IsEnabled(player), "Direct inventory add activates ERROR");
            player.RemoveRelicInternal(proof, silent: true);
            Check(ErrorModeState.IsEnabled(player), "Removing Proof must not undo activation");

            var canonical = ModelDb.Relic<ErrorRandomTestRelic>();
            Check(!canonical.DefinitionLocked, "Canonical template must remain unlocked");
            _ = canonical.DynamicDescription.GetFormattedText();
            _ = canonical.Title.GetFormattedText();
            var first = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), player, "reward");
            player.AddRelicInternal(first, silent: true);
            string title = first.Title.GetFormattedText();
            ErrorRelicsConfig.ShowFullEffects = false;
            Check(first.Title.GetFormattedText() == title && title.Length > 0, "Hidden effects retain glitch title");
            Check(first.DynamicDescription.GetFormattedText() == "", "Effect hiding still works");
            ErrorRelicsConfig.ShowFullEffects = true;

            var saved = RunManager.Instance.ToSave(null);
            var serializerContext = (System.Text.Json.Serialization.JsonSerializerContext)AccessTools.Property(
                typeof(SaveManager).Assembly.GetType("MegaCrit.Sts2.Core.Saves.MegaCritSerializerContext")!, "Default").GetValue(null)!;
            string json = JsonSerializer.Serialize(saved, typeof(SerializableRun), serializerContext);
            Check(json.Contains("ErrorRelics.Activated") && json.Contains("ErrorRelics.GenerationCount"), "Run and player extension fields are on disk");
            var disk = (SerializableRun)JsonSerializer.Deserialize(json, typeof(SerializableRun), serializerContext)!;
            var packet = new PacketWriter(); saved.Serialize(packet);
            var reader = new PacketReader(); reader.Reset(packet.Buffer);
            var network = new SerializableRun(); network.Deserialize(reader);
            Check(reader.BitPosition == packet.BitPosition, "Run packet roundtrip consumes all fields");
            var diskRun = RunState.FromSerializable(disk);
            var netRun = RunState.FromSerializable(network);
            var a = diskRun.Players[0]; var b = netRun.Players[0];
            Check(ErrorModeState.IsEnabled(a) && ErrorModeState.IsEnabled(b), "Activation survives disk/network without Proof");
            var restored = a.Relics.OfType<ErrorRandomTestRelic>().Single();
            Check(restored.GenerationIdentity == first.GenerationIdentity && restored.DefinitionLocked
                && restored.GeneratedHookId == first.GeneratedHookId && restored.GeneratedEffectId == first.GeneratedEffectId,
                "Existing ERROR restores identity and pair without regeneration");
            var identities = new HashSet<string> { first.GenerationIdentity };
            var pairs = new HashSet<string>();
            int niche = a.RunState.Rng.Niche.ToSerializable().counter;
            int shops = a.PlayerRng.Shops.ToSerializable().counter;
            for (int i = 0; i < 40; i++)
            {
                var x = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), a, "reward");
                var y = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), b, "reward");
                Check(identities.Add(x.GenerationIdentity), "Consecutive Circlets need distinct identities");
                Check(x.GenerationIdentity == y.GenerationIdentity && x.GeneratedHookId == y.GeneratedHookId
                    && x.GeneratedEffectId == y.GeneratedEffectId, "Independent peers derive identical pairs");
                pairs.Add(x.GeneratedHookId + "/" + x.GeneratedEffectId);
            }
            Check(pairs.Count > 1, "Circlets must not all repeat one pair");
            Check(niche == a.RunState.Rng.Niche.ToSerializable().counter && shops == a.PlayerRng.Shops.ToSerializable().counter,
                "Generation does not consume native gameplay RNG streams");
            a.PlayerRng.Shops.NextFloat();
            var shopA = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), a, "merchant");
            a.PlayerRng.Shops.NextFloat();
            var shopB = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), a, "merchant");
            Check(shopA.GenerationIdentity != shopB.GenerationIdentity, "Shop restocks get distinct identities");
            var afterShop = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), a, "event-obtain");
            var withoutShop = ErrorModeState.CreateLockedError(ModelDb.Relic<Circlet>(), b, "event-obtain");
            Check(afterShop.GenerationIdentity == withoutShop.GenerationIdentity && afterShop.GeneratedHookId == withoutShop.GeneratedHookId
                && afterShop.GeneratedEffectId == withoutShop.GeneratedEffectId, "Local restocks must not perturb shared generation");

            var reward = new RelicReward(player);
            AccessTools.Field(typeof(RelicReward), "_relic").SetValue(reward, first);
            var rewardSave = reward.ToSerializable();
            var rewardPacket = new PacketWriter(); rewardSave.Serialize(rewardPacket);
            var rewardReader = new PacketReader(); rewardReader.Reset(rewardPacket.Buffer);
            var rewardNetwork = new SerializableReward(); rewardNetwork.Deserialize(rewardReader);
            var loadedReward = (RelicReward)Reward.FromSerializable(rewardNetwork, player);
            Check(loadedReward.Relic is ErrorRandomTestRelic rr && rr.GenerationIdentity == first.GenerationIdentity,
                "Populated extra reward survives network serialization");
            loadedReward.Populate();
            Check(loadedReward.Relic is ErrorRandomTestRelic same && same.GenerationIdentity == first.GenerationIdentity,
                "Repopulating a preview must not reroll");
            Check(!canonical.DefinitionLocked && canonical.GenerationIdentity == "", "Generation must not contaminate ModelDb");

            var tree = (SceneTree)Engine.GetMainLoop();
            var testImage = new TextureRect { Texture = ModelDb.Relic<Circlet>().BigIcon,
                MouseFilter = Control.MouseFilterEnum.Ignore, RotationDegrees = 17, PivotOffset = new Vector2(2, 3), Size = new Vector2(48, 48) };
            tree.CurrentScene.AddChild(testImage);
            var runtimeType = typeof(ErrorRandomTestRelic).Assembly.GetType("ErrorRelics.ErrorRelicsCode.Presentation.ErrorPresentationRuntime")!;
            var runtime = AccessTools.Property(runtimeType, "Instance").GetValue(null)!;
            var corruption = AccessTools.Method(runtimeType, "CorruptCurrentScene");
            var rotationApi = typeof(ErrorRandomTestRelic).Assembly.GetType("ErrorRelics.ErrorRelicsCode.Presentation.ErrorPresentationCorruption")!;
            var forceRotation = new Harmony("ErrorRelics.Tests.ForcedRotation");
            forceRotation.Patch(AccessTools.Method(rotationApi, "ShouldRotate"), prefix: new HarmonyMethod(typeof(MaintenanceSmokeDriver), nameof(AlwaysRotate)));
            forceRotation.Patch(AccessTools.Method(rotationApi, "NextQuarterTurn"), prefix: new HarmonyMethod(typeof(MaintenanceSmokeDriver), nameof(QuarterTurn)));
            corruption.Invoke(runtime, null);
            Check(Math.Abs(testImage.RotationDegrees - 107) < .001, "Pickup rotates a noninteractive image");
            forceRotation.UnpatchAll("ErrorRelics.Tests.ForcedRotation");
            var container = new NSceneContainer(); tree.Root.AddChild(container);
            container.SetCurrentScene(new Control());
            Check(Math.Abs(testImage.RotationDegrees - 17) < .001 && testImage.PivotOffset == new Vector2(2, 3),
                "Native game scene-container transition restores surviving texture geometry");
            container.QueueFree(); testImage.QueueFree();
            var fmodServer = Engine.GetSingleton("FmodServer");
            var independent = fmodServer.Call("create_event_instance", "event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop").AsGodotObject();
            int independentStopped = independent.Call("get_playback_state").AsInt32();
            independent.Call("set_volume", 0f); independent.Call("start");
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

            AccessTools.Method(runtimeType, "PlayVanilla").Invoke(runtime, new object[] { "event:/sfx/ui/relic_activate_general", 0f });
            Check(((System.Collections.ICollection)AccessTools.Field(runtimeType, "_vanillaEvents").GetValue(runtime)!).Count == 1, "ERROR owns its FMOD event");
            await tree.ToSignal(tree.CreateTimer(3.1, true, false, true), SceneTreeTimer.SignalName.Timeout);
            AccessTools.Method(runtimeType, "_Process").Invoke(runtime, new object[] { 0.0 });
            Check(((System.Collections.ICollection)AccessTools.Field(runtimeType, "_vanillaEvents").GetValue(runtime)!).Count == 0, "ERROR event is released at the time limit");
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            Check(independent.Call("is_valid").AsBool() && independent.Call("get_playback_state").AsInt32() != independentStopped,
                "ERROR timeout does not stop an independent game sound");
            independent.Call("stop", false); independent.Call("release");
            string candidatesPath = System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(), "sfx-candidates.txt");
            if (CommandLineHelper.HasArg("errorrelic-sfx-audit") && System.IO.File.Exists(candidatesPath))
            {
                var fmod = Engine.GetSingleton("FmodServer");
                var playable = new List<string>();
                foreach (string path in System.IO.File.ReadAllLines(candidatesPath).Where(p => p.StartsWith("event:/")))
                {
                    var desc = fmod.Call("get_event", path).AsGodotObject();
                    if (desc == null) { GD.Print("ERROR_SFX_MISSING " + path); continue; }
                    GD.Print("ERROR_SFX_METADATA " + JsonSerializer.Serialize(new {
                        path, valid = desc.Call("is_valid").AsBool(),
                        oneShot = desc.Call("is_one_shot").AsBool(),
                        sustain = desc.Call("has_sustain_point").AsBool(),
                        lengthMs = desc.Call("get_length").AsInt32(),
                        parameters = desc.Call("get_parameter_count").AsInt32() }));
                    if (desc.Call("is_valid").AsBool() && desc.Call("is_one_shot").AsBool()
                        && !desc.Call("has_sustain_point").AsBool() && desc.Call("get_parameter_count").AsInt32() == 0)
                        playable.Add(path);
                }
                foreach (string[] batch in playable.Chunk(8))
                {
                    var active = new List<(string path, GodotObject instance, int stopped)>();
                    foreach (string path in batch)
                    {
                        var instance = fmod.Call("create_event_instance", path).AsGodotObject();
                        int stopped = instance.Call("get_playback_state").AsInt32();
                        instance.Call("set_volume", 0f); instance.Call("start");
                        active.Add((path, instance, stopped));
                    }
                    var elapsed = System.Diagnostics.Stopwatch.StartNew();
                    while (active.Count > 0)
                    {
                        await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                        if (elapsed.ElapsedMilliseconds < 100) continue;
                        for (int i = active.Count - 1; i >= 0; i--)
                        {
                            var item = active[i];
                            bool ended = item.instance.Call("get_playback_state").AsInt32() == item.stopped;
                            if (!ended && elapsed.ElapsedMilliseconds < 3000) continue;
                            GD.Print("ERROR_SFX_PLAYBACK " + JsonSerializer.Serialize(new { item.path, ended, elapsedMs = elapsed.ElapsedMilliseconds }));
                            if (!ended) item.instance.Call("stop", false);
                            item.instance.Call("release"); active.RemoveAt(i);
                        }
                    }
                }
            }
            // Exercise the actual frontend load path as well as pure reconstruction.
            await NGame.Instance.ReturnToMainMenuAfterRun();
            var reloaded = RunState.FromSerializable(disk);
            await RunManager.Instance.SetUpSavedSingleplayer(reloaded, disk);
            await NGame.Instance.LoadRun(reloaded, null);
            Check(ErrorModeState.IsEnabled(reloaded.Players[0]), "Native frontend load retains permanent activation");
            Check(reloaded.Players[0].Relics.OfType<ErrorRandomTestRelic>().Single().GenerationIdentity == first.GenerationIdentity,
                "Native frontend load must not reroll the saved ERROR");
            GD.Print($"ERROR_MAINTENANCE_SMOKE_PASS: {_checks} checks");
        }
        catch (Exception ex) { exit = 1; GD.Print("ERROR_MAINTENANCE_SMOKE_FAIL " + ex); }
        finally { ErrorRelicsConfig.ShowFullEffects = true; }
        ((SceneTree)Engine.GetMainLoop()).Quit(exit);
    }
}
