using System.Collections.Generic;
using System.Threading.Tasks;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;


using ErrorRelics.ErrorRelicsCode.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorTestRelic : ErrorRelicsRelic
{
    // 先把它做成普通遗物，方便测试。
    public override RelicRarity Rarity => RelicRarity.Common;
    public override bool ShouldReceiveCombatHooks => true;

    // 暂时直接在代码里写名字和描述。
    // 这样我们现在完全不用碰 localization JSON。
    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR TEST",
            "At the start of combat, gain 1 Strength.",
            "The first ERROR relic test."
        );

    // 现在先借用模板自带的测试图片。
    // 所以暂时不用制作 error_test_relic.png。
    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();

    // 这就是我们第一个 Hook：
    // “战斗开始之前”。
    public override async Task BeforeCombatStart()
    {
        var owner = Owner;

        // 理论上拿着遗物时 Owner 应该存在。
        // 测试阶段先防止因为没有主人直接炸掉。
        if (owner is null)
        {
            return;
        }

        // 让遗物闪一下，方便我们肉眼确认它触发了。
        Flash();

        // 给玩家 1 点力量。
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            owner.Creature,
            1,
            owner.Creature,
            null
        );
    }
}