using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorHookRegistry
{
    // =========================================================
    // H001
    // 来源：Vajra
    // 进入战斗
    // =========================================================

    public static bool MatchesAfterRoomEntered(
        ErrorHookId hookId,
        AbstractRoom room)
    {
        return hookId == ErrorHookId.H001_EnterCombat
               && room is CombatRoom;
    }


    // =========================================================
    // H002
    // 来源：Mercury Hourglass
    // 自己的回合开始
    // =========================================================

    public static bool MatchesAfterPlayerTurnStart(
        ErrorHookId hookId,
        Player owner,
        Player player)
    {
        return hookId == ErrorHookId.H002_PlayerTurnStart
               && player == owner;
    }


    // =========================================================
    // H003
    // 来源：Intimidating Helmet
    //
    // 自己打出一张牌
    // 且这次打牌使用至少 2 点能量
    // =========================================================

    public static bool MatchesBeforeCardPlayed(
        ErrorHookId hookId,
        Player owner,
        CardPlay cardPlay,
        int minimumEnergy)
    {
        return hookId == ErrorHookId.H003_PlayCardEnergy2Plus
               && cardPlay.Card.Owner == owner
               && cardPlay.Resources.EnergyValue >= minimumEnergy;
    }


    // =========================================================
    // H004
    // 来源：Old Coin
    // 获得 / 拾取遗物时
    // =========================================================

    public static bool MatchesAfterObtained(
        ErrorHookId hookId)
    {
        return hookId == ErrorHookId.H004_AfterObtained;
    }


    // =========================================================
    // 自动描述
    // =========================================================

    public static string GetText(ErrorHookId hookId)
    {
        return hookId switch
        {
            ErrorHookId.H001_EnterCombat
                => "When entering combat,",

            ErrorHookId.H002_PlayerTurnStart
                => "At the start of your turn,",

            ErrorHookId.H003_PlayCardEnergy2Plus
                => "When you play a card using 2 or more Energy,",

            ErrorHookId.H004_AfterObtained
                => "When obtained,",

            _ => "ERROR:"
        };
    }
}
