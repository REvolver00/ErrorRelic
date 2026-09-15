using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorHookRegistry
{
    // H001
    // 来源：Vajra
    public static bool MatchesAfterRoomEntered(
        ErrorHookId hookId,
        AbstractRoom room)
    {
        return hookId == ErrorHookId.H001_EnterCombat
               && room is CombatRoom;
    }

    // H002
    // 来源：Mercury Hourglass
    public static bool MatchesAfterPlayerTurnStart(
        ErrorHookId hookId,
        Player owner,
        Player player)
    {
        return hookId == ErrorHookId.H002_PlayerTurnStart
               && player == owner;
    }

    public static string GetText(ErrorHookId hookId)
    {
        return hookId switch
        {
            ErrorHookId.H001_EnterCombat
                => "When entering combat,",

            ErrorHookId.H002_PlayerTurnStart
                => "At the start of your turn,",

            _ => "ERROR:"
        };
    }
}