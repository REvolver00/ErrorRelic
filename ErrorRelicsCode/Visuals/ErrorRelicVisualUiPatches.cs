using System;
using ErrorRelics.ErrorRelicsCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.Visuals;

internal readonly struct ErrorVisualParameters
{
    public ErrorVisualParameters(bool flipH, bool flipV, Color tint, float seed)
    {
        FlipH = flipH;
        FlipV = flipV;
        Tint = tint;
        Seed = seed;
    }

    public bool FlipH { get; }
    public bool FlipV { get; }
    public Color Tint { get; }
    public float Seed { get; }

    public static ErrorVisualParameters From(ErrorRandomTestRelic relic)
    {
        int hash = StableHash(
            relic.VisualSourceIconPath + "|" +
            relic.GeneratedHookId + "|" +
            relic.GeneratedEffectId);

        var random = new Random(hash);
        bool flipH = random.NextDouble() < 0.55;
        bool flipV = random.NextDouble() < 0.28;
        float red = 0.62f + (float)random.NextDouble() * 0.48f;
        float green = 0.62f + (float)random.NextDouble() * 0.48f;
        float blue = 0.62f + (float)random.NextDouble() * 0.48f;
        if (Math.Abs(red-green) < .05f && Math.Abs(green-blue) < .05f)
            blue *= .72f;

        float seed = ((uint)hash & 0x00ffffffu) / 16777215f;
        return new ErrorVisualParameters(flipH, flipV, new Color(red,green,blue,1f), seed);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            uint h = 2166136261u;
            foreach (char c in value) { h ^= c; h *= 16777619u; }
            return (int)h;
        }
    }
}

internal static class ErrorVisualUi
{
    private static Shader? _shader;

    private const string ShaderCode = """
shader_type canvas_item;
uniform float error_seed = 0.0;

float h(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

void fragment() {
    vec2 uv = UV;

    float band = floor(uv.y * 12.0);
    float br = h(band + error_seed * 997.0);
    if (br > 0.67)
        uv.x = fract(uv.x + (br - 0.67) * 0.48);

    vec2 cell = floor(uv * 3.0);
    float tr = h(cell.x + cell.y * 11.0 + error_seed * 431.0);
    if (tr > 0.79)
        uv.x = fract(uv.x + 0.3333333 * (tr > 0.90 ? -1.0 : 1.0));

    vec4 c = texture(TEXTURE, uv);

    vec2 block = floor(UV * vec2(14.0, 14.0));
    float gr = h(block.x + block.y * 23.0 + error_seed * 733.0);
    if (gr > 0.955 && c.a > 0.04) {
        vec3 noise = vec3(h(gr+1.0), h(gr+2.0), h(gr+3.0));
        c.rgb = mix(c.rgb, noise, 0.90);
    }

    COLOR = c * COLOR;
}
""";

    private static Texture2D? Load(string path, bool preload)
    {
        if (string.IsNullOrEmpty(path)) return null;
        try
        {
            // ERROR visuals may come from vanilla resources or this mod's PCK.
            // ResourceLoader handles both. PreloadManager.Cache is only a cache
            // and emits "Asset not cached" for valid mod-owned resources.
            return ResourceLoader.Load<Texture2D>(path);
        }
        catch { return null; }
    }

    // This is deliberately the ONE appearance function used by both reward
    // preview and inventory NRelic. No partial inventory-only styling.
    public static void ApplyFullAppearance(
        TextureRect icon,
        TextureRect? outline,
        ErrorRandomTestRelic relic,
        bool useBigSource)
    {
        // 1) Explicitly restore the same source identity every time a UI node
        // is rebuilt. This prevents NRelic.Reload from leaving the NOPE asset.
        Texture2D? source = useBigSource
            ? Load(relic.VisualSourceBigIconPath, true)
            : Load(relic.VisualSourceIconPath, false);

        if (source == null && useBigSource)
            source = Load(relic.VisualSourceIconPath, false);

        if (source != null)
            icon.Texture = source;

        if (outline != null)
        {
            Texture2D? sourceOutline = Load(relic.VisualSourceOutlinePath, false);
            if (sourceOutline != null)
                outline.Texture = sourceOutline;
        }

        // 2) Exact same deterministic transforms/material on every path.
        var v = ErrorVisualParameters.From(relic);

        icon.MouseFilter = Control.MouseFilterEnum.Ignore;
        icon.FlipH = v.FlipH;
        icon.FlipV = v.FlipV;
        icon.RotationDegrees = 0f;
        icon.Scale = Vector2.One;
        icon.SelfModulate = v.Tint;

        try
        {
            _shader ??= new Shader { Code = ShaderCode };
            var mat = new ShaderMaterial { Shader = _shader };
            mat.SetShaderParameter("error_seed", v.Seed);
            icon.Material = mat;
        }
        catch
        {
            icon.Material = null;
        }

        if (outline != null && outline.Visible)
        {
            outline.MouseFilter = Control.MouseFilterEnum.Ignore;
            outline.FlipH = v.FlipH;
            outline.FlipV = v.FlipV;
            outline.RotationDegrees = 0f;
            outline.Scale = Vector2.One;
            outline.SelfModulate = Colors.White;
            outline.Material = null;
        }
    }
}

[HarmonyPatch(typeof(NRelic), "_Ready")]
public static class NRelicErrorVisualReadyPatch
{
    [HarmonyPostfix]
    public static void Postfix(NRelic __instance)
    {
        TryApply(__instance);
        ReapplyDeferred(__instance);
    }

    private static void TryApply(NRelic n)
    {
        try
        {
            object? model = Traverse.Create(n).Property("Model").GetValue();
            if (model is not ErrorRandomTestRelic error || !error.HasVisualSource)
                return;

            ErrorVisualUi.ApplyFullAppearance(
                n.Icon,
                n.Outline,
                error,
                true);
        }
        catch { }
    }

    private static async void ReapplyDeferred(NRelic n)
    {
        try
        {
            await n.ToSignal(n.GetTree(), SceneTree.SignalName.ProcessFrame);
            if (GodotObject.IsInstanceValid(n))
                TryApply(n);

            await n.ToSignal(n.GetTree(), SceneTree.SignalName.ProcessFrame);
            if (GodotObject.IsInstanceValid(n))
                TryApply(n);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(NRelic), "Reload")]
public static class NRelicErrorVisualReloadPatch
{
    [HarmonyPostfix]
    public static void Postfix(NRelic __instance)
    {
        try
        {
            object? model = Traverse.Create(__instance).Property("Model").GetValue();
            if (model is not ErrorRandomTestRelic error || !error.HasVisualSource)
                return;

            ErrorVisualUi.ApplyFullAppearance(
                __instance.Icon,
                __instance.Outline,
                error,
                true);
        }
        catch { }
    }
}

[HarmonyPatch(typeof(RelicReward), "CreateIcon")]
public static class RelicRewardErrorVisualPatch
{
    [HarmonyPostfix]
    public static void Postfix(RelicReward __instance, ref TextureRect __result)
    {
        try
        {
            if (__instance.Relic is not ErrorRandomTestRelic error || !error.HasVisualSource)
                return;

            ErrorVisualUi.ApplyFullAppearance(
                __result,
                null,
                error,
                true);
        }
        catch
        {
            // Reward remains usable even if presentation fails.
        }
    }
}
