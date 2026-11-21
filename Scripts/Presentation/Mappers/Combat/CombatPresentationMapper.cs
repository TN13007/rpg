using System;

public sealed class CombatPresentationMapper
{
    public CombatViewData Map(CombatSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        return new CombatViewData(
            snapshot.Round,
            snapshot.ActiveTurnLabel,
            snapshot.Enemies,
            snapshot.Intent,
            snapshot.AvailableActions,
            Array.Empty<CombatMessage>(),
            MapTimeline(snapshot.Timeline),
            snapshot.Party,
            snapshot.ActiveCombatantId);
    }

    public CombatViewData Map(CombatResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        CombatSnapshot snapshot = result.Snapshot;
        int extraMessageCount = result.Success || string.IsNullOrWhiteSpace(result.FailureReason) ? 0 : 1;
        CombatMessage[] messages = new CombatMessage[result.Events.Length + extraMessageCount];
        for (int i = 0; i < result.Events.Length; i++)
        {
            messages[i] = MapEvent(result.Events[i]);
        }

        if (extraMessageCount == 1)
        {
            messages[messages.Length - 1] = new CombatMessage(
                Guid.NewGuid().ToString("N"),
                snapshot.Round,
                CombatMessageType.System,
                CombatMessageCategory.StatusChange,
                "SYSTEM",
                result.FailureReason);
        }

        return new CombatViewData(
            snapshot.Round,
            snapshot.ActiveTurnLabel,
            snapshot.Enemies,
            snapshot.Intent,
            snapshot.AvailableActions,
            messages,
            MapTimeline(snapshot.Timeline),
            snapshot.Party,
            snapshot.ActiveCombatantId);
    }

    private static TimelineMarkerViewData[] MapTimeline(TimelineCombatantStatus[] entries)
    {
        if (entries == null || entries.Length == 0)
        {
            return Array.Empty<TimelineMarkerViewData>();
        }

        TimelineMarkerViewData[] mapped = new TimelineMarkerViewData[entries.Length];
        for (int index = 0; index < entries.Length; index++)
        {
            TimelineCombatantStatus entry = entries[index];
            double timeToReady = entry.RemainingStunTime +
                entry.RemainingProgress / entry.Speed;

            mapped[index] = new TimelineMarkerViewData(
                entry.Id,
                entry.Name,
                entry.IsEnemy,
                entry.IsPrimaryPartyMember,
                entry.IsVisible,
                entry.IsReady,
                entry.IsStunned,
                entry.Luck,
                timeToReady,
                entry.IntentDescription);
        }

        return mapped;
    }

    private CombatMessage MapEvent(CombatEvent combatEvent)
    {
        CombatMessageType type = CombatMessageType.System;
        CombatMessageCategory category = CombatMessageCategory.None;
        string speaker = "SYSTEM";
        string text = combatEvent.Detail;

        switch (combatEvent.Type)
        {
            case CombatEventType.PlayerAction:
                type = CombatMessageType.Player;
                speaker = string.IsNullOrWhiteSpace(combatEvent.ActorName)
                    ? "PARTY"
                    : combatEvent.ActorName.ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(combatEvent.TargetName))
                {
                    text = combatEvent.Detail + " (Target: " + combatEvent.TargetName + ")";
                }
                break;
            case CombatEventType.EnemyAction:
                type = CombatMessageType.Enemy;
                speaker = combatEvent.ActorName;
                if (!string.IsNullOrWhiteSpace(combatEvent.TargetName))
                {
                    text = combatEvent.Detail + " (Target: " + combatEvent.TargetName + ")";
                }
                break;
            case CombatEventType.Damage:
                category = CombatMessageCategory.Damage;
                text = combatEvent.TargetName + " loses " + combatEvent.Amount + " HP.";
                break;
            case CombatEventType.Check:
                category = CombatMessageCategory.Check;
                break;
            case CombatEventType.TurnChanged:
                category = CombatMessageCategory.Turn;
                break;
            case CombatEventType.EnvironmentChanged:
                category = CombatMessageCategory.Environment;
                break;
            case CombatEventType.Inference:
                category = CombatMessageCategory.Inference;
                break;
            case CombatEventType.Emotion:
                category = CombatMessageCategory.Emotion;
                break;
            case CombatEventType.StatusChanged:
                category = CombatMessageCategory.StatusChange;
                break;
            case CombatEventType.RoundStarted:
                category = CombatMessageCategory.Round;
                break;
        }

        return new CombatMessage(
            combatEvent.Id,
            combatEvent.Round,
            type,
            category,
            speaker,
            text);
    }
}
