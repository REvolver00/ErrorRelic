using System.Collections.Generic;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Entities.Cards;
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
    // 拾取效果
    //
    // Old Coin 本身也是 true。
    //
    // ERROR 遗物统一开启这个入口。
    // 至于当前 Hook 是不是 H004，
    // 由下面的 MatchesAfterObtained 决定。
    // =========================================================

    public override bool HasUponPickupEffect => true;


    // =========================================================
    // Fragment 数值
    // =========================================================

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[]
            {
                // Mercury Hourglass
                new DamageVar(3M, ValueProp.Unpowered),

                // Old Coin
                new GoldVar(300),

                // Intimidating Helmet
                new EnergyVar(2)
            };
        }
    }


    // =========================================================
    // 自动描述
    // =========================================================

    protected string GeneratedDescription =>
        ErrorHookRegistry.GetText(HookId)
        + " "
        + ErrorEffectRegistry.GetText(EffectId);

    protected string GeneratedFlavor =>
        $"ERROR Fragment: {HookId} + {EffectId}";
// =========================================================
// H005
// 来源：Mummified Hand
//
// 打出 Power 牌之后
// =========================================================

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesAfterPowerCardPlayed(
                HookId,
                owner,
                cardPlay))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars.Damage,
            DynamicVars.Gold,
            cardPlay
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }


    // =========================================================
    // H004
    // 来源：Old Coin
    //
    // 真正把遗物拿到手时
    // =========================================================

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesAfterObtained(
                HookId))
            return;

        Flash();

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
    // H001
    // 来源：Vajra
    // 进入战斗
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
            DynamicVars.Damage,
            DynamicVars.Gold
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }


    // =========================================================
    // H002
    // 来源：Mercury Hourglass
    // 自己的回合开始
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
            DynamicVars.Damage,
            DynamicVars.Gold
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }


    // =========================================================
    // H003
    // 来源：Intimidating Helmet
    //
    // BeforeCardPlayed
    // 自己打出的牌使用至少 2 点能量
    // =========================================================

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesBeforeCardPlayed(
                HookId,
                owner,
                cardPlay,
                DynamicVars.Energy.IntValue))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            new ThrowingPlayerChoiceContext(),
            DynamicVars.Damage,
            DynamicVars.Gold,
            cardPlay
        );


        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }
}
