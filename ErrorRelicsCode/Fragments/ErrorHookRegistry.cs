using System.Collections.Generic;
using System.Linq;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    // H015
    // 来源遗物：Neow's Talisman
    //
    // H016
    // 来源遗物：Pael's Horn
    //
    // H017
    // 来源遗物：Neow's Torment
    //
    // H018
    // 来源遗物：Storybook
    //
    // H019
    // 来源遗物：Jewelry Box
    //
    // H020
    // 来源遗物：Tanx's Whistle
    //
    // Hook：
    // 获得 / 拾取遗物时
    //
    // 十五个 Fragment 来源不同，
    // 所以保留十五个独立 Fragment ID。
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
               || hookId == ErrorHookId.H009_AfterObtained
               || hookId == ErrorHookId.H010_AfterObtained
               || hookId == ErrorHookId.H011_AfterObtained
               || hookId == ErrorHookId.H012_AfterObtained
               || hookId == ErrorHookId.H013_AfterObtained
               || hookId == ErrorHookId.H014_AfterObtained
               || hookId == ErrorHookId.H015_AfterObtained
               || hookId == ErrorHookId.H016_AfterObtained
               || hookId == ErrorHookId.H017_AfterObtained
               || hookId == ErrorHookId.H018_AfterObtained
               || hookId == ErrorHookId.H019_AfterObtained
               || hookId == ErrorHookId.H020_AfterObtained;
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
    // H021
    // 来源遗物：Happy Flower
    //
    // 原版 Hook：
    // AfterSideTurnStart
    //
    // 只在 Owner 所在一侧的回合开始时推进计数。
    // =========================================================

    public static bool MatchesHappyFlowerSideTurnStart(
        ErrorHookId hookId,
        Player owner,
        IReadOnlyList<Creature> participants)
    {
        return hookId == ErrorHookId.H021_Every3TurnsHappyFlower
               && participants.Contains(owner.Creature);
    }


    // =========================================================
    // H022
    // 来源遗物：Stone Calendar
    //
    // AfterSideTurnStart 用于状态/计数显示；
    // BeforeSideTurnEnd 在第 7 回合真正触发 Effect。
    // =========================================================

    public static bool MatchesStoneCalendarSide(
        ErrorHookId hookId,
        Player owner,
        IEnumerable<Creature> participants)
    {
        return hookId == ErrorHookId.H022_EndOfTurn7StoneCalendar
               && participants.Contains(owner.Creature);
    }


    // =========================================================
    // H023
    // 来源遗物：Pendulum
    //
    // 原版 Hook：
    // AfterPlayerTurnStart
    //
    // 只在 player == Owner 时推进 3 回合持久计数。
    // =========================================================

    public static bool MatchesPendulumPlayerTurnStart(
        ErrorHookId hookId,
        Player owner,
        Player player)
    {
        return hookId == ErrorHookId.H023_Every3TurnsPendulum
               && player == owner;
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

            ErrorHookId.H010_AfterObtained
                => "When obtained,",

            ErrorHookId.H011_AfterObtained
                => "When obtained,",

            ErrorHookId.H012_AfterObtained
                => "When obtained,",

            ErrorHookId.H013_AfterObtained
                => "When obtained,",

            ErrorHookId.H014_AfterObtained
                => "When obtained,",

            ErrorHookId.H015_AfterObtained
                => "When obtained,",

            ErrorHookId.H016_AfterObtained
                => "When obtained,",

            ErrorHookId.H017_AfterObtained
                => "When obtained,",

            ErrorHookId.H018_AfterObtained
                => "When obtained,",

            ErrorHookId.H019_AfterObtained
                => "When obtained,",

            ErrorHookId.H020_AfterObtained
                => "When obtained,",

            ErrorHookId.H021_Every3TurnsHappyFlower
                => "Every 3 turns,",

            ErrorHookId.H022_EndOfTurn7StoneCalendar
                => "At the end of turn 7,",

            ErrorHookId.H023_Every3TurnsPendulum
                => "Every 3 turns,",

            _ => "ERROR:"
        };
    }
}