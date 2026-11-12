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
        ErrorHookId hookId, ErrorEffectId effectId, ErrorContext context)
    {
        bool trace = ErrorRelicsCode.Debug.ErrorLoadDiagnostics.IsLoading;
        if (trace) MainFile.Logger.Info($"ERROR load effect begin: owner={context.Owner.NetId}, {hookId}/{effectId}");
        try
        {
            await ExecuteCoreAsync(hookId, effectId, context);
            if (trace) MainFile.Logger.Info($"ERROR load effect end: owner={context.Owner.NetId}, {hookId}/{effectId}");
        }
        catch (System.Exception ex)
        {
            if (trace) MainFile.Logger.Info($"ERROR load effect failed: owner={context.Owner.NetId}, {hookId}/{effectId}, {ex}");
            throw;
        }
    }

    private static async Task ExecuteCoreAsync(
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
        // 黑名单：只拦"会崩"的组合，不拦"没效果"的组合。
        //
        // 设计原则（用户定死）：
        // - 唯一底线是"游戏不崩"。
        // - 片段跑出 NOOP 是允许的，视为正确行为。
        //
        // 所以这里只登记"离开战斗就会读到 null CombatState /
        // 不存在的战斗堆栈，从而把空引用抛进 Hook 链"的 Effect。
        // 其余 Effect 一律放行：能生效就生效，不能生效就无事发生，
        // 既不做额外前置判断，也不要求每个新 Effect 都来登记。
        // =====================================================

        if (!CombatManager.Instance.IsInProgress
            && WouldCrashOutsideCombat(effectId))
        {
            return;
        }

        // =====================================================
        // 组合黑名单：只拦「Hook 会被自己的 Effect 再次喂出来」的组合。
        //
        // 这类组合不会崩，但会软锁：一次触发后无限循环。
        // 明细与判定口径见 ErrorFragmentBlacklist。
        // 命中即跳过，不报错、不弹窗。
        // =====================================================

        if (ErrorFragmentBlacklist.IsBlocked(hookId, effectId))
        {
            if (ErrorRelicsCode.Debug.ErrorLoadDiagnostics.IsLoading)
            {
                MainFile.Logger.Info($"ERROR blocked combination: {hookId}/{effectId}");
            }

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
    // 崩溃黑名单（不是白名单）。
    //
    // 只有"战斗外执行一定会读到 null CombatState / 不存在的战斗
    // 堆栈并抛异常"的 Effect 才登记在这里。
    // 未登记的 Effect 默认放行；它跑出 NOOP 也算正确，
    // 因此新增 Effect 时不需要为了"完整性"来补表，
    // 只有真的验证过会崩才加。
    // =========================================================

    private static bool WouldCrashOutsideCombat(
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

            // E110-E121 target the same v0.111.0 audit batch.
            //
            // 这里只登记“离开战斗就会读到 null CombatState / 空手牌堆”
            // 的效果，保证任意 H × E 组合都不会把空引用抛进游戏的
            // Hook 链。E113（金币）与 E120（最大生命）是永久型资源，
            // 在商店/事件里本来就该生效，因此不设限。
            ErrorEffectId.E110_DiscardDamageRandomEnemy3Tingsha => true,
            ErrorEffectId.E111_DiscardGainBlock3ToughBandages => true,
            ErrorEffectId.E112_BreakBlockApplyVulnerable2HandDrill => true,
            ErrorEffectId.E114_GalacticDustGainBlock10 => true,
            ErrorEffectId.E115_BellowsUpgradeAllHandCards => true,
            ErrorEffectId.E116_DivineDestinyGainStars7 => true,
            ErrorEffectId.E117_RunicCapacitorAddOrbSlots3 => true,
            ErrorEffectId.E118_FencingManualForge10 => true,
            ErrorEffectId.E119_DiamondDiademGainBlock20AndBlur1 => true,
            ErrorEffectId.E121_TeaOfDiscourtesyAdd2DazedToDraw => true,

            // E122-E125：同为 v0.111.0 同批次。
            // E124 填药水槽是战斗外也合法的一次性准备行为，因此不设限。
            ErrorEffectId.E122_SealOfGoldGainEnergy1LoseGold3 => true,
            ErrorEffectId.E123_PowerCellDraw2ZeroCostCards => true,
            ErrorEffectId.E125_ArtOfWarGainEnergy1 => true,

            // E126-E128：都必须在战斗内执行（Power / 充能球 / 手牌生成）。
            ErrorEffectId.E126_SelfFormingClayBlockNextTurn3 => true,
            ErrorEffectId.E127_EmotionChipTriggerAllOrbPassives => true,
            ErrorEffectId.E128_CrossbowAdd1FreeAttack => true,

            // E132 / E133：星辰与格挡都依赖战斗内的 PlayerCombatState。
            // E129（升级牌组）、E130/E131（回血）在战斗胜利后才触发，
            // 触发时 IsInProgress 可能已经是 false，因此绝对不能登记在这里。
            ErrorEffectId.E132_LunarPastryGain1Star => true,
            ErrorEffectId.E133_RegaliteGainBlock4 => true,

            // E134 / E137：手牌堆与能量都只在战斗内存在。
            // E135（星辰）、E136（力量）在"进入战斗房间"时触发，
            // 那一刻 IsInProgress 未必已为 true，登记了反而会被跳过，
            // 所以这两个只在 effect 内部判空自保。
            ErrorEffectId.E134_CloakClaspGainBlockPerHandCard => true,
            ErrorEffectId.E137_PaelsTearsGainEnergy2 => true,

            // E138-E141：充能球队列、能量、战斗内造卡，
            // 任何一项离开战斗都会读到 null 的 PlayerCombatState。
            ErrorEffectId.E138_InfusedCoreChannel3Lightning => true,
            ErrorEffectId.E139_BreadLoseEnergy2 => true,
            ErrorEffectId.E140_BoomingConchGainEnergy1 => true,
            ErrorEffectId.E141_BlessedAntlerAdd3DazedToDraw => true,
            ErrorEffectId.E143_GainBlock2BoneFlute => true,
            ErrorEffectId.E144_Draw2BagOfPreparation => true,

            _ => false
        };
    }

}
