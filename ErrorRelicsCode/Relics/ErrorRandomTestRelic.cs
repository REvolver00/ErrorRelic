using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Saves.Runs;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;
using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Entities.Relics;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorRandomTestRelic : ErrorGeneratedRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    [SavedProperty]
    public bool HasGeneratedDefinition { get; set; }

    [SavedProperty]
    public ErrorHookId GeneratedHookId { get; set; }

    [SavedProperty]
    public ErrorEffectId GeneratedEffectId { get; set; }

    protected override ErrorDefinition Definition =>
        new(
            GeneratedHookId,
            GeneratedEffectId
        );

    public override Task AfterObtained()
    {
        if (!HasGeneratedDefinition)
        {
            var generated = ErrorGenerator.Generate();

            GeneratedHookId = generated.HookId;
            GeneratedEffectId = generated.EffectId;

            HasGeneratedDefinition = true;
        }

        return base.AfterObtained();
    }


    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR RANDOM TEST",
            GeneratedDescription,
            GeneratedFlavor
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();
}