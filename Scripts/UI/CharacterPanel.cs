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

    public void SetCharacter(PlayerState state)
    {
        _name.Text = state.Name;
        _role.Text = state.Role;
        _level.Text = "LEVEL " + state.Level;
        _healthValue.Text = state.Health + " / " + state.MaxHealth;
        _energyValue.Text = state.Energy + " / " + state.MaxEnergy;
        _moodValue.Text = state.Mood + " / 100";
        _gold.Text = state.Gold + " G";
        _healthBar.MaxValue = state.MaxHealth;
        _healthBar.Value = state.Health;
        _energyBar.MaxValue = state.MaxEnergy;
        _energyBar.Value = state.Energy;
        _moodBar.MaxValue = 100;
        _moodBar.Value = state.Mood;
    }
}
