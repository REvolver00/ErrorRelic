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
    public bool DefinitionLocked { get; set; }


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
        try
        {
            VisualSourceOutlinePath =
                source.IconOutline.ResourcePath;
        }
        catch
        {
            VisualSourceOutlinePath =
                string.Empty;
        }

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
            var generated = ErrorGenerator.Generate(Owner, Id, $"obtained:{Owner.Relics.ToList().IndexOf(this)}");

            GeneratedHookId = generated.HookId;
            GeneratedEffectId = generated.EffectId;
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
    public string FullDescription => DefinitionLocked || Owner != null
        ? ErrorDescriptionText.Describe(GeneratedHookId, GeneratedEffectId, OriginalName,
            LocManager.Instance == null ? "colorless" : RunManager.Instance.GetLocalCharacterEnergyIconPrefix() ?? "colorless")
        : "获得时随机组合一个触发条件和一个效果。";

    private static string? OriginalName(string table, string key) => LocManager.Instance == null
        ? null : LocString.GetIfExists(table, key)?.GetFormattedText();

    public override List<(string, string)>? Localization => new RelicLoc(
        "ERROR", "{ErrorDescription}", "");

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
