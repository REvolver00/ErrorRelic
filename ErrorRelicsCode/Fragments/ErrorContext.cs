using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public sealed class ErrorContext
{
    public ErrorContext(
        Player owner,
        PlayerChoiceContext choiceContext,
        DynamicVarSet vars,
        CardPlay? cardPlay = null)
    {
        Owner = owner;
        ChoiceContext = choiceContext;
        Vars = vars;

        // 保留旧 Effect 已经在用的快捷入口。
        Damage = vars.Damage;
        Gold = vars.Gold;

        CardPlay = cardPlay;
    }

    public Player Owner { get; }

    public PlayerChoiceContext ChoiceContext { get; }

    // =========================================================
    // 当前 ERROR 遗物的完整 DynamicVarSet。
    //
    // 新 Effect 如果有自己的原版命名数值，
    // 直接从这里读取，不需要把所有来源遗物的数值
    // 都硬塞进 ErrorContext 的构造参数。
    // =========================================================
    public DynamicVarSet Vars { get; }

    public DamageVar Damage { get; }

    public GoldVar Gold { get; }

    public CardPlay? CardPlay { get; }
}
