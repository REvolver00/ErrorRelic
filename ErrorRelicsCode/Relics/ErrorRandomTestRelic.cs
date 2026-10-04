using MegaCrit.Sts2.Core.Localization;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;
using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Config;


namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public class ErrorRandomTestRelic : ErrorGeneratedRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;


    // =========================================================
    // ERROR RANDOM 只允许由 ERROR 模式主动生成。
    //
    // 保留 [Pool] 是为了让 BaseLib 正常注册 Model，
    // 但 IsAllowed=false 会让它不作为普通原版遗物
    // 自己混进玩家的 GrabBag。
    // =========================================================

    public override bool IsAllowed(IRunState runState) => false;


    // =========================================================
    // 每一件 RANDOM TEST 自己保存的 Hook / Effect
    // =========================================================

    [SavedProperty]
    public ErrorHookId GeneratedHookId { get; set; }

    [SavedProperty]
    public ErrorEffectId GeneratedEffectId { get; set; }

    [SavedProperty]
    public string GenerationIdentity { get; set; } = string.Empty;

    [SavedProperty]
    public int GenerationSequence { get; set; }

    [SavedProperty]
    public bool DefinitionLocked { get; set; }

    // Discovery state is saved per ERROR. Keeping EffectRevealed preserves
    // compatibility with the first Data Crack build. HookRevealed is new and
    // defaults true so old saves keep their previously-visible Hook. Newly
    // generated ERRORs in Discovery Mode explicitly set both to false.
    [SavedProperty]
    public bool EffectRevealed { get; set; } = true;

    [SavedProperty]
    public bool HookRevealed { get; set; } = true;

    // Initialize discovery only when a brand-new ERROR definition is generated.
    // Do NOT call this during save reconstruction or Gift transfer: those paths
    // must preserve the relic's existing SavedProperty discovery state.
    public void InitializeDiscoveryForNewGeneration()
    {
        AssertMutable();
        bool discovery = !ErrorRelicsConfig.ShowFullEffects
            && ErrorRelicsConfig.DataCrackRevealMode;
        EffectRevealed = !discovery;
        HookRevealed = !discovery;
    }

    public void RevealEffect()
    {
        AssertMutable();
        EffectRevealed = true;
        InvokeDisplayAmountChanged();
    }

    public void RevealHook()
    {
        AssertMutable();
        HookRevealed = true;
        InvokeDisplayAmountChanged();
    }


    // =========================================================
    // VISUAL2 metadata only
    //
    // These fields remember which vanilla relic this ERROR
    // replaced. They DO NOT change RelicModel.Icon/BigIcon paths.
    //
    // The actual visual corruption is applied later, purely in
    // UI nodes (NRelic.Reload / RelicReward.CreateIcon).
    // =========================================================

    [SavedProperty]
    public string VisualSourceIconPath { get; set; } =
        string.Empty;

    [SavedProperty]
    public string VisualSourceOutlinePath { get; set; } =
        string.Empty;

    [SavedProperty]
    public string VisualSourceBigIconPath { get; set; } =
        string.Empty;


    public bool HasVisualSource =>
        !string.IsNullOrEmpty(
            VisualSourceIconPath
        );


    public void SetVisualSource(
        MegaCrit.Sts2.Core.Models.RelicModel source)
    {
        // Small icon path is public and does not require changing
        // the ERROR model's own icon path.
        VisualSourceIconPath =
            source.IconPath;

        // Outline / BigIcon paths are not both public, so read the
        // already-supported texture ResourcePath from the vanilla
        // source model. Failure only disables that one visual path.
        // ERROR keeps the source relic artwork, but deliberately does
        // not inherit the vanilla white outline.  The transparent
        // ERROR outline resource is used instead.
        VisualSourceOutlinePath =
            "relic_outline.png".RelicImagePath();

        try
        {
            VisualSourceBigIconPath =
                source.BigIcon.ResourcePath;
        }
        catch
        {
            VisualSourceBigIconPath =
                string.Empty;
        }
    }


    // =========================================================
    // ErrorGeneratedRelic 实际使用的 Definition
    // =========================================================

    protected override ErrorDefinition Definition =>
        new(
            GeneratedHookId,
            GeneratedEffectId
        );


    // =========================================================
    // 真正获得这一件遗物时，独立随机一次
    // =========================================================

    public override async Task AfterObtained()
    {
        // 普通随机获得：
        // 正常随机 H + E。
        //
        // 调试命令生成：
        // DefinitionLocked = true，
        // 保留命令提前塞好的 H + E。
        if (!DefinitionLocked)
        {
            ErrorRelicsCode.ErrorMode.ErrorRunState.AssignIdentity(this, Owner, "obtained");
            var generated = ErrorGenerator.Generate(Owner, Id, "obtained", GenerationIdentity);

            GeneratedHookId = generated.HookId;
            GeneratedEffectId = generated.EffectId;
            InitializeDiscoveryForNewGeneration();
            DefinitionLocked = true;
        }

        // 这里非常重要。
        //
        // 继续进入 ErrorGeneratedRelic.AfterObtained()，
        // 因此 H004 也能正常触发它自己的 E。
        MainFile.Logger.Info($"ERROR entered inventory: owner={Owner?.NetId}, pair={GeneratedHookId}/{GeneratedEffectId}, present={Owner?.Relics.Contains(this)}");
        await base.AfterObtained();
    }


    // The localization template is shared, but its variable is filled from
    // this instance by ErrorRelicDescriptionPatch. Never rewrite the table.
    public string FullDescription => DefinitionLocked
        ? ErrorDescriptionText.Describe(GeneratedHookId, GeneratedEffectId, OriginalName,
            LocManager.Instance == null ? "colorless" : RunManager.Instance.GetLocalCharacterEnergyIconPrefix() ?? "colorless")
        : "获得时随机组合一个触发条件和一个效果。";

    public string HookOnlyDescription => DefinitionLocked
        ? ErrorDescriptionText.DescribeHookOnly(GeneratedHookId,
            LocManager.Instance == null ? "colorless" : RunManager.Instance.GetLocalCharacterEnergyIconPrefix() ?? "colorless")
        : "获得时随机组合一个触发条件和一个未知效果。";

    public string EffectOnlyDescription => DefinitionLocked
        ? ErrorDescriptionText.DescribeEffectOnly(GeneratedEffectId, OriginalName,
            LocManager.Instance == null ? "colorless" : RunManager.Instance.GetLocalCharacterEnergyIconPrefix() ?? "colorless")
        : "获得时随机组合一个未知触发条件和一个效果。";

    private static string? OriginalName(string table, string key) => LocManager.Instance == null
        ? null : LocString.GetIfExists(table, key)?.GetFormattedText();

    public override List<(string, string)>? Localization => new RelicLoc(
        "{ErrorTitle}", "{ErrorDescription}", "");

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
