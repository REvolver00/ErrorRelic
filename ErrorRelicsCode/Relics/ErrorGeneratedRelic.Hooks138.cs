using System.Threading.Tasks;

using ErrorRelics.ErrorRelicsCode.Fragments;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ErrorRelics.ErrorRelicsCode.Relics;

/// <summary>
/// H138-H142：v0.111.0 反编译里"没有任何原版遗物使用"的五个时机。
///
/// 与 H105-H137 不同，这五个 Hook 没有可逐字对齐的遗物实现，
/// 触发条件按 AbstractModel 的签名与 Hook 分发语义直接定义
/// （见 MegaCrit.Sts2.Core.Models.AbstractModel 与 Hooks/Hook.cs）。
/// 对应的 E 片段复用既有池，不新增效果。
/// </summary>
public abstract partial class ErrorGeneratedRelic
{
    // =========================================================
    // H138 AfterRewardTaken
    // 原生签名：AbstractModel.cs:1131
    //   AfterRewardTaken(Player player, Reward reward)
    // 分发：Hooks/Hook.cs:1104 AfterRewardTaken(IRunState, Player, Reward)
    // 判定：奖励被本遗物主人领取时触发
    // =========================================================
    public override async Task AfterRewardTaken(Player player, Reward reward)
    {
        if (HookId != ErrorHookId.H138_AfterRewardTaken)
            return;

        var owner = Owner;

        if (owner is null || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H139 AfterRestSiteSmith
    // 原生签名：AbstractModel.cs:1121 AfterRestSiteSmith(Player player)
    // 分发：Hooks/Hook.cs:1092 AfterRestSiteSmith(IRunState, Player)
    // 判定：本遗物主人在营火锻打（升级卡牌）时触发
    // =========================================================
    public override async Task AfterRestSiteSmith(Player player)
    {
        if (HookId != ErrorHookId.H139_AfterRestSiteSmith)
            return;

        var owner = Owner;

        if (owner is null || player != owner)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H140 AfterActEntered
    // 原生签名：AbstractModel.cs:208 AfterActEntered()
    // 分发：Hooks/Hook.cs:68 AfterActEntered(IRunState)
    // 判定：进入新的一幕时触发（无 Player 参数，按遗物主人自己触发）
    // =========================================================
    public override async Task AfterActEntered()
    {
        if (HookId != ErrorHookId.H140_AfterActEntered)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H141 AfterBlockGained
    // 原生签名：AbstractModel.cs:330
    //   AfterBlockGained(Creature creature, decimal amount,
    //                    ValueProp props, CardModel? cardSource)
    // 分发：Hooks/Hook.cs:143 AfterBlockGained(ICombatState, Creature, ...)
    // 判定：本遗物主人获得格挡时（amount > 0）
    // =========================================================
    public override async Task AfterBlockGained(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (HookId != ErrorHookId.H141_AfterBlockGained)
            return;

        var owner = Owner;

        if (owner is null || creature != owner.Creature || amount <= 0M)
            return;

        Flash();
        await ExecuteExpandedEffect(new ThrowingPlayerChoiceContext());
    }

    // =========================================================
    // H142 AfterDamageGiven
    // 原生签名：AbstractModel.cs:593
    //   AfterDamageGiven(PlayerChoiceContext, Creature? dealer,
    //                    DamageResult, ValueProp, Creature target,
    //                    CardModel? cardSource)
    // 分发：Hooks/Hook.cs:401 AfterDamageGiven(...)
    // 判定：本遗物主人（或其宠物）造成伤害、且目标不是自己时触发
    // =========================================================
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (HookId != ErrorHookId.H142_AfterDamageGiven)
            return;

        var owner = Owner;

        if (owner is null)
            return;

        bool isOwnDealer = dealer == owner.Creature
            || dealer?.PetOwner == owner;

        if (!isOwnDealer || target == owner.Creature)
            return;

        Flash();
        await ExecuteExpandedEffect(choiceContext);
    }
}
