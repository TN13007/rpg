using Godot;

/// <summary>Presents the selected character in Exploration or the active combat Party member.</summary>
public partial class CharacterPanel : PanelContainer
{
    private Label _sectionTitle;
    private TextureRect _portrait;
    private Label _name;
    private Label _role;
    private Label _level;
    private Label _healthValue;
    private Label _fatigueValue;
    private Label _staminaValue;
    private Label _moodValue;
    private Label _gold;
    private Label _arousalValue;
    private Control _healthHeader;
    private Control _fatigueHeader;
    private Control _moodHeader;
    private Control _staminaHeader;
    private Control _arousalHeader;
    private ProgressBar _healthBar;
    private ProgressBar _fatigueBar;
    private ProgressBar _staminaBar;
    private ProgressBar _moodBar;
    private ProgressBar _arousalBar;

    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _sectionTitle = GetNode<Label>("Margin/Content/SectionTitle");
        _portrait = GetNode<TextureRect>("Margin/Content/Portrait");
        _name = GetNode<Label>("Margin/Content/Name");
        _role = GetNode<Label>("Margin/Content/Role");
        _level = GetNode<Label>("Margin/Content/Level");
        _healthHeader = GetNode<Control>("Margin/Content/HealthHeader");
        _healthValue = GetNode<Label>("Margin/Content/HealthHeader/HealthValue");
        _fatigueHeader = GetNode<Control>("Margin/Content/FatigueHeader");
        _fatigueValue = GetNode<Label>("Margin/Content/FatigueHeader/FatigueValue");
        _staminaHeader = GetNode<Control>("Margin/Content/StaminaHeader");
        _staminaValue = GetNode<Label>("Margin/Content/StaminaHeader/StaminaValue");
        _moodHeader = GetNode<Control>("Margin/Content/MoodHeader");
        _moodValue = GetNode<Label>("Margin/Content/MoodHeader/MoodValue");
        _gold = GetNode<Label>("Margin/Content/Gold");
        _arousalHeader = GetNode<Control>("Margin/Content/ArousalHeader");
        _arousalValue = GetNode<Label>("Margin/Content/ArousalHeader/ArousalValue");
        _healthBar = GetNode<ProgressBar>("Margin/Content/HealthBar");
        _fatigueBar = GetNode<ProgressBar>("Margin/Content/FatigueBar");
        _staminaBar = GetNode<ProgressBar>("Margin/Content/StaminaBar");
        _moodBar = GetNode<ProgressBar>("Margin/Content/MoodBar");
        _arousalBar = GetNode<ProgressBar>("Margin/Content/ArousalBar");
        _arousalHeader.Visible = false;
        _arousalBar.Visible = false;
    }

    /// <summary>Displays the complete Exploration character profile.</summary>
    public void SetCharacter(CharacterViewModel character)
    {
        if (character == null)
        {
            GD.PushError("CharacterPanel received null view model.");
            return;
        }

        _sectionTitle.Text = "YOUR CHARACTER";
        _portrait.Visible = true;
        _healthHeader.Visible = true;
        _healthBar.Visible = true;
        _level.Visible = true;
        _fatigueHeader.Visible = true;
        _fatigueBar.Visible = true;
        _staminaHeader.Visible = true;
        _staminaBar.Visible = true;
        _moodHeader.Visible = true;
        _moodBar.Visible = true;
        _gold.Visible = true;
        _arousalHeader.Visible = false;
        _arousalBar.Visible = false;

        _name.Text = character.Name;
        _role.Text = character.Role;
        _level.Text = "LEVEL " + character.Level;
        _healthValue.Text = character.Health + " / " + character.MaxHealth;
        _fatigueValue.Text = character.Fatigue.ToString("0.0") + " / " + character.MaxFatigue.ToString("0");
        _staminaValue.Text = character.Stamina + " / " + character.MaxStamina;
        _moodValue.Text = character.Mood + " / " + character.MaxMood + " · " + FormatMoodBand(character.MoodBand);
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

    /// <summary>Displays only combat-relevant values for the currently selected Party member.</summary>
    public void SetCombatCharacter(PartyMemberCardViewData member)
    {
        _sectionTitle.Text = member == null ? "ENEMY TURN" : "ACTIVE PARTY MEMBER";
        _portrait.Visible = false;
        _level.Visible = false;
        _fatigueHeader.Visible = false;
        _fatigueBar.Visible = false;
        _moodHeader.Visible = false;
        _moodBar.Visible = false;
        _gold.Visible = false;
        _arousalHeader.Visible = member != null && member.Arousal.HasValue;
        _arousalBar.Visible = member != null && member.Arousal.HasValue;

        if (member == null)
        {
            _name.Text = "Enemies are acting";
            _role.Text = "Party on standby";
            _healthHeader.Visible = false;
            _healthBar.Visible = false;
            _staminaHeader.Visible = false;
            _staminaBar.Visible = false;
            return;
        }

        _healthHeader.Visible = true;
        _healthBar.Visible = true;
        _name.Text = member.Name;
        _role.Text = member.Condition.ToUpperInvariant();
        _healthValue.Text = member.CurrentHealth + " / " + member.MaxHealth;
        _healthBar.MaxValue = member.MaxHealth;
        _healthBar.Value = member.CurrentHealth;

        _staminaHeader.Visible = member.MaxStamina > 0;
        _staminaBar.Visible = member.MaxStamina > 0;
        if (member.MaxStamina > 0)
        {
            _staminaValue.Text = member.CurrentStamina + " / " + member.MaxStamina;
            _staminaBar.MaxValue = member.MaxStamina;
            _staminaBar.Value = member.CurrentStamina;
        }

        _arousalHeader.Visible = member.Arousal.HasValue;
        _arousalBar.Visible = member.Arousal.HasValue;
        if (member.Arousal.HasValue)
        {
            _arousalValue.Text = member.Arousal.Value + " / 100";
            _arousalBar.MaxValue = 100;
            _arousalBar.Value = member.Arousal.Value;
        }
    }

    private static string FormatMoodBand(MoodBand moodBand)
    {
        return moodBand switch
        {
            MoodBand.VeryLow => "Very Low",
            MoodBand.Low => "Low",
            MoodBand.Steady => "Steady",
            MoodBand.Positive => "Positive",
            _ => throw new System.ArgumentOutOfRangeException(nameof(moodBand))
        };
    }
}
