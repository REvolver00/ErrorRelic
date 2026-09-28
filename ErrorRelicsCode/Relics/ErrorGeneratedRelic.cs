using System.Collections.Generic;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
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
    // H015：Neow's Talisman
    // H016：Pael's Horn
    // H017：Neow's Torment
    // H018：Storybook
    // H019：Jewelry Box
    // H020：Tanx's Whistle
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
                new EnergyVar(2),

                // =================================================
                // 来源遗物：Happy Flower
                // 原版效果：获得 1 Energy
                // =================================================
                new EnergyVar(
                    "HappyFlowerEnergy",
                    1
                ),

                // =================================================
                // 来源遗物：Happy Flower / Pendulum
                // 原版回合阈值：3
                // =================================================
                new DynamicVar(
                    "Turns",
                    3M
                ),

                // =================================================
                // 来源遗物：Stone Calendar
                // 原版效果：52 点 Unpowered 伤害
                // =================================================
                new DamageVar(
                    "StoneCalendarDamage",
                    52M,
                    ValueProp.Unpowered
                ),

                // =================================================
                // 来源遗物：Stone Calendar
                // 原版触发回合：7
                // =================================================
                new DynamicVar(
                    "DamageTurn",
                    7M
                ),

                // =================================================
                // 来源遗物：Pendulum
                // 原版效果：抽 1 张牌
                // =================================================
                new CardsVar(
                    "PendulumCards",
                    1
                ),

                // Sparkling Rouge：Strength 1
                new DynamicVar(
                    "SparklingRougeStrength",
                    1M
                ),

                // Sparkling Rouge：Dexterity 1
                new DynamicVar(
                    "SparklingRougeDexterity",
                    1M
                ),

                // Oddly Smooth Stone：Dexterity 1
                new DynamicVar(
                    "OddlySmoothStoneDexterity",
                    1M
                ),

                // Horn Cleat：Block 14
                new DynamicVar(
                    "HornCleatBlock",
                    14M
                ),

                // Captain's Wheel：Block 18
                new DynamicVar(
                    "CaptainsWheelBlock",
                    18M
                ),

                // Candelabra：Energy 2
                new DynamicVar(
                    "CandelabraEnergy",
                    2M
                ),

                // =================================================
                // 来源遗物：Meal Ticket
                // 用户本机反编译源码：HealVar(15)
                // =================================================
                new DynamicVar(
                    "MealTicketHeal",
                    15M
                ),

                // =================================================
                // 来源遗物：Joss Paper
                // 用户本机反编译源码：ExhaustAmount = 5
                // =================================================
                new DynamicVar(
                    "JossPaperExhaustAmount",
                    5M
                ),

                // =================================================
                // 来源遗物：Joss Paper
                // 用户本机反编译源码：CardsVar(1)
                // =================================================
                new CardsVar(
                    "JossPaperCards",
                    1
                ),

                // =================================================
                // 来源遗物：Planisphere
                // 用户本机反编译源码：HealVar(5)
                // =================================================
                new DynamicVar(
                    "PlanisphereHeal",
                    5M
                ),

                // H032-E065 expansion values
                new DynamicVar("BigMushroomMaxHp", 20M),
                new DynamicVar("LetterOpenerCards", 3M),
                new DynamicVar("LetterOpenerDamage", 5M),
                new DynamicVar("KusarigamaCards", 3M),
                new DynamicVar("KusarigamaDamage", 6M),
                new DynamicVar("OrnamentalFanCards", 3M),
                new DynamicVar("OrnamentalFanBlock", 4M),
                new DynamicVar("NunchakuCards", 10M),
                new DynamicVar("NunchakuEnergy", 1M),
                new DynamicVar("TuningForkCards", 10M),
                new DynamicVar("TuningForkBlock", 7M),
                new DynamicVar("BagOfMarblesVulnerable", 1M),
                new DynamicVar("TwistedFunnelPoison", 4M),
                new DynamicVar("AkabekoVigor", 8M),
                new DynamicVar("CentennialPuzzleCards", 3M),
                new DynamicVar("OrichalcumBlock", 6M),
                new DynamicVar("RippleBasinBlock", 4M),
                new DynamicVar("ParryingShieldBlockThreshold", 10M),
                new DynamicVar("ParryingShieldDamage", 6M),
                new DynamicVar("PantographHeal", 25M),
                new DynamicVar("BloodVialHeal", 2M),
                new DynamicVar("AnchorBlock", 10M),
                new DynamicVar("DataDiskFocus", 1M),
                new DynamicVar("FestivePopperDamage", 9M),
                new DynamicVar("LeesWaffleMaxHp", 7M),
                new DynamicVar("AbacusBlock", 6M),
                new DynamicVar("ScreamingFlagonDamage", 20M),
                new DynamicVar("GamePieceCards", 1M),
                new DynamicVar("RedMaskWeak", 1M),
                new DynamicVar("BrimstoneSelfStrength", 2M),
                new DynamicVar("BrimstoneEnemyStrength", 1M),
                new DynamicVar("NinjaScrollShivs", 3M),
                new DynamicVar("ChosenCheeseMaxHp", 1M),
                new DynamicVar("MeatOnTheBoneHeal", 12M),
                new DynamicVar("ReptileTrinketStrength", 3M),
                new DynamicVar("RoyalPoisonDamage", 4M)
            };
        }
    }


    // =========================================================
    // 回合计数 Fragment 状态
    //
    // H021 Happy Flower / H023 Pendulum：
    // TurnsSeen 是原版 [SavedProperty]。
    // 战斗结束不清零，所以跨战斗继续累计。
    //
    // H022 Stone Calendar：
    // 不保存跨战斗计数，直接看本场 TurnNumber。
    // =========================================================

    private bool _isCounterActivating;

    private int _turnsSeen;


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    // =========================================================

    private bool _jossPaperIsActivating;

    private int _jossPaperCardsExhausted;

    private int _jossPaperEtherealCount;

    // H032-H061 expansion state
    private int _letterOpenerSkillsThisTurn;
    private int _kusarigamaAttacksThisTurn;
    private int _ornamentalFanAttacksThisTurn;
    private int _nunchakuAttacksPlayed;
    private int _tuningForkSkillsPlayed;
    private bool _centennialPuzzleUsedThisCombat;
    private bool _orichalcumShouldTrigger;
    private bool _rippleBasinAttackPlayedThisTurn;

    [SavedProperty]
    public int NunchakuAttacksPlayed
    {
        get => _nunchakuAttacksPlayed;
        set { AssertMutable(); _nunchakuAttacksPlayed = value; }
    }

    [SavedProperty]
    public int TuningForkSkillsPlayed
    {
        get => _tuningForkSkillsPlayed;
        set { AssertMutable(); _tuningForkSkillsPlayed = value; }
    }

    private async Task ExecuteExpandedEffect(
        PlayerChoiceContext choiceContext,
        CardPlay? cardPlay = null)
    {
        var owner = Owner;
        if (owner is null) return;
        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId, EffectId,
            new ErrorContext(owner, choiceContext, DynamicVars, cardPlay));
    }


    private bool IsCounterActivating
    {
        get => _isCounterActivating;

        set
        {
            AssertMutable();
            _isCounterActivating = value;
            InvokeDisplayAmountChanged();
        }
    }


    [SavedProperty]
    public int TurnsSeen
    {
        get => _turnsSeen;

        set
        {
            AssertMutable();
            _turnsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    //
    // 完整保留用户本机反编译源码里的：
    // - CardsExhausted SavedProperty
    // - EtherealCount 延迟结算
    // - Active 状态（阈值前 1）
    // =========================================================

    private bool JossPaperIsActivating
    {
        get => _jossPaperIsActivating;

        set
        {
            AssertMutable();
            _jossPaperIsActivating = value;
            InvokeDisplayAmountChanged();
        }
    }


    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int JossPaperCardsExhausted
    {
        get => _jossPaperCardsExhausted;

        set
        {
            AssertMutable();

            _jossPaperCardsExhausted = value;

            Status =
                (decimal)_jossPaperCardsExhausted
                    == DynamicVars[
                        "JossPaperExhaustAmount"
                    ].BaseValue - 1M
                    ? RelicStatus.Active
                    : RelicStatus.Normal;

            InvokeDisplayAmountChanged();
        }
    }


    private int JossPaperEtherealCount
    {
        get => _jossPaperEtherealCount;

        set
        {
            AssertMutable();
            _jossPaperEtherealCount = value;
        }
    }


    public override bool ShowCounter
    {
        get
        {
            if (HookId == ErrorHookId.H021_Every3TurnsHappyFlower
                || HookId == ErrorHookId.H023_Every3TurnsPendulum
                || HookId == ErrorHookId.H030_Every5ExhaustsJossPaper)
            {
                return true;
            }

            if (HookId
                == ErrorHookId.H022_EndOfTurn7StoneCalendar)
            {
                return DisplayAmount > -1;
            }

            return false;
        }
    }


    public override int DisplayAmount
    {
        get
        {
            if (HookId == ErrorHookId.H021_Every3TurnsHappyFlower
                || HookId == ErrorHookId.H023_Every3TurnsPendulum)
            {
                if (!IsCounterActivating)
                {
                    return TurnsSeen;
                }

                return DynamicVars["Turns"].IntValue;
            }

            if (HookId
                == ErrorHookId.H030_Every5ExhaustsJossPaper)
            {
                if (!JossPaperIsActivating)
                {
                    return JossPaperCardsExhausted;
                }

                return DynamicVars[
                    "JossPaperExhaustAmount"
                ].IntValue;
            }

            if (HookId
                == ErrorHookId.H022_EndOfTurn7StoneCalendar)
            {
                if (!CombatManager.Instance.IsInProgress)
                {
                    return -1;
                }

                if (IsCanonical)
                {
                    return -1;
                }

                int damageTurn =
                    DynamicVars["DamageTurn"].IntValue;

                if (IsCounterActivating)
                {
                    return damageTurn;
                }

                int turnNumber =
                    Owner.PlayerCombatState.TurnNumber;

                if (turnNumber >= damageTurn)
                {
                    return -1;
                }

                return turnNumber;
            }

            return 0;
        }
    }


    // Pendulum 原版使用抽牌遗物音效。
    public override string FlashSfx =>
        HookId == ErrorHookId.H023_Every3TurnsPendulum
        || HookId == ErrorHookId.H030_Every5ExhaustsJossPaper
            ? "event:/sfx/ui/relic_activate_draw"
            : base.FlashSfx;


    private async Task DoCounterActivateVisuals()
    {
        IsCounterActivating = true;

        Flash();

        await Cmd.Wait(1f);

        IsCounterActivating = false;
    }


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    // =========================================================

    private async Task DoJossPaperActivateVisuals()
    {
        JossPaperIsActivating = true;

        Flash();

        await Cmd.Wait(1f);

        JossPaperIsActivating = false;
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

        // H034 LetterOpener: every 3 Skills this turn.
        if (HookId == ErrorHookId.H034_Every3SkillsThisTurnLetterOpener
            && CombatManager.Instance.IsInProgress
            && cardPlay.Card.Owner == owner
            && cardPlay.Card.Type == CardType.Skill)
        {
            _letterOpenerSkillsThisTurn++;
            if (_letterOpenerSkillsThisTurn % DynamicVars["LetterOpenerCards"].IntValue == 0)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext, cardPlay);
            }
            return;
        }

        // H035 Kusarigama / H036 OrnamentalFan / H037 Nunchaku.
        if (cardPlay.Card.Owner == owner && cardPlay.Card.Type == CardType.Attack)
        {
            if (HookId == ErrorHookId.H035_Every3AttacksThisTurnKusarigama)
            {
                _kusarigamaAttacksThisTurn++;
                if (CombatManager.Instance.IsInProgress
                    && _kusarigamaAttacksThisTurn % DynamicVars["KusarigamaCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return;
            }
            if (HookId == ErrorHookId.H036_Every3AttacksThisTurnOrnamentalFan)
            {
                _ornamentalFanAttacksThisTurn++;
                if (CombatManager.Instance.IsInProgress
                    && _ornamentalFanAttacksThisTurn % DynamicVars["OrnamentalFanCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return;
            }
            if (HookId == ErrorHookId.H037_Every10AttacksPersistentNunchaku)
            {
                NunchakuAttacksPlayed++;
                if (CombatManager.Instance.IsInProgress
                    && NunchakuAttacksPlayed % DynamicVars["NunchakuCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return;
            }
            if (HookId == ErrorHookId.H044_EndTurnIfNoAttackRippleBasin)
            {
                _rippleBasinAttackPlayedThisTurn = true;
                return;
            }
        }

        // H038 TuningFork: cumulative Skills persist across combats.
        if (HookId == ErrorHookId.H038_Every10SkillsPersistentTuningFork
            && cardPlay.Card.Owner == owner
            && cardPlay.Card.Type == CardType.Skill)
        {
            TuningForkSkillsPlayed++;
            if (TuningForkSkillsPlayed >= DynamicVars["TuningForkCards"].IntValue)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext, cardPlay);
                TuningForkSkillsPlayed -= DynamicVars["TuningForkCards"].IntValue;
            }
            return;
        }

        // H054 GamePiece duplicates the native Power-card trigger as its own fragment.
        if (HookId == ErrorHookId.H054_PowerCardPlayedGamePiece
            && CombatManager.Instance.IsInProgress
            && cardPlay.Card.Owner == owner
            && cardPlay.Card.Type == CardType.Power)
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext, cardPlay);
            return;
        }

        if (!ErrorHookRegistry.MatchesAfterPowerCardPlayed(
                HookId,
                owner,
                cardPlay))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars,
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

        if (!ErrorHookRegistry.MatchesAfterObtained(HookId)
            && HookId != ErrorHookId.H032_AfterObtainedBigMushroom
            && HookId != ErrorHookId.H051_AfterObtainedLeesWaffle)
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            new ThrowingPlayerChoiceContext(),
            DynamicVars
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
    // H025 来源遗物：
    // Oddly Smooth Stone
    //
    // 两者原版 Hook：
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

        // =====================================================
        // H022
        // 来源遗物：Stone Calendar
        //
        // 原版进入 CombatRoom：
        // Status = Normal + 刷新计数显示。
        // 不在这里触发 Effect。
        // =====================================================
        if (HookId == ErrorHookId.H022_EndOfTurn7StoneCalendar
            && room is CombatRoom)
        {
            Status = RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }

        if (HookId == ErrorHookId.H049_EnterCombatDataDisk
            && room is CombatRoom)
        {
            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
            return;
        }

        // =====================================================
        // H001
        // 来源遗物：Vajra
        // =====================================================
        if (!ErrorHookRegistry.MatchesAfterRoomEntered(
                HookId,
                owner,
                room))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            new ThrowingPlayerChoiceContext(),
            DynamicVars
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

        // =====================================================
        // H023
        // 来源遗物：Pendulum
        //
        // 原版：
        // TurnsSeen = (TurnsSeen + 1) % 3
        // 计到 2 时 Active，回到 0 时触发 Effect。
        // =====================================================
        if (ErrorHookRegistry.MatchesPendulumPlayerTurnStart(
                HookId,
                owner,
                player))
        {
            int turns =
                DynamicVars["Turns"].IntValue;

            TurnsSeen =
                (TurnsSeen + 1) % turns;

            Status =
                TurnsSeen == turns - 1
                    ? RelicStatus.Active
                    : RelicStatus.Normal;

            if (TurnsSeen != 0)
                return;

            TaskHelper.RunSafely(
                DoCounterActivateVisuals()
            );

            var pendulumContext =
                new ErrorContext(
                    owner,
                    choiceContext,
                    DynamicVars
                );

            await ErrorExecutionCompatibility.ExecuteAsync(
                HookId,
                EffectId,
                pendulumContext
            );

            return;
        }

        if (player == owner
            && owner.PlayerCombatState.TurnNumber == 1
            && (HookId == ErrorHookId.H050_FirstTurnAfterPlayerTurnStartFestivePopper
                || HookId == ErrorHookId.H061_FirstTurnAfterPlayerTurnStartRoyalPoison))
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
            return;
        }

        // =====================================================
        // H002
        // 来源遗物：Mercury Hourglass
        // =====================================================
        if (!ErrorHookRegistry.MatchesAfterPlayerTurnStart(
                HookId,
                owner,
                player))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars
        );

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H021
    // 来源遗物：Happy Flower
    //
    // 原版 Hook：
    // AfterSideTurnStart(
    //     CombatSide side,
    //     IReadOnlyList<Creature> participants,
    //     ICombatState combatState)
    //
    // 原版：
    // TurnsSeen = (TurnsSeen + 1) % 3
    // 计到 2 时 Active，回到 0 时触发 Effect。
    // =========================================================
    //
    // H022
    // 来源遗物：Stone Calendar
    //
    // 同一个原生入口负责：
    // 第 7 回合开始点亮 Active + 更新显示。
    // 真正 Effect 在 BeforeSideTurnEnd。
    // =========================================================

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (participants.Contains(owner.Creature))
        {
            if (HookId == ErrorHookId.H034_Every3SkillsThisTurnLetterOpener
                && owner.PlayerCombatState.TurnNumber != 1)
            {
                _letterOpenerSkillsThisTurn = 0;
            }

            if (HookId == ErrorHookId.H041_FirstTurnAfterSideTurnStartAkabeko
                && owner.PlayerCombatState.TurnNumber == 1)
            {
                Flash();
                await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
                return;
            }

            if (HookId == ErrorHookId.H056_AfterSideTurnStartBrimstone)
            {
                Flash();
                await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
                return;
            }
        }

        if (ErrorHookRegistry.MatchesHappyFlowerSideTurnStart(
                HookId,
                owner,
                participants))
        {
            int turns =
                DynamicVars["Turns"].IntValue;

            TurnsSeen =
                (TurnsSeen + 1) % turns;

            Status =
                TurnsSeen == turns - 1
                    ? RelicStatus.Active
                    : RelicStatus.Normal;

            if (TurnsSeen != 0)
                return;

            TaskHelper.RunSafely(
                DoCounterActivateVisuals()
            );

            // Happy Flower 原版 Hook 没有 PlayerChoiceContext。
            var happyFlowerContext =
                new ErrorContext(
                    owner,
                    new ThrowingPlayerChoiceContext(),
                    DynamicVars
                );

            await ErrorExecutionCompatibility.ExecuteAsync(
                HookId,
                EffectId,
                happyFlowerContext
            );

            return;
        }

        if (ErrorHookRegistry.MatchesStoneCalendarSide(
                HookId,
                owner,
                participants))
        {
            if (owner.PlayerCombatState.TurnNumber
                == DynamicVars["DamageTurn"].IntValue)
            {
                Status = RelicStatus.Active;
            }

            InvokeDisplayAmountChanged();
            return;
        }

        // H028 来源遗物：Candelabra
        if (ErrorHookRegistry.MatchesCandelabraSideTurnStart(
                HookId,
                owner,
                participants))
        {
            Flash();

            var candelabraContext =
                new ErrorContext(
                    owner,
                    new ThrowingPlayerChoiceContext(),
                    DynamicVars
                );

            await ErrorExecutionCompatibility.ExecuteAsync(
                HookId,
                EffectId,
                candelabraContext
            );
        }
    }


    // =========================================================
    // H022
    // 来源遗物：Stone Calendar
    //
    // 原版 Hook：
    // BeforeSideTurnEnd(
    //     PlayerChoiceContext choiceContext,
    //     CombatSide side,
    //     IEnumerable<Creature> participants)
    //
    // Owner 第 7 回合结束时触发当前 ERROR Effect。
    // =========================================================

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (participants.Contains(owner.Creature))
        {
            if (HookId == ErrorHookId.H043_EndTurnIfZeroBlockOrichalcum
                && _orichalcumShouldTrigger)
            {
                _orichalcumShouldTrigger = false;
                Flash();
                await ExecuteExpandedEffect(choiceContext);
                return;
            }

            if (HookId == ErrorHookId.H044_EndTurnIfNoAttackRippleBasin
                && !_rippleBasinAttackPlayedThisTurn)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext);
                return;
            }

            if (HookId == ErrorHookId.H053_EndTurnEmptyHandScreamingFlagon
                && PileType.Hand.GetPile(owner).IsEmpty)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext);
                return;
            }
        }

        if (!ErrorHookRegistry.MatchesStoneCalendarSide(
                HookId,
                owner,
                participants))
            return;

        int damageTurn =
            DynamicVars["DamageTurn"].IntValue;

        int turnNumber =
            owner.PlayerCombatState.TurnNumber;

        Status = RelicStatus.Normal;

        if (turnNumber != damageTurn)
            return;

        TaskHelper.RunSafely(
            DoCounterActivateVisuals()
        );

        var context =
            new ErrorContext(
                owner,
                choiceContext,
                DynamicVars
            );

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );

        InvokeDisplayAmountChanged();
    }


    // =========================================================
    // 来源遗物：
    // Happy Flower / Stone Calendar / Pendulum
    //
    // 原版 AfterCombatEnd：
    // 只恢复 Status。
    //
    // Happy Flower / Pendulum 的 TurnsSeen
    // 明确不在这里清零，所以跨战斗继续累计。
    // =========================================================

    public override async Task AfterCombatEnd(
        CombatRoom room)
    {
        if (HookId == ErrorHookId.H021_Every3TurnsHappyFlower
            || HookId == ErrorHookId.H022_EndOfTurn7StoneCalendar
            || HookId == ErrorHookId.H023_Every3TurnsPendulum)
        {
            Status = RelicStatus.Normal;

            if (HookId
                == ErrorHookId.H022_EndOfTurn7StoneCalendar)
            {
                InvokeDisplayAmountChanged();
            }
        }

        // H030 来源遗物：Joss Paper
        // 原版只清零临时 EtherealCount。
        // CardsExhausted 是 SavedProperty，跨战斗保留。
        if (HookId == ErrorHookId.H030_Every5ExhaustsJossPaper)
        {
            JossPaperEtherealCount = 0;
        }

        _centennialPuzzleUsedThisCombat = false;
        _letterOpenerSkillsThisTurn = 0;
        _kusarigamaAttacksThisTurn = 0;
        _ornamentalFanAttacksThisTurn = 0;
        _rippleBasinAttackPlayedThisTurn = false;
        _orichalcumShouldTrigger = false;

        if (HookId == ErrorHookId.H058_AfterCombatEndChosenCheese)
        {
            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        }
    }


    // =========================================================
    // H024 / H026 / H027
    //
    // 来源遗物：
    // Sparkling Rouge / Horn Cleat / Captain's Wheel
    //
    // 用户本机 sts2.dll 三者都使用：
    // AfterBlockCleared(Creature creature)
    //
    // 这个原生 Hook 没有 PlayerChoiceContext，
    // 因此这里保持 ThrowingPlayerChoiceContext。
    // =========================================================

    public override async Task AfterBlockCleared(
        Creature creature)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesAfterBlockCleared(
                HookId,
                owner,
                creature))
            return;

        Flash();

        var context =
            new ErrorContext(
                owner,
                new ThrowingPlayerChoiceContext(),
                DynamicVars
            );

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    //
    // 用户本机 sts2.dll 原版 Hook：
    //
    // AfterCardExhausted(
    //     PlayerChoiceContext choiceContext,
    //     CardModel card,
    //     bool causedByEthereal)
    //
    // 非 Ethereal：
    // 立即累计并检查 5 张阈值。
    //
    // Ethereal：
    // 只累计到临时 EtherealCount，
    // 等 AfterSideTurnEnd 再统一结算。
    // =========================================================

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (!ErrorHookRegistry.MatchesJossPaperCardExhausted(
                HookId,
                owner,
                card))
            return;

        if (causedByEthereal)
        {
            JossPaperEtherealCount++;
            return;
        }

        JossPaperCardsExhausted++;

        await TriggerJossPaperThresholds(
            choiceContext
        );
    }


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    //
    // 用户本机 sts2.dll 原版 Hook：
    // AfterSideTurnEnd(...)
    //
    // 把本回合 Ethereal 自动 Exhaust 的数量
    // 一次加入持久计数后，再检查阈值。
    // =========================================================

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (participants.Contains(owner.Creature))
        {
            if (HookId == ErrorHookId.H035_Every3AttacksThisTurnKusarigama)
            {
                _kusarigamaAttacksThisTurn = 0;
                return;
            }

            if (HookId == ErrorHookId.H045_AfterTurnEndIfBlock10ParryingShield
                && owner.Creature.Block >= DynamicVars["ParryingShieldBlockThreshold"].IntValue)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext);
                return;
            }
        }

        if (!ErrorHookRegistry.MatchesJossPaperSideTurnEnd(
                HookId,
                owner,
                participants))
            return;

        JossPaperCardsExhausted +=
            JossPaperEtherealCount;

        JossPaperEtherealCount = 0;

        await TriggerJossPaperThresholds(
            choiceContext
        );
    }


    // =========================================================
    // H030
    // 来源遗物：Joss Paper
    //
    // 原版达到阈值后：
    // Draw(cardsExhausted / 5)
    // cardsExhausted %= 5
    //
    // ERROR Hook 拆分以后：
    // 每跨过一个 5 张阈值，就执行当前 Effect 一次。
    //
    // 因此：
    // 5 张 -> 1 次 Effect
    // 10 张 -> 2 次 Effect
    //
    // 当 Effect == E031（Joss Paper Draw 1）时，
    // 结果与原版完全一致。
    // =========================================================

    private async Task TriggerJossPaperThresholds(
        PlayerChoiceContext choiceContext)
    {
        int threshold =
            DynamicVars[
                "JossPaperExhaustAmount"
            ].IntValue;

        if (JossPaperCardsExhausted < threshold)
            return;

        int activationCount =
            JossPaperCardsExhausted / threshold;

        TaskHelper.RunSafely(
            DoJossPaperActivateVisuals()
        );

        var owner = Owner;

        if (owner is null)
            return;

        var context =
            new ErrorContext(
                owner,
                choiceContext,
                DynamicVars
            );

        for (int i = 0; i < activationCount; ++i)
        {
            await ErrorExecutionCompatibility.ExecuteAsync(
                HookId,
                EffectId,
                context
            );
        }

        JossPaperCardsExhausted %= threshold;
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
            DynamicVars,
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

        if (HookId == ErrorHookId.H057_FirstTurnBeforeHandDrawNinjaScroll
            && player == owner
            && owner.PlayerCombatState.TurnNumber == 1)
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
            return;
        }

        if (!ErrorHookRegistry.MatchesBeforeHandDraw(
                HookId,
                owner,
                player))
            return;

        Flash();

        var context = new ErrorContext(
            owner,
            choiceContext,
            DynamicVars
        );

        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId,
            EffectId,
            context
        );
    }

    // =========================================================
    // H032-H061 expansion native hooks
    // =========================================================

    public override async Task BeforeCombatStart()
    {
        // Peer inventories are reconstructed before combat. Reset the owner's
        // transient fields at the same boundary as a newly reconstructed copy.
        // Persistent SavedProperty counters deliberately remain untouched.
        _centennialPuzzleUsedThisCombat = false;
        _letterOpenerSkillsThisTurn = 0;
        _kusarigamaAttacksThisTurn = 0;
        _ornamentalFanAttacksThisTurn = 0;
        _rippleBasinAttackPlayedThisTurn = false;
        _orichalcumShouldTrigger = false;
        _jossPaperEtherealCount = 0;
        var owner = Owner;
        if (owner is null) return;

        if (HookId == ErrorHookId.H048_BeforeCombatStartAnchor)
        {
            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
            return;
        }

        if (HookId == ErrorHookId.H046_BeforeBossCombatPantograph
            && !owner.Creature.IsDead
            && owner.RunState.CurrentRoom.RoomType == RoomType.Boss)
        {
            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var owner = Owner;
        if (owner is null || !participants.Contains(owner.Creature)) return;

        if (HookId == ErrorHookId.H036_Every3AttacksThisTurnOrnamentalFan)
        {
            _ornamentalFanAttacksThisTurn = 0;
            return;
        }

        if (HookId == ErrorHookId.H044_EndTurnIfNoAttackRippleBasin)
        {
            _rippleBasinAttackPlayedThisTurn = false;
            return;
        }

        if (owner.PlayerCombatState.TurnNumber == 1
            && (HookId == ErrorHookId.H039_FirstTurnBeforeSideTurnStartBagOfMarbles
                || HookId == ErrorHookId.H040_FirstTurnBeforeSideTurnStartTwistedFunnel
                || HookId == ErrorHookId.H055_FirstTurnBeforeSideTurnStartRedMask))
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
        }
    }

    public override Task BeforeSideTurnEndVeryEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var owner = Owner;
        if (owner is not null
            && HookId == ErrorHookId.H043_EndTurnIfZeroBlockOrichalcum
            && participants.Contains(owner.Creature)
            && owner.Creature.Block <= 0)
        {
            _orichalcumShouldTrigger = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature target,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H033_EnemyDeathGremlinHorn
            || target.Side == owner.Creature.Side)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H042_FirstUnblockedDamageEachCombatCentennialPuzzle
            || !CombatManager.Instance.IsInProgress
            || target != owner.Creature
            || result.UnblockedDamage <= 0
            || _centennialPuzzleUsedThisCombat)
            return;

        _centennialPuzzleUsedThisCombat = true;
        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    public override async Task AfterPlayerTurnStartLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H047_FirstTurnStartLateBloodVial
            || player != owner
            || owner.PlayerCombatState.TurnNumber > 1)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    public override async Task AfterShuffle(
        PlayerChoiceContext choiceContext,
        Player shuffler)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H052_AfterShuffleTheAbacus
            || shuffler != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    public override async Task AfterCombatVictoryEarly(CombatRoom room)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H059_VictoryAtHalfHpMeatOnTheBone
            || owner.Creature.IsDead)
            return;

        int threshold = (int)(owner.Creature.MaxHp * 0.5M);
        if (owner.Creature.CurrentHp > threshold) return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        var owner = Owner;
        if (owner is null
            || HookId != ErrorHookId.H060_AfterPotionUsedReptileTrinket
            || potion.Owner != owner
            || !CombatManager.Instance.IsInProgress)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

}
