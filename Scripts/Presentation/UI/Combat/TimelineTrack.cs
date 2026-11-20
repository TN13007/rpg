using System;
using System.Collections.Generic;
using Godot;

public partial class TimelineTrack : Control
{
    private sealed class MarkerLayout
    {
        public string CombatantId { get; }
        public TimelineMarker Marker { get; }
        public float CenterX { get; }
        public int Lane { get; }
        public bool WasVisible { get; }

        public MarkerLayout(
            string combatantId,
            TimelineMarker marker,
            float centerX,
            int lane,
            bool wasVisible)
        {
            CombatantId = combatantId;
            Marker = marker;
            CenterX = centerX;
            Lane = lane;
            WasVisible = wasVisible;
        }
    }

    private const float _markerSize = 16.0f;
    private const float _railHeight = 2.0f;
    private const float _laneSpacing = 18.0f;
    private const float _baseTrackHeight = 52.0f;
    private const float _defaultRailY = 19.0f;
    private const float _topPadding = 2.0f;
    private const float _axisLabelHeight = 12.0f;
    private const float _bottomPadding = 4.0f;

    [Export]
    public PackedScene MarkerScene { get; set; }

    [Export(PropertyHint.Range, "1,30,0.5")]
    public double DisplayWindowSeconds { get; set; } = 10.0;

    [Export(PropertyHint.Range, "0.05,1,0.05")]
    public double MarkerTweenDuration { get; set; } = 0.35;

    private ColorRect _rail;
    private Control _markerLayer;
    private Label _nowLabel;
    private Label _windowLabel;

    private TimelineMarkerViewData[] _combatants = Array.Empty<TimelineMarkerViewData>();
    private readonly Dictionary<string, TimelineMarker> _markers =
        new Dictionary<string, TimelineMarker>(StringComparer.Ordinal);
    private readonly Dictionary<string, Tween> _markerTweens =
        new Dictionary<string, Tween>(StringComparer.Ordinal);

    public override void _Ready()
    {
        _rail = GetNode<ColorRect>("Rail");
        _markerLayer = GetNode<Control>("MarkerLayer");
        _nowLabel = GetNode<Label>("NowLabel");
        _windowLabel = GetNode<Label>("WindowLabel");

        Resized += HandleResized;
        RefreshTimeline(false);
    }

    public void SetCombatants(TimelineMarkerViewData[] combatants)
    {
        _combatants = combatants ?? Array.Empty<TimelineMarkerViewData>();
        RefreshTimeline(true);
    }

    private void HandleResized()
    {
        RefreshTimeline(false);
    }

