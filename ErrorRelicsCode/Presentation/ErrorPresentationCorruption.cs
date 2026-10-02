using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ErrorRelics.ErrorRelicsCode.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes;

namespace ErrorRelics.ErrorRelicsCode.Presentation;

/// <summary>
/// Local-only ERROR presentation corruption.  It never consumes gameplay RNG,
/// never changes models, hitboxes, combat state, or network state.
/// </summary>
internal static class ErrorPresentationCorruption
{
    private const double RotateChance = 0.35;
    private static readonly Random LocalRandom = new();

    public static void OnErrorPickedUp()
    {
        if (!ErrorRelicsConfig.PickupRandomRotation)
            return;

        ErrorPresentationRuntime.EnsureInstalled();
        ErrorPresentationRuntime.Instance?.CorruptCurrentScene();
    }

    public static string PickFlashSfx(string fallback)
    {
        if (!ErrorRelicsConfig.RandomTriggerSfx)
            return fallback;

        ErrorPresentationRuntime.EnsureInstalled();
        return ErrorPresentationRuntime.Instance?.PickFlashSfx(fallback) ?? fallback;
    }

    internal static int NextQuarterTurn()
        => LocalRandom.Next(1, 4); // 1/2/3 => 90/180/270

    internal static bool ShouldRotate()
        => LocalRandom.NextDouble() < RotateChance;

    internal static int NextIndex(int count)
        => LocalRandom.Next(count);
}

internal sealed partial class ErrorPresentationRuntime : Node
{
    internal const string CustomMarker = "errorrelics-custom://";
    internal const string VanillaMarker = "errorrelics-vanilla://";
    private const double VanillaMaxSeconds = 3.0;
    private const string VanillaListPath = "res://ErrorRelics/audio/vanilla_sfx.txt";
    private const string CustomDirPath = "res://ErrorRelics/audio/custom_sfx";
    private const string CustomManifestPath = "res://ErrorRelics/audio/custom_sfx_manifest.txt";

    private sealed record RotationState(TextureRect Node, float Rotation, Vector2 Pivot);

    private readonly List<RotationState> _rotated = new();
    private readonly List<string> _vanillaSfx = new();
    private readonly List<string> _customSfx = new();
    private ulong _sceneId;
    private bool _pendingCorruption;
    private AudioStreamPlayer? _customPlayer;
    private readonly List<(GodotObject Instance, ulong Deadline)> _vanillaEvents = new();

    public static ErrorPresentationRuntime? Instance { get; private set; }

    public static void EnsureInstalled()
    {
        if (Instance != null && GodotObject.IsInstanceValid(Instance))
            return;

        if (Engine.GetMainLoop() is not SceneTree tree)
            return;

        var runtime = new ErrorPresentationRuntime
        {
            Name = "ErrorRelicsPresentationRuntime",
            ProcessMode = ProcessModeEnum.Always
        };
        tree.Root.CallDeferred(Node.MethodName.AddChild, runtime);
        Instance = runtime;
    }

    public override void _Ready()
    {
        Instance = this;
        _customPlayer = new AudioStreamPlayer
        {
            Name = "ErrorRelicsCustomSfx",
            Bus = "SFX"
        };
        AddChild(_customPlayer);
        LoadAudioPools();

        if (GetTree().CurrentScene is Node scene)
            _sceneId = scene.GetInstanceId();
        if (_pendingCorruption) Callable.From(CorruptCurrentScene).CallDeferred();
    }

    public override void _Process(double delta)
    {
        ulong ticks = Time.GetTicksMsec();
        for (int i = _vanillaEvents.Count - 1; i >= 0; i--)
        {
            if (ticks < _vanillaEvents[i].Deadline) continue;
            StopVanillaInstance(_vanillaEvents[i].Instance);
            _vanillaEvents.RemoveAt(i);
        }
        Node? scene = GetTree().CurrentScene;
        ulong now = scene?.GetInstanceId() ?? 0;
        if (now == _sceneId)
            return;

        // Explicit reset also covers the unusual case where a visual node survives
        // a scene transition instead of being freed with the old scene.
        RestoreRotations();
        _sceneId = now;
    }

    public override void _ExitTree()
    {
        ResetScene();
        if (Instance == this) Instance = null;
    }

    public void ResetScene()
    {
        RestoreRotations();
        StopVanilla();
        _pendingCorruption = false;
    }

