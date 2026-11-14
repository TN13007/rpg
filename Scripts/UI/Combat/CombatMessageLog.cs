using Godot;

public partial class CombatMessageLog : PanelContainer
{
    private const int MaximumMessages = 60;

    private ScrollContainer _scroll;
    private VBoxContainer _entries;
    private PackedScene _entryScene;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _scroll = GetNode<ScrollContainer>("Margin/Scroll");
        _entries = GetNode<VBoxContainer>("Margin/Scroll/Entries");
        _entryScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/CombatMessageEntry.tscn");
    }

    public void Append(CombatMessage message)
    {
        if (_entryScene == null)
        {
            GD.PushError("CombatMessageEntry.tscn could not be loaded.");
            return;
        }

        CombatMessageEntry entry = _entryScene.Instantiate<CombatMessageEntry>();
        _entries.AddChild(entry);
        entry.SetMessage(message);

        while (_entries.GetChildCount() > MaximumMessages)
        {
            Node oldestEntry = _entries.GetChild(0);
            _entries.RemoveChild(oldestEntry);
            oldestEntry.QueueFree();
        }

        _scroll.SetDeferred("scroll_vertical", 100000);
    }

    public void Clear()
    {
        foreach (Node child in _entries.GetChildren())
        {
            _entries.RemoveChild(child);
            child.QueueFree();
        }

        _scroll.ScrollVertical = 0;
    }
}
