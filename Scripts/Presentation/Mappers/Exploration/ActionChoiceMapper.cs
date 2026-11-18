using System;
using System.Collections.Generic;

public sealed class ActionChoiceMapper
{
    private readonly ActionDurationEstimateMapper _durationMapper;

    public ActionChoiceMapper(ActionDurationEstimateMapper durationMapper)
    {
        _durationMapper = durationMapper ?? throw new ArgumentNullException(nameof(durationMapper));
    }

    public List<ActionChoiceViewData> Map(IReadOnlyList<AvailableInteraction> interactions, Character player)
    {
        if (interactions == null) throw new ArgumentNullException(nameof(interactions));
        if (player == null) throw new ArgumentNullException(nameof(player));

        List<ActionChoiceViewData> choices = new List<ActionChoiceViewData>();
        foreach (InteractionSourceKind sourceKind in new[]
        {
            InteractionSourceKind.Player,
            InteractionSourceKind.Location,
            InteractionSourceKind.Travel,
            InteractionSourceKind.Event
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
                    InteractionSourceKind.Travel => "TRAVEL",
                    InteractionSourceKind.Event => "CURRENT SITUATION",
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
