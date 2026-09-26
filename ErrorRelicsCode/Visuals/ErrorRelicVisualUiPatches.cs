using System;

using ErrorRelics.ErrorRelicsCode.Relics;

using Godot;

using HarmonyLib;

using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace ErrorRelics.ErrorRelicsCode.Visuals;


// =============================================================
// VISUAL2
//
// UI-ONLY.
//
// No RelicModel icon getter patches.
// No image generation.
// No RelicCmd / RelicFactory changes.
//
// The stable ERROR model keeps its original relic.png internally.
// Only after a UI node has loaded normally do we:
// 1. replace that TextureRect's displayed texture with the vanilla
//    relic that this ERROR replaced;
// 2. apply deterministic "wrong" presentation:
//    - horizontal / vertical flip
//    - 90/180/270 degree rotation sometimes
//    - mild size error
//    - color tint / brightness error
//
// Because the parameters are derived from saved source path + H/E,
// save/load produces the same visual without another saved seed.
// =============================================================

internal readonly struct ErrorVisualParameters
{
    public ErrorVisualParameters(
        bool flipH,
        bool flipV,
        float rotationDegrees,
        float scale,
        Color tint)
    {
        FlipH = flipH;
        FlipV = flipV;
        RotationDegrees = rotationDegrees;
        Scale = scale;
        Tint = tint;
    }

    public bool FlipH { get; }

    public bool FlipV { get; }

    public float RotationDegrees { get; }

    public float Scale { get; }

    public Color Tint { get; }


    public static ErrorVisualParameters From(
        ErrorRandomTestRelic relic)
    {
        int seed =
            StableHash(
                relic.VisualSourceIconPath
                + "|"
                + relic.GeneratedHookId
                + "|"
                + relic.GeneratedEffectId
            );

        Random random =
            new(seed);

        bool flipH =
            random.NextDouble()
            < 0.55;

        bool flipV =
            random.NextDouble()
            < 0.28;

        float rotation =
            0f;

        if (random.NextDouble() < 0.46)
        {
            int turn =
                random.Next(
                    1,
                    4
                );

            rotation =
                90f * turn;
        }

        float scale =
            0.88f
            + (float) random.NextDouble()
            * 0.22f;

        // Medium-strength tint. Keep alpha at 1 so silhouettes and
        // hit testing are unaffected.
        float red =
            0.62f
            + (float) random.NextDouble()
            * 0.48f;

        float green =
            0.62f
            + (float) random.NextDouble()
            * 0.48f;

        float blue =
            0.62f
            + (float) random.NextDouble()
            * 0.48f;

        // Guarantee visible color error when random channels happen
        // to be too close to white.
        if (Math.Abs(red - green) < 0.05f
            && Math.Abs(green - blue) < 0.05f)
        {
            blue *= 0.72f;
        }

        return new ErrorVisualParameters(
            flipH,
            flipV,
            rotation,
            scale,
            new Color(
                red,
                green,
                blue,
                1f
            )
        );
    }


    private static int StableHash(
        string value)
    {
        unchecked
        {
            uint hash =
                2166136261u;

            foreach (char c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return (int) hash;
        }
    }
}


internal static class ErrorVisualUi
{
    public static Texture2D? LoadSmall(
        ErrorRandomTestRelic relic)
    {
        if (string.IsNullOrEmpty(
                relic.VisualSourceIconPath))
        {
            return null;
        }

        try
        {
            return ResourceLoader.Load<Texture2D>(
                relic.VisualSourceIconPath
            );
        }
        catch
        {
            return null;
        }
    }


    public static Texture2D? LoadOutline(
        ErrorRandomTestRelic relic)
    {
        if (string.IsNullOrEmpty(
                relic.VisualSourceOutlinePath))
        {
            return null;
        }

        try
        {
            return ResourceLoader.Load<Texture2D>(
                relic.VisualSourceOutlinePath
            );
        }
        catch
        {
            return null;
        }
    }


    public static Texture2D? LoadBig(
        ErrorRandomTestRelic relic)
    {
        if (string.IsNullOrEmpty(
                relic.VisualSourceBigIconPath))
        {
            return null;
        }

        try
        {
            return PreloadManager.Cache.GetTexture2D(
                relic.VisualSourceBigIconPath
            );
        }
        catch
        {
            return null;
        }
    }


    public static void Apply(
        TextureRect icon,
        TextureRect? outline,
        ErrorRandomTestRelic relic)
    {
        ErrorVisualParameters visual =
            ErrorVisualParameters.From(
                relic
            );

        ApplyTransform(
            icon,
            visual,
            true
        );

        if (outline != null
            && outline.Visible)
        {
            ApplyTransform(
                outline,
                visual,
                false
            );
        }
    }


    private static void ApplyTransform(
        TextureRect texture,
        ErrorVisualParameters visual,
        bool applyTint)
    {
        // VISUAL2.1:
        // This node is presentation only. Never let the transformed
        // TextureRect consume input that belongs to the relic holder.
        texture.MouseFilter =
            Control.MouseFilterEnum.Ignore;

        // Keep only transforms that do NOT move the Control's hit area.
        // Rotation/Scale on a Godot Control can make the visible relic
        // drift away from the merchant holder's actual click rectangle.
        texture.FlipH =
            visual.FlipH;

        texture.FlipV =
            visual.FlipV;

        texture.RotationDegrees =
            0f;

        texture.Scale =
            Vector2.One;

        texture.SelfModulate =
            applyTint
                ? visual.Tint
                : Colors.White;
    }
}


// -------------------------------------------------------------
// Most relic visuals: inventory, shop, treasure, etc.
// -------------------------------------------------------------

[HarmonyPatch(
    typeof(NRelic),
    "Reload"
)]
public static class NRelicErrorVisualPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        NRelic __instance)
    {
        try
        {
            if (__instance.Model
                is not ErrorRandomTestRelic errorRelic
                || !errorRelic.HasVisualSource)
            {
                return;
            }

            // NRelic is documented as visuals-only. Explicitly
            // make the wrapper pass mouse input through to RelicHolder.
            __instance.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            __instance.Icon.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            __instance.Outline.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            NRelic.IconSize iconSize =
                Traverse.Create(__instance)
                    .Field("_iconSize")
                    .GetValue<NRelic.IconSize>();

            if (iconSize
                == NRelic.IconSize.Small)
            {
                Texture2D? small =
                    ErrorVisualUi.LoadSmall(
                        errorRelic
                    );

                if (small != null)
                {
                    __instance.Icon.Texture =
                        small;
                }

                Texture2D? outline =
                    ErrorVisualUi.LoadOutline(
                        errorRelic
                    );

                if (outline != null)
                {
                    __instance.Outline.Texture =
                        outline;
                }
            }
            else
            {
                Texture2D? big =
                    ErrorVisualUi.LoadBig(
                        errorRelic
                    );

                if (big != null)
                {
                    __instance.Icon.Texture =
                        big;
                }
            }

            ErrorVisualUi.Apply(
                __instance.Icon,
                __instance.Outline,
                errorRelic
            );
        }
        catch
        {
            // Visuals are never allowed to break the run.
            // Vanilla NRelic.Reload has already completed.
        }
    }
}


// -------------------------------------------------------------
// RelicReward creates a TextureRect directly rather than NRelic.
// Keep this patch presentation-only as well.
// -------------------------------------------------------------

[HarmonyPatch(
    typeof(RelicReward),
    "CreateIcon"
)]
public static class RelicRewardErrorVisualPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        RelicReward __instance,
        ref TextureRect __result)
    {
        try
        {
            if (__instance.Relic
                is not ErrorRandomTestRelic errorRelic
                || !errorRelic.HasVisualSource)
            {
                return;
            }

            Texture2D? big =
                ErrorVisualUi.LoadBig(
                    errorRelic
                );

            if (big != null)
            {
                __result.Texture =
                    big;
            }

            ErrorVisualUi.Apply(
                __result,
                null,
                errorRelic
            );
        }
        catch
        {
            // Original reward icon remains usable.
        }
    }
}
