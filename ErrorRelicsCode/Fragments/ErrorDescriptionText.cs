using System;
using System.Globalization;

namespace ErrorRelics.ErrorRelicsCode.Fragments;

public static class ErrorDescriptionText
{
    // Use player-facing base-game phrasing, not the engine hook's timing details.
    // Index by the public H/E label; E009/E010 have historical storage order.
    private static readonly string[] Hooks =
    {
        "在每场战斗开始时",
        "在你的回合开始时",
        "每当你打出一张耗能大于等于{Energy2}的牌",
        "拾起时",
        "你每打出一张能力牌",
        "拾起时",
        "在每场战斗开始时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "每[blue]3[/blue]个回合",
        "在第[blue]7[/blue]回合结束时",
        "每[blue]3[/blue]个回合",
        "在你的第[blue]3[/blue]回合开始时",
        "在每场战斗开始时",
        "在你的第[blue]2[/blue]回合开始时",
        "在你的第[blue]3[/blue]回合开始时",
        "在你的第[blue]2[/blue]回合开始时",
        "每当你进入商店房间时",
        "你每[gold]消耗[/gold][blue]5[/blue]张牌",
        "每当你进入[gold]？[/gold]房间时",
        "拾起时",
        "每当一个敌人死亡时",
        "你每在同一回合内打出[blue]3[/blue]张技能牌",
        "你每在同一回合内打出[blue]3[/blue]张攻击牌",
        "你每在同一回合内打出[blue]3[/blue]张攻击牌",
        "你每打出[blue]10[/blue]张攻击牌",
        "你每打出[blue]10[/blue]张技能牌",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "每场战斗中，当你第一次失去生命时",
        "如果你在回合结束时没有任何[gold]格挡[/gold]",
        "在回合结束时，如果你本回合没有打出过攻击牌",
        "如果你在回合结束时拥有至少[blue]10[/blue]点[gold]格挡[/gold]",
        "在[gold]Boss[/gold]战开始时",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "拾起时",
        "每当你洗牌时",
        "如果你在回合结束时没有手牌",
        "你每打出一张能力牌",
        "在每场战斗开始时",
        "在你的回合开始时",
        "在每场战斗开始时",
        "在每场战斗结束时",
        "如果你在战斗结束时生命值等于或低于[blue]50%[/blue]",
        "当你使用药水时",
        "在每场战斗开始时",
        "你每在同一回合内打出[blue]3[/blue]张攻击牌",
        "你每在同一回合内打出[blue]3[/blue]张攻击牌",
        "在每场战斗的第一回合开始时",
        "在每场战斗的第一回合开始时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "每当你打出一张耗能大于等于{Energy3}的牌",
        "在你的回合开始时",
        "在你的第[blue]3[/blue]回合开始时",
        "在每场战斗开始时",
        "你每打出一张攻击牌",
        "你每打出一张能力牌",
        "每当你[gold]消耗[/gold]一张牌",
        "每当你[gold]消耗[/gold]一张牌",
        "你每打出[blue]4[/blue]张牌",
        "在每场战斗开始时",
        "在每场战斗开始时",
        "你每打出一张[gold]{Shiv}[/gold]",
        "每场战斗中，当你第一次打出能力牌时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "拾起时",
        "在每场战斗的第一回合开始时",
        "在每场战斗的第一回合开始时",
        "在第一回合起始手牌抽取之前",
        "在你的第一回合开始之前",
        "在每场战斗的第一回合开始时",
        "拾起时",
        "拾起时",
        "拾起时",
        "在每场战斗的第一回合开始时",
        "拾起时",
        "拾起时",
        "每完成[blue]3[/blue]场战斗，最多触发[blue]5[/blue]次",
        "每当你弃掉一张牌时",
        "每当你击破一个敌人的[gold]格挡[/gold]时",
        "每当你获得金币时",
        "你每累计消耗[blue]10[/blue]点[gold]星辰[/gold]",
        "你每生成[blue]7[/blue]个[gold]充能球[/gold]",
        "每当你手牌被清空时",
        "每当你把一张牌加入[gold]牌组[/gold]时",
        "每当你或你的宠物发动攻击时",
        "每当你在商店购买东西时",
        "在每场战斗开始的最后阶段",
        "在你的手牌刷新之后",
        "每当你在营火休息回血时",
        "每当你方回合开始、且你至少有[blue]3[/blue]金币时",
        "在你的第一回合开始前",
        "在每场战斗开始时",
        "在你的第一回合开始时",
        "当你的能量重置、且上一回合没有打出攻击牌时",
        "当你受到[gold]未格挡伤害[/gold]时",
        "在你的回合开始、且上一回合失去过生命时",
        "在你的回合开始时",
        "在你赢得一场精英战后",
        "在你赢得一场战斗后",
        "在你赢得一场战斗后",
        "在你的回合结束时",
        "当有卡牌为战斗生成时",
        "在你的回合结束、且手牌不为空时",
        "每当你进入战斗时",
        "每当你进入战斗时",
        "在你的回合开始、且上一回合有未用完的能量时",
        "在你的第一回合开始时",
        "在你的第一回合开始时",
        "在精英战的第一回合开始时",
        "在你的第一回合抽牌之前",
        "每当你领取奖励时",
        "每当你在营火锻打时",
        "每当你进入新的一幕时",
        "每当你获得[gold]格挡[/gold]时",
        "每当你造成伤害时",
        "每当你获得一瓶药水时",
        "（该时机已取消）",
        "每当一个生物加入战斗时",
        "每当你的死亡被阻止时",
        "每当你获得额外回合时",
        "每当你丢弃一瓶药水时",
        "在战斗奖励生成之前",
        "每当你的生命值发生变化时"
    };

