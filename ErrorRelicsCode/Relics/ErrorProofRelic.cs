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
            "ERROR",
            "[ERROR · 0.92.2 Multiplayer Preview]\n本Mod会随机组合遗物的触发条件与效果，共有104种触发条件、109种效果，理论上可产生11,336种H/E组合。\n当前E002、E007暂不参与随机生成，因此实际随机池为11,128种组合。\n组合可能极强、极弱、奇怪，或在某些情况下没有效果。\n本Mod仍可能出现异常、报错、卡死、存档问题或无法继续游戏。\n若发现本应生效的ERROR没有效果，或发生严重异常，请记录ERROR编号（如H076-E028）及当时情况。\n反馈：Ciareay@outlook.com",
            ""
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
