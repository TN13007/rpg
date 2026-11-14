using Godot;

public partial class CharacterPanel : PanelContainer
{
    private Label _name;
    private Label _role;
    private Label _level;
    private Label _healthValue;
    private Label _staminaValue;
    private Label _gold;

    private ProgressBar _healthBar;
    private ProgressBar _staminaBar;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);

        _name = GetNode<Label>("Margin/Content/Name");
        _role = GetNode<Label>("Margin/Content/Role");
        _level = GetNode<Label>("Margin/Content/Level");
        _healthValue = GetNode<Label>("Margin/Content/HealthValue");
        _staminaValue = GetNode<Label>("Margin/Content/StaminaValue");
        _gold = GetNode<Label>("Margin/Content/Gold");

        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _staminaBar = GetNode<ProgressBar>("Margin/Content/StaminaBar");
    }

    public void SetCharacter(string characterName, string role, int level, int hp, int maxHp, int stamina, int maxStamina, int gold)
    {
        _name.Text = characterName;
        _role.Text = role;
        _level.Text = "LEVEL " + level;
        _healthValue.Text = hp + " / " + maxHp;
        _staminaValue.Text = stamina + " / " + maxStamina;
        _gold.Text = gold + " G";

        _healthBar.MaxValue = maxHp;
        _healthBar.Value = hp;
        _staminaBar.MaxValue = maxStamina;
        _staminaBar.Value = stamina;
    }
}
