using System.Collections.Generic;
using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ErrorRelics.ErrorRelicsCode.Relics;

/// <summary>
/// H105-H116：v0.111.0 反编译同批次补录的 12 个原生时机。
///
/// 每个 override 都只做两件事：
/// 1. 用与来源遗物完全相同的条件判断本次 Hook 是否属于本遗物；
/// 2. 交给 ExecuteExpandedEffect，由 ErrorEffectRegistry 执行 E 片段。
///
/// 这里不做任何延迟执行、不做 Pending 队列、不跨战斗保存。
/// 计数型 Hook（H108/H109）的计数器按来源遗物的原版作用域保存：
/// Galactic Dust 的星辰计数是 SavedProperty；Metronome 的充能球计数
/// 每场战斗清零（在原版里由 AfterRoomEntered / AfterCombatEnd 清零，
/// 这里统一在 BeforeCombatStartLate 的同一时机清零）。
/// </summary>
public abstract partial class ErrorGeneratedRelic
{
    // =========================================================
    // H108 / H109 计数
    // =========================================================

    // 来源遗物：Galactic Dust —— [SavedProperty] int StarsSpent
    private int _galacticDustStarsSpent;

    [SavedProperty]
    public int GalacticDustStarsSpent
    {
        get => _galacticDustStarsSpent;
        set
        {
            AssertMutable();
            _galacticDustStarsSpent = value;
            InvokeDisplayAmountChanged();
        }
    }

    // 来源遗物：Metronome —— 普通字段，每场战斗清零
    private int _metronomeOrbsChanneled;

    // 来源遗物：Emotion Chip —— 本回合是否真的掉过血
    private bool _emotionChipLostHp;

    // 来源遗物：Regalite —— 每回合只触发一次
    private bool _regaliteUsedThisTurn;

    // =========================================================
    // H105
    // 来源遗物：Tingsha / Tough Bandages
    //
    // 原生 AfterCardDiscarded：
    //   card.Owner == Owner
    //   且 Owner.Creature.Side == Owner.Creature.CombatState.CurrentSide
    // =========================================================

