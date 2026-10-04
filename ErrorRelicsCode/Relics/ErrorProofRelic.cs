using System.Collections.Generic;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;
using ErrorRelics.ErrorRelicsCode.Config;
using ErrorRelics.ErrorRelicsCode.Gameplay;
using MegaCrit.Sts2.Core.Saves.Runs;

using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorProofRelic : ErrorRelicsRelic
{
    public const int MaxDataCrackCharge = 4;
    private int _dataCrackCharge;

    [SavedProperty]
    public int DataCrackCharge
    {
        get => _dataCrackCharge;
        set
        {
            AssertMutable();
            _dataCrackCharge = System.Math.Clamp(value, 0, MaxDataCrackCharge);
            InvokeDisplayAmountChanged();
        }
    }

    public override bool ShowCounter => ErrorDataCrackService.IsDiscoveryActive;
    public override int DisplayAmount => DataCrackCharge;

    public bool TrySpendDataCrackCharge()
    {
        AssertMutable();
        if (!ErrorDataCrackService.IsDiscoveryActive || DataCrackCharge <= 0)
            return false;
        DataCrackCharge--;
        return true;
    }

    // Optional Data Crack rules: every combat starts by granting one charge,
    // capped at four. This is deliberately separate from ERROR activation.
    public override async System.Threading.Tasks.Task BeforeCombatStart()
    {
        if (ErrorDataCrackService.IsDiscoveryActive)
            DataCrackCharge = System.Math.Min(MaxDataCrackCharge, DataCrackCharge + 1);
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public override RelicRarity Rarity =>
        RelicRarity.Common;

    // The Proof is synthetic and is never drawn from a vanilla relic bag.
    // Making it stackable prevents RelicCmd.Obtain from trying to remove
    // this custom relic from Player/SharedRelicGrabBag.
    public override bool IsStackable => true;

    public override bool IsAllowed(
        IRunState runState)
    {
        return false;
    }

    public override List<(string, string)>? Localization
    {
        get
        {
            string description =
                "[ERROR · 0.93.1]\n" +
                "红镐子是本局 ERROR 的启动凭证。ERROR 会把原版遗物的触发条件与效果重新接线。\n" +
                "当前核心池：104 Hooks × 109 Effects = 11,336 个理论 H/E 组合。\n" +
                "任意玩家获得红镐子后，本局 ERROR 永久开启；之后失去红镐子也不会关闭。\n";

            if (ErrorDataCrackService.IsDiscoveryActive)
            {
                description +=
                    "【发现模式】每场战斗开始时获得1充能（最多4）。新 ERROR 的 H/E 初始均隐藏；右键自己的 ERROR 第一次消耗1充能揭示 Effect，第二次再消耗1充能揭示 Hook。\n";
            }

            description +=
                "ERROR 可能极强、极弱、无效或产生意外组合。若遇到严重异常，请记录 H/E 编号与当时场景。\n" +
                "反馈：Ciareay@outlook.com";

            return new RelicLoc("ERROR PROOF", description, "");
        }
    }

    public override string PackedIconPath =>
        "error_proof.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "error_proof_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "error_proof.png".BigRelicImagePath();
}