    private static readonly string[] Effects =
    {
        "获得[blue]1[/blue]点[gold]力量[/gold]。",
        "[gold]升级[/gold]这张牌。",
        "对所有敌人造成[blue]3[/blue]点伤害。",
        "获得[blue]300[/blue]金币。",
        "使[gold]手牌[/gold]中的一张随机牌在这个回合可以免费打出。",
        "将[blue]3[/blue]张[gold]{Apparition}[/gold]加入你的[gold]牌组[/gold]。",
        "从[blue]3[/blue]张随机[gold]无色[/gold]牌中选择[blue]1[/blue]张加入你的[gold]手牌[/gold]。",
        "选择[blue]3[/blue]张牌进行[gold]变化[/gold]，然后将这些牌[gold]升级[/gold]。",
        "从[gold]牌组[/gold]中选择一张攻击牌或技能牌，为它[gold]附魔[/gold]：[purple]{RoyallyApproved}[/purple]。",
        "将[blue]2[/blue]张随机[gold]诅咒[/gold]加入你的[gold]牌组[/gold]。",
        "为你[gold]牌组[/gold]中的所有“[gold]打击[/gold]”[gold]附魔[/gold]：[purple]{TezcatarasEmber}[/purple]。",
        "为所有的“[gold]防御[/gold]”[gold]附魔[/gold]：[purple]{Goopy}[/purple]。",
        "随机[gold]升级[/gold]你牌组中的[blue]6[/blue]张牌。",
        "随机[gold]升级[/gold][blue]2[/blue]张技能牌。",
        "随机[gold]升级[/gold][blue]2[/blue]张攻击牌。",
        "[gold]升级[/gold]你的[blue]1[/blue]张[gold]打击[/gold]和[blue]1[/blue]张[gold]防御[/gold]。",
        "将[blue]2[/blue]张[gold]{Relax}[/gold]加入你的[gold]牌组[/gold]。",
        "将[blue]1[/blue]张[gold]{NeowsFury}[/gold]加入你的[gold]牌组[/gold]。",
        "将[blue]1[/blue]张[gold]{BrightestFlame}[/gold]加入你的[gold]牌组[/gold]。",
        "将[blue]1[/blue]张[gold]{Apotheosis}[/gold]加入你的[gold]牌组[/gold]。",
        "将[blue]1[/blue]张[gold]{Whistle}[/gold]加入你的[gold]牌组[/gold]。",
        "获得{Energy1}。",
        "对所有敌人造成[blue]52[/blue]点伤害。",
        "抽[blue]1[/blue]张牌。",
        "获得[blue]1[/blue]点[gold]力量[/gold]和[blue]1[/blue]点[gold]敏捷[/gold]。",
        "获得[blue]1[/blue]点[gold]敏捷[/gold]。",
        "获得[blue]14[/blue]点[gold]格挡[/gold]。",
        "获得[blue]18[/blue]点[gold]格挡[/gold]。",
        "获得{Energy2}。",
        "回复[green]15[/green]点生命。",
        "抽[blue]1[/blue]张牌。",
        "回复[green]5[/green]点生命。",
        "将你的最大生命值提升[blue]20[/blue]。",
        "获得{Energy1}。",
        "抽[blue]1[/blue]张牌。",
        "对所有敌人造成[blue]5[/blue]点伤害。",
        "对随机敌人造成[blue]6[/blue]点伤害。",
        "获得[blue]4[/blue]点[gold]格挡[/gold]。",
        "获得{Energy1}。",
        "获得[blue]7[/blue]点[gold]格挡[/gold]。",
        "给予所有敌人[blue]1[/blue]层[gold]易伤[/gold]。",
        "给予所有敌人[blue]4[/blue]层[gold]中毒[/gold]。",
        "获得[blue]8[/blue]点[gold]活力[/gold]。",
        "抽[blue]3[/blue]张牌。",
        "获得[blue]6[/blue]点[gold]格挡[/gold]。",
        "获得[blue]4[/blue]点[gold]格挡[/gold]。",
        "对随机敌人造成[blue]6[/blue]点伤害。",
        "回复[green]25[/green]点生命。",
        "回复[green]2[/green]点生命。",
        "获得[blue]10[/blue]点[gold]格挡[/gold]。",
        "获得[blue]1[/blue]点[gold]集中[/gold]。",
        "对所有敌人造成[blue]9[/blue]点伤害。",
        "将你的最大生命值提升[blue]7[/blue]。",
        "回复所有生命。",
        "获得[blue]6[/blue]点[gold]格挡[/gold]。",
        "对所有敌人造成[blue]20[/blue]点伤害。",
        "抽[blue]1[/blue]张牌。",
        "给予所有敌人[blue]1[/blue]层[gold]虚弱[/gold]。",
        "获得[blue]2[/blue]点[gold]力量[/gold]。",
        "给予所有敌人[blue]1[/blue]点[gold]力量[/gold]。",
        "将[blue]3[/blue]张[gold]{Shiv}[/gold]加入你的[gold]手牌[/gold]。",
        "将你的最大生命值提升[blue]1[/blue]。",
        "回复[green]12[/green]点生命。",
        "在本回合获得[blue]3[/blue]点[gold]力量[/gold]。",
        "失去[blue]4[/blue]点生命。",
        "获得[blue]1[/blue]点[gold]力量[/gold]。",
        "获得[blue]1[/blue]点[gold]敏捷[/gold]。",
        "获得{Energy1}。",
        "获得{Energy4}。",
        "将你的最大生命值提升[blue]7[/blue]。",
        "将你的最大生命值提升[blue]10[/blue]。",
        "将你的最大生命值提升[blue]14[/blue]。",
        "将你的最大生命值提升[blue]31[/blue]。",
        "将你的最大生命值提升[blue]11[/blue]。",
        "获得[blue]150[/blue]金币。",
        "获得[blue]888[/blue]金币。",
        "获得{Energy1}。",
        "获得[blue]7[/blue]点[gold]格挡[/gold]。",
        "获得{Energy3}。",
        "获得[blue]3[/blue]点[gold]力量[/gold]。",
        "获得[blue]4[/blue]点[gold]格挡[/gold]。",
        "对所有敌人造成[blue]8[/blue]点伤害。",
        "对所有敌人造成[blue]3[/blue]点伤害。",
        "播放钝击特效，并对随机敌人造成[blue]4[/blue]点伤害。",
        "抽[blue]1[/blue]张牌。",
        "获得[blue]3[/blue]点[gold]荆棘[/gold]。",
        "获得[blue]4[/blue]层[gold]覆甲[/gold]。",
        "在本回合获得[blue]1[/blue]点[gold]敏捷[/gold]。",
        "获得[blue]7[/blue]点[gold]格挡[/gold]。",
        "从[gold]牌组[/gold]中选择[blue]2[/blue]张牌并移除。",
        "从[gold]牌组[/gold]中选择[blue]1[/blue]张牌进行[gold]升级[/gold]。",
        "将[blue]2[/blue]张不同的随机[gold]虚无[/gold]牌加入你的[gold]手牌[/gold]。",
        "将[blue]2[/blue]张不同的随机[gold]无色[/gold]牌加入你的[gold]手牌[/gold]。",
        "将[blue]1[/blue]张[gold]冷光[/gold]加入你的[gold]手牌[/gold]。",
        "生成[blue]1[/blue]个[gold]闪电[/gold]充能球。",
        "生成[blue]1[/blue]个[gold]黑暗[/gold]充能球。",
        "将[blue]1[/blue]张召唤铃铛的独特[gold]诅咒[/gold]加入你的[gold]牌组[/gold]。",
        "获得[blue]1[/blue]件普通、[blue]1[/blue]件罕见和[blue]1[/blue]件稀有遗物。",
        "获得[blue]5[/blue]件蜡制遗物。",
        "融化最左侧尚未融化的蜡制遗物。",
        "从[gold]牌组[/gold]中选择[blue]1[/blue]张非任务牌进行复制。",
        "从[gold]牌组[/gold]中选择至多[blue]3[/blue]张攻击牌，附魔：[purple]锋利 3[/purple]。",
        "从[gold]牌组[/gold]中选择至多[blue]3[/blue]张牌，附魔：[purple]伶俐 3[/purple]。",
        "从[gold]牌组[/gold]中选择[blue]1[/blue]张攻击牌，附魔：[purple]动量 5[/purple]。",
        "从[gold]牌组[/gold]中选择[blue]3[/blue]张攻击牌，附魔：[purple]本能 1[/purple]。",
        "获得[blue]5[/blue]次卡牌奖励，每次从[blue]3[/blue]张牌中选择。",
        "获得[blue]2[/blue]组普通、[blue]2[/blue]组罕见和[blue]1[/blue]组稀有卡牌奖励。",
        "获得[blue]1[/blue]件随机遗物。",
        "从[blue]5[/blue]张随机角色牌中选择[blue]1[/blue]张加入你的[gold]手牌[/gold]，被选中的牌获得[gold]保留[/gold]。",
        "对随机敌人造成[blue]3[/blue]点伤害。",
        "获得[blue]3[/blue]点[gold]格挡[/gold]。",
        "给予随机敌人[blue]2[/blue]层[gold]易伤[/gold]。",
        "获得[blue]15[/blue]金币。",
        "获得[blue]10[/blue]点[gold]格挡[/gold]。",
        "[gold]升级[/gold]你[gold]手牌[/gold]中的所有牌。",
        "获得[blue]7[/blue]点[gold]星辰[/gold]。",
        "获得[blue]3[/blue]个额外[gold]充能球[/gold]槽位。",
        "[gold]铸造[/gold][blue]10[/blue]点[gold]君王之剑[/gold]。",
        "获得[blue]20[/blue]点[gold]格挡[/gold]和[blue]1[/blue]层[gold]模糊[/gold]。",
        "将你的最大生命值提升[blue]6[/blue]。",
        "将[blue]2[/blue]张[gold]晕眩[/gold]以随机位置加入你的[gold]抽牌堆[/gold]。",
        "获得[blue]1[/blue]点能量，失去[blue]3[/blue]金币。",
        "抽[blue]2[/blue]张费用为[blue]0[/blue]的牌。",
        "尽可能填满你的[gold]药水槽[/gold]。",
        "获得[blue]1[/blue]点能量。",
        "在下个回合开始时获得[blue]3[/blue]点[gold]格挡[/gold]。",
        "触发你所有[gold]充能球[/gold]的被动效果。",
        "将[blue]1[/blue]张随机攻击牌加入你的[gold]手牌[/gold]，它本回合费用为[blue]0[/blue]。",
        "升级你[gold]牌组[/gold]中[blue]4[/blue]张随机可升级的牌。",
        "回复[blue]6[/blue]点生命。",
        "回复[blue]12[/blue]点生命。",
        "获得[blue]1[/blue]点[gold]星辰[/gold]。",
        "获得[blue]4[/blue]点[gold]格挡[/gold]。",
        "每有[blue]1[/blue]张[gold]手牌[/gold]，便获得[blue]1[/blue]点[gold]格挡[/gold]。",
        "获得[blue]3[/blue]点[gold]星辰[/gold]。",
        "获得[blue]2[/blue]层[gold]力量[/gold]。",
        "获得[blue]2[/blue]点能量。",
        "生成[blue]3[/blue]个[gold]闪电[/gold]充能球。",
        "失去[blue]2[/blue]点能量。",
        "获得[blue]1[/blue]点能量。",
        "将[blue]3[/blue]张[gold]晕眩[/gold]以随机位置加入你的[gold]抽牌堆[/gold]。",
        "获得[blue]12[/blue]金币。",
        "获得[blue]2[/blue]点[gold]格挡[/gold]。",
        "抽[blue]2[/blue]张牌。"
    };

