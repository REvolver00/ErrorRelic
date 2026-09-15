using System.Collections.Generic;
using System.Threading.Tasks;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorTestRelic : ErrorRelicsRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShouldReceiveCombatHooks => true;

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR TEST",
            "At the start of your turn, deal 3 damage to ALL enemies.",
            "Mercury Hourglass hook test."
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[]
            {
                new DamageVar(3M, ValueProp.Unpowered)
            };
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (player != owner)
            return;

        Flash();

        await CreatureCmd.Damage(
            choiceContext,
            player.Creature.CombatState.HittableEnemies,
            DynamicVars.Damage,
            owner.Creature
        );
    }
}