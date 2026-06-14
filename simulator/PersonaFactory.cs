namespace NEmaris.Simulator;

internal static class PersonaFactory
{
    private static readonly string[] FirstNames =
    {
        "Ante", "Marko", "Petar", "Ivan", "Luka", "Tomislav", "Marin", "Stipe",
        "Ana", "Marija", "Iva", "Maja", "Petra", "Nina", "Ivana", "Lucija"
    };

    private static readonly string[] LastNames =
    {
        "Antinovic", "Horvat", "Kovacic", "Maric", "Babic", "Vlahovic",
        "Pavlovic", "Bilic", "Tomic", "Knezevic", "Juric", "Matic"
    };

    private static readonly string[] TimePhrases =
    {
        "in about 5 minutes",
        "in 15 minutes",
        "in half an hour",
        "in 45 minutes",
        "in an hour",
        "in 90 minutes",
        "in 2 hours"
    };

    public static GuestPersona Random(Random rng, double cancelProbability = 0.5)
    {
        var firstName = FirstNames[rng.Next(FirstNames.Length)];
        var lastName = LastNames[rng.Next(LastNames.Length)];
        var phone = $"+385 {rng.Next(50, 100)} {rng.Next(100, 1000)} {rng.Next(1000, 10000)}";
        var partySize = rng.Next(2, 11);
        var timePhrase = TimePhrases[rng.Next(TimePhrases.Length)];
        var wantsToCancel = rng.NextDouble() < cancelProbability;
        return new GuestPersona(firstName, lastName, phone, partySize, timePhrase, wantsToCancel);
    }
}
