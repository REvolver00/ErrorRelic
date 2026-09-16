using System;
using System.Collections.Generic;
using System.Linq;

using ErrorRelics.ErrorRelicsCode.Fragments;
using ErrorRelics.ErrorRelicsCode.Relics;

using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.Debug;

public sealed class RelicSpawnConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "relicspawn";

    public override string Args =>
        "<relic-id> | <hook> <effect>";

    public override string Description =>
        "Spawns a real relic reward. " +
        "Use 'relicspawn 4 5' to spawn a specific ERROR H/E combination.";

    public override bool IsNetworked => false;


    public override CmdResult Process(
        Player? issuingPlayer,
        string[] args)
    {
        if (issuingPlayer is null)
        {
            return new CmdResult(
                false,
                "A run is currently not in progress."
            );
        }


        // =====================================================
        // 新模式：
        //
        // relicspawn 4 5
        // relicspawn H004 E005
        // relicspawn H004_AfterObtained E005_RandomHandCardFreeThisTurn
        //
        // 两个参数都能解析成 H / E 时，
        // 直接生成指定组合的 ERROR RANDOM TEST。
        // =====================================================

        if (args.Length >= 2
            && TryParseHook(args[0], out var hookId)
            && TryParseEffect(args[1], out var effectId))
        {
            return SpawnErrorCombination(
                issuingPlayer,
                hookId,
                effectId
            );
        }


        // =====================================================
        // 原来的模式：
        //
        // relicspawn OldCoin
        // relicspawn Vajra
        // relicspawn ErrorTestRelic
        // =====================================================

        if (args.Length < 1)
        {
            return new CmdResult(
                false,
                "Usage: relicspawn <relic-id> OR relicspawn <hook> <effect>"
            );
        }


        var relic = FindRelic(args[0]);

        if (relic is null)
        {
            return new CmdResult(
                false,
                $"Unable to find relic '{args[0]}'."
            );
        }


        var mutableRelic = relic.ToMutable();

        var reward = new RelicReward(
            mutableRelic,
            issuingPlayer
        );

        var rewards = new RewardsSet(issuingPlayer)
            .WithCustomRewards(
                new List<Reward>
                {
                    reward
                }
            );


        return new CmdResult(
            rewards.Offer(),
            true,
            $"Spawned relic reward: {relic.Id.Entry}"
        );
    }


    // =========================================================
    // 指定 ERROR H + E
    // =========================================================

    private static CmdResult SpawnErrorCombination(
        Player issuingPlayer,
        ErrorHookId hookId,
        ErrorEffectId effectId)
    {
        var template =
            ModelDb.AllRelics.FirstOrDefault(
                relic => relic is ErrorRandomTestRelic
            );

        if (template is null)
        {
            return new CmdResult(
                false,
                "Unable to find ErrorRandomTestRelic in ModelDb."
            );
        }


        var mutable = template.ToMutable();


        if (mutable is not ErrorRandomTestRelic errorRelic)
        {
            return new CmdResult(
                false,
                "Unable to create mutable ErrorRandomTestRelic."
            );
        }


        // =====================================================
        // 在它进入 Reward 之前就把 Definition 塞进去。
        //
        // 玩家之后点击拿起的时候，
        // AfterObtained 会发现 DefinitionLocked = true，
        // 因此不会重新随机。
        // =====================================================

        errorRelic.GeneratedHookId = hookId;
        errorRelic.GeneratedEffectId = effectId;
        errorRelic.DefinitionLocked = true;


        var reward = new RelicReward(
            errorRelic,
            issuingPlayer
        );


        var rewards = new RewardsSet(issuingPlayer)
            .WithCustomRewards(
                new List<Reward>
                {
                    reward
                }
            );


        return new CmdResult(
            rewards.Offer(),
            true,
            $"Spawned ERROR combination: {hookId} + {effectId}"
        );
    }


    // =========================================================
    // H 解析
    //
    // 支持：
    //
    // 4
    // 004
    // H004
    // H004_AfterObtained
    // =========================================================

    private static bool TryParseHook(
        string input,
        out ErrorHookId result)
    {
        return TryParseFragment(
            input,
            'H',
            out result
        );
    }


    // =========================================================
    // E 解析
    //
    // 支持：
    //
    // 5
    // 005
    // E005
    // E005_RandomHandCardFreeThisTurn
    // =========================================================

    private static bool TryParseEffect(
        string input,
        out ErrorEffectId result)
    {
        return TryParseFragment(
            input,
            'E',
            out result
        );
    }


    // =========================================================
    // 通用 Fragment ID 解析器
    // =========================================================

    private static bool TryParseFragment<TEnum>(
        string input,
        char prefix,
        out TEnum result)
        where TEnum : struct, Enum
    {
        var search = input.Trim();


        // -----------------------------------------------------
        // 1. 完整 enum 名
        //
        // H004_AfterObtained
        // E005_RandomHandCardFreeThisTurn
        // -----------------------------------------------------

        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (value.ToString().Equals(
                    search,
                    StringComparison.OrdinalIgnoreCase))
            {
                result = value;
                return true;
            }
        }


        // -----------------------------------------------------
        // 2. 去掉 H / E
        //
        // H004 -> 004
        // E005 -> 005
        // -----------------------------------------------------

        if (search.Length > 0
            && char.ToUpperInvariant(search[0])
            == char.ToUpperInvariant(prefix))
        {
            search = search.Substring(1);
        }


        // -----------------------------------------------------
        // 3. 数字
        //
        // 4
        // 004
        // -----------------------------------------------------

        if (int.TryParse(search, out var number))
        {
            var idPrefix =
                $"{char.ToUpperInvariant(prefix)}{number:000}_";


            foreach (var value in Enum.GetValues<TEnum>())
            {
                if (value.ToString().StartsWith(
                        idPrefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result = value;
                    return true;
                }
            }
        }


        result = default;
        return false;
    }


    // =========================================================
    // 普通遗物搜索
    // =========================================================

    private static RelicModel? FindRelic(string input)
    {
        var search = input.Trim();

        var allRelics =
            ModelDb.AllRelics.ToList();


        // 精确 Entry
        var result = allRelics.FirstOrDefault(
            relic =>
                relic.Id.Entry.Equals(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 精确类名
        result = allRelics.FirstOrDefault(
            relic =>
                relic.GetType().Name.Equals(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // Entry 开头
        result = allRelics.FirstOrDefault(
            relic =>
                relic.Id.Entry.StartsWith(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 类名开头
        result = allRelics.FirstOrDefault(
            relic =>
                relic.GetType().Name.StartsWith(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 包含
        return allRelics.FirstOrDefault(
            relic =>
                relic.Id.Entry.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                relic.GetType().Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }


    // =========================================================
    // Tab 自动补全
    // =========================================================

    public override CompletionResult GetArgumentCompletions(
        Player? player,
        string[] args)
    {
        // 第一个参数：
        // 普通 relic ID + 所有 H
        if (args.Length <= 1)
        {
            var partial =
                args.Length == 0
                    ? string.Empty
                    : args[0];


            var candidates =
                ModelDb.AllRelics
                    .Select(relic => relic.Id.Entry)
                    .Concat(
                        Enum.GetValues<ErrorHookId>()
                            .Select(hook => hook.ToString())
                    )
                    .OrderBy(value => value)
                    .ToList();


            return CompleteArgument(
                candidates,
                Array.Empty<string>(),
                partial
            );
        }


        // 第二个参数：
        // 如果第一个参数是 H，
        // 就补全所有 E。
        if (args.Length == 2
            && TryParseHook(args[0], out _))
        {
            var partial = args[1];

            var candidates =
                Enum.GetValues<ErrorEffectId>()
                    .Select(effect => effect.ToString())
                    .OrderBy(value => value)
                    .ToList();


            return CompleteArgument(
                candidates,
                Array.Empty<string>(),
                partial
            );
        }


        return base.GetArgumentCompletions(
            player,
            args
        );
    }
}
