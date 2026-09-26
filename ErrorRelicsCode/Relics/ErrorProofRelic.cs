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

    public override bool IsAllowed(
        IRunState runState)
    {
        return false;
    }

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR",
            "[109]",
            ""
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}
