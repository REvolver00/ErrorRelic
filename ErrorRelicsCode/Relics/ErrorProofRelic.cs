using System.Collections.Generic;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;

using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorProofRelic : ErrorRelicsRelic
{
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

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR PROOF",
            "[ERROR · 0.93.1]\n" +
            "红镐子是本局 ERROR 的启动凭证。ERROR 会把原版遗物的触发条件与效果重新接线。\n" +
            "当前核心池：104 Hooks × 109 Effects = 11,336 个理论 H/E 组合。\n" +
            "任意玩家获得红镐子后，本局 ERROR 永久开启；之后失去红镐子也不会关闭。\n" +
            "ERROR 可能极强、极弱、无效或产生意外组合。若遇到严重异常，请记录 H/E 编号与当时场景。\n" +
            "反馈：Ciareay@outlook.com",
            ""
        );

    public override string PackedIconPath =>
        "error_proof.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "error_proof_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "error_proof.png".BigRelicImagePath();
}
