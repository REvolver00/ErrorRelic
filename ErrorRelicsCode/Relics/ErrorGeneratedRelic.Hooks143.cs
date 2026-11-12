using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ErrorRelics.ErrorRelicsCode.Relics;

/// <summary>
/// H143-H147：v0.111.0 反编译里只被"某个遗物"用过、但效果可以离散化的时机。
///
/// 触发条件逐字对齐来源遗物的 override：
/// H143 BeltBuckle.AfterPotionProcured、（H144 GhostSeed.AfterCardEnteredCombat 已取消）、
/// H145 PhilosophersStone/FurCoat.AfterCreatureAddedToCombat、
/// H146 LizardTail.AfterPreventingDeath、H147 PaelsEye.AfterTakingExtraTurn。
/// E 片段复用既有池，不新增效果。
/// </summary>
public abstract partial class ErrorGeneratedRelic
{
    // =========================================================
    // H143 每当你获得一瓶药水
    // 来源遗物：Belt Buckle（BeltBuckle.cs:AfterPotionProcured）
    // 原生：AbstractModel.cs:1039 AfterPotionProcured(PotionModel potion)
    // =========================================================
    public override async Task AfterPotionProcured(PotionModel potion)
    {
        if (HookId != ErrorHookId.H143_AfterPotionProcured)
            return;

        var owner = Owner;

        if (owner is null || potion.Owner != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H144（已取消）—— 一张牌进入战斗时
    // 原来源遗物：Ghost Seed（AfterCardEnteredCombat）
    // 取消原因：触发点落在战斗初始化期间，玩家难以理解，
    // 且容易引出选择器未就绪的崩溃。枚举位保留，不参与生成。
    // =========================================================

    // =========================================================
    // H145 每当一个生物加入战斗
    // 来源遗物：PhilosophersStone / FurCoat
    // 原生：AbstractModel.cs:565 AfterCreatureAddedToCombat(Creature creature)
    // =========================================================
    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (HookId != ErrorHookId.H145_AfterCreatureAddedToCombat)
            return;

        if (creature.IsPlayer)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H146 每当一次死亡被阻止
    // 来源遗物：Lizard Tail（LizardTail.cs:AfterPreventingDeath）
    // 原生：AbstractModel.cs:1097 AfterPreventingDeath(Creature creature)
    // =========================================================
    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (HookId != ErrorHookId.H146_AfterPreventingDeath)
            return;

        var owner = Owner;

        if (owner is null || creature != owner.Creature)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H147 每当你获得额外回合
    // 来源遗物：Pael's Eye（PaelsEye.cs:AfterTakingExtraTurn）
    // 原生：AbstractModel.cs:1220 AfterTakingExtraTurn(Player player)
    // =========================================================
    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (HookId != ErrorHookId.H147_AfterTakingExtraTurn)
            return;

        var owner = Owner;

        if (owner is null || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }
}
