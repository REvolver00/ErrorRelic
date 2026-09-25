using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

/// <summary>
/// ERROR 遗物执行兼容层。
///
/// 这个类负责：
///
/// Hook
///     ↓
/// Compatibility
///     ↓
/// Effect
///
/// 当前版本不进行任何延迟执行。
///
/// 特别注意：
/// 不允许把 Effect 暂存到下一回合；
/// 不允许跨战斗保存 Effect；
/// 不维护 static Pending Queue。
///
/// 后续如果某些 Effect（例如 Toolbox）确实需要特殊兼容，
/// 统一从这里分流到专用兼容器。
/// </summary>
public static class ErrorExecutionCompatibility
{
    /// <summary>
    /// 执行 ERROR Effect。
    ///
    /// 当前版本全部直接执行，
    /// 相当于一个“透明兼容层”。
    ///
    /// 保留 HookId 参数，
    /// 是为了以后可以根据 Hook + Effect 的组合
    /// 做精确兼容，而不用再次修改 ErrorGeneratedRelic。
    /// </summary>
    public static async Task ExecuteAsync(
        ErrorHookId hookId,
        ErrorEffectId effectId,
        ErrorContext context)
    {
        // =====================================================
        // 当前：
        //
        // 所有效果都直接执行。
        //
        // 例如：
        //
        // H005 + E007
        //
        // 会直接使用 H005 原生传下来的
        // PlayerChoiceContext。
        //
        // 不再：
        // - Enqueue
        // - Pending
        // - 等到下一回合
        // - 跨战斗保存
        // =====================================================

        // =====================================================
        // 非战斗 Hook -> 战斗专用 Effect 的兼容边界
        //
        // 例如：
        // H029（来源遗物：Meal Ticket，进入商店）
        // +
        // E023（来源遗物：Stone Calendar，对所有敌人造成 52 伤害）
        //
        // 商店房没有有效 CombatState / HittableEnemies。
        // 如果仍直接执行 E023，会访问不存在的战斗状态并报错。
        //
        // ERROR 遗物允许任意 H + E，因此这里统一处理：
        //
        // - 战斗进行中：完全保留原 Effect，不削弱、不改数值。
        // - 非战斗状态：仅对“必须依赖当前战斗”的 Effect 安全跳过。
        // - 不 Pending，不延迟到下一场战斗，不跨房间补发。
        // =====================================================

        if (!CombatManager.Instance.IsInProgress
            && RequiresActiveCombat(effectId))
        {
            return;
        }

        await ErrorEffectRegistry.ExecuteAsync(
            effectId,
            context
        );
    }


    // =========================================================
    // 必须依赖当前战斗状态的 Effect。
    // 每一项保留来源遗物，后续新增 Effect 时继续扩表。
    // =========================================================

    private static bool RequiresActiveCombat(
        ErrorEffectId effectId)
    {
        return effectId switch
        {
            // E001 来源遗物：Vajra
            ErrorEffectId.E001_GainStrength1 => true,

            // E003 来源遗物：Mercury Hourglass
            ErrorEffectId.E003_DamageAllEnemies3 => true,

            // E005 来源遗物：Mummified Hand
            ErrorEffectId.E005_RandomHandCardFreeThisTurn => true,

            // E007 来源遗物：Toolbox
            ErrorEffectId.E007_Choose1Of3ColorlessToHand => true,

            // E022 来源遗物：Happy Flower
            ErrorEffectId.E022_GainEnergy1 => true,

            // E023 来源遗物：Stone Calendar
            ErrorEffectId.E023_DamageAllEnemies52 => true,

            // E024 来源遗物：Pendulum
            ErrorEffectId.E024_Draw1 => true,

            // E025 来源遗物：Sparkling Rouge
            ErrorEffectId.E025_GainStrength1Dexterity1 => true,

            // E026 来源遗物：Oddly Smooth Stone
            ErrorEffectId.E026_GainDexterity1 => true,

            // E027 来源遗物：Horn Cleat
            ErrorEffectId.E027_GainBlock14 => true,

            // E028 来源遗物：Captain's Wheel
            ErrorEffectId.E028_GainBlock18 => true,

            // E029 来源遗物：Candelabra
            ErrorEffectId.E029_GainEnergy2 => true,

            // E031 来源遗物：Joss Paper
            ErrorEffectId.E031_Draw1JossPaper => true,

            _ => false
        };
    }

}