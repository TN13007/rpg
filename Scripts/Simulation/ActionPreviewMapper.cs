using System;
using System.Collections.Generic;

public sealed class ActionPreviewMapper
{
    private const int MinutesPerDay = 24 * 60;
    private const int WakeUpMinutes = 7 * 60 + 30;

    private readonly ActivityTimeCalculator _timeCalculator;
    private readonly ActionExecutionService _executionService;

    public ActionPreviewMapper(ActivityTimeCalculator timeCalculator, ActionExecutionService executionService)
    {
        _timeCalculator = timeCalculator ?? throw new ArgumentNullException(nameof(timeCalculator));
        _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
    }

    public ActionPreviewData Map(ActionDefinition action, Character actor, DayClock clock)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        if (clock == null) throw new ArgumentNullException(nameof(clock));

        bool canConfirm = _executionService.CanExecute(action, actor, out string rejectionReason);
        bool isNextDay = action.TimeMode == ActionTimeMode.NextDay;
        int durationMinutes = isNextDay ? 0 : _timeCalculator.Calculate(action, actor);
        string durationText = isNextDay ? "Until the next morning" : FormatDuration(durationMinutes);
        string expectedTime = isNextDay ? GetNextDayTime(clock) : GetExpectedTime(clock, durationMinutes);

        return new ActionPreviewData(
            action.Label,
            clock.GetDisplayText(),
            durationText,
            action.EnergyCost + " Energy",
            expectedTime,
            FormatEffects(action.Effects),
            canConfirm,
            rejectionReason);
    }

    private string FormatEffects(ActionEffectDefinition[] effects)
    {
        if (effects.Length == 0) return "No additional effects";

        List<string> lines = new List<string>();
        foreach (ActionEffectDefinition effect in effects)
        {
            switch (effect.Type)
            {
                case ActionEffectType.RestoreHealth:
                    lines.Add("Restore " + effect.Amount + " Health");
                    break;
                case ActionEffectType.RestoreEnergy:
                    lines.Add("Restore " + effect.Amount + " Energy");
                    break;
                case ActionEffectType.ChangeMood:
                    string sign = effect.Amount > 0 ? "+" : string.Empty;
                    lines.Add("Mood " + sign + effect.Amount);
                    break;
                case ActionEffectType.AddGold:
                    lines.Add("Gain " + effect.Amount + " Gold");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect.Type));
            }
        }

        return string.Join("\n", lines);
    }

    private string GetExpectedTime(DayClock clock, int durationMinutes)
    {
        long totalMinutes = (long)clock.MinutesOfDay + durationMinutes;
        int elapsedDays = (int)(totalMinutes / MinutesPerDay);
        int day = checked(clock.Day + elapsedDays);
        int minutesOfDay = (int)(totalMinutes % MinutesPerDay);
        return FormatDayTime(day, minutesOfDay);
    }

    private string GetNextDayTime(DayClock clock)
    {
        int day = clock.Day;
        if (clock.MinutesOfDay >= WakeUpMinutes) day = checked(day + 1);
        return FormatDayTime(day, WakeUpMinutes);
    }

    private string FormatDayTime(int day, int minutesOfDay)
    {
        int hour = minutesOfDay / 60;
        int minute = minutesOfDay % 60;
        return "DAY " + day.ToString("00") + " · " + hour.ToString("00") + ":" + minute.ToString("00");
    }

    private string FormatDuration(int durationMinutes)
    {
        return durationMinutes == 1 ? "1 minute" : durationMinutes + " minutes";
    }
}
