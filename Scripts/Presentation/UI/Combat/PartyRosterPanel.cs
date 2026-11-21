using System;
using System.Collections.Generic;
using Godot;

/// <summary>Presents party roster information and exposes its UI interactions.</summary>
public partial class PartyRosterPanel : PanelContainer
{
    private HBoxContainer _roster;
    private PackedScene _memberCardScene;
    private readonly Dictionary<string, PartyMemberCard> _memberCards =
        new Dictionary<string, PartyMemberCard>(StringComparer.Ordinal);
    private Label _emptyLabel;

    /// <summary>Initializes the Godot control and connects its UI signals.</summary>
    public override void _Ready()
    {
        UiTheme.StylePanel(this);
        _roster = GetNode<HBoxContainer>("Margin/Content/RosterScroll/Roster");
        _memberCardScene = GD.Load<PackedScene>("res://Scenes/UI/Combat/PartyMemberCard.tscn");
        if (_memberCardScene == null)
        {
            GD.PushError("PartyMemberCard.tscn could not be loaded.");
        }
    }

    /// <summary>Sets members.</summary>
    /// <param name="members">The members used by this operation.</param>
    public void SetMembers(PartyCombatStatus[] members)
    {
        HashSet<string> currentIds = new HashSet<string>(StringComparer.Ordinal);
        int cardIndex = 0;

        if (members != null)
        {
            foreach (PartyCombatStatus member in members)
            {
                if (member == null || string.IsNullOrWhiteSpace(member.Id))
                {
                    continue;
                }

                currentIds.Add(member.Id);
                if (!_memberCards.TryGetValue(member.Id, out PartyMemberCard card))
                {
                    if (_memberCardScene == null)
                    {
                        continue;
                    }

                    card = _memberCardScene.Instantiate<PartyMemberCard>();
                    _roster.AddChild(card);
                    _memberCards.Add(member.Id, card);
                }

                card.SetMember(member);
                _roster.MoveChild(card, cardIndex++);
            }
        }

        List<string> removedIds = new List<string>();
        foreach (string memberId in _memberCards.Keys)
        {
            if (!currentIds.Contains(memberId))
            {
                removedIds.Add(memberId);
            }
        }

        foreach (string memberId in removedIds)
        {
            _memberCards[memberId].QueueFree();
            _memberCards.Remove(memberId);
        }

        UpdateEmptyState();
    }

    /// <summary>Plays feedback feedback for the selected combatant or action.</summary>
    /// <param name="characterName">The character name used by this operation.</param>
    /// <param name="feedbackType">The feedback type used by this operation.</param>
    public void PlayFeedback(string characterName, CombatFeedbackType feedbackType)
    {
        foreach (PartyMemberCard card in _memberCards.Values)
        {
            if (card.MemberName == characterName)
            {
                card.PlayFeedback(feedbackType);
            }
        }
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
