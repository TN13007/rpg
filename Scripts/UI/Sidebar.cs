using System;
using Godot;

public partial class Sidebar : PanelContainer
{
    public event Action<string> TabSelected;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        Bind("Margin/Content/Character/Profile", "Profile");
        Bind("Margin/Content/Character/Skills", "Skills");
        Bind("Margin/Content/Character/Inventory", "Inventory");
        Bind("Margin/Content/Character/Relationships", "Relationships");
        Bind("Margin/Content/Diary/Goals", "Goals");
        Bind("Margin/Content/Diary/Journal", "Journal");
        Bind("Margin/Content/System/Settings", "Settings");
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
}
