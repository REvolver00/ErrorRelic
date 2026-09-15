using System.Collections.Generic;
using System.Threading.Tasks;

using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using ErrorRelics.ErrorRelicsCode.Fragments;

using ErrorRelics.ErrorRelicsCode.Extensions;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ErrorRelics.ErrorRelicsCode.Relics;

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorTestRelic : ErrorRelicsRelic
{
    // =========================================================
    // 现在只需要改这两行，就可以重新接线。
    // =========================================================

    private const ErrorHookId CurrentHook =
        ErrorHookId.H001_EnterCombat;

    private const ErrorEffectId CurrentEffect =
        ErrorEffectId.E001_GainStrength1;


    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShouldReceiveCombatHooks => true;


    // =========================================================
    // Localization
    // 描述也由 H + E 自动拼起来。
    // =========================================================

    public override List<(string, string)>? Localization =>
        new RelicLoc(
            "ERROR TEST",
            GetHookText() + " " + GetEffectText(),
            $"ERROR Fragment Test: {CurrentHook} + {CurrentEffect}"
        );


    public override string PackedIconPath =>
        "relic.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        "relic_outline.png".RelicImagePath();

    protected override string BigIconPath =>
        "relic.png".BigRelicImagePath();


    // =========================================================
    // E003 使用的原版 DamageVar。
    // 数值仍然是 Mercury Hourglass 的原版 3。
    // =========================================================

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new DynamicVar[]
            {
                new DamageVar(3M, ValueProp.Unpowered)
            };
        }
    }


    // =========================================================
    // H001
    //
    // 来源：Vajra
    // 语义：进入战斗
    //
    // Vajra 原版真实实现：
    // AfterRoomEntered + CombatRoom 判断
    // =========================================================

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        // 当前没有选择 H001，就完全不响应这个 Hook。
        if (CurrentHook != ErrorHookId.H001_EnterCombat)
            return;

        // H001 的条件：必须进入 CombatRoom。
        if (room is not CombatRoom)
            return;

        var owner = Owner;

        if (owner is null)
            return;

        Flash();

        // H 不关心后面是什么 Effect。
        await RunCurrentEffect(
            new ThrowingPlayerChoiceContext()
        );
    }


    // =========================================================
    // H002
    //
    // 来源：Mercury Hourglass
    // 语义：玩家回合开始
    // =========================================================

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        // 当前没有选择 H002，就完全不响应这个 Hook。
        if (CurrentHook != ErrorHookId.H002_PlayerTurnStart)
            return;

        var owner = Owner;

        if (owner is null)
            return;

        // 只响应遗物持有者自己的回合。
        if (player != owner)
            return;

        Flash();

        // H 不关心后面是什么 Effect。
        await RunCurrentEffect(choiceContext);
    }


    // =========================================================
    // Effect Dispatcher
    //
    // Hook 最终只会来到这里。
    // 它根据 E 编号决定执行哪个效果。
    // =========================================================

    private async Task RunCurrentEffect(
        PlayerChoiceContext choiceContext)
    {
        var owner = Owner;

        if (owner is null)
            return;

        switch (CurrentEffect)
        {
            // =============================================
            // E001
            // 来源：Vajra
            // 效果：获得 1 力量
            // =============================================

            case ErrorEffectId.E001_GainStrength1:

                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    owner.Creature,
                    1,
                    owner.Creature,
                    null
                );

                break;


            // =============================================
            // E003
            // 来源：Mercury Hourglass
            // 效果：所有可攻击敌人受到 3 点伤害
            // =============================================

            case ErrorEffectId.E003_DamageAllEnemies3:

                await CreatureCmd.Damage(
                    choiceContext,
                    owner.Creature.CombatState.HittableEnemies,
                    DynamicVars.Damage,
                    owner.Creature
                );

                break;
        }
    }


    // =========================================================
    // H 自己提供描述。
    // =========================================================

    private static string GetHookText()
    {
        return CurrentHook switch
        {
            ErrorHookId.H001_EnterCombat
                => "When entering combat,",

            ErrorHookId.H002_PlayerTurnStart
                => "At the start of your turn,",

            _ => "ERROR:"
        };
    }


    // =========================================================
    // E 自己提供描述。
    // =========================================================

    private static string GetEffectText()
    {
        return CurrentEffect switch
        {
            ErrorEffectId.E001_GainStrength1
                => "gain 1 Strength.",

            ErrorEffectId.E003_DamageAllEnemies3
                => "deal 3 damage to ALL enemies.",

            _ => "do nothing."
        };
    }
}


// =============================================================
// Hook Registry 第一版
// =============================================================

