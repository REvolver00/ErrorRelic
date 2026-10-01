using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

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

        // Room-entry hooks are awaited before the normal fade-in. Reveal E101's
        // selector here; combat-loop hooks run alongside that fade-in and must
        // not start a competing tween that cancels it.
        if (effectId == ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard
            && (hookId is ErrorHookId.H001_EnterCombat
                or ErrorHookId.H025_EnterCombatOddlySmoothStone
                or ErrorHookId.H029_EnterMerchantMealTicket
                or ErrorHookId.H031_EnterFirstUnknownRoomPlanisphere
                or ErrorHookId.H049_EnterCombatDataDisk
                or ErrorHookId.H076_EnterCombatSwordOfJade
                or ErrorHookId.H082_EnterCombatBronzeScales
                or ErrorHookId.H083_EnterCombatGorget)
            && NGame.Instance?.Transition.InTransition == true)
        {
            await RunManager.Instance.FadeIn();
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

            ErrorEffectId.E034_GainEnergy1GremlinHorn => true,
            ErrorEffectId.E035_Draw1GremlinHorn => true,
            ErrorEffectId.E036_DamageAllEnemies5 => true,
            ErrorEffectId.E037_DamageRandomEnemy6Kusarigama => true,
            ErrorEffectId.E038_GainBlock4OrnamentalFan => true,
            ErrorEffectId.E039_GainEnergy1Nunchaku => true,
            ErrorEffectId.E040_GainBlock7TuningFork => true,
            ErrorEffectId.E041_ApplyVulnerable1All => true,
            ErrorEffectId.E042_ApplyPoison4All => true,
            ErrorEffectId.E043_ApplyVigor8Self => true,
            ErrorEffectId.E044_Draw3 => true,
            ErrorEffectId.E045_GainBlock6Orichalcum => true,
            ErrorEffectId.E046_GainBlock4RippleBasin => true,
            ErrorEffectId.E047_DamageRandomEnemy6ParryingShield => true,
            ErrorEffectId.E050_GainBlock10 => true,
            ErrorEffectId.E051_ApplyFocus1 => true,
            ErrorEffectId.E052_DamageAllEnemies9 => true,
            ErrorEffectId.E055_GainBlock6Abacus => true,
            ErrorEffectId.E056_DamageAllEnemies20 => true,
            ErrorEffectId.E057_Draw1GamePiece => true,
            ErrorEffectId.E058_ApplyWeak1All => true,
            ErrorEffectId.E059_GainStrength2Self => true,
            ErrorEffectId.E060_GainStrength1AllEnemies => true,
            ErrorEffectId.E061_Create3ShivsInHand => true,
            ErrorEffectId.E064_ApplyReptileTrinketPower3 => true,
            ErrorEffectId.E065_SelfDamage4Unblockable => true,

            // E066-E100 target v0.111.0 expansion.
            // Only effects that require an active CombatState are guarded here.
            ErrorEffectId.E066_ShurikenGainStrength1 => true,
            ErrorEffectId.E067_KunaiGainDexterity1 => true,
            ErrorEffectId.E068_LanternGainEnergy1 => true,
            ErrorEffectId.E069_VeryHotCocoaGainEnergy4 => true,
            ErrorEffectId.E077_IvoryTileGainEnergy1 => true,
            ErrorEffectId.E078_SaiGainBlock7 => true,
            ErrorEffectId.E079_ChandelierGainEnergy3 => true,
            ErrorEffectId.E080_SwordOfJadeGainStrength3 => true,
            ErrorEffectId.E081_DaughterOfTheWindGainBlock4 => true,
            ErrorEffectId.E082_LostWispDamageAllEnemies8 => true,
            ErrorEffectId.E083_CharonsAshesDamageAllEnemies3 => true,
            ErrorEffectId.E084_ForgottenSoulDamageRandomEnemy4WithBluntVfx => true,
            ErrorEffectId.E085_IronClubDraw1 => true,
            ErrorEffectId.E086_BronzeScalesApplyThorns3 => true,
            ErrorEffectId.E087_GorgetApplyPlating4 => true,
            ErrorEffectId.E088_HelicalDartApplyHelicalDartPower1 => true,
            ErrorEffectId.E089_PermafrostGainBlock7 => true,
            ErrorEffectId.E092_BigHatAdd2RandomEtherealToHand => true,
            ErrorEffectId.E093_OrangeDoughAdd2RandomColorlessToHand => true,
            ErrorEffectId.E094_RadiantPearlAdd1LuminesceToHand => true,
            ErrorEffectId.E095_CrackedCoreChannel1Lightning => true,
            ErrorEffectId.E096_SymbioticVirusChannel1Dark => true,

            // E109 uses CombatCardGeneration + hand insertion + combat choice UI.
            // Deck selectors and reward screens E101-E108 are intentionally
            // allowed both in and out of combat, matching their self-contained
            // vanilla flows.
            ErrorEffectId.E109_ChoicesParadoxChoose1Of5RetainToHand => true,

            _ => false
        };
    }

}
