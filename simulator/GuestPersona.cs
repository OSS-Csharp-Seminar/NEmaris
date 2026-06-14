namespace NEmaris.Simulator;

public sealed record GuestPersona(
    string FirstName,
    string LastName,
    string Phone,
    int PartySize,
    string TimePhrase,
    bool WantsToCancel)
{
    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() =>
        $"{FullName} ({Phone}), party {PartySize}, {TimePhrase}" +
        (WantsToCancel ? ", will cancel" : "");
}
