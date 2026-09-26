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
            "[109-ANCIENT1]",
            ""
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
