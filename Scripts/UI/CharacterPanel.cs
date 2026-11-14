using Godot;

public partial class CharacterPanel : PanelContainer
{
    private Label _name;
    private Label _role;
    private Label _level;
    private Label _healthValue;
    private Label _energyValue;
    private Label _moodValue;
    private Label _gold;
    private ProgressBar _healthBar;
    private ProgressBar _energyBar;
    private ProgressBar _moodBar;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _name = GetNode<Label>("Margin/Content/Name");
        _role = GetNode<Label>("Margin/Content/Role");
        _level = GetNode<Label>("Margin/Content/Level");
        _healthValue = GetNode<Label>("Margin/Content/HealthHeader/HealthValue");
        _energyValue = GetNode<Label>("Margin/Content/EnergyHeader/EnergyValue");
        _moodValue = GetNode<Label>("Margin/Content/MoodHeader/MoodValue");
        _gold = GetNode<Label>("Margin/Content/Gold");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _energyBar = GetNode<ProgressBar>("Margin/Content/EnergyBar");
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
        _energyValue.Text = character.Energy + " / " + character.MaxEnergy;
        _moodValue.Text = character.Mood + " / " + character.MaxMood;
        _gold.Text = character.Gold + " G";
        _healthBar.MaxValue = character.MaxHealth;
        _healthBar.Value = character.Health;
        _energyBar.MaxValue = character.MaxEnergy;
        _energyBar.Value = character.Energy;
        _moodBar.MaxValue = character.MaxMood;
        _moodBar.Value = character.Mood;
    }
}
