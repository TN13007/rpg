using System;
using System.Collections.Generic;
using Godot;

/// <summary>Displays the horizontal Party Bar and lets the player choose an eligible acting member.</summary>
public partial class PartyRosterPanel : PanelContainer
{
    /// <summary>Raised when the player selects a Party member who can act.</summary>
    public event Action<string> MemberSelected;

    private HBoxContainer _roster;
    private PackedScene _memberCardScene;
    private readonly Dictionary<string, PartyMemberCard> _memberCards =
        new Dictionary<string, PartyMemberCard>(StringComparer.Ordinal);
    private Label _emptyLabel;
    private string _selectedMemberId = string.Empty;

    /// <summary>Gets the selected Party member identifier.</summary>
    public string SelectedMemberId => _selectedMemberId;

    /// <summary>Initializes the Party Bar and loads the reusable member card scene.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _roster = GetNode<HBoxContainer>("Margin/Content/RosterScroll/Roster");
        _memberCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/PartyMemberCard.tscn");
        if (_memberCardScene == null)
            GD.PushError("PartyMemberCard.tscn could not be loaded.");
    }

    /// <summary>Updates all Party cards and moves focus to the next eligible actor when required.</summary>
    public void SetMembers(PartyMemberCardViewData[] members)
    {
        HashSet<string> currentIds = new HashSet<string>(StringComparer.Ordinal);
        int cardIndex = 0;
        string firstEligibleId = string.Empty;
        string firstLivingId = string.Empty;

        if (members != null)
        {
            foreach (PartyMemberCardViewData member in members)
            {
                if (member == null || string.IsNullOrWhiteSpace(member.Id))
                    continue;

                currentIds.Add(member.Id);
                if (!string.Equals(member.Condition, "Downed", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(firstLivingId))
                    firstLivingId = member.Id;
                if (member.CanAct && string.IsNullOrEmpty(firstEligibleId))
                    firstEligibleId = member.Id;

                if (!_memberCards.TryGetValue(member.Id, out PartyMemberCard card))
                {
                    if (_memberCardScene == null)
                        continue;

                    card = _memberCardScene.Instantiate<PartyMemberCard>();
                    card.Selected += HandleMemberSelected;
                    _roster.AddChild(card);
                    _memberCards.Add(member.Id, card);
                }

                card.SetMember(member);
                card.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                card.SizeFlagsStretchRatio = 1.0f;
                _roster.MoveChild(card, cardIndex++);
            }
        }

        List<string> removedIds = new List<string>();
        foreach (string memberId in _memberCards.Keys)
        {
            if (!currentIds.Contains(memberId))
                removedIds.Add(memberId);
        }

        foreach (string memberId in removedIds)
        {
            _memberCards[memberId].QueueFree();
            _memberCards.Remove(memberId);
        }

        bool selectedStillLiving = _memberCards.TryGetValue(_selectedMemberId, out PartyMemberCard selectedCard) &&
            !selectedCard.IsDowned;

        // If another eligible actor exists and the previous member already acted, move focus forward.
        // During Enemy Turn, no one is eligible, so the current living member remains focused.
        if (!selectedStillLiving || (firstEligibleId.Length > 0 && !selectedCard.CanAct))
            _selectedMemberId = firstEligibleId.Length > 0 ? firstEligibleId : firstLivingId;

        UpdateSelectionState();
        UpdateEmptyState();
    }

    /// <summary>Plays a combat feedback cue for the Party member with the supplied name.</summary>
    public void PlayFeedback(string characterName, CombatFeedbackType feedbackType)
    {
        foreach (PartyMemberCard card in _memberCards.Values)
        {
            if (string.Equals(card.MemberName, characterName, StringComparison.OrdinalIgnoreCase))
                card.PlayFeedback(feedbackType);
        }
    }

    private void HandleMemberSelected(string memberId)
    {
        if (!_memberCards.TryGetValue(memberId, out PartyMemberCard card) || !card.CanAct || card.IsDowned)
            return;

        _selectedMemberId = memberId;
        UpdateSelectionState();
        MemberSelected?.Invoke(memberId);
    }

    private void UpdateSelectionState()
    {
        foreach (KeyValuePair<string, PartyMemberCard> entry in _memberCards)
            entry.Value.SetSelected(entry.Key == _selectedMemberId);
    }

    private void UpdateEmptyState()
    {
        if (_memberCards.Count == 0)
        {
            if (_emptyLabel == null)
            {
                _emptyLabel = new Label
                {
                    Text = "No party members",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                _emptyLabel.AddThemeFontSizeOverride("font_size", 10);
                _emptyLabel.AddThemeColorOverride("font_color", UiTheme.Muted);
                _roster.AddChild(_emptyLabel);
            }
            return;
        }

        if (_emptyLabel != null)
        {
            _emptyLabel.QueueFree();
            _emptyLabel = null;
        }
    }
}
