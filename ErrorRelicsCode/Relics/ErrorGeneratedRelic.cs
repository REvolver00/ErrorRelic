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
            DynamicVars.Gold
        );

        await ErrorEffectRegistry.ExecuteAsync(
            EffectId,
            context
        );
    }
}
