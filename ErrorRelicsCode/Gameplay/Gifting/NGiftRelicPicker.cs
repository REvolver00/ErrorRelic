using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting;

public partial class NGiftRelicPicker : Control
{
    private Player _player = null!;
    private TaskCompletionSource<int?> _result = null!;

    public static async System.Threading.Tasks.Task<int?> Pick(Player player)
    {
        var picker = new NGiftRelicPicker { _player = player, _result = new TaskCompletionSource<int?>() };
        Node? parent = (Node?)MegaCrit.Sts2.Core.Nodes.NRun.Instance?.GlobalUi ?? MegaCrit.Sts2.Core.Nodes.Rooms.NRestSiteRoom.Instance;
        if (parent == null) return null;
        parent.AddChild(picker);
        int? result = await picker._result.Task;
        if (GodotObject.IsInstanceValid(picker)) picker.QueueFree();
        return result;
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        var shade = new ColorRect { Color = new Color(0,0,0,0.72f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(shade);

        var panel = new PanelContainer();
        panel.AnchorLeft = 0.08f; panel.AnchorTop = 0.08f; panel.AnchorRight = 0.92f; panel.AnchorBottom = 0.88f;
        AddChild(panel);
        var box = new VBoxContainer(); panel.AddChild(box);
        var title = new Label { Text = "选择要赠送的遗物", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 26); box.AddChild(title);
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; box.AddChild(scroll);
        var grid = new GridContainer { Columns = 6, SizeFlagsHorizontal = SizeFlags.ExpandFill }; scroll.AddChild(grid);

        for (int i = 0; i < _player.Relics.Count; i++)
        {
            int idx = i;
            var relic = _player.Relics[i];
            bool allowed = GiftRelicSynchronizer.CanGiftIndex(_player, idx);
            var b = new Button { CustomMinimumSize = new Vector2(115, 135), Disabled = !allowed };
            var vb = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            try
            {
                var icon = NRelic.Create(relic, NRelic.IconSize.Small);
                if (icon != null) { icon.MouseFilter = MouseFilterEnum.Ignore; vb.AddChild(icon); }
            }
            catch { }
            string name;
            try { name = relic.Title.GetFormattedText(); } catch { name = relic.Id.Entry; }
            vb.AddChild(new Label { Text = name, HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore });
            b.AddChild(vb);
            if (allowed) b.Pressed += () => _result.TrySetResult(idx);
            grid.AddChild(b);
        }

        var cancel = new Button { Text = "取消", CustomMinimumSize = new Vector2(180, 42) };
        cancel.Pressed += () => _result.TrySetResult(null); box.AddChild(cancel);
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseButton m && m.ButtonIndex == MouseButton.Right && m.IsPressed())
        { _result.TrySetResult(null); GetViewport().SetInputAsHandled(); }
    }
}
