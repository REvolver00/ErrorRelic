using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace ErrorRelics.ErrorRelicsCode.Debug;

// Keep the original exceptions and awaits. Paired log entries identify the last
// unfinished load stage or ERROR effect without hiding the failure.
[HarmonyPatch]
internal static class ErrorLoadDiagnostics
{
    private static int _loads;
    internal static bool IsLoading => _loads > 0;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(NGame), nameof(NGame.LoadRun));
        foreach (string name in new[] { "SetUpSavedSingleplayer", "SetUpSavedMultiplayer",
            "GenerateMap", "LoadIntoLatestMapCoord", "EnterRoomInternal" })
            yield return AccessTools.Method(typeof(RunManager), name);
    }

    [HarmonyPrefix]
    private static void Prefix(MethodBase __originalMethod, out bool __state)
    {
        string name = __originalMethod.Name;
        __state = !TestMode.IsOn && (IsLoading || name is "LoadRun" or "SetUpSavedSingleplayer" or "SetUpSavedMultiplayer");
        if (!__state) return;
        if (name == "LoadRun") _loads++;
        MainFile.Logger.Info($"ERROR load begin: {name}");
    }

    [HarmonyPostfix]
    private static void Postfix(MethodBase __originalMethod, bool __state, ref Task __result)
    {
        if (__state) __result = Trace(__result, __originalMethod.Name);
    }

    private static async Task Trace(Task original, string stage)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            await original;
            MainFile.Logger.Info($"ERROR load end: {stage}, ms={watch.ElapsedMilliseconds}");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Info($"ERROR load failed: {stage}, ms={watch.ElapsedMilliseconds}, {ex}");
            throw;
        }
        finally
        {
            if (stage == "LoadRun") _loads--;
        }
    }
}

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
internal static class ErrorReconstructionTracePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        if (!TestMode.IsOn) MainFile.Logger.Info("ERROR load begin: RunState.FromSerializable");
    }

    [HarmonyFinalizer]
    private static void Finalizer(Exception? __exception)
    {
        if (!TestMode.IsOn)
            MainFile.Logger.Info(__exception == null ? "ERROR load end: RunState.FromSerializable"
                : $"ERROR load failed: RunState.FromSerializable, {__exception}");
    }
}
