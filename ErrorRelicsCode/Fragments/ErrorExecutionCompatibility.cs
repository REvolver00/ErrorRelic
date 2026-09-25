using System.Threading.Tasks;

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

        await ErrorEffectRegistry.ExecuteAsync(
            effectId,
            context
        );
    }
}