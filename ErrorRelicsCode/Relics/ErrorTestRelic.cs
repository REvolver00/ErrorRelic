using System.Collections.Generic;
using System.Threading.Tasks;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using ErrorRelics.ErrorRelicsCode.Extensions;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorTestRelic : ErrorRelicsRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShouldReceiveCombatHooks => true;

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR TEST",
            "When entering combat, gain 1 Strength.",
            "The first ERROR relic test."
        );

    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();

    // Vajra 原版使用的触发位置：
    // 进入房间以后进行检查。
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        // 不是战斗房间，就什么也不做。
        if (room is not CombatRoom)
            return;

        var owner = Owner;
        if (owner is null)
            return;

        Flash();

        // 触发效果。
        await RunEffect();
    }

    // 目前我们的第一个 Effect：
    // 获得 1 点力量。
    private async Task RunEffect()
    {
        var owner = Owner;
        if (owner is null)
            return;

        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            owner.Creature,
            1,
            owner.Creature,
            null
        );
    }
}