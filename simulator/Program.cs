using NEmaris.Simulator;

var backendUrl = Environment.GetEnvironmentVariable("SIMULATOR_BACKEND_URL") ?? "http://localhost:5199";
var ollamaUrl = Environment.GetEnvironmentVariable("SIMULATOR_OLLAMA_URL") ?? "http://localhost:11434";
var model = Environment.GetEnvironmentVariable("SIMULATOR_MODEL") ?? "llama3.2:3b";
var timeZone = Environment.GetEnvironmentVariable("SIMULATOR_TIMEZONE") ?? "Europe/Zagreb";
var maxTurns = TryParseInt("SIMULATOR_MAX_TURNS", 20);
var numCtx = TryParseInt("SIMULATOR_NUM_CTX", 8192);
var seed = TryParseInt("SIMULATOR_SEED", Random.Shared.Next());
var runs = Math.Max(1, TryParseInt("SIMULATOR_RUNS", 1));
var forceCancel = TryParseBool("SIMULATOR_FORCE_CANCEL");

var rng = new Random(seed);

Console.WriteLine("=== NEmaris guest simulator ===");
Console.WriteLine($"[sim]     seed={seed} model={model} backend={backendUrl} tz={timeZone} runs={runs}");
Console.WriteLine();

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
var backend = new BackendChatClient(http, backendUrl);
var guest = new GuestAgent(http, ollamaUrl, model, numCtx);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("[sim]     cancellation requested — finishing current run");
};

var results = new List<RunResult>(runs);

for (var run = 1; run <= runs && !cts.IsCancellationRequested; run++)
{
    var basePersona = PersonaFactory.Random(rng);
    var persona = forceCancel switch
    {
        true => basePersona with { WantsToCancel = true },
        false => basePersona with { WantsToCancel = false },
        null => basePersona
    };

    Console.WriteLine($"=== Run {run}/{runs} ===");
    Console.WriteLine($"[sim]     persona: {persona}");

    try
    {
        var result = await SimulationLoop.RunAsync(persona, backend, guest, timeZone, maxTurns, cts.Token);
        results.Add(result);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("[sim]     cancelled");
        break;
    }

    Console.WriteLine();
}

PrintSummary(results, runs);

static void PrintSummary(IReadOnlyList<RunResult> results, int requested)
{
    if (results.Count == 0)
    {
        Console.WriteLine("=== Summary === (no runs completed)");
        return;
    }

    Console.WriteLine($"=== Summary over {results.Count}/{requested} runs ===");

    var counts = results
        .GroupBy(r => r.Outcome)
        .ToDictionary(g => g.Key, g => g.Count());

    Console.WriteLine("Outcomes:");
    Console.WriteLine($"  Booking confirmed:       {counts.GetValueOrDefault(RunOutcome.BookingConfirmed)}");
    Console.WriteLine($"  Cancellation confirmed:  {counts.GetValueOrDefault(RunOutcome.CancellationConfirmed)}");
    Console.WriteLine($"  Farewell exit:           {counts.GetValueOrDefault(RunOutcome.FarewellExit)}");
    Console.WriteLine($"  No availability exit:    {counts.GetValueOrDefault(RunOutcome.NoAvailabilityExit)}");
    Console.WriteLine($"  Hit max turns:           {counts.GetValueOrDefault(RunOutcome.MaxTurnsHit)}");
    Console.WriteLine($"  Errors:                  {counts.GetValueOrDefault(RunOutcome.ErrorAborted)}");

    var bookingAttempts = results.Count(r => r.BookingAttempted);
    var verbatimSeen = results.Count(r => r.ConfirmationVerbatimSeen);
    if (bookingAttempts > 0)
        Console.WriteLine($"Confirmation string verbatim: {verbatimSeen}/{bookingAttempts} booking attempts");

    var turnsSorted = results.Select(r => r.Turns).OrderBy(t => t).ToList();
    var median = turnsSorted[turnsSorted.Count / 2];
    Console.WriteLine($"Turns: min={turnsSorted[0]} median={median} max={turnsSorted[^1]}");

    var totalSeconds = results.Sum(r => r.Elapsed.TotalSeconds);
    var avgSeconds = totalSeconds / results.Count;
    Console.WriteLine($"Elapsed: {totalSeconds:0.0}s total (avg {avgSeconds:0.0}s/run)");

    var errors = results.Where(r => r.Outcome == RunOutcome.ErrorAborted).ToList();
    if (errors.Count > 0)
    {
        Console.WriteLine("Errors:");
        foreach (var e in errors)
            Console.WriteLine($"  - {e.Persona.FullName}: {e.FailureReason}");
    }
}

static int TryParseInt(string envName, int fallback)
{
    var raw = Environment.GetEnvironmentVariable(envName);
    return int.TryParse(raw, out var value) ? value : fallback;
}

static bool? TryParseBool(string envName)
{
    var raw = Environment.GetEnvironmentVariable(envName);
    return bool.TryParse(raw, out var value) ? value : null;
}
