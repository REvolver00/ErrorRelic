using BaseLib.Config.UI;
using ErrorRelics.ErrorRelicsCode.Config;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Saves.Runs;

[ModInitializer(nameof(Initialize))]
public static class ConfigSmokeDriver
{
    private static Godot.Timer? _timer;
    public static void Initialize()
    {
        if (!CommandLineHelper.HasArg("errorrelic-config-smoke")) return;
        _timer = new Godot.Timer { WaitTime = 0.5, Autostart = true };
        _timer.Timeout += Run;
        ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, _timer);
    }

    private static bool HasCheckbox(Node node) => node is NConfigTickbox || node.GetChildren().Any(HasCheckbox);
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

    private static void Run()
    {
        if (NGame.Instance?.MainMenu == null || !NGame.Instance.MainMenu.IsNodeReady()) return;
        _timer!.Stop();
        int exit = 0;
        var config = new ErrorRelicsConfig();
        try
        {
            Require(ErrorRelicsConfig.ShowFullEffects, "Fresh config must default on");
            Require(ErrorRelicsConfig.PickupRandomRotation, "Pickup rotation must default on");
            Require(ErrorRelicsConfig.RandomTriggerSfx, "Random trigger SFX must default on");
            Require(ErrorRelicsConfig.AllowAncientSourceEffects, "Ancient-source Effects must default on");
            string label = new LocString("settings_ui", config.ModPrefix + StringHelper.Slugify(nameof(ErrorRelicsConfig.ShowFullEffects)) + ".title").GetFormattedText();
            Require(label == "显示完整效果", "Settings label must be localized");
            string rotationLabel = new LocString("settings_ui", config.ModPrefix + StringHelper.Slugify(nameof(ErrorRelicsConfig.PickupRandomRotation)) + ".title").GetFormattedText();
            string sfxLabel = new LocString("settings_ui", config.ModPrefix + StringHelper.Slugify(nameof(ErrorRelicsConfig.RandomTriggerSfx)) + ".title").GetFormattedText();
            Require(rotationLabel == "拾取 ERROR 时随机旋转贴图", "Rotation setting label must be localized");
            Require(sfxLabel == "随机播放 ERROR 触发音效", "SFX setting label must be localized");
            string ancientEffectLabel = new LocString("settings_ui", config.ModPrefix + StringHelper.Slugify(nameof(ErrorRelicsConfig.AllowAncientSourceEffects)) + ".title").GetFormattedText();
            Require(ancientEffectLabel == "允许生成先古遗物来源的 Effect", "Ancient Effect setting label must be localized");
            var panel = new VBoxContainer();
            ((SceneTree)Engine.GetMainLoop()).Root.AddChild(panel);
            config.SetupConfigUI(panel);
            Require(HasCheckbox(panel), "BaseLib settings page must expose the checkbox");
            ErrorRelicsConfig.AllowAncientSourceEffects = false;
            for (int i = 0; i < 512; i++)
            {
                var filtered = ErrorRelics.ErrorRelicsCode.Fragments.ErrorGenerator.Generate();
                Require(!ErrorRelics.ErrorRelicsCode.Fragments.ErrorEffectSourceMetadata.IsAncientSource(filtered.EffectId),
                    "Ancient-source Effect leaked into disabled generation pool");
            }
            ErrorRelicsConfig.AllowAncientSourceEffects = true;
            panel.QueueFree();
            var relic = (ErrorRandomTestRelic)ModelDb.Relic<ErrorRandomTestRelic>().ToMutable();
            relic.GeneratedHookId = ErrorHookId.H037_Every10AttacksPersistentNunchaku;
            relic.GeneratedEffectId = ErrorRelics.ErrorRelicsCode.Fragments.ErrorEffectId.E009_Enchant1CardRoyallyApproved;
            relic.DefinitionLocked = true;
            string state = relic.ToSerializable().Props!.ToString();
            string shown = relic.DynamicDescription.GetFormattedText();
            Require(shown.Contains("王室认证"), "Enabled descriptions must show the translated effect");
            ErrorRelicsConfig.ShowFullEffects = false;
            config.Save();
            Require(relic.DynamicDescription.GetFormattedText() == "", "Inventory text must be empty when off");
            Require(relic.DynamicEventDescription.GetFormattedText() == "", "Event text must be empty when off");
            Require(ModelDb.Relic<ErrorTestRelic>().DynamicDescription.GetFormattedText() == "", "Fixed ERROR test effects must also be hidden");
            Require(ModelDb.Relic<ErrorTestRelic>().Flavor.GetFormattedText() == "", "Flavor must not leak fragment IDs");
            Require(ModelDb.Relic<Vajra>().DynamicDescription.GetFormattedText() != "", "Vanilla relics must retain their descriptions");
            Require(relic.ToSerializable().Props!.ToString() == state, "Display setting must not modify relic data");
            ErrorRelicsConfig.ShowFullEffects = true;
            _ = new ErrorRelicsConfig();
            Require(!ErrorRelicsConfig.ShowFullEffects, "Reload must restore the saved off preference");
            ErrorRelicsConfig.ShowFullEffects = true;
            Require(relic.DynamicDescription.GetFormattedText() == shown, "Re-enabling must restore the full text");
            GD.Print("ERROR_CONFIG_SMOKE_PASS: checkbox, default, persistence, hidden inventory/event/flavor, unchanged data");
        }
        catch (Exception error) { exit = 1; GD.Print("ERROR_CONFIG_SMOKE_FAIL: " + error); }
        finally
        {
            ErrorRelicsConfig.ShowFullEffects = true;
            ErrorRelicsConfig.PickupRandomRotation = true;
            ErrorRelicsConfig.RandomTriggerSfx = true;
            ErrorRelicsConfig.AllowAncientSourceEffects = true;
            config.Save();
        }
        ((SceneTree)Engine.GetMainLoop()).Quit(exit);
    }
}
