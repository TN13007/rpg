using System;
using System.Collections.Generic;
using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private const double TimelineWindow = 8.0;
    private const float MarkerWidth = 108.0f;
    private const float MarkerHeight = 20.0f;
    private const int TimelineLaneCount = 6;
    private const float LaneSpacing = 22.0f;

    private Label _round;
    private Label _turn;
    private Control _timelineTrack;
    private Label _stunnedLabel;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;
    private TimelineCombatantStatus[] _timelineEntries = Array.Empty<TimelineCombatantStatus>();

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _round = GetNode<Label>("Margin/Content/Heading/Round");
        _turn = GetNode<Label>("Margin/Content/Heading/Turn");
        _timelineTrack = GetNode<Control>("Margin/Content/TimelineTrack");
        _stunnedLabel = GetNode<Label>("Margin/Content/StunnedLabel");
        _intentCard = GetNode<PanelContainer>("Margin/Content/IntentCard");
        _intentHeading = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Heading");
        _intentDescription = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Description");
        _intentConfidence = GetNode<Label>("Margin/Content/IntentCard/Margin/Content/Confidence");
        _intentCard.Visible = false;
        _timelineTrack.Resized += RefreshTimeline;
    }

    public void SetTurn(int round, string turnLabel)
    {
        _round.Text = "ROUND " + round.ToString("00");
        _turn.Text = turnLabel.ToUpperInvariant();
    }

    public void SetTimeline(TimelineCombatantStatus[] entries)
    {
        _timelineEntries = entries ?? Array.Empty<TimelineCombatantStatus>();
        RefreshTimeline();
    }

    public void SetIntent(IntentObservation intent)
    {
        if (intent == null)
        {
            ClearIntent();
            return;
        }

        _intentCard.Visible = true;
        _intentHeading.Text = intent.EnemyName;
        _intentDescription.Text = intent.Description;
        _intentConfidence.Text = intent.Confidence == IntentConfidence.Recognized ? "RECOGNIZED" : "SUSPECTED";
    }

    private void RefreshTimeline()
    {
        ClearChildren(_timelineTrack);

        float width = _timelineTrack.Size.X;
        if (width < 180.0f)
        {
            return;
        }

        const float left = 44.0f;
        const float rightMargin = 16.0f;
        const float railY = 143.0f;
        float railWidth = width - left - rightMargin;

        AddRectangle(
            _timelineTrack,
            new Vector2(left, railY),
            new Vector2(railWidth, 3.0f),
            new Color("#566171"));

        AddRectangle(
            _timelineTrack,
            new Vector2(left, railY - 5.0f),
            new Vector2(2.0f, 13.0f),
            UiTheme.Accent);

        for (int tick = 0; tick <= (int)TimelineWindow; tick += 2)
        {
            float tickX = left + railWidth * tick / (float)TimelineWindow;
            AddRectangle(
                _timelineTrack,
                new Vector2(tickX - 1.0f, railY - 4.0f),
                new Vector2(2.0f, 11.0f),
                new Color("#7b8795"));

            Label tickLabel = new Label
            {
                Text = tick == 0 ? "NOW" : tick == (int)TimelineWindow ? "8 CT" : tick.ToString(),
                Position = new Vector2(tick == 0 ? tickX - 36.0f : tickX - 9.0f, railY + 7.0f),
                Size = new Vector2(tick == 0 ? 36.0f : 24.0f, 18.0f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            tickLabel.AddThemeFontSizeOverride("font_size", 9);
            tickLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
            _timelineTrack.AddChild(tickLabel);
        }

        List<TimelineCombatantStatus> visibleEntries = new List<TimelineCombatantStatus>();
        List<string> stunnedNames = new List<string>();

        foreach (TimelineCombatantStatus entry in _timelineEntries)
        {
            if (entry.IsStunned)
            {
                stunnedNames.Add(entry.Name);
                continue;
            }

            if (entry.Speed <= 0.0)
            {
                continue;
            }

            visibleEntries.Add(entry);
        }

        visibleEntries.Sort(CompareTimelineEntries);
        float[] laneLastCenter = new float[TimelineLaneCount];
        for (int lane = 0; lane < TimelineLaneCount; lane++)
        {
            laneLastCenter[lane] = float.NegativeInfinity;
        }

        foreach (TimelineCombatantStatus entry in visibleEntries)
        {
            double timeToReady = GetTimeToReady(entry);
            double clampedTime = Math.Min(timeToReady, TimelineWindow);
            float markerCenter = left + railWidth * (float)(clampedTime / TimelineWindow);
            int lane = FindAvailableLane(markerCenter, laneLastCenter);
            laneLastCenter[lane] = markerCenter;

            float markerX = Mathf.Clamp(markerCenter - MarkerWidth / 2.0f, 0.0f, width - MarkerWidth);
            float markerY = 2.0f + lane * LaneSpacing;
            Color markerBackground = entry.IsReady
                ? new Color("#514735")
                : entry.IsEnemy ? new Color("#3b2d2d") : new Color("#303744");
            Color markerText = entry.IsEnemy ? new Color("#e7a69d") : UiTheme.Text;

            AddRectangle(
                _timelineTrack,
                new Vector2(markerCenter - 1.0f, markerY + MarkerHeight),
                new Vector2(2.0f, Math.Max(0.0f, railY - markerY - MarkerHeight)),
                entry.IsEnemy ? new Color("#a85f55") : UiTheme.Accent);

            PanelContainer marker = new PanelContainer
            {
                Position = new Vector2(markerX, markerY),
                Size = new Vector2(MarkerWidth, MarkerHeight),
                MouseFilter = Control.MouseFilterEnum.Ignore,
                TooltipText = entry.Name + " · " + (entry.IsReady
                    ? "Ready now"
                    : timeToReady.ToString("0.0") + " Combat Time")
            };

            StyleBoxFlat markerStyle = new StyleBoxFlat
            {
                BgColor = markerBackground,
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4,
                CornerRadiusBottomRight = 4
            };
            markerStyle.ContentMarginLeft = 3;
            markerStyle.ContentMarginRight = 3;
            markerStyle.ContentMarginTop = 1;
            markerStyle.ContentMarginBottom = 1;
            marker.AddThemeStyleboxOverride("panel", markerStyle);

            string timeLabel = entry.IsReady
                ? "NOW"
                : timeToReady > TimelineWindow
                    ? ">8 CT"
                    : timeToReady.ToString("0.0") + " CT";
            Label markerLabel = new Label
            {
                Text = (entry.IsEnemy ? "E" : "P") + " · " + entry.Name + "  " + timeLabel,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            markerLabel.AddThemeFontSizeOverride("font_size", 9);
            markerLabel.AddThemeColorOverride("font_color", markerText);
            marker.AddChild(markerLabel);
            _timelineTrack.AddChild(marker);
        }

        _stunnedLabel.Text = stunnedNames.Count == 0
            ? string.Empty
            : "STUNNED  ·  " + string.Join("  ·  ", stunnedNames);
        _stunnedLabel.Visible = stunnedNames.Count > 0;
    }

    private static double GetTimeToReady(TimelineCombatantStatus entry)
    {
        return entry.IsReady ? 0.0 : entry.RemainingProgress / entry.Speed;
    }

    private static int CompareTimelineEntries(TimelineCombatantStatus left, TimelineCombatantStatus right)
    {
        int timeComparison = GetTimeToReady(left).CompareTo(GetTimeToReady(right));
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        int luckComparison = right.Luck.CompareTo(left.Luck);
        if (luckComparison != 0)
        {
            return luckComparison;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private static int FindAvailableLane(float markerCenter, float[] laneLastCenter)
    {
        for (int lane = 0; lane < laneLastCenter.Length; lane++)
        {
            if (markerCenter - laneLastCenter[lane] >= MarkerWidth + 6.0f)
            {
                return lane;
            }
        }

        int selectedLane = 0;
        for (int lane = 1; lane < laneLastCenter.Length; lane++)
        {
            if (laneLastCenter[lane] < laneLastCenter[selectedLane])
            {
                selectedLane = lane;
            }
        }

        return selectedLane;
    }

    private static void AddRectangle(Control parent, Vector2 position, Vector2 size, Color color)
    {
        ColorRect rectangle = new ColorRect
        {
            Position = position,
            Size = size,
            Color = color,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        parent.AddChild(rectangle);
    }

    private void ClearIntent()
    {
        _intentCard.Visible = false;
        _intentHeading.Text = string.Empty;
        _intentDescription.Text = string.Empty;
        _intentConfidence.Text = string.Empty;
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
