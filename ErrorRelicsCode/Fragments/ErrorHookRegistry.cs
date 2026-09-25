using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Combat;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorHookRegistry
{
    // =========================================================
    // H001
    // 来源遗物：Vajra
    //
    // Hook：
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
    // 来源遗物：Mercury Hourglass
    //
    // Hook：
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
    // 来源遗物：Intimidating Helmet
    //
    // Hook：
    // 自己打出一张牌
    // 且这次打牌使用至少 2 点 Energy
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
    // 来源遗物：Old Coin
    //
    // H006
    // 来源遗物：Distinguished Cape
    //
    // H008
    // 来源遗物：Astrolabe
    //
    // H009
    // 来源遗物：Royal Stamp
    //
    // Hook：
    // 获得 / 拾取遗物时
    //
    // 四个 Fragment 来源不同，
    // 所以保留四个独立 Fragment ID。
    //
    // 但是原版 Hook 都是 AfterObtained，
    // 因此共用这一套代码。
    // =========================================================

    public static bool MatchesAfterObtained(
        ErrorHookId hookId)
    {
        return hookId == ErrorHookId.H004_AfterObtained
               || hookId == ErrorHookId.H006_AfterObtained
               || hookId == ErrorHookId.H008_AfterObtained
               || hookId == ErrorHookId.H009_AfterObtained;
    }


    // =========================================================
    // H005
    // 来源遗物：Mummified Hand
    //
    // Hook：
    // 战斗中
    // 自己打出一张 Power 牌之后
    // =========================================================

    public static bool MatchesAfterPowerCardPlayed(
        ErrorHookId hookId,
        Player owner,
        CardPlay cardPlay)
    {
        return hookId == ErrorHookId.H005_PowerCardPlayed
               && CombatManager.Instance.IsInProgress
               && cardPlay.Card.Owner == owner
               && cardPlay.Card.Type == CardType.Power;
    }


    // =========================================================
    // H007
    // 来源遗物：Toolbox
    //
    // Hook：
    // 第一回合起始手牌抽取之前
    //
    // 原版源码：
    //
    // BeforeHandDraw
    // + player == Owner
    // + Owner.PlayerCombatState.TurnNumber == 1
    //
    // combatState 参数原版没有实际使用，
    // 所以这里也不需要检查。
    // =========================================================

    public static bool MatchesBeforeHandDraw(
        ErrorHookId hookId,
        Player owner,
        Player player)
    {
        return hookId == ErrorHookId.H007_FirstTurnBeforeHandDraw
               && player == owner
               && owner.PlayerCombatState.TurnNumber == 1;
    }


    // =========================================================
    // 自动描述
    // =========================================================

    public static string GetText(
        ErrorHookId hookId)
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

            ErrorHookId.H005_PowerCardPlayed
                => "After you play a Power card,",

            ErrorHookId.H006_AfterObtained
                => "When obtained,",

            ErrorHookId.H007_FirstTurnBeforeHandDraw
                => "Before drawing your opening hand,",

            ErrorHookId.H008_AfterObtained
                => "When obtained,",

            ErrorHookId.H009_AfterObtained
                => "When obtained,",

            _ => "ERROR:"
        };
    }
}