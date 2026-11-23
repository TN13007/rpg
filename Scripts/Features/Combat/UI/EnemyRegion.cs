using System;
using System.Collections.Generic;
using Godot;

/// <summary>Owns the visible Enemy card collection and its target-selection feedback.</summary>
public partial class EnemyRegion : PanelContainer
{
    /// <summary>Raised when the player selects a currently valid Enemy target.</summary>
    public event Action<string> EnemySelected;

    private VBoxContainer _enemyList;
    private PackedScene _enemyCardScene;
    private readonly Dictionary<string, EnemyCard> _cards =
        new Dictionary<string, EnemyCard>(StringComparer.Ordinal);
    private readonly HashSet<string> _validTargetIds =
        new HashSet<string>(StringComparer.Ordinal);
    private Label _emptyLabel;
    private string _selectedTargetId = string.Empty;
    private bool _targetSelectionActive;

    /// <summary>Initializes the Enemy collection and its reusable card scene.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _enemyList = GetNode<VBoxContainer>("Margin/Content/EnemyScroll/EnemyList");
        _enemyCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/EnemyCard.tscn");
        if (_enemyCardScene == null)
        {
            GD.PushError("EnemyRegion could not load EnemyCard.tscn.");
        }
    }

    /// <summary>Updates Enemy cards while preserving their stable IDs and current target state.</summary>
    public void SetEnemies(EnemyCardViewData[] enemies)
    {
        HashSet<string> currentIds = new HashSet<string>(StringComparer.Ordinal);
        int cardIndex = 0;

        if (enemies != null)
        {
            foreach (EnemyCardViewData enemy in enemies)
            {
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.Id))
                {
                    continue;
                }

                currentIds.Add(enemy.Id);
                if (!_cards.TryGetValue(enemy.Id, out EnemyCard card))
                {
                    if (_enemyCardScene == null)
                    {
                        continue;
                    }

                    card = _enemyCardScene.Instantiate<EnemyCard>();
                    card.Selected += HandleEnemySelected;
                    _enemyList.AddChild(card);
                    _cards.Add(enemy.Id, card);
                    card.Setup(enemy);
                }
                else
                {
                    card.UpdateDisplay(enemy);
                }

                _enemyList.MoveChild(card, cardIndex++);
                card.SetTargetSelectionState(
                    _targetSelectionActive,
                    _validTargetIds.Contains(enemy.Id),
                    enemy.Id == _selectedTargetId);
            }
        }

        List<string> removedIds = new List<string>();
        foreach (string enemyId in _cards.Keys)
        {
            if (!currentIds.Contains(enemyId))
            {
                removedIds.Add(enemyId);
            }
        }

        foreach (string enemyId in removedIds)
        {
            EnemyCard card = _cards[enemyId];
            card.Selected -= HandleEnemySelected;
            card.QueueFree();
            _cards.Remove(enemyId);
            _validTargetIds.Remove(enemyId);
        }

        UpdateEmptyState();
    }

    /// <summary>Highlights valid Enemy targets and dims all other visible Enemy cards.</summary>
    public void BeginTargetSelection(string[] validTargetIds)
    {
        _targetSelectionActive = true;
        _selectedTargetId = string.Empty;
        _validTargetIds.Clear();

        if (validTargetIds != null)
        {
            foreach (string targetId in validTargetIds)
            {
                if (!string.IsNullOrWhiteSpace(targetId))
                {
                    _validTargetIds.Add(targetId);
                }
            }
        }

        UpdateTargetVisuals();
    }

    /// <summary>Clears target highlights and restores the normal Enemy card appearance.</summary>
    public void ClearTargetSelection()
    {
        _targetSelectionActive = false;
        _selectedTargetId = string.Empty;
        _validTargetIds.Clear();
        UpdateTargetVisuals();
    }

    /// <summary>Plays a presentation feedback cue on the Enemy with the supplied display name.</summary>
    public void PlayFeedback(string enemyName, CombatFeedbackType feedbackType)
    {
        foreach (EnemyCard card in _cards.Values)
        {
            if (string.Equals(card.EnemyName, enemyName, StringComparison.OrdinalIgnoreCase))
            {
                card.PlayFeedback(feedbackType);
            }
        }
    }

    private void HandleEnemySelected(string enemyId)
    {
        if (!_targetSelectionActive || !_validTargetIds.Contains(enemyId))
        {
            return;
        }

        _selectedTargetId = enemyId;
        UpdateTargetVisuals();
        EnemySelected?.Invoke(enemyId);
    }

    private void UpdateTargetVisuals()
    {
        foreach (KeyValuePair<string, EnemyCard> entry in _cards)
        {
            bool selectable = _validTargetIds.Contains(entry.Key);
            entry.Value.SetTargetSelectionState(
                _targetSelectionActive,
                selectable,
                entry.Key == _selectedTargetId);
        }
    }

    private void UpdateEmptyState()
    {
        if (_cards.Count == 0)
        {
            if (_emptyLabel == null)
            {
                _emptyLabel = new Label
                {
                    Text = "No enemies",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                _emptyLabel.AddThemeFontSizeOverride("font_size", 11);
                _emptyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
                _enemyList.AddChild(_emptyLabel);
            }

            return;
        }

        if (_emptyLabel != null)
        {
            _emptyLabel.QueueFree();
            _emptyLabel = null;
        }
    }
}
