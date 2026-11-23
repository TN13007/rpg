using System;
using System.Collections.Generic;

/// <summary>Maps application data into action choice presentation models.</summary>
public sealed class ActionChoiceMapper
{
    private readonly ActionDurationEstimateMapper _durationMapper;

    /// <summary>Initializes a new instance of <see cref="ActionChoiceMapper"/>.</summary>
    /// <param name="durationMapper">The duration mapper used to initialize the instance.</param>
    public ActionChoiceMapper(ActionDurationEstimateMapper durationMapper)
    {
        _durationMapper = durationMapper ?? throw new ArgumentNullException(nameof(durationMapper));
    }

    /// <summary>Maps the supplied map data into presentation data.</summary>
    /// <param name="interactions">The interactions used by this operation.</param>
    /// <param name="player">The player used by this operation.</param>
    /// <returns>The resulting list action choice view data.</returns>
    public List<ActionChoiceViewData> Map(IReadOnlyList<AvailableInteraction> interactions, Character player)
    {
        if (interactions == null) throw new ArgumentNullException(nameof(interactions));
        if (player == null) throw new ArgumentNullException(nameof(player));

        List<ActionChoiceViewData> choices = new List<ActionChoiceViewData>();
        foreach (InteractionSourceKind sourceKind in new[]
        {
            InteractionSourceKind.Player,
            InteractionSourceKind.Location,
            InteractionSourceKind.Npc,
            InteractionSourceKind.Travel,
            InteractionSourceKind.Event,
            InteractionSourceKind.Interactable
        })
        {
            foreach (AvailableInteraction interaction in interactions)
            {
                if (interaction.SourceKind != sourceKind) continue;

                ActionSelectionRequest request = new ActionSelectionRequest(
                    interaction.SourceKind,
                    interaction.SourceId,
                    interaction.ActionId,
                    interaction.TargetId);
                string groupTitle = interaction.SourceKind switch
                {
                    InteractionSourceKind.Player => "PERSONAL",
                    InteractionSourceKind.Location => "AT THIS LOCATION",
                    InteractionSourceKind.Npc => "WITH NPC",
                    InteractionSourceKind.Travel => "TRAVEL",
                    InteractionSourceKind.Event => "CURRENT SITUATION",
                    InteractionSourceKind.Interactable => interaction.ActionId == "select" ? "OBJECTS" : "WITH OBJECT",
                    _ => throw new ArgumentOutOfRangeException()
                };

                choices.Add(new ActionChoiceViewData(
                    request.Encode(),
                    interaction.Action.Label,
                    _durationMapper.Map(interaction.Action, player),
                    groupTitle));
            }
        }

        return choices;
    }
}
