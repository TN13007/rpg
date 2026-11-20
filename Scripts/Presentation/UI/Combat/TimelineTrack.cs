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
        public bool IsEnemy { get; }

        public MarkerLayout(string combatantId, TimelineMarker marker, float centerX, int lane, bool isEnemy)
        {
            CombatantId = combatantId;
            Marker = marker;
            CenterX = centerX;
            Lane = lane;
            IsEnemy = isEnemy;
        }
    }

    private const float _markerSize = 16.0f;
    private const float _railHeight = 2.0f;
    private const float _laneSpacing = 18.0f;
    private const float _baseTrackHeight = 90.0f;
    private const float _topPadding = 2.0f;
    private const float _axisLabelHeight = 12.0f;
    private const float _bottomPadding = 4.0f;

    [Export]
    public PackedScene MarkerScene { get; set; }

    [Export(PropertyHint.Range, "1,30,0.5")]
    public double DisplayWindowSeconds { get; set; } = 10.0;

    private ColorRect _rail;
    private Control _markerLayer;
    private Label _nowLabel;
    private Label _windowLabel;
    private TimelineMarkerViewData[] _combatants = Array.Empty<TimelineMarkerViewData>();
    private readonly Dictionary<string, TimelineMarker> _markers =
        new Dictionary<string, TimelineMarker>(StringComparer.Ordinal);

    public override void _Ready()
    {
        _rail = GetNode<ColorRect>("Rail");
        _markerLayer = GetNode<Control>("MarkerLayer");
        _nowLabel = GetNode<Label>("NowLabel");
        _windowLabel = GetNode<Label>("WindowLabel");
        Resized += HandleResized;
        RefreshTimeline();
    }

    public void SetCombatants(TimelineMarkerViewData[] combatants)
    {
        _combatants = combatants ?? Array.Empty<TimelineMarkerViewData>();
        RefreshTimeline();
    }

    private void HandleResized()
    {
        RefreshTimeline();
    }

    private void RefreshTimeline()
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
            }
        }

        visibleCombatants.Sort(CompareCombatants);
        List<float> partyLaneLastCenterX = new List<float>();
        List<float> enemyLaneLastCenterX = new List<float>();
        List<MarkerLayout> layouts = new List<MarkerLayout>();

        foreach (TimelineMarkerViewData combatant in visibleCombatants)
        {
            if (!_markers.TryGetValue(combatant.Id, out TimelineMarker marker))
            {
                marker = CreateMarker(combatant.Id);
            }

            marker.SetCombatant(combatant, displayWindow);
            marker.Visible = true;
            double visibleTime = Math.Clamp(combatant.TimeToReadySeconds, 0.0, displayWindow);
            float markerCenterX = railLeft + railWidth * (float)(visibleTime / displayWindow);
            List<float> sideLanes = combatant.IsEnemy ? enemyLaneLastCenterX : partyLaneLastCenterX;
            int lane = FindAvailableLane(markerCenterX, sideLanes);
            layouts.Add(new MarkerLayout(combatant.Id, marker, markerCenterX, lane, combatant.IsEnemy));
        }

        int partyLaneCount = Math.Max(1, partyLaneLastCenterX.Count);
        int enemyLaneCount = Math.Max(1, enemyLaneLastCenterX.Count);
        float railY = _markerSize / 2.0f + _topPadding + partyLaneCount * _laneSpacing;
        float requiredHeight = Math.Max(
            _baseTrackHeight,
            railY + enemyLaneCount * _laneSpacing + _markerSize / 2.0f + _axisLabelHeight + _bottomPadding);

        if (Math.Abs(CustomMinimumSize.Y - requiredHeight) > 1.0f)
        {
            CustomMinimumSize = new Vector2(CustomMinimumSize.X, requiredHeight);
        }

        _rail.Position = new Vector2(railLeft, railY - _railHeight / 2.0f);
        _rail.Size = new Vector2(railWidth, _railHeight);
        _nowLabel.Position = new Vector2(0.0f, requiredHeight - _axisLabelHeight);
        _nowLabel.Size = new Vector2(48.0f, _axisLabelHeight);
        _windowLabel.Position = new Vector2(Math.Max(0.0f, Size.X - 48.0f), requiredHeight - _axisLabelHeight);
        _windowLabel.Size = new Vector2(48.0f, _axisLabelHeight);

        foreach (MarkerLayout layout in layouts)
        {
            float laneDistance = (layout.Lane + 1) * _laneSpacing;
            float markerCenterY = layout.IsEnemy ? railY + laneDistance : railY - laneDistance;
            layout.Marker.Position = new Vector2(
                layout.CenterX - _markerSize / 2.0f,
                markerCenterY - _markerSize / 2.0f);
        }

        List<string> removedIds = new List<string>();
        foreach (KeyValuePair<string, TimelineMarker> markerEntry in _markers)
        {
            if (currentIds.Contains(markerEntry.Key))
            {
                continue;
            }

            removedIds.Add(markerEntry.Key);
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
}
