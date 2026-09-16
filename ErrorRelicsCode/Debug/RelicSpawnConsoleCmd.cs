using System;
using System.Collections.Generic;
using System.Linq;

using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.Debug;

public sealed class RelicSpawnConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "relicspawn";

    public override string Args => "<relic-id>";

    public override string Description =>
        "Spawns a relic as a real reward so it must be manually picked up.";

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

        if (args.Length < 1)
        {
            return new CmdResult(
                false,
                "Usage: relicspawn <relic-id>"
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


        // =====================================================
        // 关键区别：
        //
        // 不调用：
        // RelicCmd.Obtain(...)
        //
        // 而是制造真正的 RelicReward，
        // 然后让游戏把奖励界面打开。
        //
        // 玩家点击遗物以后，
        // RelicReward 自己才会走 RelicCmd.Obtain(...)
        // =====================================================

        var reward = new RelicReward(
            relic.ToMutable(),
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
    // 支持：
    //
    // 精确 ID
    // ID 开头
    // ID 包含
    // 类名
    //
    // 所以测试 MOD 遗物比较方便。
    // =========================================================

    private static RelicModel? FindRelic(string input)
    {
        var search = input.Trim();

        var allRelics = ModelDb.AllRelics.ToList();


        // 1. 精确 Entry
        var result = allRelics.FirstOrDefault(
            relic =>
                relic.Id.Entry.Equals(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 2. 精确类名
        result = allRelics.FirstOrDefault(
            relic =>
                relic.GetType().Name.Equals(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 3. Entry 开头匹配
        result = allRelics.FirstOrDefault(
            relic =>
                relic.Id.Entry.StartsWith(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 4. 类名开头匹配
        result = allRelics.FirstOrDefault(
            relic =>
                relic.GetType().Name.StartsWith(
                    search,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        if (result is not null)
            return result;


        // 5. 最后再做包含匹配
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
        if (args.Length <= 1)
        {
            var partial =
                args.Length == 0
                    ? string.Empty
                    : args[0];

            var candidates = ModelDb.AllRelics
                .Select(relic => relic.Id.Entry)
                .OrderBy(id => id)
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