    public void CorruptCurrentScene()
    {
        if (!IsInsideTree() || !IsNodeReady())
        {
            _pendingCorruption = true;
            return;
        }
        _pendingCorruption = false;
        Node? scene = GetTree().CurrentScene;
        if (scene == null)
            return;

        if (_sceneId != scene.GetInstanceId()) RestoreRotations();
        _sceneId = scene.GetInstanceId();

        foreach (TextureRect texture in EnumerateTextureRects(scene))
        {
            if (!IsSafeCandidate(texture) || !ErrorPresentationCorruption.ShouldRotate())
                continue;

            // Save each node only once. Repeated ERROR pickups in the same scene can
            // rotate it again; quarter-turns can therefore accidentally "repair" it.
            if (!_rotated.Any(x => GodotObject.IsInstanceValid(x.Node) && x.Node == texture))
                _rotated.Add(new RotationState(texture, texture.RotationDegrees, texture.PivotOffset));

            texture.PivotOffset = texture.Size * 0.5f;
            texture.RotationDegrees += 90f * ErrorPresentationCorruption.NextQuarterTurn();
        }
    }

    public string PickFlashSfx(string fallback)
    {
        // Any later ERROR sound ends our previous custom sound, never game audio.
        if (_customPlayer != null && GodotObject.IsInstanceValid(_customPlayer))
            _customPlayer.Stop();
        // EnsureInstalled() can create this runtime before its deferred AddChild has
        // reached _Ready().  Lazy-loading here makes the very first ERROR flash work.
        if (_vanillaSfx.Count == 0 && _customSfx.Count == 0)
            LoadAudioPools();

        int total = _vanillaSfx.Count + _customSfx.Count;
        if (total == 0)
        {
            MainFile.Logger.Info($"ERROR audio: pool empty; using fallback {fallback}");
            return fallback;
        }

        int index = ErrorPresentationCorruption.NextIndex(total);
        if (index < _vanillaSfx.Count)
        {
            string selected = _vanillaSfx[index];
            MainFile.Logger.Info($"ERROR audio: selected vanilla {selected}");
            return VanillaMarker + selected;
        }

        string custom = _customSfx[index - _vanillaSfx.Count];
        MainFile.Logger.Info($"ERROR audio: selected custom {custom}");
        return CustomMarker + custom;
    }

    public void PlayVanilla(string path, float volume)
    {
        // Own only the instance we start. A malformed/long bank event cannot keep
        // playing indefinitely and stopping it cannot affect the game's instances.
        var fmod = Engine.GetSingleton("FmodServer");
        var description = fmod.Call("get_event", path).AsGodotObject();
        if (description == null || !description.Call("is_valid").AsBool()
            || !description.Call("is_one_shot").AsBool() || description.Call("has_sustain_point").AsBool())
        {
            MainFile.Logger.Info($"ERROR audio rejected non-one-shot or missing event: {path}");
            return;
        }
        var instance = fmod.Call("create_event_instance", path).AsGodotObject();
        if (instance == null) return;
        _vanillaEvents.Add((instance, Time.GetTicksMsec() + (ulong)(VanillaMaxSeconds * 1000)));
        instance.Call("set_volume", volume);
        instance.Call("start");
    }

    private void StopVanilla()
    {
        foreach (var item in _vanillaEvents) StopVanillaInstance(item.Instance);
        _vanillaEvents.Clear();
    }

    private static void StopVanillaInstance(GodotObject instance)
    {
        if (!GodotObject.IsInstanceValid(instance)) return;
        if (instance.Call("is_valid").AsBool())
        {
            instance.Call("stop", false);
            instance.Call("release");
        }
    }

    public void PlayCustom(string resourcePath)
    {
        if (_customPlayer == null || !GodotObject.IsInstanceValid(_customPlayer))
            return;

        try
        {
            // Hard cut: a later ERROR custom sound replaces the earlier one.
            _customPlayer.Stop();
            AudioStream? stream = ResourceLoader.Load<AudioStream>(resourcePath);
            if (stream == null)
                return;
            _customPlayer.Stream = stream;
            _customPlayer.Play();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Info($"ERROR custom audio failed: {ex}");
        }
    }

    private void RestoreRotations()
    {
        foreach (RotationState state in _rotated)
        {
            if (!GodotObject.IsInstanceValid(state.Node))
                continue;
            state.Node.RotationDegrees = state.Rotation;
            state.Node.PivotOffset = state.Pivot;
        }
        _rotated.Clear();
    }

