using Godot;

public partial class CharacterPanel : PanelContainer
{
    private Label _name;
    private Label _role;
    private Label _level;
    private Label _healthValue;
    private Label _fatigueValue;
    private Label _staminaValue;
    private Label _moodValue;
    private Label _gold;
    private ProgressBar _healthBar;
    private ProgressBar _fatigueBar;
    private ProgressBar _staminaBar;
    private ProgressBar _moodBar;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _name = GetNode<Label>("Margin/Content/Name");
        _role = GetNode<Label>("Margin/Content/Role");
        _level = GetNode<Label>("Margin/Content/Level");
        _healthValue = GetNode<Label>("Margin/Content/HealthHeader/HealthValue");
        _fatigueValue = GetNode<Label>("Margin/Content/FatigueHeader/FatigueValue");
        _staminaValue = GetNode<Label>("Margin/Content/StaminaHeader/StaminaValue");
        _moodValue = GetNode<Label>("Margin/Content/MoodHeader/MoodValue");
        _gold = GetNode<Label>("Margin/Content/Gold");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _fatigueBar = GetNode<ProgressBar>("Margin/Content/FatigueBar");
        _staminaBar = GetNode<ProgressBar>("Margin/Content/StaminaBar");
        _moodBar = GetNode<ProgressBar>("Margin/Content/MoodBar");
    }

    public void SetCharacter(CharacterViewModel character)
    {
        if (character == null)
        {
            GD.PushError("CharacterPanel received null view model.");
            return;
        }

        _name.Text = character.Name;
        _role.Text = character.Role;
        _level.Text = "LEVEL " + character.Level;
        _healthValue.Text = character.Health + " / " + character.MaxHealth;
        _fatigueValue.Text = character.Fatigue.ToString("0.0") + " / " + character.MaxFatigue.ToString("0");
        _staminaValue.Text = character.Stamina + " / " + character.MaxStamina;
        _moodValue.Text = character.Mood + " / " + character.MaxMood;
        _gold.Text = character.Gold + " G";
        _healthBar.MaxValue = character.MaxHealth;
        _healthBar.Value = character.Health;
        _fatigueBar.MaxValue = character.MaxFatigue;
        _fatigueBar.Value = character.Fatigue;
        _staminaBar.MaxValue = System.Math.Max(1, character.MaxStamina);
        _staminaBar.Value = character.Stamina;
        _moodBar.MaxValue = character.MaxMood;
        _moodBar.Value = character.Mood;
    }
}
