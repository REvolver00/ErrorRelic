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
            var generated = ErrorGenerator.Generate();

            GeneratedHookId = generated.HookId;
            GeneratedEffectId = generated.EffectId;
        }

        // 这里非常重要。
        //
        // 继续进入 ErrorGeneratedRelic.AfterObtained()，
        // 因此 H004 也能正常触发它自己的 E。
        await base.AfterObtained();
    }


    // =========================================================
    // DEBUG：每个遗物实例自己的悬停信息
    //
    // 同一个 Model ID 的 Localization 是共用的，
    // 所以不能拿主描述判断不同实例。
    //
    // 这里读取的是当前这一件遗物自己的 SavedProperty，
    // 因此可以看到它真正的 Hook / Effect。
    // =========================================================

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            string hookId =
                GeneratedHookId
                    .ToString()
                    .Split('_')[0];

            string effectId =
                GeneratedEffectId
                    .ToString()
                    .Split('_')[0];

            yield return new HoverTip(
                Title,
                $"[DEBUG]\n{hookId}-{effectId}"
            );
        }
    }


    // =========================================================
    // 主描述故意固定
    //
    // 不再使用 GeneratedDescription，
    // 否则多个相同 ID 的 RANDOM TEST 会显示同一份旧描述，
    // 很容易把我们测试搞混。
    // =========================================================

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR",
            "[当？将有？发生]",
            ""
        );


    // =========================================================
    // 图片
    // =========================================================

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
