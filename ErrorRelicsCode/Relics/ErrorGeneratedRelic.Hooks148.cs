using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ErrorRelics.ErrorRelicsCode.Relics;

/// <summary>
/// H148-H150：剩余仍属"触发型"（而非 modifier 型）的时机。
///
/// 逐字对齐来源遗物：
/// H148 Belt Buckle.AfterPotionDiscarded、
/// H149 Lasting Candy.BeforeCombatRewardOffered、
/// H150 Meat on the Bone / Red Skull.AfterCurrentHpChanged。
/// E 片段复用既有池，不新增效果。
///
/// 刻意不收 AfterPreventingDraw：Hook.cs:1068 只把该回调发给"阻止抽牌的
/// 那个模型"本身，不是所有遗物，片段永远收不到，收进来只会是死代码。
/// </summary>
public abstract partial class ErrorGeneratedRelic
{
    // =========================================================
    // H148 每当你丢弃一瓶药水
    // 来源遗物：Belt Buckle（BeltBuckle.cs:AfterPotionDiscarded）
    // 原生：AbstractModel.cs:1029 AfterPotionDiscarded(PotionModel potion)
    // =========================================================
    public override async Task AfterPotionDiscarded(PotionModel potion)
    {
        if (HookId != ErrorHookId.H148_AfterPotionDiscarded)
            return;

        var owner = Owner;

        if (owner is null || potion.Owner != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H149 战斗奖励生成之前
    // 来源遗物：Lasting Candy（LastingCandy.cs:BeforeCombatRewardOffered）
    // 原生：AbstractModel.cs:532 BeforeCombatRewardOffered(RewardsSet, CombatRoom)
    // =========================================================
    public override async Task BeforeCombatRewardOffered(
        RewardsSet rewards,
        CombatRoom room)
    {
        if (HookId != ErrorHookId.H149_BeforeCombatRewardOffered)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H150 每当你的当前生命值变化
    // 来源遗物：Meat on the Bone / Red Skull
    // 原生：AbstractModel.cs:578 AfterCurrentHpChanged(Creature creature, decimal delta)
    // =========================================================
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (HookId != ErrorHookId.H150_AfterCurrentHpChanged)
            return;

        var owner = Owner;

        if (owner is null || creature != owner.Creature || delta == 0M)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }
}
