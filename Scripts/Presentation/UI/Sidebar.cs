using System;
using System.Collections.Generic;
using Godot;

public partial class Sidebar : PanelContainer
{
    public event Action<string> TabSelected;
    public event Action DebugEncounterRequested;
    public event Action<string> EnemySelected;

    [Export]
    public PackedScene EnemyCardScene { get; set; }

    private VBoxContainer _characterSection;
    private VBoxContainer _diarySection;
    private VBoxContainer _systemSection;
    private VBoxContainer _enemySection;
    private VBoxContainer _enemyList;
    private Control _navigationSpacer;
    private VBoxContainer _debugSection;
    private Label _emptyEnemyLabel;
    private readonly Dictionary<string, EnemyCard> _enemyCards =
        new Dictionary<string, EnemyCard>(StringComparer.Ordinal);
    private bool _showDebugTools;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _characterSection = GetNode<VBoxContainer>("Margin/Content/Character");
        _diarySection = GetNode<VBoxContainer>("Margin/Content/Diary");
        _systemSection = GetNode<VBoxContainer>("Margin/Content/System");
        _enemySection = GetNode<VBoxContainer>("Margin/Content/Enemies");
        _enemyList = GetNode<VBoxContainer>("Margin/Content/Enemies/EnemyScroll/EnemyList");
        _navigationSpacer = GetNode<Control>("Margin/Content/Spacer");
        _debugSection = GetNode<VBoxContainer>("Margin/Content/Debug");
        _showDebugTools = OS.IsDebugBuild();
        _debugSection.Visible = _showDebugTools;

        if (EnemyCardScene == null)
        {
            GD.PushError("Sidebar requires an EnemyCard scene.");
        }

        Button debugButton = GetNode<Button>("Margin/Content/Debug/ForceEncounter");
        UiTheme.StyleButton(debugButton, true);
        debugButton.Pressed += HandleDebugEncounterRequested;

        Bind("Margin/Content/Character/Profile", "Profile");
        Bind("Margin/Content/Character/Skills", "Skills");
        Bind("Margin/Content/Character/Inventory", "Inventory");
        Bind("Margin/Content/Character/Relationships", "Relationships");
        Bind("Margin/Content/Diary/Goals", "Goals");
        Bind("Margin/Content/Diary/Journal", "Journal");
        Bind("Margin/Content/System/Settings", "Settings");
    }

    public void EnterCombat(EnemyCombatStatus[] enemies)
    {
        _characterSection.Visible = false;
        _diarySection.Visible = false;
        _systemSection.Visible = false;
        _navigationSpacer.Visible = false;
        _debugSection.Visible = false;
        _enemySection.Visible = true;
        UpdateCombatEnemies(enemies);
    }

    public void UpdateCombatEnemies(EnemyCombatStatus[] enemies)
    {
        if (!_enemySection.Visible)
        {
            return;
        }

        HashSet<string> currentEnemyIds = new HashSet<string>(StringComparer.Ordinal);
        int cardIndex = 0;

        if (enemies != null)
        {
            foreach (EnemyCombatStatus enemy in enemies)
            {
                if (enemy == null || !enemy.IsVisible || string.IsNullOrWhiteSpace(enemy.Id))
                {
                    continue;
                }

                currentEnemyIds.Add(enemy.Id);

                if (!_enemyCards.TryGetValue(enemy.Id, out EnemyCard card))
                {
                    if (EnemyCardScene == null)
                    {
                        continue;
                    }

                    card = EnemyCardScene.Instantiate<EnemyCard>();
                    card.Selected += HandleEnemySelected;
                    _enemyList.AddChild(card);
                    card.Setup(enemy);
                    _enemyCards.Add(enemy.Id, card);
                }
                else
                {
                    card.UpdateDisplay(enemy);
                }

                _enemyList.MoveChild(card, cardIndex);
                cardIndex++;
            }
        }

        List<string> removedEnemyIds = new List<string>();
        foreach (KeyValuePair<string, EnemyCard> entry in _enemyCards)
        {
            if (!currentEnemyIds.Contains(entry.Key))
            {
                removedEnemyIds.Add(entry.Key);
            }
        }

        foreach (string enemyId in removedEnemyIds)
        {
            RemoveEnemyCard(enemyId);
        }

        UpdateEmptyEnemyState();
    }

    public void SetEnemyTargetSelection(string[] selectableEnemyIds, string selectedEnemyId = "")
    {
        HashSet<string> selectableIds = new HashSet<string>(
            selectableEnemyIds ?? Array.Empty<string>(),
            StringComparer.Ordinal);

        foreach (KeyValuePair<string, EnemyCard> entry in _enemyCards)
        {
            bool selectable = selectableIds.Contains(entry.Key);
            entry.Value.SetTargetSelectionState(
                true,
                selectable,
                selectable && entry.Key == selectedEnemyId);
        }
    }

    public void PlayEnemyFeedback(string enemyName, CombatFeedbackType feedbackType)
    {
        foreach (EnemyCard card in _enemyCards.Values)
        {
            if (card.EnemyName == enemyName)
            {
                card.PlayFeedback(feedbackType);
            }
        }
    }

    public void ClearEnemyTargetSelection()
    {
        foreach (EnemyCard card in _enemyCards.Values)
        {
            card.SetTargetSelectionState(false, false, false);
        }
    }

    public void ExitCombat()
    {
        ClearEnemyCards();
        _enemySection.Visible = false;
        _characterSection.Visible = true;
        _diarySection.Visible = true;
        _systemSection.Visible = true;
        _navigationSpacer.Visible = true;
        _debugSection.Visible = _showDebugTools;
    }

    private void Bind(string path, string tab)
    {
        SidebarButton button = GetNode<SidebarButton>(path);
        button.TabId = tab;
        UiTheme.StyleButton(button);
        button.Selected += HandleTabSelected;
    }

    private void HandleTabSelected(string tab) => TabSelected?.Invoke(tab);
    private void HandleDebugEncounterRequested() => DebugEncounterRequested?.Invoke();
    private void HandleEnemySelected(string enemyId) => EnemySelected?.Invoke(enemyId);

    private void RemoveEnemyCard(string enemyId)
    {
        if (!_enemyCards.TryGetValue(enemyId, out EnemyCard card))
        {
            return;
        }

        card.Selected -= HandleEnemySelected;
        _enemyCards.Remove(enemyId);
        card.QueueFree();
    }

    private void ClearEnemyCards()
    {
        foreach (EnemyCard card in _enemyCards.Values)
        {
            card.Selected -= HandleEnemySelected;
            card.QueueFree();
        }

        _enemyCards.Clear();

        if (_emptyEnemyLabel != null)
        {
            _emptyEnemyLabel.QueueFree();
            _emptyEnemyLabel = null;
        }
    }

    private void UpdateEmptyEnemyState()
    {
        if (_enemyCards.Count == 0)
        {
            if (_emptyEnemyLabel == null)
            {
                _emptyEnemyLabel = new Label
                {
                    Text = "No enemies detected",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                _emptyEnemyLabel.AddThemeFontSizeOverride("font_size", 11);
                _emptyEnemyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
                _enemyList.AddChild(_emptyEnemyLabel);
            }

            return;
        }

        if (_emptyEnemyLabel != null)
        {
            _emptyEnemyLabel.QueueFree();
            _emptyEnemyLabel = null;
        }
    }
}