using ErrorRelics.ErrorRelicsCode.Presentation;
using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
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
                new DynamicVar("RoyalPoisonDamage", 4M),

                // H062-H104 / E066-E100 expansion values.
                // Numbers below were traced from the uploaded target sts2.dll.
                new DynamicVar("ShurikenCards", 3M),
                new DynamicVar("ShurikenStrength", 1M),
                new DynamicVar("KunaiCards", 3M),
                new DynamicVar("KunaiDexterity", 1M),
                new DynamicVar("LanternEnergy", 1M),
                new DynamicVar("VeryHotCocoaEnergy", 4M),
                new DynamicVar("StrawberryMaxHp", 7M),
                new DynamicVar("PearMaxHp", 10M),
                new DynamicVar("MangoMaxHp", 14M),
                new DynamicVar("LoomingFruitMaxHp", 31M),
                new DynamicVar("NutritiousOysterMaxHp", 11M),
                new DynamicVar("GoldenPearlGold", 150M),
                new DynamicVar("SignetRingGold", 888M),
                new DynamicVar("IvoryTileEnergyThreshold", 3M),
                new DynamicVar("IvoryTileEnergy", 1M),
                new DynamicVar("SaiBlock", 7M),
                new DynamicVar("ChandelierTurn", 3M),
                new DynamicVar("ChandelierEnergy", 3M),
                new DynamicVar("SwordOfJadeStrength", 3M),
                new DynamicVar("DaughterOfTheWindBlock", 4M),
                new DynamicVar("LostWispDamage", 8M),
                new DynamicVar("CharonsAshesDamage", 3M),
                new DynamicVar("ForgottenSoulDamage", 4M),
                new DynamicVar("IronClubCards", 4M),
                new DynamicVar("IronClubDraw", 1M),
                new DynamicVar("BronzeScalesThorns", 3M),
                new DynamicVar("GorgetPlating", 4M),
                new DynamicVar("HelicalDartDexterity", 1M),
                new DynamicVar("PermafrostBlock", 7M),
                new DynamicVar("EmptyCageCards", 2M),
                new DynamicVar("PomanderCards", 1M),
                new DynamicVar("BigHatCards", 2M),
                new DynamicVar("OrangeDoughCards", 2M),
                new DynamicVar("RadiantPearlCards", 1M),
                new DynamicVar("CrackedCoreLightning", 1M),
                new DynamicVar("SymbioticVirusDark", 1M),
                new DynamicVar("CallingBellRelics", 3M),
                new DynamicVar("ToyBoxRelics", 5M),
                new DynamicVar("ToyBoxCombats", 3M),

                // E101-E109: additional source values from the same target DLL.
                new DynamicVar("DollysMirrorCards", 1M),
                new DynamicVar("GnarledHammerCards", 3M),
                new DynamicVar("GnarledHammerSharpAmount", 3M),
                new DynamicVar("KifudaCards", 3M),
                new DynamicVar("KifudaAdroitAmount", 3M),
                new DynamicVar("PunchDaggerCards", 1M),
                new DynamicVar("PunchDaggerMomentum", 5M),
                new DynamicVar("TriBoomerangCards", 3M),
                new DynamicVar("TriBoomerangInstinct", 1M),
                new DynamicVar("OrreryRewards", 5M),
                new DynamicVar("OrreryChoicesPerReward", 3M),
                new DynamicVar("GlassEyeChoicesPerReward", 3M),
                new DynamicVar("ChoicesParadoxCards", 5M),
                new DynamicVar("ChoicesParadoxChoose", 1M)
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

    [SavedProperty]
    public int EffectExecutions { get; set; }

    private ErrorContext CreateErrorContext(
        Player owner, PlayerChoiceContext choiceContext, DynamicVarSet vars,
        CardPlay? cardPlay = null)
    {
        // Reserve before any UI await. Each relic has its own saved sequence,
        // so other players' asynchronous choices cannot consume this stream.
        int execution = EffectExecutions++;
        string key = System.FormattableString.Invariant(
            $"ErrorRelicsEffect|{owner.RunState.TotalFloor}|{owner.Relics.ToList().IndexOf(this)}|{(int)HookId}|{(int)EffectId}|{execution}");
        var rng = new MegaCrit.Sts2.Core.Random.Rng(owner, Id,
            StringHelper.GetDeterministicHashCode(key));
        return new ErrorContext(owner, choiceContext, vars, cardPlay, rng);
    }

    private async Task ExecuteExpandedEffect(
        PlayerChoiceContext choiceContext,
        CardPlay? cardPlay = null)
    {
        var owner = Owner;
        if (owner is null) return;
        await ErrorExecutionCompatibility.ExecuteAsync(
            HookId, EffectId,
            CreateErrorContext(owner, choiceContext, DynamicVars, cardPlay));
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
                || HookId == ErrorHookId.H030_Every5ExhaustsJossPaper
                || HookId == ErrorHookId.H081_Every4CardsPersistentIronClub)
            {
                return true;
            }

            if (HookId == ErrorHookId.H062_Every3AttacksThisTurnShuriken
                || HookId == ErrorHookId.H063_Every3AttacksThisTurnKunai)
            {
                return CombatManager.Instance.IsInProgress;
            }

            if (HookId == ErrorHookId.H104_Every3CombatsUpToFiveToyBox)
            {
                return ToyBoxCombatsSeen
                    < DynamicVars["ToyBoxCombats"].IntValue
                      * DynamicVars["ToyBoxRelics"].IntValue;
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

            if (HookId == ErrorHookId.H062_Every3AttacksThisTurnShuriken)
            {
                return _shurikenAttacksThisTurn
                    % DynamicVars["ShurikenCards"].IntValue;
            }

            if (HookId == ErrorHookId.H063_Every3AttacksThisTurnKunai)
            {
                return _kunaiAttacksThisTurn
                    % DynamicVars["KunaiCards"].IntValue;
            }

            if (HookId == ErrorHookId.H081_Every4CardsPersistentIronClub)
            {
                return IronClubCardsPlayed
                    % DynamicVars["IronClubCards"].IntValue;
            }

            if (HookId == ErrorHookId.H104_Every3CombatsUpToFiveToyBox)
            {
                return ToyBoxCombatsSeen
                    % DynamicVars["ToyBoxCombats"].IntValue;
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


    // ERROR presentation corruption: every Flash chooses a local-only random
    // one-shot SFX from the current game build, plus optional mod-owned custom SFX.
    // This does not consume gameplay RNG and does not affect H/E execution.
    public override string FlashSfx
    {
        get
        {
            string fallback =
                HookId == ErrorHookId.H023_Every3TurnsPendulum
                || HookId == ErrorHookId.H030_Every5ExhaustsJossPaper
                    ? "event:/sfx/ui/relic_activate_draw"
                    : base.FlashSfx;

            return ErrorPresentationCorruption.PickFlashSfx(fallback);
        }
    }


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

        if (await TryHandleAfterCardPlayedFragments(choiceContext, cardPlay))
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

        var context = CreateErrorContext(
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

        // Guaranteed pickup presentation bonus. It is local, transient, and
        // restored automatically when CurrentScene changes.
        ErrorPresentationCorruption.OnErrorPickedUp();

        var owner = Owner;

        if (owner is null)
            return;

        if (await TryHandleAfterObtainedFragments())
            return;

        if (!ErrorHookRegistry.MatchesAfterObtained(HookId)
            && HookId != ErrorHookId.H032_AfterObtainedBigMushroom
            && HookId != ErrorHookId.H051_AfterObtainedLeesWaffle)
            return;

        Flash();

        var context = CreateErrorContext(
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

        if (await TryHandleAfterRoomEnteredFragments(room))
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

        // Room entry awaits this hook before its normal fade-in. E008 waits
        // for a deck selection, which would otherwise sit under an opaque,
        // input-blocking transition forever. Reveal this room before selecting;
        // keep the effect awaited here, in the same room and hook order.
        if (HookId == ErrorHookId.H031_EnterFirstUnknownRoomPlanisphere
            && EffectId == ErrorEffectId.E008_Transform3AndUpgrade
            && NGame.Instance?.Transition.InTransition == true)
        {
            await RunManager.Instance.FadeIn();
        }

        Flash();

        var context = CreateErrorContext(
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

        if (await TryHandleAfterPlayerTurnStartFragments(choiceContext, player))
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
                CreateErrorContext(
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

        var context = CreateErrorContext(
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

        if (await TryHandleAfterSideTurnStartFragments(participants))
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
                CreateErrorContext(
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
                CreateErrorContext(
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
            CreateErrorContext(
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
        if (await TryHandleAfterCombatEndFragments())
            return;

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
            CreateErrorContext(
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

        if (await TryHandleAfterCardExhaustedFragments(choiceContext, card))
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
            CreateErrorContext(
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

        var context = CreateErrorContext(
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

        if (await TryHandleBeforeHandDrawFragments(player, choiceContext))
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

        var context = CreateErrorContext(
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
        ResetFragmentTransientCombatState();

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
        if (owner is null) return;

        if (await TryHandleBeforeSideTurnStartFragments(choiceContext, participants))
            return;

        if (!participants.Contains(owner.Creature)) return;

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


    // =========================================================
    // H062-H104 state and hook handling
    // =========================================================

    // Transient per-turn / per-combat source-relic state.
    private int _shurikenAttacksThisTurn;
    private int _kunaiAttacksThisTurn;
    private bool _permafrostActivatedThisCombat;

    // The target Iron Club counter is SavedProperty and persists across combats.
    private int _ironClubCardsPlayed;

    [SavedProperty]
    public int IronClubCardsPlayed
    {
        get => _ironClubCardsPlayed;
        set
        {
            AssertMutable();
            _ironClubCardsPlayed = value;
            InvokeDisplayAmountChanged();
        }
    }

    // Toy Box persists CombatsSeen. Target constants are 3 combats and 5 Wax relics,
    // so the source relic stops after 15 combats / 5 activations.
    private int _toyBoxCombatsSeen;

    [SavedProperty]
    public int ToyBoxCombatsSeen
    {
        get => _toyBoxCombatsSeen;
        set
        {
            AssertMutable();
            _toyBoxCombatsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    private async Task<bool> TryHandleAfterCardPlayedFragments(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner is null)
            return false;

        if (cardPlay.Card.Owner != owner)
            return IsAfterCardPlayedFragment(HookId);

        switch (HookId)
        {
            case ErrorHookId.H062_Every3AttacksThisTurnShuriken:
                if (!CombatManager.Instance.IsInProgress
                    || cardPlay.Card.Type != CardType.Attack)
                    return true;

                _shurikenAttacksThisTurn++;
                InvokeDisplayAmountChanged();
                if (_shurikenAttacksThisTurn % DynamicVars["ShurikenCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H063_Every3AttacksThisTurnKunai:
                if (!CombatManager.Instance.IsInProgress
                    || cardPlay.Card.Type != CardType.Attack)
                    return true;

                _kunaiAttacksThisTurn++;
                InvokeDisplayAmountChanged();
                if (_kunaiAttacksThisTurn % DynamicVars["KunaiCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H073_PlayCardEnergy3PlusIvoryTile:
                if (cardPlay.Resources.EnergyValue
                    >= DynamicVars["IvoryTileEnergyThreshold"].IntValue)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H077_AttackCardPlayedDaughterOfTheWind:
                if (cardPlay.Card.Type == CardType.Attack)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H078_PowerCardPlayedLostWisp:
                if (CombatManager.Instance.IsInProgress
                    && cardPlay.Card.Type == CardType.Power)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H081_Every4CardsPersistentIronClub:
                IronClubCardsPlayed++;
                if (CombatManager.Instance.IsInProgress
                    && IronClubCardsPlayed % DynamicVars["IronClubCards"].IntValue == 0)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H084_ShivCardPlayedHelicalDart:
                if (cardPlay.Card.Tags.Contains(CardTag.Shiv))
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                }
                return true;

            case ErrorHookId.H085_FirstPowerEachCombatPermafrost:
                if (CombatManager.Instance.IsInProgress
                    && cardPlay.Card.Type == CardType.Power
                    && !_permafrostActivatedThisCombat)
                {
                    Flash();
                    await ExecuteExpandedEffect(choiceContext, cardPlay);
                    // Target Permafrost sets this after its block task completes.
                    _permafrostActivatedThisCombat = true;
                }
                return true;

            default:
                return false;
        }
    }

    private static bool IsAfterCardPlayedFragment(ErrorHookId hookId)
    {
        return hookId == ErrorHookId.H062_Every3AttacksThisTurnShuriken
               || hookId == ErrorHookId.H063_Every3AttacksThisTurnKunai
               || hookId == ErrorHookId.H073_PlayCardEnergy3PlusIvoryTile
               || hookId == ErrorHookId.H077_AttackCardPlayedDaughterOfTheWind
               || hookId == ErrorHookId.H078_PowerCardPlayedLostWisp
               || hookId == ErrorHookId.H081_Every4CardsPersistentIronClub
               || hookId == ErrorHookId.H084_ShivCardPlayedHelicalDart
               || hookId == ErrorHookId.H085_FirstPowerEachCombatPermafrost;
    }

    private async Task<bool> TryHandleAfterObtainedFragments()
    {
        if (!IsAfterObtainedFragment(HookId))
            return false;

        var owner = Owner;
        if (owner is null)
            return true;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        return true;
    }

    private static bool IsAfterObtainedFragment(ErrorHookId hookId)
    {
        return hookId switch
        {
            ErrorHookId.H066_AfterObtainedStrawberry => true,
            ErrorHookId.H067_AfterObtainedPear => true,
            ErrorHookId.H068_AfterObtainedMango => true,
            ErrorHookId.H069_AfterObtainedLoomingFruit => true,
            ErrorHookId.H070_AfterObtainedNutritiousOyster => true,
            ErrorHookId.H071_AfterObtainedGoldenPearl => true,
            ErrorHookId.H072_AfterObtainedSignetRing => true,
            ErrorHookId.H086_AfterObtainedEmptyCage => true,
            ErrorHookId.H087_AfterObtainedDollysMirror => true,
            ErrorHookId.H088_AfterObtainedPomander => true,
            ErrorHookId.H089_AfterObtainedGnarledHammer => true,
            ErrorHookId.H090_AfterObtainedKifuda => true,
            ErrorHookId.H091_AfterObtainedPunchDagger => true,
            ErrorHookId.H092_AfterObtainedTriBoomerang => true,
            ErrorHookId.H098_AfterObtainedOrrery => true,
            ErrorHookId.H099_AfterObtainedGlassEye => true,
            ErrorHookId.H100_AfterObtainedSmallCapsule => true,
            ErrorHookId.H102_AfterObtainedCallingBell => true,
            ErrorHookId.H103_AfterObtainedToyBox => true,
            _ => false
        };
    }

    private async Task<bool> TryHandleAfterRoomEnteredFragments(AbstractRoom room)
    {
        if (HookId == ErrorHookId.H085_FirstPowerEachCombatPermafrost)
        {
            if (room is CombatRoom)
                _permafrostActivatedThisCombat = false;
            return true;
        }

        if (HookId != ErrorHookId.H076_EnterCombatSwordOfJade
            && HookId != ErrorHookId.H082_EnterCombatBronzeScales
            && HookId != ErrorHookId.H083_EnterCombatGorget)
            return false;

        if (room is not CombatRoom)
            return true;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        return true;
    }

    private async Task<bool> TryHandleAfterSideTurnStartFragments(
        IReadOnlyList<Creature> participants)
    {
        var owner = Owner;
        if (owner is null)
            return false;

        bool ownsSide = participants.Contains(owner.Creature);

        switch (HookId)
        {
            case ErrorHookId.H064_FirstTurnAfterSideTurnStartLantern:
            case ErrorHookId.H065_FirstTurnAfterSideTurnStartVeryHotCocoa:
            case ErrorHookId.H093_FirstTurnAfterSideTurnStartBigHat:
            case ErrorHookId.H094_FirstTurnAfterSideTurnStartOrangeDough:
            case ErrorHookId.H097_FirstTurnAfterSideTurnStartSymbioticVirus:
                if (ownsSide && owner.PlayerCombatState.TurnNumber <= 1)
                {
                    Flash();
                    await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
                }
                return true;

            case ErrorHookId.H074_AfterSideTurnStartSai:
                if (ownsSide)
                {
                    Flash();
                    await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
                }
                return true;

            case ErrorHookId.H075_Turn3AfterSideTurnStartChandelier:
                if (ownsSide
                    && owner.PlayerCombatState.TurnNumber
                    == DynamicVars["ChandelierTurn"].IntValue)
                {
                    Flash();
                    await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
                }
                return true;

            default:
                return false;
        }
    }

    private async Task<bool> TryHandleBeforeSideTurnStartFragments(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<Creature> participants)
    {
        var owner = Owner;
        if (owner is null)
            return false;

        bool ownsSide = participants.Contains(owner.Creature);

        if (HookId == ErrorHookId.H062_Every3AttacksThisTurnShuriken)
        {
            if (ownsSide)
            {
                _shurikenAttacksThisTurn = 0;
                InvokeDisplayAmountChanged();
            }
            return true;
        }

        if (HookId == ErrorHookId.H063_Every3AttacksThisTurnKunai)
        {
            if (ownsSide)
            {
                _kunaiAttacksThisTurn = 0;
                InvokeDisplayAmountChanged();
            }
            return true;
        }

        if (HookId == ErrorHookId.H096_FirstTurnBeforeSideTurnStartCrackedCore)
        {
            if (ownsSide && owner.PlayerCombatState.TurnNumber <= 1)
            {
                Flash();
                await ExecuteExpandedEffect(choiceContext);
            }
            return true;
        }

        return false;
    }

    private async Task<bool> TryHandleBeforeHandDrawFragments(
        Player player,
        PlayerChoiceContext choiceContext)
    {
        if (HookId != ErrorHookId.H095_FirstTurnBeforeHandDrawRadiantPearl)
            return false;

        var owner = Owner;
        if (owner is not null
            && player == owner
            && owner.PlayerCombatState.TurnNumber == 1)
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
        }

        return true;
    }

    private async Task<bool> TryHandleAfterPlayerTurnStartFragments(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (HookId != ErrorHookId.H101_FirstTurnAfterPlayerTurnStartChoicesParadox)
            return false;

        var owner = Owner;
        if (owner is not null
            && player == owner
            && owner.PlayerCombatState.TurnNumber == 1)
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
        }

        return true;
    }

    private async Task<bool> TryHandleAfterCardExhaustedFragments(
        PlayerChoiceContext choiceContext,
        CardModel card)
    {
        if (HookId != ErrorHookId.H079_CardExhaustedCharonsAshes
            && HookId != ErrorHookId.H080_CardExhaustedForgottenSoul)
            return false;

        var owner = Owner;
        if (owner is not null && card.Owner == owner)
        {
            Flash();
            await ExecuteExpandedEffect(choiceContext);
        }

        return true;
    }

    private async Task<bool> TryHandleAfterCombatEndFragments()
    {
        if (HookId == ErrorHookId.H062_Every3AttacksThisTurnShuriken)
        {
            _shurikenAttacksThisTurn = 0;
            InvokeDisplayAmountChanged();
            return true;
        }

        if (HookId == ErrorHookId.H063_Every3AttacksThisTurnKunai)
        {
            _kunaiAttacksThisTurn = 0;
            InvokeDisplayAmountChanged();
            return true;
        }

        if (HookId != ErrorHookId.H104_Every3CombatsUpToFiveToyBox)
            return false;

        int combats = DynamicVars["ToyBoxCombats"].IntValue;
        int relics = DynamicVars["ToyBoxRelics"].IntValue;
        int maxCombats = combats * relics;

        // Faithful IsUsedUp gate from target Toy Box: no further increment after 15.
        if (ToyBoxCombatsSeen >= maxCombats)
            return true;

        ToyBoxCombatsSeen++;
        if (ToyBoxCombatsSeen % combats != 0)
            return true;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        return true;
    }

    private void ResetFragmentTransientCombatState()
    {
        _shurikenAttacksThisTurn = 0;
        _kunaiAttacksThisTurn = 0;
        _permafrostActivatedThisCombat = false;
    }
}
