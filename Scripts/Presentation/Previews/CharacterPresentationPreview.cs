using Godot;

public partial class CharacterPresentationPreview : Control
{
    private TextureRect _fullBodyImage;
    private TextureRect _portraitImage;
    private Label _fullBodyName;
    private Label _portraitName;
    private Label _profileName;
    private Label _profileRole;
    private Label _profileLevel;
    private Label _profileAttributes;
    private Label _profileCondition;

    public override void _Ready()
    {
        _fullBodyImage = GetNode<TextureRect>("Margin/Content/Views/FullBodyPanel/Content/CharacterImage");
        _portraitImage = GetNode<TextureRect>("Margin/Content/Views/PortraitPanel/Content/PortraitImage");
        _fullBodyName = GetNode<Label>("Margin/Content/Views/FullBodyPanel/Content/CharacterName");
        _portraitName = GetNode<Label>("Margin/Content/Views/PortraitPanel/Content/CharacterName");
        _profileName = GetNode<Label>("Margin/Content/Views/ProfilePanel/Content/CharacterName");
        _profileRole = GetNode<Label>("Margin/Content/Views/ProfilePanel/Content/Role");
        _profileLevel = GetNode<Label>("Margin/Content/Views/ProfilePanel/Content/Level");
        _profileAttributes = GetNode<Label>("Margin/Content/Views/ProfilePanel/Content/Attributes");
        _profileCondition = GetNode<Label>("Margin/Content/Views/ProfilePanel/Content/Condition");

        LoadPlayerPresentation();
    }

    private void LoadPlayerPresentation()
    {
        GameContentCatalog content = GameContentLoader.Load("res://Data");
        Character player = new Character(content.GetCharacter("player-ari"));
        CharacterViewModel character = new CharacterViewModel(player);

        Texture2D fullBodyTexture = GD.Load<Texture2D>("res://Assets/CharacterFullBody.svg");
        Texture2D portraitTexture = GD.Load<Texture2D>("res://Assets/CharacterPortrait.svg");

        _fullBodyImage.Texture = fullBodyTexture;
        _portraitImage.Texture = portraitTexture;

        _fullBodyName.Text = character.Name;
        _portraitName.Text = character.Name;
        _profileName.Text = character.Name;
        _profileRole.Text = character.Role;
        _profileLevel.Text = "LEVEL " + character.Level;
        _profileAttributes.Text =
            "STRENGTH       " + character.Stats.Strength
            + "\nAGILITY           " + character.Stats.Agility
            + "\nINTELLIGENCE  " + character.Stats.Intelligence
            + "\nENDURANCE    " + character.Stats.Endurance
            + "\nWISDOM           " + character.Stats.Wisdom;
        _profileCondition.Text =
            "HEALTH     " + character.Health + " / " + character.MaxHealth
            + "\nFATIGUE   " + character.Fatigue.ToString("0.0") + " / " + character.MaxFatigue.ToString("0")
            + "\nSTAMINA  " + character.Stamina + " / " + character.MaxStamina
            + "\nMOOD      " + character.Mood + " / " + character.MaxMood
            + "\nGOLD       " + character.Gold + " G";
    }
}