    private static readonly (string Token, string Table, string Key, string Fallback)[] Names =
    {
        ("Apparition", "cards", "APPARITION.title", "灵体"),
        ("RoyallyApproved", "enchantments", "ROYALLY_APPROVED.title", "王室认证"),
        ("TezcatarasEmber", "enchantments", "TEZCATARAS_EMBER.title", "特兹卡塔拉的余烬"),
        ("Goopy", "enchantments", "GOOPY.title", "黏糊"),
        ("Relax", "cards", "RELAX.title", "放松"),
        ("NeowsFury", "cards", "NEOWS_FURY.title", "涅奥之怒"),
        ("BrightestFlame", "cards", "BRIGHTEST_FLAME.title", "至亮之焰"),
        ("Apotheosis", "cards", "APOTHEOSIS.title", "神化"),
        ("Whistle", "cards", "WHISTLE.title", "吹哨"),
        ("Shiv", "cards", "SHIV.title", "小刀"),
    };

    public static string DescribeHookOnly(ErrorHookId hook, string energyIconPrefix = "colorless")
    {
        string h = hook.ToString().Split('_')[0];
        string text = Lookup(Hooks, h, 'H');
        string energyIcon = $"[img]res://images/packed/sprite_fonts/{energyIconPrefix}_energy_icon.png[/img]";
        text = text.Replace("{Energy1}", energyIcon, StringComparison.Ordinal)
            .Replace("{Energy2}", energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy3}", energyIcon + energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy4}", energyIcon + energyIcon + energyIcon + energyIcon, StringComparison.Ordinal);
        return $"{text}，???\n[color=#9b9b9b]({h}-E???)[/color]";
    }

    public static string DescribeEffectOnly(ErrorEffectId effect,
        Func<string, string, string?>? localize = null, string energyIconPrefix = "colorless")
    {
        string e = effect.ToString().Split('_')[0];
        string text = Lookup(Effects, e, 'E');
        foreach (var name in Names)
        {
            string token = "{" + name.Token + "}";
            if (text.Contains(token, StringComparison.Ordinal))
                text = text.Replace(token, localize?.Invoke(name.Table, name.Key) ?? name.Fallback,
                    StringComparison.Ordinal);
        }
        string energyIcon = $"[img]res://images/packed/sprite_fonts/{energyIconPrefix}_energy_icon.png[/img]";
        text = text.Replace("{Energy1}", energyIcon, StringComparison.Ordinal)
            .Replace("{Energy2}", energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy3}", energyIcon + energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy4}", energyIcon + energyIcon + energyIcon + energyIcon, StringComparison.Ordinal);
        return $"???，{text}\n[color=#9b9b9b](H???-{e})[/color]";
    }

    public static string Describe(ErrorHookId hook, ErrorEffectId effect,
        Func<string, string, string?>? localize = null, string energyIconPrefix = "colorless")
    {
        string h = hook.ToString().Split('_')[0];
        string e = effect.ToString().Split('_')[0];
        string text = $"{Lookup(Hooks, h, 'H')}，{Lookup(Effects, e, 'E')}";
        foreach (var name in Names)
        {
            string token = "{" + name.Token + "}";
            if (text.Contains(token, StringComparison.Ordinal))
                text = text.Replace(token, localize?.Invoke(name.Table, name.Key) ?? name.Fallback,
                    StringComparison.Ordinal);
        }
        // Same resource path/repetition rule used by the base game's EnergyIconsFormatter.
        string energyIcon = $"[img]res://images/packed/sprite_fonts/{energyIconPrefix}_energy_icon.png[/img]";
        text = text.Replace("{Energy1}", energyIcon, StringComparison.Ordinal)
            .Replace("{Energy2}", energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy3}", energyIcon + energyIcon + energyIcon, StringComparison.Ordinal)
            .Replace("{Energy4}", energyIcon + energyIcon + energyIcon + energyIcon, StringComparison.Ordinal);
        return $"{text}\n[color=#9b9b9b]({h}-{e})[/color]";
    }

    private static string Lookup(string[] texts, string id, char prefix)
    {
        if (id.Length > 1 && id[0] == prefix
            && int.TryParse(id.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out int number)
            && number >= 1 && number <= texts.Length)
            return texts[number - 1];
        return $"未知片段 {id}";
    }
}
