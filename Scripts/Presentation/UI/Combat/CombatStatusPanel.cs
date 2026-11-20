using System;
using System.Collections.Generic;
using Godot;

public partial class CombatStatusPanel : PanelContainer
{
    private const double _initialProgress = 100.0;
    private const float _markerSize = 14.0f;
    private const float _railStart = 10.0f;
    private const float _railEnd = 10.0f;
    private const double _markerTweenDuration = 0.45;

    private static readonly Color _railColor = new Color("#493d30");
    private static readonly Color _ariColor = new Color("#55a7ff");
    private static readonly Color _partyColor = new Color("#e8bd55");
    private static readonly Color _enemyColor = new Color("#e45f68");
    private static readonly Color _markerBorderColor = new Color("#20252d");

    private Label _round;
    private Label _turn;
    private Control _timelineTrack;
    private ColorRect _timelineRail;
    private Control _markerLayer;
    private Label _stunnedLabel;
    private PanelContainer _intentCard;
    private Label _intentHeading;
    private Label _intentDescription;
    private Label _intentConfidence;

    private TimelineCombatantStatus[] _timelineEntries = Array.Empty<TimelineCombatantStatus>();
    private readonly Dictionary<string, Panel> _markers =
        new Dictionary<string, Panel>(StringComparer.Ordinal);
    private readonly Dictionary<string, Tween> _markerTweens =
        new Dictionary<string, Tween>(StringComparer.Ordinal);
    private bool _hasRenderedTimeline;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _round = GetNode<Label>("Margin/Content/Heading/Round");
        _turn = GetNode<Label>("Margin/Content/Heading/Turn");
        _timelineTrack = GetNode<Control>("Margin/Content/TimelineTrack");
        _timelineRail = GetNode<ColorRect>("Margin/Content/TimelineTrack/Rail");
        _markerLayer = GetNode<Control>("Margin/Content/TimelineTrack/Markers");
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
        _intentConfidence.Text = intent.Confidence == IntentConfidence.Recognized
            ? "RECOGNIZED"
            : "SUSPECTED";
    }

    private void RefreshTimeline()
    {
        float width = _timelineTrack.Size.X;
        float height = _timelineTrack.Size.Y;
        if (width < 80.0f || height < _markerSize)
        {
            return;
        }

        float railWidth = Math.Max(1.0f, width - _railStart - _railEnd);
        float railY = (height - 3.0f) / 2.0f;

        _timelineRail.Position = new Vector2(_railStart, railY);
        _timelineRail.Size = new Vector2(railWidth, 3.0f);

        float markerY = (height - _markerSize) / 2.0f;
        float firstMarkerX = _railStart - _markerSize / 2.0f;
        HashSet<string> visibleIds = new HashSet<string>(StringComparer.Ordinal);
        List<string> stunnedNames = new List<string>();

        foreach (TimelineCombatantStatus entry in _timelineEntries)
        {
            if (!entry.IsVisible)
            {
                continue;
            }

            visibleIds.Add(entry.Id);
            if (entry.IsStunned)
            {
                stunnedNames.Add(entry.Name);
            }

            Panel marker = GetOrCreateMarker(entry);
            bool wasVisible = marker.Visible;
            marker.TooltipText = GetMarkerTooltip(entry);

            double progress = GetProgressFraction(entry);
            float targetX = _railStart + railWidth * (float)progress - _markerSize / 2.0f;
            targetX = Mathf.Clamp(targetX, 0.0f, width - _markerSize);
            Vector2 targetPosition = new Vector2(targetX, markerY);

            marker.Visible = true;

            if (!_hasRenderedTimeline)
            {
                marker.Position = new Vector2(firstMarkerX, markerY);
                MoveMarker(entry.Id, marker, targetPosition);
            }
            else if (wasVisible)
            {
                MoveMarker(entry.Id, marker, targetPosition);
            }
            else
            {
                StopMarkerTween(entry.Id);
                marker.Position = targetPosition;
            }
        }

        foreach (KeyValuePair<string, Panel> item in _markers)
        {
            if (visibleIds.Contains(item.Key))
            {
                continue;
            }

            item.Value.Visible = false;
            StopMarkerTween(item.Key);
        }

        _hasRenderedTimeline = true;
        _stunnedLabel.Text = stunnedNames.Count == 0
            ? string.Empty
            : "STUNNED  ·  " + string.Join("  ·  ", stunnedNames);
        _stunnedLabel.Visible = stunnedNames.Count > 0;
    }

    private Panel GetOrCreateMarker(TimelineCombatantStatus entry)
    {
        if (_markers.TryGetValue(entry.Id, out Panel existingMarker))
        {
            return existingMarker;
        }

        Color markerColor = GetMarkerColor(entry);
        StyleBoxFlat markerStyle = new StyleBoxFlat
        {
            BgColor = markerColor,
            BorderColor = _markerBorderColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 7,
            CornerRadiusTopRight = 7,
            CornerRadiusBottomLeft = 7,
            CornerRadiusBottomRight = 7
        };

        Panel marker = new Panel
        {
            Name = "Marker",
            Size = new Vector2(_markerSize, _markerSize),
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        marker.AddThemeStyleboxOverride("panel", markerStyle);
        marker.AddToGroup("timeline_markers");
        _markerLayer.AddChild(marker);
        _markers.Add(entry.Id, marker);
        return marker;
    }

    private void MoveMarker(string combatantId, Panel marker, Vector2 targetPosition)
    {
        StopMarkerTween(combatantId);

        Tween tween = marker.CreateTween();
        tween.TweenProperty(marker, "position", targetPosition, _markerTweenDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _markerTweens[combatantId] = tween;
    }

    private void StopMarkerTween(string combatantId)
    {
        if (!_markerTweens.TryGetValue(combatantId, out Tween tween))
        {
            return;
        }

        tween.Kill();
        _markerTweens.Remove(combatantId);
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

    private static string GetMarkerTooltip(TimelineCombatantStatus entry)
    {
        if (entry.IsStunned)
        {
            return entry.Name + " · STUNNED";
        }

        if (entry.IsReady)
        {
            return entry.Name + " · READY";
        }

        return entry.Name + " · " +
            (GetProgressFraction(entry) * 100.0).ToString("0") + "%";
    }

    private void ClearIntent()
    {
        _intentCard.Visible = false;
        _intentHeading.Text = string.Empty;
        _intentDescription.Text = string.Empty;
        _intentConfidence.Text = string.Empty;
    }
}
