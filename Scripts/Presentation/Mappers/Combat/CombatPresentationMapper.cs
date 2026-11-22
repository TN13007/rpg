using System;
using System.Collections.Generic;

/// <summary>Maps Domain combat results into Presentation-owned view data.</summary>
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
            snapshot.Phase == CombatPhase.PlayerTurn,
            snapshot.CurrentActionPoints,
            snapshot.ActionCapacity,
            snapshot.ActiveTurnLabel,
            MapEnemies(snapshot.Enemies),
            MapParty(snapshot.Party),
            MapActions(snapshot.AvailableActions),
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
            snapshot.Phase == CombatPhase.PlayerTurn,
            snapshot.CurrentActionPoints,
            snapshot.ActionCapacity,
            snapshot.ActiveTurnLabel,
            MapEnemies(snapshot.Enemies),
            MapParty(snapshot.Party),
            MapActions(snapshot.AvailableActions),
            messages,
            MapFeedback(result.Events));
    }

    private static EnemyCardViewData[] MapEnemies(EnemyCombatStatus[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            return Array.Empty<EnemyCardViewData>();
        }

        List<EnemyCardViewData> viewData = new List<EnemyCardViewData>();
        foreach (EnemyCombatStatus enemy in enemies)
        {
            if (enemy == null || !enemy.IsVisible)
            {
                continue;
            }

            viewData.Add(new EnemyCardViewData(
                enemy.Id,
                enemy.Name,
                enemy.CurrentHealth,
                enemy.MaxHealth,
                enemy.Condition));
        }

        return viewData.ToArray();
    }

    private static PartyMemberCardViewData[] MapParty(PartyCombatStatus[] party)
    {
        if (party == null || party.Length == 0)
        {
            return Array.Empty<PartyMemberCardViewData>();
        }

        PartyMemberCardViewData[] viewData = new PartyMemberCardViewData[party.Length];
        for (int index = 0; index < party.Length; index++)
        {
            PartyCombatStatus member = party[index];
            viewData[index] = new PartyMemberCardViewData(
                member.Id,
                member.Name,
                member.CurrentHealth,
                member.MaxHealth,
                member.Condition,
                member.IsActive,
                member.CurrentStamina,
                member.MaxStamina);
        }

        return viewData;
    }

    private static CombatActionCardViewData[] MapActions(CombatActionOption[] actions)
    {
        if (actions == null || actions.Length == 0)
        {
            return Array.Empty<CombatActionCardViewData>();
        }

        CombatActionCardViewData[] viewData = new CombatActionCardViewData[actions.Length];
        for (int index = 0; index < actions.Length; index++)
        {
            CombatActionOption action = actions[index];
            CombatActionTargetViewData[] targets = new CombatActionTargetViewData[action.Targets.Length];
            for (int targetIndex = 0; targetIndex < action.Targets.Length; targetIndex++)
            {
                CombatActionTargetOption target = action.Targets[targetIndex];
                targets[targetIndex] = new CombatActionTargetViewData(target.Id, target.Name);
            }

            CombatActionTargetKind targetKind = action.Id == "revive_ally"
                ? CombatActionTargetKind.Party
                : targets.Length > 0
                    ? CombatActionTargetKind.Enemy
                    : CombatActionTargetKind.None;

            viewData[index] = new CombatActionCardViewData(
                action.Id,
                action.ActorId,
                action.Label,
                action.CostLabel,
                action.Description,
                action.IsAvailable,
                action.UnavailableReason,
                MapActionCategory(action.Id),
                targetKind,
                targets);
        }

        return viewData;
    }

    private static CombatActionCategory MapActionCategory(string actionId)
    {
        return actionId switch
        {
            "quick_strike" or "heavy_strike" or "disrupting_strike" => CombatActionCategory.Offense,
            "guard" or "evade" => CombatActionCategory.Defense,
            "catch_breath" or "revive_ally" => CombatActionCategory.Support,
            _ => CombatActionCategory.Other
        };
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
                "quick_strike" => CombatFeedbackType.QuickAttack,
                "heavy_strike" => CombatFeedbackType.HeavyAttack,
                "disrupting_strike" => CombatFeedbackType.DisruptingAttack,
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
