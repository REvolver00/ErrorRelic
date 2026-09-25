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
    // 来源遗物：
    //
    // H004：Old Coin
    // H006：Distinguished Cape
    // H008：Astrolabe
    // H009：Royal Stamp
    // H010：Nutritious Soup
    // H011：Pael's Claw
    // H012：Sand Castle
    // H013：War Paint
    // H014：Whetstone
    //
    // 以上遗物原版都使用 AfterObtained。
    //
    // ERROR 遗物统一开启这个入口。
    // 当前 H 是否真正属于 AfterObtained，
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
                // =================================================
                // 来源遗物：Mercury Hourglass
                // 原版数值：3 点伤害
                // =================================================
                new DamageVar(
                    3M,
                    ValueProp.Unpowered
                ),

                // =================================================
                // 来源遗物：Old Coin
                // 原版数值：300 Gold
                // =================================================
                new GoldVar(300),

                // =================================================
                // 来源遗物：Intimidating Helmet
                // 原版触发阈值：至少使用 2 Energy
                // =================================================
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
    //
    // 来源遗物：
    // Mummified Hand
    //
    // 原版 Hook：
    // AfterCardPlayed(
    //     PlayerChoiceContext choiceContext,
    //     CardPlay cardPlay)
    //
    // 原版触发条件：
    // 1. Combat 正在进行
    // 2. 这张牌属于自己
    // 3. CardType == Power
    //
    // ERROR：
    // 当 H005 条件成立时执行当前随机 Effect。
    //
    // 重要：
    // 这里直接保留游戏传进来的 choiceContext。
    // 如果 Effect 本身需要 PlayerChoice，
    // 必须优先使用这个原生 Context。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H004
    //
    // 来源遗物：
    // Old Coin
    //
    // 原版 Hook：
    // AfterObtained()
    //
    //
    // H006
    //
    // 来源遗物：
    // Distinguished Cape
    //
    // H008
    // 来源遗物：Astrolabe
    //
    // H009
    // 来源遗物：Royal Stamp
    //
    // H010
    // 来源遗物：Nutritious Soup
    //
    // H011
    // 来源遗物：Pael's Claw
    //
    // H012
    // 来源遗物：Sand Castle
    //
    // H013
    // 来源遗物：War Paint
    //
    // H014
    // 来源遗物：Whetstone
    //
    // 原版 Hook：
    // AfterObtained()
    //
    // 这些 Fragment 使用相同原生 Hook，
    // 所以共用这一入口。
    //
    // 注意：
    // AfterObtained 原版没有传入 PlayerChoiceContext。
    //
    // 当前仍使用 ThrowingPlayerChoiceContext。
    // 如果以后 E007 等 Choice Effect
    // 与这些 AfterObtained Hook 组合发生问题，
    // 将由 Compatibility 层单独处理。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H001
    //
    // 来源遗物：
    // Vajra
    //
    // 原版 Hook：
    // AfterRoomEntered(AbstractRoom room)
    //
    // 原版条件：
    // room is CombatRoom
    //
    // Vajra 原版同样使用：
    // new ThrowingPlayerChoiceContext()
    //
    // 因此这里继续忠于原版 Hook 环境。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H002
    //
    // 来源遗物：
    // Mercury Hourglass
    //
    // Hook：
    // 自己的回合开始
    //
    // 原生 Hook 会提供 PlayerChoiceContext，
    // 所以直接向 Effect 传递原生 Context。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H003
    //
    // 来源遗物：
    // Intimidating Helmet
    //
    // 原版 Hook：
    // BeforeCardPlayed(CardPlay cardPlay)
    //
    // 原版条件：
    // 自己打出的牌使用至少 2 点 Energy。
    //
    // 这个原生 Hook 没有提供 PlayerChoiceContext，
    // 因此当前保持 ThrowingPlayerChoiceContext。
    //
    // Choice 类 Effect 如果与 H003 出现兼容问题，
    // 后续由 Compatibility 层单独处理。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H007
    //
    // 来源遗物：
    // Toolbox
    //
    // 原版 Hook：
    //
    // BeforeHandDraw(
    //     Player player,
    //     PlayerChoiceContext choiceContext,
    //     ICombatState combatState)
    //
    // 原版触发条件：
    // 1. player == Owner
    // 2. TurnNumber == 1
    //
    // combatState 在原版 Toolbox 条件判断中没有参与。
    //
    // 这里直接保留原生 choiceContext。
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

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }
}