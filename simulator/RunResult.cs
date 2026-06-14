namespace NEmaris.Simulator;

public enum RunOutcome
{
    BookingConfirmed,
    CancellationConfirmed,
    FarewellExit,
    MaxTurnsHit,
    ErrorAborted,
    NoAvailabilityExit
}

public sealed record RunResult(
    GuestPersona Persona,
    RunOutcome Outcome,
    int Turns,
    bool BookingAttempted,
    bool ConfirmationVerbatimSeen,
    TimeSpan Elapsed,
    string? FailureReason);
