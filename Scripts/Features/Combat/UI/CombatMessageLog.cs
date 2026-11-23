using Godot;

/// <summary>Displays the sequence of combat messages and supports clearing the log.</summary>
public partial class CombatMessageLog : PanelContainer
{
    private const int _maximumMessages = 60;
    private const int _bottomThreshold = 4;

    private ScrollContainer _scroll;
    private VBoxContainer _entries;
    private PackedScene _entryScene;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _scroll = GetNode<ScrollContainer>("Margin/Scroll");
        _entries = GetNode<VBoxContainer>("Margin/Scroll/Entries");
        _entryScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/CombatMessageEntry.tscn");
    }

    /// <summary>Appends append to the combat message log.</summary>
    /// <param name="message">The message used by this operation.</param>
    public void Append(CombatMessage message)
    {
        if (_entryScene == null)
        {
            GD.PushError("CombatMessageEntry.tscn could not be loaded.");
            return;
        }

        bool shouldFollowLatest = IsAtBottom();
        CombatMessageEntry entry = _entryScene.Instantiate<CombatMessageEntry>();
        _entries.AddChild(entry);
        entry.SetMessage(message);

        while (_entries.GetChildCount() > _maximumMessages)
        {
            Node oldestEntry = _entries.GetChild(0);
            _entries.RemoveChild(oldestEntry);
            oldestEntry.QueueFree();
        }

        if (shouldFollowLatest)
        {
            _scroll.SetDeferred("scroll_vertical", 100000);
        }
    }

    /// <summary>Clears clear from the current UI state.</summary>
    public void Clear()
    {
        foreach (Node child in _entries.GetChildren())
        {
            _entries.RemoveChild(child);
            child.QueueFree();
        }

        _scroll.ScrollVertical = 0;
    }

    private bool IsAtBottom()
    {
        VScrollBar scrollBar = _scroll.GetVScrollBar();
        int maximumScroll = Mathf.Max(0, (int)(scrollBar.MaxValue - scrollBar.Page));
        return _scroll.ScrollVertical >= maximumScroll - _bottomThreshold;
    }
}
