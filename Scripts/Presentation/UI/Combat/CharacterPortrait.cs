using Godot;

/// <summary>Displays the selected Party member using layered, Presentation-supplied portrait assets.</summary>
public partial class CharacterPortrait : PanelContainer
{
    private TextureRect _basePose;
    private TextureRect _outfitLayer;
    private TextureRect _expressionLayer;
    private TextureRect _statusOverlay;
    private Label _nameLabel;
    private Label _statusLabel;
    private Label _visualStateLabel;

    /// <summary>Initializes the portrait layers and panel style.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _basePose = GetNode<TextureRect>("Margin/Content/PortraitFrame/BasePose");
        _outfitLayer = GetNode<TextureRect>("Margin/Content/PortraitFrame/OutfitLayer");
        _expressionLayer = GetNode<TextureRect>("Margin/Content/PortraitFrame/ExpressionLayer");
        _statusOverlay = GetNode<TextureRect>("Margin/Content/PortraitFrame/StatusOverlay");
        _nameLabel = GetNode<Label>("Margin/Content/Name");
        _statusLabel = GetNode<Label>("Margin/Content/Status");
        _visualStateLabel = GetNode<Label>("Margin/Content/VisualState");
        Clear();
    }

    /// <summary>Displays the supplied character portrait, pose, outfit, expression and status layers.</summary>
    public void SetCharacter(CharacterPortraitViewData portrait)
    {
        if (portrait == null)
        {
            Clear();
            return;
        }

        _nameLabel.Text = portrait.Name;
        _statusLabel.Text = portrait.Status;
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(portrait.Status);
        _visualStateLabel.Text = BuildVisualState(portrait);
        _visualStateLabel.Visible = !string.IsNullOrWhiteSpace(_visualStateLabel.Text);

        SetLayer(_basePose, string.IsNullOrWhiteSpace(portrait.BasePosePath)
            ? "res://Assets/CharacterPortrait.svg"
            : portrait.BasePosePath);
        SetLayer(_outfitLayer, portrait.OutfitLayerPath);
        SetLayer(_expressionLayer, portrait.ExpressionLayerPath);
        SetLayer(_statusOverlay, portrait.StatusOverlayPath);
    }

    /// <summary>Clears the current portrait when no Party member can act.</summary>
    public void Clear()
    {
        if (_nameLabel == null)
        {
            return;
        }

        _nameLabel.Text = "No active character";
        _statusLabel.Text = string.Empty;
        _statusLabel.Visible = false;
        _visualStateLabel.Text = string.Empty;
        _visualStateLabel.Visible = false;
        SetLayer(_basePose, "res://Assets/CharacterPortrait.svg");
        SetLayer(_outfitLayer, string.Empty);
        SetLayer(_expressionLayer, string.Empty);
        SetLayer(_statusOverlay, string.Empty);
    }

    private static void SetLayer(TextureRect layer, string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            layer.Texture = null;
            layer.Visible = false;
            return;
        }

        Texture2D texture = ResourceLoader.Load<Texture2D>(resourcePath);
        layer.Texture = texture;
        layer.Visible = texture != null;
        if (texture == null)
        {
            GD.PushWarning("Character portrait layer could not be loaded: " + resourcePath);
        }
    }

    private static string BuildVisualState(CharacterPortraitViewData portrait)
    {
        string[] parts =
        {
            portrait.StanceId,
            portrait.ExpressionId,
            portrait.OutfitId
        };

        return string.Join("  ·  ", System.Array.FindAll(parts, value => !string.IsNullOrWhiteSpace(value)));
    }
}
