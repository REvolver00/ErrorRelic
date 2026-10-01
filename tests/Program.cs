using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;
using ErrorRelics.ErrorRelicsCode.ErrorMode;
using ErrorRelics.ErrorRelicsCode.Visuals;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using System.Reflection;
using System.Runtime.Loader;

internal static class Program
{
    private static int _checks;
    static int Main(string[] args)
    {
        string gameData = args[0];
        AssemblyLoadContext.Default.Resolving += (_, name) =>
        {
            string path = Path.Combine(gameData, name.Name + ".dll");
            return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
        };
        try { Run(); Console.WriteLine($"PASS: {_checks} checks"); return 0; }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }

    static void Check(bool result, string name)
    {
        if (!result) throw new Exception(name);
        _checks++;
    }

    static void Run()
    {
        var a = new Rng(123456UL);
        var b = new Rng(123456UL);
        var seenH = new HashSet<ErrorHookId>();
        var seenE = new HashSet<ErrorEffectId>();
        for (int i = 0; i < 20000; i++)
        {
            var left = ErrorGenerator.Generate(a);
            Check(left == ErrorGenerator.Generate(b), "Two independent peers must generate the same stream");
            Check(left.EffectId != ErrorEffectId.E002_UpgradePlayedCard
                  && left.EffectId != ErrorEffectId.E007_Choose1Of3ColorlessToHand, "Disabled effects must stay excluded");
            seenH.Add(left.HookId); seenE.Add(left.EffectId);
        }
        Check(seenH.Count == 104 && seenE.Count == 107, "All 104 hooks and 107 active effects remain reachable");
        Check((int)ErrorEffectId.E010_Add2RandomCurses == 8 && (int)ErrorEffectId.E009_Enchant1CardRoyallyApproved == 9,
            "Historical enum storage order must not change");
        foreach (var h in Enum.GetValues<ErrorHookId>())
        foreach (var e in Enum.GetValues<ErrorEffectId>())
        {
            string text = ErrorDescriptionText.Describe(h, e);
            Check(!text.Contains("未知") && text.Contains('，') && text.Contains('\n'), "Every fragment pair needs full text");
        }
        Check(ErrorDescriptionText.Describe(ErrorHookId.H004_AfterObtained, ErrorEffectId.E009_Enchant1CardRoyallyApproved).Contains("附魔"), "E009 text follows its label");
        Check(ErrorDescriptionText.Describe(ErrorHookId.H004_AfterObtained, ErrorEffectId.E010_Add2RandomCurses).Contains("诅咒"), "E010 text follows its label");

        string Strip(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\[[^\]]*\]", "").Split('\n')[0];
        string royal = ErrorDescriptionText.Describe(ErrorHookId.H037_Every10AttacksPersistentNunchaku,
            ErrorEffectId.E009_Enchant1CardRoyallyApproved);
        Check(Strip(royal) == "你每打出10张攻击牌，从牌组中选择一张攻击牌或技能牌，为它附魔：王室认证。",
            "Nunchaku/Royal Stamp should use native phrasing and translated enchantment");
        Check(royal.Contains("[blue]10[/blue]") && royal.Contains("[purple]王室认证[/purple]"), "Use native numeric and enchantment highlights");
        Check(Strip(ErrorDescriptionText.Describe(ErrorHookId.H039_FirstTurnBeforeSideTurnStartBagOfMarbles,
            ErrorEffectId.E016_UpgradeBasicStrikeAndDefend)) == "在每场战斗开始时，升级你的1张打击和1张防御。",
            "Do not expose side-turn timing or redundant basic-card implementation details");
        Check(ErrorDescriptionText.Describe(ErrorHookId.H004_AfterObtained, ErrorEffectId.E006_Add3Apparitions).Contains("灵体"),
            "Use the game's Apparition translation");
        Check(ErrorDescriptionText.Describe(ErrorHookId.H004_AfterObtained, ErrorEffectId.E009_Enchant1CardRoyallyApproved,
            (table, key) => table == "enchantments" && key == "ROYALLY_APPROVED.title" ? "运行时译名" : null).Contains("运行时译名"),
            "Prefer the current game localization table to fallback text");
        Check(ErrorDescriptionText.Describe(ErrorHookId.H002_PlayerTurnStart, ErrorEffectId.E022_GainEnergy1,
            energyIconPrefix: "ironclad").Contains("ironclad_energy_icon.png"), "Use native energy icon paths");
        foreach (var h in Enum.GetValues<ErrorHookId>())
        foreach (var e in Enum.GetValues<ErrorEffectId>())
        {
            string text = ErrorDescriptionText.Describe(h, e);
            Check(!text.Contains('{') && !text.Contains("跨战斗计数") && !text.Contains("所在一方") && !text.Contains("符合条件"),
                "Player descriptions must resolve every name and hide engine details");
        }

        TestMode.IsOn = true;
        var harmony = new Harmony("ErrorRelics.Tests");
        harmony.Patch(AccessTools.Method(typeof(TestMode), nameof(TestMode.IsTestRunFromCmdline)),
            prefix: new HarmonyMethod(typeof(Program), nameof(TestRun)));
        MegaCrit.Sts2.Core.Modding.AssemblyInfo.Init();
        MegaCrit.Sts2.Core.Modding.AssemblyInfo.MockTypes = typeof(ErrorRandomTestRelic).Assembly.GetTypes()
            .Where(t => typeof(AbstractModel).IsAssignableFrom(t))
            .ToDictionary(t => t, t => ((MegaCrit.Sts2.Core.Modding.Mod?)null, true));
        ModelDb.Init(new[] { typeof(ErrorRandomTestRelic), typeof(ErrorRandomUncommonRelic),
            typeof(ErrorRandomRareRelic), typeof(ErrorRandomShopRelic), typeof(ErrorProofRelic) });
        // The console harness has no Godot native bindings. Suppress only the
        // cache's informational log; exercise its real ID/property registration.
        harmony.Patch(AccessTools.Method(typeof(ModelIdSerializationCache), nameof(ModelIdSerializationCache.Init)),
            transpiler: new HarmonyMethod(typeof(Program), nameof(NoCacheLog)));
        ModelIdSerializationCache.Init();
        ModelDb.InitIds();
        var original = (ErrorRandomTestRelic)ModelDb.Relic<ErrorRandomTestRelic>().ToMutable();
        original.GeneratedHookId = ErrorHookId.H021_Every3TurnsHappyFlower;
        original.GeneratedEffectId = ErrorEffectId.E010_Add2RandomCurses;
        original.DefinitionLocked = true;
        original.EffectExecutions = 17;
        original.TurnsSeen = 2;
        original.NunchakuAttacksPlayed = 9;
        original.TuningForkSkillsPlayed = 8;
        original.JossPaperCardsExhausted = 4;
        original.VisualSourceIconPath = "res://test.png";
        var packet = new PacketWriter();
        original.ToSerializable().Serialize(packet);
        var reader = new PacketReader();
        reader.Reset(packet.Buffer);
        var transmitted = new SerializableRelic();
        transmitted.Deserialize(reader);
        Check(reader.BitPosition == packet.BitPosition, "Network reader must consume the complete relic payload");
        var restored = (ErrorRandomTestRelic)RelicModel.FromSerializable(transmitted);
        Check(restored.GeneratedHookId == original.GeneratedHookId && restored.GeneratedEffectId == original.GeneratedEffectId,
            "Save restore must keep the H/E pair");
        Check(restored.DefinitionLocked && restored.TurnsSeen == 2 && restored.JossPaperCardsExhausted == 4
            && restored.NunchakuAttacksPlayed == 9 && restored.TuningForkSkillsPlayed == 8,
            "Save restore must keep locking and persistent counters");
        Check(restored.EffectExecutions == 17, "Network reconstruction must preserve the effect RNG ordinal");
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
        string propsJson = System.Text.Json.JsonSerializer.Serialize(original.ToSerializable().Props, jsonOptions);
        var diskProps = System.Text.Json.JsonSerializer.Deserialize<SavedProperties>(propsJson, jsonOptions)!;
        var diskRestored = (ErrorRandomTestRelic)ModelDb.Relic<ErrorRandomTestRelic>().ToMutable();
        diskProps.Fill(diskRestored);
        Check(diskRestored.GeneratedHookId == original.GeneratedHookId && diskRestored.GeneratedEffectId == original.GeneratedEffectId
            && diskRestored.DefinitionLocked && diskRestored.EffectExecutions == 17, "JSON save must retain H/E, lock and effect RNG ordinal");
        Check(restored.VisualSourceIconPath == "res://test.png", "Save restore must preserve source icon");
        Check(original.FullDescription == restored.FullDescription, "Description must survive reconstruction");
        var localOwner = (MegaCrit.Sts2.Core.Entities.Players.Player)
            System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(MegaCrit.Sts2.Core.Entities.Players.Player));
        AccessTools.Field(typeof(MegaCrit.Sts2.Core.Entities.Players.Player), "_relics")
            .SetValue(localOwner, new List<RelicModel>());
        localOwner.AddRelicInternal(restored, silent: true);
        Check(ReferenceEquals(restored.Owner, localOwner), "Inventory reconstruction must bind the restored owner");

