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
        Button button = GetNode<Button>(path);
        UiTheme.StyleButton(button);
        button.Pressed += HandleButtonPressed;
        button.SetMeta("tab_id", tab);
    }

    private void HandleButtonPressed()
    {
        Button button = GetViewport().GuiGetFocusOwner() as Button;
        if (button == null)
        {
            return;
        }

        TabSelected?.Invoke(button.GetMeta("tab_id").AsString());
    }
}
