using System.Collections.Generic;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Combat;
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
    // Old Coin / Distinguished Cape
    // 都使用 AfterObtained。
    //
    // ERROR 遗物统一开启这个入口。
    // 至于当前 H 是否属于 AfterObtained，
    // 由 ErrorHookRegistry 判断。
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
    // 来源遗物：Mummified Hand
    //
    // Hook：
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
    // 来源遗物：Old Coin
    //
    // H006
    // 来源遗物：Distinguished Cape
    //
    // Hook：
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
    // 来源遗物：Vajra
    //
    // Hook：
    // 进入战斗
    // =========================================================

    public override async Task AfterRoomEntered(
        AbstractRoom room)
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
    // 来源遗物：Mercury Hourglass
    //
    // Hook：
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
    // 来源遗物：Intimidating Helmet
    //
    // Hook：
    // BeforeCardPlayed
    // 自己打出的牌使用至少 2 点 Energy
    // =========================================================

    public override async Task BeforeCardPlayed(
        CardPlay cardPlay)
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


    // =========================================================
    // H007
    // 来源遗物：Toolbox
    //
    // Hook：
    // 第一回合起始手牌抽取之前
    //
    // 原版 Toolbox 使用：
    //
    // BeforeHandDraw(
    //     Player player,
    //     PlayerChoiceContext choiceContext,
    //     ICombatState combatState)
    //
    // combatState 原版没有参与条件判断，
    // 所以这里只负责接住原生 Hook。
    // =========================================================

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesBeforeHandDraw(
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
}