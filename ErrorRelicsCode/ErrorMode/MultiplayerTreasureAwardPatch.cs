using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace ErrorRelics.ErrorRelicsCode.ErrorMode;

// Leave the shared choices, voting, holder lookup and consolation prizes vanilla.
// Convert only the mutable award, once the original flow has decided its owner.
[HarmonyPatch]
public static class MultiplayerTreasureAwardPatch
{
    private static MethodBase TargetMethod() => AccessTools.AsyncMoveNext(
        AccessTools.Method(typeof(NTreasureRoomRelicCollection), "AnimateRelicAwards"));

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        int replacements = 0;
        for (int i = 1; i < codes.Count; i++)
        {
            if (codes[i].operand is not MethodInfo method || method.Name != "ToMutable"
                || method.ReturnType != typeof(RelicModel)
                || codes[i - 1].opcode != OpCodes.Ldfld
                || codes[i - 1].operand is not FieldInfo field
                || field.DeclaringType != typeof(RelicPickingResult) || field.Name != "relic")
                continue;

            // Stack before ldfld holds RelicPickingResult. Keep it, so the
            // replacement receives both the source relic and the awarded player.
            codes[i - 1].opcode = OpCodes.Nop;
            codes[i - 1].operand = null;
            codes[i].opcode = OpCodes.Call;
            codes[i].operand = AccessTools.Method(typeof(MultiplayerTreasureAwardPatch), nameof(CreateAwardRelic));
            replacements++;
        }
        if (replacements != 1)
            throw new InvalidOperationException("ERROR treasure award patch no longer matches the game. Expected one award clone.");
        return codes;
    }

    public static RelicModel CreateAwardRelic(RelicPickingResult result)
    {
        var player = result.player;
        if (result.type == RelicPickingResultType.Skipped || player == null
            || player.RunState.Players.Count == 1 || !ErrorModeState.IsEnabled(player)
            || ErrorModeState.IsErrorRelic(result.relic) || ErrorModeState.IsProofRelic(result.relic))
            return result.relic.ToMutable();

        var replacement = ErrorModeState.CreateLockedError(result.relic, player, "treasure");
        // Generation already removed the source from the shared bag. The new
        // synthetic model cannot remove the original from this player's bag.
        player.RelicGrabBag.Remove(result.relic);
        return replacement;
    }
}
