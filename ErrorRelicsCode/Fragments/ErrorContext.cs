using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public sealed class ErrorContext
{
    public ErrorContext(
        Player owner,
        PlayerChoiceContext choiceContext,
        DamageVar damage)
    {
        Owner = owner;
        ChoiceContext = choiceContext;
        Damage = damage;
    }

    public Player Owner { get; }

    public PlayerChoiceContext ChoiceContext { get; }

    public DamageVar Damage { get; }
}