    private void RefreshTimeline(bool animateExistingMarkers)
    {
        if (_rail == null || _markerLayer == null || Size.X < 40.0f)
        {
            return;
        }

        double displayWindow = Math.Max(0.1, DisplayWindowSeconds);
        float railLeft = _markerSize / 2.0f;
        float railWidth = Math.Max(1.0f, Size.X - _markerSize);
        _windowLabel.Text = displayWindow.ToString("0.#") + "s";

        List<TimelineMarkerViewData> visibleCombatants = new List<TimelineMarkerViewData>();
        HashSet<string> currentIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (TimelineMarkerViewData combatant in _combatants)
        {
            currentIds.Add(combatant.Id);

            if (combatant.IsVisible)
            {
                visibleCombatants.Add(combatant);
            }
            else if (_markers.TryGetValue(combatant.Id, out TimelineMarker hiddenMarker))
            {
                hiddenMarker.Visible = false;
                StopMarkerTween(combatant.Id);
            }
        }

        visibleCombatants.Sort(CompareCombatants);

        List<float> laneLastCenterX = new List<float>();
        List<MarkerLayout> layouts = new List<MarkerLayout>();
        float minimumLaneOffset = 0.0f;
        float maximumLaneOffset = 0.0f;

        foreach (TimelineMarkerViewData combatant in visibleCombatants)
        {
            bool isNewMarker = !_markers.TryGetValue(combatant.Id, out TimelineMarker marker);
            if (isNewMarker)
            {
                marker = CreateMarker(combatant.Id);
            }

            bool wasVisible = !isNewMarker && marker.Visible;
            marker.SetCombatant(combatant, displayWindow);
            marker.Visible = true;

            double visibleTime = Math.Clamp(combatant.TimeToReadySeconds, 0.0, displayWindow);
            float markerCenterX = railLeft + railWidth * (float)(visibleTime / displayWindow);
            int lane = FindAvailableLane(markerCenterX, laneLastCenterX);
            float laneOffset = GetLaneOffset(lane);

            minimumLaneOffset = Math.Min(minimumLaneOffset, laneOffset);
            maximumLaneOffset = Math.Max(maximumLaneOffset, laneOffset);

            layouts.Add(new MarkerLayout(
                combatant.Id,
                marker,
                markerCenterX,
                lane,
                wasVisible));
        }

        float railY = Math.Max(
            _defaultRailY,
            _markerSize / 2.0f + _topPadding - minimumLaneOffset);
        float requiredHeight = Math.Max(
            _baseTrackHeight,
            railY + maximumLaneOffset + _markerSize / 2.0f +
                _axisLabelHeight + _bottomPadding);

        if (Math.Abs(CustomMinimumSize.Y - requiredHeight) > 1.0f)
        {
            CustomMinimumSize = new Vector2(CustomMinimumSize.X, requiredHeight);
        }

        _rail.Position = new Vector2(railLeft, railY - _railHeight / 2.0f);
        _rail.Size = new Vector2(railWidth, _railHeight);
        _nowLabel.Position = new Vector2(0.0f, requiredHeight - _axisLabelHeight);
        _nowLabel.Size = new Vector2(48.0f, _axisLabelHeight);
        _windowLabel.Position = new Vector2(
            Math.Max(0.0f, Size.X - 48.0f),
            requiredHeight - _axisLabelHeight);
        _windowLabel.Size = new Vector2(48.0f, _axisLabelHeight);

        foreach (MarkerLayout layout in layouts)
        {
            float markerY = railY + GetLaneOffset(layout.Lane) - _markerSize / 2.0f;
            Vector2 targetPosition = new Vector2(
                layout.CenterX - _markerSize / 2.0f,
                markerY);

            if (animateExistingMarkers && layout.WasVisible)
            {
                MoveMarker(layout.CombatantId, layout.Marker, targetPosition);
            }
            else
            {
                StopMarkerTween(layout.CombatantId);
                layout.Marker.Position = targetPosition;
            }
        }

        List<string> removedIds = new List<string>();
        foreach (KeyValuePair<string, TimelineMarker> markerEntry in _markers)
        {
            if (currentIds.Contains(markerEntry.Key))
            {
                continue;
            }

            removedIds.Add(markerEntry.Key);
            StopMarkerTween(markerEntry.Key);
            markerEntry.Value.QueueFree();
        }

        foreach (string removedId in removedIds)
        {
            _markers.Remove(removedId);
        }
    }

    private TimelineMarker CreateMarker(string combatantId)
    {
        if (MarkerScene == null)
        {
            GD.PushError("TimelineTrack requires a MarkerScene PackedScene.");
            throw new InvalidOperationException("TimelineTrack MarkerScene is not assigned.");
        }

        TimelineMarker marker = MarkerScene.Instantiate<TimelineMarker>();
        marker.Name = "Marker_" + combatantId;
        marker.Size = new Vector2(_markerSize, _markerSize);
        _markerLayer.AddChild(marker);
        _markers.Add(combatantId, marker);
        return marker;
    }

    private static int FindAvailableLane(float markerCenterX, List<float> laneLastCenterX)
    {
        for (int lane = 0; lane < laneLastCenterX.Count; lane++)
        {
            if (markerCenterX - laneLastCenterX[lane] >= _markerSize + 2.0f)
            {
                laneLastCenterX[lane] = markerCenterX;
                return lane;
            }
        }

        laneLastCenterX.Add(markerCenterX);
        return laneLastCenterX.Count - 1;
    }

    private static float GetLaneOffset(int lane)
    {
        if (lane == 0)
        {
            return 0.0f;
        }

        int offsetMultiplier = (lane + 1) / 2;
        return lane % 2 == 1
            ? -offsetMultiplier * _laneSpacing
            : offsetMultiplier * _laneSpacing;
    }

    private static int CompareCombatants(TimelineMarkerViewData left, TimelineMarkerViewData right)
    {
        int timeComparison = left.TimeToReadySeconds.CompareTo(right.TimeToReadySeconds);
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

    private void MoveMarker(string combatantId, TimelineMarker marker, Vector2 targetPosition)
    {
        StopMarkerTween(combatantId);

        Tween tween = marker.CreateTween();
        tween.TweenProperty(marker, "position", targetPosition, MarkerTweenDuration)
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
}