    public override async Task AfterCardDiscarded(
        PlayerChoiceContext choiceContext,
        CardModel card)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H105_AfterCardDiscarded
            || card.Owner != owner)
            return;

        // 原版两个遗物都只在“自己的行动阶段”弃牌时生效。
        // CombatState 在战斗外为 null，这里直接返回，不做延迟补发。
        var combatState = owner.Creature.CombatState;

        if (combatState is null
            || owner.Creature.Side != combatState.CurrentSide)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H106
    // 来源遗物：Hand Drill
    //
    // 原生 AfterBlockBroken：
    //   breaker == Owner.Creature || breaker?.PetOwner == Owner
    //   且 !target.IsPlayer
    // =========================================================

    public override async Task AfterBlockBroken(
        PlayerChoiceContext choiceContext,
        Creature target,
        Creature? breaker)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H106_AfterBlockBroken)
            return;

        bool fromOwner = breaker == owner.Creature
                         || (breaker is not null && breaker.PetOwner == owner);

        if (!fromOwner || target.IsPlayer)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H107
    // 来源遗物：Dragon Fruit
    //
    // 原生 AfterGoldGained：player == Owner
    // =========================================================

    public override async Task AfterGoldGained(Player player)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H107_AfterGoldGained
            || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H108
    // 来源遗物：Galactic Dust
    //
    // 原生 AfterStarsSpent：累计 StarsSpent，达到 Stars 阈值（10）时
    // 结算一次并保留余数。原版阈值来自 StarsVar(10)，这里保持同值。
    // =========================================================

    private const int GalacticDustStarsThreshold = 10;

    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H108_Every10StarsGalacticDust
            || spender != owner)
            return;

        GalacticDustStarsSpent += amount;

        // 一次可能跨过多个阈值（例如一次消耗 25 点星辰）。
        while (GalacticDustStarsSpent >= GalacticDustStarsThreshold)
        {
            GalacticDustStarsSpent -= GalacticDustStarsThreshold;
            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
        }
    }

    // =========================================================
    // H109
    // 来源遗物：Metronome
    //
    // 原生 AfterOrbChanneled：累计生成的充能球数，
    // 达到 OrbCount 阈值（7）时结算。阈值来自 DynamicVar("OrbCount", 7)。
    // =========================================================

    private const int MetronomeOrbThreshold = 7;

    public override async Task AfterOrbChanneled(
        PlayerChoiceContext choiceContext,
        Player player,
        OrbModel orb)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H109_Every7OrbsMetronome
            || player != owner)
            return;

        _metronomeOrbsChanneled++;

        if (_metronomeOrbsChanneled < MetronomeOrbThreshold)
            return;

        _metronomeOrbsChanneled -= MetronomeOrbThreshold;
        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H110
    // 来源遗物：Unceasing Top
    //
    // 原生 AfterHandEmptied：
    //   player == Owner 且 IsValidPhase(player.PlayerCombatState.Phase)
    //   IsValidPhase: (uint)(phase - 2) <= 2u
    // =========================================================

    public override async Task AfterHandEmptied(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H110_AfterHandEmptied
            || player != owner)
            return;

        var combatState = player.PlayerCombatState;

        if (combatState is null)
            return;

        // 原版注释：起始手牌抽取前与手牌刷新后不触发，否则自动打出的牌
        // 会让手牌恒为空。这里保持完全一致。
        if ((uint)((int)combatState.Phase - 2) > 2u)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H111
    // 来源遗物：Bing Bong / Lucky Fysh / Darkstone Periapt
    //
    // 原生 AfterCardChangedPiles：
    //   card.Pile?.Type == PileType.Deck
    //   且 card.Owner == Owner
    //   且 clonedBy == null
    // =========================================================

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H111_CardEnteredDeck
            || card.Owner != owner
            || clonedBy is not null
            || oldPileType == PileType.Deck
            || card.Pile is null
            || card.Pile.Type != PileType.Deck)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H112
    // 来源遗物：Bone Flute
    //
    // 原生 AfterAttack：
    //   command.Attacker 是 Owner.Creature，或 command.Attacker 的
    //   PetOwner == Owner（宠物攻击同样计入）。
    // =========================================================

    public override async Task AfterAttack(
        PlayerChoiceContext choiceContext,
        AttackCommand command)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H112_AfterAttackBoneFlute)
            return;

        Creature? attacker = command.Attacker;

        if (attacker is null)
            return;

        bool fromOwner = attacker == owner.Creature
                         || attacker.PetOwner == owner;

        if (!fromOwner)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H113
    // 来源遗物：Maw Bank
    //
    // 原生 AfterItemPurchased：
    //   player == Owner 且 goldSpent > 0
    // （Maw Bank 另有“每局只触发一次”的状态，那是它自己的
    //   HasItemBeenBought；ERROR 遗物不复制这个一次性限制。）
    // =========================================================

    public override async Task AfterItemPurchased(
        Player player,
        MerchantEntry itemPurchased,
        int goldSpent)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H113_AfterItemPurchasedMawBank
            || player != owner
            || goldSpent <= 0)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H114 / H109 清零点
    // 来源遗物：Petrified Toad（H114）
    //
    // 原生 BeforeCombatStartLate()
    // =========================================================

    public override async Task BeforeCombatStartLate()
    {
        // Metronome 的充能球计数与它自己的开战时机无关，
        // 这里借用同一个“战斗真正开始前的最后阶段”统一清零。
        _metronomeOrbsChanneled = 0;
        _emotionChipLostHp = false;
        _artOfWarAttackThisTurn = false;
        _artOfWarAttackLastTurn = false;
        _regaliteUsedThisTurn = false;

        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H114_BeforeCombatStartLatePetrifiedToad)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H115
    // 来源遗物：Bookmark
    //
    // 原生 AfterFlush：player == Owner
    // （Bookmark 自己还会挑一张保留牌降费，那属于它的 E 片段，
    //  不写进 Hook 条件里。）
    // =========================================================

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H115_AfterFlushBookmark
            || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }

    // =========================================================
    // H116
    // 来源遗物：Regal Pillow / Stone Humidifier
    //
    // 原生 AfterRestSiteHeal：player == Owner
    // =========================================================

    public override async Task AfterRestSiteHeal(
        Player player,
        bool isMimicked)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H116_AfterRestSiteHealRegalPillow
            || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H121
    // 来源遗物：Art of War（Rare）
    //
    // 原生：AfterCardPlayed 记录本回合是否打出攻击；
    //       AfterSideTurnEnd 把本回合标志结转成“上一回合”；
    //       AfterEnergyReset 在“不是第一回合且上回合没打攻击”时 +1 能量。
    //
    // ERROR 的结转点放在下一个己方回合开始（BeforeSideTurnStart），
    // 因为 AfterSideTurnEnd 已被主文件占用。判定与数值与原版一致。
    // =========================================================

    private bool _artOfWarAttackThisTurn;
    private bool _artOfWarAttackLastTurn;

    public override async Task AfterEnergyReset(Player player)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H121_AfterEnergyResetNoAttackArtOfWar
            || player != owner)
            return;

        if (owner.PlayerCombatState is null
            || owner.PlayerCombatState.TurnNumber <= 1)
            return;

        if (_artOfWarAttackLastTurn)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H125 / H126 / H127
    // 来源遗物：War Hammer（H125）
    //           Burning Blood（H126）
    //           Black Blood（H127）
    //
    // 原生 AfterCombatVictory(CombatRoom)：
    //   War Hammer：room.RoomType == RoomType.Elite
    //   Burning Blood / Black Blood：!Owner.Creature.IsDead
    //
    // 注意：这个 Hook 在战斗已经结束之后才触发，
    // 所以对应的 E 片段（升级牌组 / 回血）都不能进 RequiresActiveCombat。
    // =========================================================

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var owner = Owner;

        if (owner is null)
            return;

        if (HookId == ErrorHookId.H125_AfterCombatVictoryEliteWarHammer)
        {
            if (room.RoomType != RoomType.Elite)
                return;

            Flash();
            await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
            return;
        }

        if (HookId != ErrorHookId.H126_AfterCombatVictoryBurningBlood
            && HookId != ErrorHookId.H127_AfterCombatVictoryBlackBlood)
            return;

        if (owner.Creature.IsDead)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H129
    // 来源遗物：Regalite
    //
    // 原生 AfterCardGeneratedForCombat：
    //   creator == Owner 且本回合还没用过
    // =========================================================

    public override async Task AfterCardGeneratedForCombat(
        CardModel card,
        Player? creator)
    {
        var owner = Owner;

        if (owner is null
            || HookId != ErrorHookId.H129_AfterCardGeneratedForCombatRegalite
            || creator != owner
            || _regaliteUsedThisTurn)
            return;

        _regaliteUsedThisTurn = true;
        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }
}