        foreach (string field in new[] { "_letterOpenerSkillsThisTurn", "_kusarigamaAttacksThisTurn", "_ornamentalFanAttacksThisTurn", "_jossPaperEtherealCount" })
            AccessTools.Field(typeof(ErrorGeneratedRelic), field).SetValue(original, 2);
        original.BeforeCombatStart().GetAwaiter().GetResult();
        foreach (string field in new[] { "_letterOpenerSkillsThisTurn", "_kusarigamaAttacksThisTurn", "_ornamentalFanAttacksThisTurn", "_jossPaperEtherealCount" })
            Check((int)AccessTools.Field(typeof(ErrorGeneratedRelic), field).GetValue(original)! == 0, "Combat start must clear transient counters");
        Check(original.TurnsSeen == 2 && original.JossPaperCardsExhausted == 4 && original.NunchakuAttacksPlayed == 9,
            "Combat start must not clear persistent counters");
        harmony.PatchAll(typeof(MultiplayerTreasureAwardPatch).Assembly);
        var firstDescription = new MegaCrit.Sts2.Core.Localization.LocString("relics", "test.description");
        ErrorRelicDescriptionPatch.Postfix(original, ref firstDescription);
        restored.GeneratedHookId = ErrorHookId.H002_PlayerTurnStart;
        restored.GeneratedEffectId = ErrorEffectId.E004_GainGold300;
        var secondDescription = new MegaCrit.Sts2.Core.Localization.LocString("relics", "test.description");
        ErrorRelicDescriptionPatch.Postfix(restored, ref secondDescription);
        Check((string)firstDescription.Variables["ErrorDescription"] == original.FullDescription,
            "Primary tooltip must contain this relic's complete text");
        Check((string)secondDescription.Variables["ErrorDescription"] == restored.FullDescription
            && (string)secondDescription.Variables["ErrorDescription"] != (string)firstDescription.Variables["ErrorDescription"],
            "Same-model relic tooltips must not overwrite each other");
        Check(Harmony.GetPatchInfo(AccessTools.AsyncMoveNext(AccessTools.Method(
            typeof(MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic.NTreasureRoomRelicCollection), "AnimateRelicAwards"))) != null,
            "Treasure transpiler must match the actual game");
    }

    static IEnumerable<CodeInstruction> NoCacheLog(IEnumerable<CodeInstruction> codes)
    {
        foreach (var code in codes)
        {
            if (code.operand is MethodInfo m && m.DeclaringType == typeof(MegaCrit.Sts2.Core.Logging.Log)
                && m.Name == "Info")
            {
                foreach (var _ in m.GetParameters()) yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Pop);
            }
            else yield return code;
        }
    }

    static bool TestRun(ref bool __result) { __result = true; return false; }
}
