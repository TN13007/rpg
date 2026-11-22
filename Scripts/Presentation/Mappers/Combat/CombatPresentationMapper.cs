using System;
using System.Collections.Generic;

/// <summary>Maps Domain combat results and snapshots into presentation-only view data.</summary>
public sealed class CombatPresentationMapper
{
    /// <summary>Maps a Domain snapshot without adding historical messages.</summary>
    public CombatViewData Map(CombatSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        return new CombatViewData(
            snapshot.Round,
            snapshot.Phase,
            snapshot.CurrentActionPoints,
            snapshot.ActionCapacity,
            snapshot.ActiveTurnLabel,
            snapshot.Enemies,
            snapshot.Party,
            snapshot.AvailableActions,
            Array.Empty<CombatMessage>());
    }

    /// <summary>Maps a Domain result, preserving the order of its resolved events.</summary>
    public CombatViewData Map(CombatResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        CombatSnapshot snapshot = result.Snapshot;
        int extraMessageCount = result.Success || string.IsNullOrWhiteSpace(result.FailureReason) ? 0 : 1;
        CombatMessage[] messages = new CombatMessage[result.Events.Length + extraMessageCount];

        for (int index = 0; index < result.Events.Length; index++)
        {
            messages[index] = MapEvent(result.Events[index]);
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
            snapshot.Phase,
            snapshot.CurrentActionPoints,
            snapshot.ActionCapacity,
            snapshot.ActiveTurnLabel,
            snapshot.Enemies,
            snapshot.Party,
            snapshot.AvailableActions,
            messages,
            feedback: MapFeedback(result.Events));
    }

    private static CombatFeedbackViewData[] MapFeedback(CombatEvent[] events)
    {
        if (events == null || events.Length == 0)
        {
            return Array.Empty<CombatFeedbackViewData>();
        }

        List<CombatFeedbackViewData> feedback = new List<CombatFeedbackViewData>();
        foreach (CombatEvent combatEvent in events)
        {
            if (combatEvent.Type == CombatEventType.Damage &&
                !string.IsNullOrWhiteSpace(combatEvent.TargetName))
            {
                feedback.Add(new CombatFeedbackViewData(
                    string.Empty,
                    combatEvent.TargetName,
                    CombatFeedbackType.Hit));
                continue;
            }

            if (combatEvent.Type == CombatEventType.EnemyAction)
            {
                if (!string.IsNullOrWhiteSpace(combatEvent.TargetName))
                {
                    feedback.Add(new CombatFeedbackViewData(
                        combatEvent.ActorName,
                        string.Empty,
                        CombatFeedbackType.Attack));
                }

                continue;
            }

            if (combatEvent.Type != CombatEventType.PlayerAction)
            {
                continue;
            }

            CombatFeedbackType? type = combatEvent.ActionId switch
            {
                "quick_strike" => CombatFeedbackType.Attack,
                "heavy_strike" => CombatFeedbackType.Attack,
                "disrupting_strike" => CombatFeedbackType.Attack,
                "guard" => CombatFeedbackType.Guard,
                "evade" => CombatFeedbackType.Evade,
                "catch_breath" => CombatFeedbackType.Recover,
                "revive_ally" => CombatFeedbackType.Revive,
                _ => null
            };

            if (type.HasValue && !string.IsNullOrWhiteSpace(combatEvent.ActorName))
            {
                feedback.Add(new CombatFeedbackViewData(
                    combatEvent.ActorName,
                    combatEvent.TargetName,
                    type.Value));
            }
        }

        return feedback.ToArray();
    }

    private static CombatMessage MapEvent(CombatEvent combatEvent)
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