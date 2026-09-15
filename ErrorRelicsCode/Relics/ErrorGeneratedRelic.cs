using System.Collections.Generic;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ErrorRelics.ErrorRelicsCode.Relics;

public abstract class ErrorGeneratedRelic : ErrorRelicsRelic
{
    // 子遗物只需要告诉系统：
    // 我要哪个 H？
    // 我要哪个 E？
    protected abstract ErrorHookId HookId { get; }

    protected abstract ErrorEffectId EffectId { get; }


    public override bool ShouldReceiveCombatHooks => true;


    // 暂时保留 Mercury Hourglass 的 DamageVar。
    // 后面我们再把数值也进一步积木化。
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


    // 自动拼遗物描述
    protected string GeneratedDescription =>
        ErrorHookRegistry.GetText(HookId)
        + " "
        + ErrorEffectRegistry.GetText(EffectId);

    protected string GeneratedFlavor =>
        $"ERROR Fragment: {HookId} + {EffectId}";


    // =========================================================
    // 游戏 API 插口：进入房间
    // =========================================================

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesAfterRoomEntered(
                HookId,
                room))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            new ThrowingPlayerChoiceContext(),
            DynamicVars.Damage
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }


    // =========================================================
    // 游戏 API 插口：玩家回合开始
    // =========================================================

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesAfterPlayerTurnStart(
                HookId,
                owner,
                player))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars.Damage
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }
}
