using System;
using Godot;

public partial class Sidebar : PanelContainer
{
    public event Action<string> TabSelected;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        Bind("Margin/Content/World/Explore", "Explore");
        Bind("Margin/Content/World/Map", "Map");

        Bind("Margin/Content/Character/Profile", "Profile");
        Bind("Margin/Content/Character/Relationships", "Relationships");
        Bind("Margin/Content/Character/Inventory", "Inventory");

        Bind("Margin/Content/Adventure/Quests", "Quests");
        Bind("Margin/Content/Adventure/Skills", "Skills");

        Bind("Margin/Content/System/Settings", "Settings");
    }

    private void Bind(string path, string tab)
    {
        Button button = GetNode<Button>(path);
        UiTheme.StyleButton(button);
        button.Pressed += () => TabSelected?.Invoke(tab);
    }
}
