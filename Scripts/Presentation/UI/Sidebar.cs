using System;
using Godot;

public partial class Sidebar : PanelContainer
{
    public event Action<string> TabSelected;

    private VBoxContainer _characterSection;
    private VBoxContainer _diarySection;
    private VBoxContainer _systemSection;
    private VBoxContainer _enemySection;
    private VBoxContainer _enemyList;
    private Control _navigationSpacer;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _characterSection = GetNode<VBoxContainer>("Margin/Content/Character");
        _diarySection = GetNode<VBoxContainer>("Margin/Content/Diary");
        _systemSection = GetNode<VBoxContainer>("Margin/Content/System");
        _enemySection = GetNode<VBoxContainer>("Margin/Content/Enemies");
        _enemyList = GetNode<VBoxContainer>("Margin/Content/Enemies/EnemyScroll/EnemyList");
        _navigationSpacer = GetNode<Control>("Margin/Content/Spacer");

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
        _enemySection.Visible = true;
        UpdateCombatEnemies(enemies);
    }

    public void UpdateCombatEnemies(EnemyCombatStatus[] enemies)
    {
        if (!_enemySection.Visible)
        {
            return;
        }

        ClearChildren(_enemyList);

        if (enemies == null || enemies.Length == 0)
        {
            Label emptyLabel = new Label
            {
                Text = "No enemies detected",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            emptyLabel.AddThemeFontSizeOverride("font_size", 11);
            emptyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
            _enemyList.AddChild(emptyLabel);
            return;
        }

        foreach (EnemyCombatStatus enemy in enemies)
        {
            _enemyList.AddChild(CreateEnemyCard(enemy));
        }
    }

    public void ExitCombat()
    {
        ClearChildren(_enemyList);
        _enemySection.Visible = false;
        _characterSection.Visible = true;
        _diarySection.Visible = true;
        _systemSection.Visible = true;
        _navigationSpacer.Visible = true;
    }

    private void Bind(string path, string tab)
    {
        SidebarButton button = GetNode<SidebarButton>(path);
        button.TabId = tab;
        UiTheme.StyleButton(button);
        button.Selected += HandleTabSelected;
    }

    private void HandleTabSelected(string tab)
    {
        TabSelected?.Invoke(tab);
    }

    private Control CreateEnemyCard(EnemyCombatStatus enemy)
    {
        PanelContainer card = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        StyleBoxFlat cardStyle = new StyleBoxFlat
        {
            BgColor = UiTheme.PanelLight,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        cardStyle.ContentMarginLeft = 8;
        cardStyle.ContentMarginRight = 8;
        cardStyle.ContentMarginTop = 8;
        cardStyle.ContentMarginBottom = 8;
        card.AddThemeStyleboxOverride("panel", cardStyle);

        VBoxContainer content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 5);
        card.AddChild(content);

        Label name = new Label
        {
            Text = enemy.Name,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        name.AddThemeFontSizeOverride("font_size", 12);
        name.AddThemeColorOverride("font_color", UiTheme.Text);
        content.AddChild(name);

        Label health = new Label
        {
            Text = "HP  " + enemy.CurrentHealth + " / " + enemy.MaxHealth,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        health.AddThemeFontSizeOverride("font_size", 10);
        health.AddThemeColorOverride("font_color", UiTheme.Muted);
        content.AddChild(health);

        ProgressBar healthBar = new ProgressBar
        {
            CustomMinimumSize = new Vector2(0, 8),
            MaxValue = enemy.MaxHealth,
            Value = enemy.CurrentHealth,
            ShowPercentage = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        content.AddChild(healthBar);

        Label condition = new Label
        {
            Text = enemy.Condition,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        condition.AddThemeFontSizeOverride("font_size", 10);
        condition.AddThemeColorOverride("font_color", UiTheme.Muted);
        content.AddChild(condition);

        return card;
    }

    private void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
