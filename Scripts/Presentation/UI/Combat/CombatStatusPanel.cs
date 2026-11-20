using System;
using System.Collections.Generic;
using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private const double _initialProgress = 100.0;
    private const float _markerSize = 14.0f;
    private const float _laneSpacing = 15.0f;
    private const float _railY = 138.0f;
    private const float _leftMargin = 12.0f;
    private const float _rightMargin = 16.0f;
    private const double _markerTweenDuration = 0.45;

    private static readonly Color _railColor = new Color("#758397");
    private static readonly Color _ariColor = new Color("#55a7ff");
    private static readonly Color _partyColor = new Color("#e8bd55");
    private static readonly Color _enemyColor = new Color("#e45f68");

    private Label _round;
    private Label _turn;
    private Control _timelineTrack;
    private Label _stunnedLabel;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;
    private TimelineCombatantStatus[] _timelineEntries = Array.Empty<TimelineCombatantStatus>();
    private readonly Dictionary<string, double> _lastProgressByCombatant =
        new Dictionary<string, double>(StringComparer.Ordinal);
    private bool _hasRenderedTimeline;

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
        if (width < 80.0f)
        {
            return;
        }

        List<TimelineCombatantStatus> visibleEntries = new List<TimelineCombatantStatus>();
        List<string> stunnedNames = new List<string>();

        foreach (TimelineCombatantStatus entry in _timelineEntries)
        {
            if (!entry.IsVisible)
            {
                continue;
            }

            visibleEntries.Add(entry);
            if (entry.IsStunned)
            {
                stunnedNames.Add(entry.Name);
            }
        }

        visibleEntries.Sort(CompareProgress);
        float railY = Math.Max(_railY, 24.0f + Math.Max(0, visibleEntries.Count - 1) * _laneSpacing);
        float requiredHeight = railY + 32.0f;
        if (Math.Abs(_timelineTrack.CustomMinimumSize.Y - requiredHeight) > 1.0f)
        {
            _timelineTrack.CustomMinimumSize = new Vector2(_timelineTrack.CustomMinimumSize.X, requiredHeight);
        }

        float railWidth = Math.Max(1.0f, width - _leftMargin - _rightMargin);
        AddRectangle(
            _timelineTrack,
            new Vector2(_leftMargin, railY),
            new Vector2(railWidth, 2.0f),
            _railColor);

        AddRectangle(
            _timelineTrack,
            new Vector2(_leftMargin, railY - 4.0f),
            new Vector2(2.0f, 10.0f),
            UiTheme.Accent);

        AddRectangle(
            _timelineTrack,
            new Vector2(_leftMargin + railWidth - 2.0f, railY - 4.0f),
            new Vector2(2.0f, 10.0f),
            UiTheme.Accent);

        AddEndpointLabel("START", new Vector2(_leftMargin, railY + 6.0f), new Vector2(45.0f, 16.0f));
        AddEndpointLabel("READY", new Vector2(_leftMargin + railWidth - 45.0f, railY + 6.0f), new Vector2(45.0f, 16.0f));

        float[] laneLastCenter = new float[Math.Max(1, visibleEntries.Count)];
        for (int lane = 0; lane < laneLastCenter.Length; lane++)
        {
            laneLastCenter[lane] = float.NegativeInfinity;
        }

        Dictionary<string, double> currentProgressByCombatant =
            new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (TimelineCombatantStatus entry in visibleEntries)
        {
            double progress = GetProgressFraction(entry);
            currentProgressByCombatant[entry.Id] = progress;

            float markerCenter = _leftMargin + railWidth * (float)progress;
            int lane = FindAvailableLane(markerCenter, laneLastCenter);
            laneLastCenter[lane] = markerCenter;

            float markerY = 3.0f + lane * _laneSpacing;
            float markerX = Mathf.Clamp(
                markerCenter - _markerSize / 2.0f,
                _leftMargin - _markerSize / 2.0f,
                _leftMargin + railWidth - _markerSize / 2.0f);

            Color markerColor = GetMarkerColor(entry);
            Color stemColor = new Color(markerColor.R, markerColor.G, markerColor.B, 0.58f);
            float stemTop = markerY + _markerSize;
            float stemHeight = Math.Max(0.0f, railY - stemTop);

            if (stemHeight > 0.0f)
            {
                AddRectangle(
                    _timelineTrack,
                    new Vector2(markerCenter - 1.0f, stemTop),
                    new Vector2(2.0f, stemHeight),
                    stemColor);
            }

            PanelContainer marker = CreateMarker(entry, markerColor, progress);
            double previousProgress = 0.0;
            if (_hasRenderedTimeline &&
                _lastProgressByCombatant.TryGetValue(entry.Id, out double lastProgress))
            {
                previousProgress = lastProgress;
            }
            else if (_hasRenderedTimeline)
            {
                previousProgress = progress;
            }

            float previousX = _leftMargin + railWidth * (float)previousProgress - _markerSize / 2.0f;
            marker.Position = new Vector2(previousX, markerY);
            _timelineTrack.AddChild(marker);

            Vector2 targetPosition = new Vector2(markerX, markerY);
            marker.CreateTween()
                .TweenProperty(marker, "position", targetPosition, _markerTweenDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }

        foreach (TimelineCombatantStatus entry in _timelineEntries)
        {
            if (!currentProgressByCombatant.ContainsKey(entry.Id))
            {
                _lastProgressByCombatant[entry.Id] = GetProgressFraction(entry);
            }
        }

        foreach (KeyValuePair<string, double> current in currentProgressByCombatant)
        {
            _lastProgressByCombatant[current.Key] = current.Value;
        }

        _hasRenderedTimeline = _timelineEntries.Length > 0;
        _stunnedLabel.Text = stunnedNames.Count == 0
            ? string.Empty
            : "STUNNED  ·  " + string.Join("  ·  ", stunnedNames);
        _stunnedLabel.Visible = stunnedNames.Count > 0;
    }

    private PanelContainer CreateMarker(
        TimelineCombatantStatus entry,
        Color markerColor,
        double progress)
    {
        PanelContainer marker = new PanelContainer
        {
            Size = new Vector2(_markerSize, _markerSize),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            TooltipText = entry.Name + " · " +
                (entry.IsStunned
                    ? "STUNNED"
                    : entry.IsReady
                        ? "READY"
                        : (progress * 100.0).ToString("0") + "% progress")
        };

        StyleBoxFlat markerStyle = new StyleBoxFlat
        {
            BgColor = markerColor,
            CornerRadiusTopLeft = 7,
            CornerRadiusTopRight = 7,
            CornerRadiusBottomLeft = 7,
            CornerRadiusBottomRight = 7
        };
        marker.AddThemeStyleboxOverride("panel", markerStyle);
        return marker;
    }

    private static double GetProgressFraction(TimelineCombatantStatus entry)
    {
        if (entry.IsReady || entry.RemainingProgress <= 0.0)
        {
            return 1.0;
        }

        double remainingFraction = entry.RemainingProgress / _initialProgress;
        return Math.Clamp(1.0 - remainingFraction, 0.0, 1.0);
    }

    private static Color GetMarkerColor(TimelineCombatantStatus entry)
    {
        if (entry.IsEnemy)
        {
            return _enemyColor;
        }

        return entry.IsPrimaryPartyMember ? _ariColor : _partyColor;
    }

    private static int CompareProgress(TimelineCombatantStatus left, TimelineCombatantStatus right)
    {
        int progressComparison = GetProgressFraction(left).CompareTo(GetProgressFraction(right));
        if (progressComparison != 0)
        {
            return progressComparison;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private static int FindAvailableLane(float markerCenter, float[] laneLastCenter)
    {
        for (int lane = 0; lane < laneLastCenter.Length; lane++)
        {
            if (markerCenter - laneLastCenter[lane] >= _markerSize + 4.0f)
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

    private void AddEndpointLabel(string text, Vector2 position, Vector2 size)
    {
        Label label = new Label
        {
            Text = text,
            Position = position,
            Size = size,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", 9);
        label.AddThemeColorOverride("font_color", UiTheme.Muted);
        _timelineTrack.AddChild(label);
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
