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
    protected abstract ErrorDefinition Definition { get; }

    protected ErrorHookId HookId => Definition.HookId;

    protected ErrorEffectId EffectId => Definition.EffectId;

    public override bool ShouldReceiveCombatHooks => true;


    // =========================================================
    // 所有 Fragment 共用的动态数值
    // =========================================================

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[]
            {
                new DamageVar(3M, ValueProp.Unpowered),
                new GoldVar(300)
            };
        }
    }


    // =========================================================
    // 自动生成描述
    // =========================================================

    protected string GeneratedDescription =>
        ErrorHookRegistry.GetText(HookId)
        + " "
        + ErrorEffectRegistry.GetText(EffectId);

    protected string GeneratedFlavor =>
        $"ERROR Fragment: {HookId} + {EffectId}";


    // =========================================================
    // H001：进入战斗
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

        // AfterRoomEntered 没有真正的 PlayerChoiceContext，
        // 所以这里使用 ThrowingPlayerChoiceContext。
        var context = new ErrorContext(
            owner,
            new ThrowingPlayerChoiceContext(),
            DynamicVars.Damage,
            DynamicVars.Gold
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }


    // =========================================================
    // H002：玩家回合开始
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

        // 这里游戏已经给了真正的 choiceContext，
        // 直接传进去。
        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars.Damage,
            DynamicVars.Gold
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }
}