    private static IEnumerable<TextureRect> EnumerateTextureRects(Node root)
    {
        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            Node node = stack.Pop();
            if (node is TextureRect texture)
                yield return texture;

            foreach (Node child in node.GetChildren())
                stack.Push(child);
        }
    }

    private static bool IsSafeCandidate(TextureRect texture)
    {
        if (!texture.Visible || texture.Texture == null)
            return false;

        // The TextureRect itself must not participate in GUI input.  Parent input
        // regions remain untouched because only this drawing Control is rotated.
        if (texture.MouseFilter != Control.MouseFilterEnum.Ignore)
            return false;

        for (Node? n = texture; n != null; n = n.GetParent())
        {
            Type t = n.GetType();
            string ns = t.Namespace ?? string.Empty;
            string name = t.Name;

            // Cards are a hard safety boundary: readability/dragging/targeting wins.
            if (ns.StartsWith("MegaCrit.Sts2.Core.Nodes.Cards", StringComparison.Ordinal))
                return false;

            // Never corrupt controls whose own geometry is directly interactive.
            if (n is BaseButton || n is LineEdit || n is TextEdit || n is SpinBox
                || n is HSlider || n is VSlider || n is ScrollBar)
                return false;

            // Conservative extra guard for explicit hitbox/drag controls.
            if (name.Contains("Hitbox", StringComparison.OrdinalIgnoreCase)
                || name.Contains("CardHolder", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private void LoadAudioPools()
    {
        _vanillaSfx.Clear();
        _customSfx.Clear();

        try
        {
            if (Godot.FileAccess.FileExists(VanillaListPath))
            {
                string text = Godot.FileAccess.GetFileAsString(VanillaListPath);
                foreach (string raw in text.Split('\n'))
                {
                    string line = raw.Trim();
                    if (line.StartsWith("event:/sfx/", StringComparison.Ordinal))
                        _vanillaSfx.Add(line);
                }
            }
            else
            {
                MainFile.Logger.Info($"ERROR audio: vanilla list missing at {VanillaListPath}");
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Info($"ERROR audio: failed to load vanilla list: {ex.Message}");
        }

        // The asset pack creates a manifest because an empty directory does not
        // exist inside a PCK, and imported audio may be represented by remaps.
        try
        {
            if (Godot.FileAccess.FileExists(CustomManifestPath))
            {
                string text = Godot.FileAccess.GetFileAsString(CustomManifestPath);
                foreach (string raw in text.Split('\n'))
                {
                    string file = raw.Trim();
                    if (file.Length == 0)
                        continue;
                    string ext = file.GetExtension().ToLowerInvariant();
                    if (ext is "ogg" or "wav")
                        _customSfx.Add($"{CustomDirPath}/{file}");
                }
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Info($"ERROR audio: failed to load custom manifest: {ex.Message}");
        }

        MainFile.Logger.Info($"ERROR audio pools loaded: vanilla={_vanillaSfx.Count}, custom={_customSfx.Count}");
    }
}

/// <summary>
/// Intercepts only ERROR's private markers; ordinary game audio is untouched.
/// </summary>
[HarmonyPatch]
internal static class ErrorCustomSfxPlayOneShotPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(NAudioManager)
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == "PlayOneShot"
                        && m.GetParameters().Length > 0
                        && m.GetParameters()[0].ParameterType == typeof(string));
    }

    private static bool Prefix(object[] __args)
    {
        if (__args.Length == 0 || __args[0] is not string path) return true;
        bool custom = path.StartsWith(ErrorPresentationRuntime.CustomMarker, StringComparison.Ordinal);
        bool vanilla = path.StartsWith(ErrorPresentationRuntime.VanillaMarker, StringComparison.Ordinal);
        if (!custom && !vanilla) return true;
        ErrorPresentationRuntime.EnsureInstalled();
        if (custom)
            ErrorPresentationRuntime.Instance?.PlayCustom(path[ErrorPresentationRuntime.CustomMarker.Length..]);
        else
            ErrorPresentationRuntime.Instance?.PlayVanilla(path[ErrorPresentationRuntime.VanillaMarker.Length..], __args.OfType<float>().LastOrDefault(1f));
        return false;
    }
}

// The game swaps its own scene containers while SceneTree.CurrentScene stays
// unchanged. Restore surviving HUD/menu TextureRects before those transitions.
[HarmonyPatch(typeof(NSceneContainer), nameof(NSceneContainer.SetCurrentScene))]
internal static class ErrorPresentationScenePatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        var runtime = ErrorPresentationRuntime.Instance;
        if (runtime != null && GodotObject.IsInstanceValid(runtime)) runtime.ResetScene();
    }
}
