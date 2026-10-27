namespace Ambio.Domain.Settings;

/// <summary>The preferences of the account, a single row (ADR 0002). Until it is first saved, the defaults apply.
/// The theme isn't stored here: it's a choice of each browser.</summary>
public class UserSettings
{
    public const int DefaultFollowUpAfterDays = 7;

    public const int MinFollowUpAfterDays = 1;

    public const int MaxFollowUpAfterDays = 30;

    public Guid Id { get; } = Guid.CreateVersion7();

    // An application still Applied after this many days, with no status change, needs a follow-up.
    public int FollowUpAfterDays
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, MinFollowUpAfterDays);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxFollowUpAfterDays);
            field = value;
        }
    } = DefaultFollowUpAfterDays;
}
