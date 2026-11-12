using System.Collections.Generic;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

/// <summary>
/// ERROR 片段组合黑名单。
///
/// 这里登记的是「Hook + Effect 这个特定组合」的风险，
/// 而不是单个 Effect 的风险：
/// 同一个 Effect 配别的 Hook 可能完全正常，
/// 只有当某个 Hook 的触发源会被该 Effect 自己再次喂出来时才会出事。
///
/// 判定口径（用户实测结论，2026-10-30）：
/// - 不崩是底线；
/// - 会崩的组合进黑名单；
/// - 会让游戏软锁（一直转圈、无法继续推进）的组合同样进黑名单；
/// - 跑出 NOOP 不算问题，不需要登记。
///
/// 被拦下的组合只是跳过，不报错、不弹窗，
/// 在 DEBUG 诊断里会留一行日志。
/// </summary>
public static class ErrorFragmentBlacklist
{
    /// <summary>
    /// 会自我喂养、导致软锁的组合。
    ///
    /// 这一类不是崩溃，而是「触发一次就永远停不下来」：
    /// Hook 由 Effect 自己的结果再次触发，
    /// 于是无限循环地给格挡 / 回血，游戏卡在结算里。
    /// </summary>
    private static readonly HashSet<(ErrorHookId HookId, ErrorEffectId EffectId)> Blocked = new()
    {
        // H141_AfterBlockGained：获得格挡就触发。
        // 配任何「给予格挡」的 Effect，都会在 Effect 生效后再次触发自己。
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E027_GainBlock14),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E045_GainBlock6Orichalcum),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E050_GainBlock10),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E111_DiscardGainBlock3ToughBandages),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E114_GalacticDustGainBlock10),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E133_RegaliteGainBlock4),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E134_CloakClaspGainBlockPerHandCard),
        (ErrorHookId.H141_AfterBlockGained, ErrorEffectId.E143_GainBlock2BoneFlute),

        // H150_AfterCurrentHpChanged：生命值变化就触发。
        // 配任何「治疗 / 提升最大生命」的 Effect，
        // 都会让生命值再次变化，从而不断重新触发自己。
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E030_Heal15),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E032_Heal5),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E033_GainMaxHp20),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E048_Heal25),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E049_Heal2),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E063_Heal12),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E120_DarkstonePeriaptGainMaxHp6),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E130_BurningBloodHeal6),
        (ErrorHookId.H150_AfterCurrentHpChanged, ErrorEffectId.E131_BlackBloodHeal12),
    };

    /// <summary>
    /// 判断某个 Hook + Effect 组合是否被拉黑。
    /// 命中返回 true，调用方应当直接跳过本次执行（NOOP）。
    /// </summary>
    public static bool IsBlocked(ErrorHookId hookId, ErrorEffectId effectId)
    {
        return Blocked.Contains((hookId, effectId));
    }
}
