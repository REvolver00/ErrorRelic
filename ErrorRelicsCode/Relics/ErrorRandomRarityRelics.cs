using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using MegaCrit.Sts2.Core.Entities.Relics;

namespace ErrorRelics.ErrorRelicsCode.Relics;


// =============================================================
// ERROR 模式的稀有度外壳。
//
// H / E、保存、计数、描述、图标等全部继承
// ErrorRandomTestRelic 的同一套实现。
//
// 这里只保留原本奖励的 Rarity，避免：
// - 商店价格全部按 Common
// - Uncommon / Rare 奖励失去原来的稀有度语义
//
// 这些类型同样继承 IsAllowed=false，
// 不会自己作为普通遗物滚进 GrabBag。
// =============================================================

[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorRandomUncommonRelic
    : ErrorRandomTestRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;
}


[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorRandomRareRelic
    : ErrorRandomTestRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;
}


[Pool(typeof(MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool))]
public sealed class ErrorRandomShopRelic
    : ErrorRandomTestRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Shop;
}
