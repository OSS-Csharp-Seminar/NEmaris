using System.Text.RegularExpressions;

namespace NEmaris.Simulator;

internal static class SimulationLoop
{
    public static async Task<RunResult> RunAsync(
        GuestPersona persona,
        BackendChatClient backend,
        GuestAgent guest,
        string timeZone,
        int maxTurns,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var sessionId = Guid.NewGuid().ToString();
        Console.WriteLine($"[sim]     session={sessionId}");
        var systemPrompt = BuildPersonaPrompt(persona);
        var backendHistory = new List<ChatMessage>();
        var simulatorHistory = new List<ChatMessage>();

        var opener = BuildOpener(persona);
        Console.WriteLine($"[guest]   {opener}");
        backendHistory.Add(new ChatMessage { Role = "user", Content = opener });
        simulatorHistory.Add(new ChatMessage { Role = "assistant", Content = opener });

        var bookingAttempted = false;
        var confirmationVerbatim = false;
        var bookingDone = false;
        var turns = 1;
        var consecutiveNoAvailability = 0;
        var noAvailabilitySeen = false;

        for (var turn = 0; turn < maxTurns; turn++)
        {
            string reply;
            try
            {
                reply = await backend.SendAsync(backendHistory, timeZone, sessionId, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[sim]     backend call failed: {ex.Message}");
                stopwatch.Stop();
                return new RunResult(persona, RunOutcome.ErrorAborted, turns, bookingAttempted,
                    confirmationVerbatim, stopwatch.Elapsed, ex.Message);
            }

            Console.WriteLine($"[backend] {reply}");
            backendHistory.Add(new ChatMessage { Role = "assistant", Content = reply });
            simulatorHistory.Add(new ChatMessage { Role = "user", Content = reply });

            if (LooksLikeBookingAttempt(reply))
                bookingAttempted = true;
            if (IsConfirmationVerbatim(reply))
                confirmationVerbatim = true;

            if (!bookingDone && IsBookingComplete(reply))
            {
                var firstNameOk = ContainsWholeWord(reply, persona.FirstName);
                var lastNameOk = ContainsWholeWord(reply, persona.LastName);
                if (!firstNameOk || !lastNameOk)
                {
                    var missing = (!firstNameOk, !lastNameOk) switch
                    {
                        (true, true) => $"both names ('{persona.FirstName} {persona.LastName}')",
                        (true, false) => $"first name ('{persona.FirstName}')",
                        (false, true) => $"last name ('{persona.LastName}')",
                        _ => ""
                    };
                    Console.WriteLine($"[sim]     name mismatch in confirmation — expected {missing}, attempting orphan cleanup before erroring");

                    var cleanupMsg = $"Actually, please cancel that reservation. My phone number is {persona.Phone} and my last name is {persona.LastName}.";
                    Console.WriteLine($"[guest]   {cleanupMsg}");
                    backendHistory.Add(new ChatMessage { Role = "user", Content = cleanupMsg });
                    var cleanupTurns = turns + 1;

                    string cleanupReply = "";
                    try
                    {
                        cleanupReply = await backend.SendAsync(backendHistory, timeZone, sessionId, cancellationToken);
                        Console.WriteLine($"[backend] {cleanupReply}");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"[sim]     cleanup cancel call failed: {ex.Message}");
                    }

                    if (IsCancellationComplete(cleanupReply))
                        Console.WriteLine("[sim]     orphan reservation cleaned up");
                    else
                        Console.WriteLine("[sim]     orphan cleanup did NOT confirm; reservation may still be active in DB");

                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.ErrorAborted, cleanupTurns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed,
                        $"confirmation message did not contain persona {missing}");
                }

                bookingDone = true;
                bookingAttempted = true;
                if (!persona.WantsToCancel)
                {
                    Console.WriteLine("[sim]     booking confirmed — done");
                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.BookingConfirmed, turns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed, null);
                }
            }

            if (IsCancellationComplete(reply))
            {
                Console.WriteLine("[sim]     cancellation confirmed — done");
                stopwatch.Stop();
                return new RunResult(persona, RunOutcome.CancellationConfirmed, turns, bookingAttempted,
                    confirmationVerbatim, stopwatch.Elapsed, null);
            }

            if (IsNoAvailability(reply))
            {
                noAvailabilitySeen = true;
                consecutiveNoAvailability++;
                if (consecutiveNoAvailability >= 2)
                {
                    Console.WriteLine("[sim]     no availability — backend refused twice, stopping");
                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.NoAvailabilityExit, turns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed, "backend reported no tables available");
                }
            }
            else
            {
                consecutiveNoAvailability = 0;
            }

            string nextGuestMsg;
            try
            {
                nextGuestMsg = await guest.NextMessageAsync(systemPrompt, simulatorHistory, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[sim]     guest agent failed: {ex.Message}");
                stopwatch.Stop();
                return new RunResult(persona, RunOutcome.ErrorAborted, turns, bookingAttempted,
                    confirmationVerbatim, stopwatch.Elapsed, ex.Message);
            }

            if (string.IsNullOrWhiteSpace(nextGuestMsg))
            {
                Console.WriteLine("[sim]     guest agent returned empty — stopping");
                stopwatch.Stop();
                return new RunResult(persona, RunOutcome.ErrorAborted, turns, bookingAttempted,
                    confirmationVerbatim, stopwatch.Elapsed, "guest agent returned empty");
            }

            Console.WriteLine($"[guest]   {nextGuestMsg}");
            backendHistory.Add(new ChatMessage { Role = "user", Content = nextGuestMsg });
            simulatorHistory.Add(new ChatMessage { Role = "assistant", Content = nextGuestMsg });
            turns++;

            if (turn >= 2 && IsFarewell(nextGuestMsg))
            {
                if (noAvailabilitySeen && !bookingDone)
                {
                    Console.WriteLine("[sim]     farewell after no-availability — clean exit");
                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.NoAvailabilityExit, turns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed,
                        "backend reported no tables available; persona accepted gracefully");
                }

                if (persona.WantsToCancel)
                {
                    Console.WriteLine("[sim]     premature farewell — cancel never confirmed, marking error");
                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.ErrorAborted, turns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed,
                        "persona said farewell before cancellation was confirmed");
                }

                if (!bookingDone)
                {
                    Console.WriteLine("[sim]     premature farewell — booking never confirmed, marking error");
                    stopwatch.Stop();
                    return new RunResult(persona, RunOutcome.ErrorAborted, turns, bookingAttempted,
                        confirmationVerbatim, stopwatch.Elapsed,
                        "persona said farewell before booking was confirmed");
                }

                Console.WriteLine("[sim]     farewell exit — done");
                stopwatch.Stop();
                return new RunResult(persona, RunOutcome.FarewellExit, turns, bookingAttempted,
                    confirmationVerbatim, stopwatch.Elapsed, null);
            }
        }

        Console.WriteLine($"[sim]     hit max turns ({maxTurns}) — stopping");
        stopwatch.Stop();
        return new RunResult(persona, RunOutcome.MaxTurnsHit, turns, bookingAttempted,
            confirmationVerbatim, stopwatch.Elapsed, null);
    }

    private static bool ContainsWholeWord(string text, string word)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(word)) return false;
        return Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);
    }

    private static bool IsBookingComplete(string reply) =>
        reply.Contains("Reservation confirmed", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("reservation has been confirmed", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("reservation is confirmed", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("booking confirmed", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("booking has been confirmed", StringComparison.OrdinalIgnoreCase);

    private static bool IsConfirmationVerbatim(string reply) =>
        reply.Contains("Reservation confirmed for ", StringComparison.OrdinalIgnoreCase) &&
        reply.Contains(" at table ", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeBookingAttempt(string reply) =>
        reply.Contains("create_reservation", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("proceed with creating", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("Reservation confirmed", StringComparison.OrdinalIgnoreCase);

    private static bool IsCancellationComplete(string reply) =>
        reply.Contains("successfully cancelled", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("has been cancelled", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("now free", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("cancellation successful", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("cancellation confirmed", StringComparison.OrdinalIgnoreCase) ||
        reply.Contains("cancellation complete", StringComparison.OrdinalIgnoreCase);

    private static readonly string[] NoAvailabilityPhrases =
    {
        "no tables available",
        "no available tables",
        "no tables for",
        "no availability",
        "fully booked",
        "are unavailable",
        "don't have any tables",
        "do not have any tables",
        "don't have a table",
        "do not have a table"
    };

    private static bool IsNoAvailability(string reply)
    {
        foreach (var phrase in NoAvailabilityPhrases)
        {
            if (reply.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static readonly string[] FarewellKeywords =
    {
        "bye", "goodbye", "good night", "take care", "see you", "see ya",
        "have a great", "have a good", "have a wonderful", "that's all", "that is all"
    };

    private static bool IsFarewell(string message)
    {
        var lower = message.Trim().ToLowerInvariant();
        if (lower.Length > 60) return false;
        if (lower.Contains('?')) return false;

        foreach (var keyword in FarewellKeywords)
        {
            if (lower.Contains(keyword)) return true;
        }

        if (lower is "thanks" or "thanks!" or "thank you" or "thank you!" or "ok thanks" or "okay thanks")
            return true;

        return false;
    }

    private static string BuildOpener(GuestPersona persona) =>
        $"Hi, I'd like to reserve a table for {persona.PartySize} {persona.TimePhrase}.";

    private static string BuildPersonaPrompt(GuestPersona persona)
    {
        var goal = persona.WantsToCancel
            ? "Once the assistant confirms your reservation, change your mind: tell them you'd like to cancel because you can't make it in time. Follow through until the cancellation is confirmed, then end the conversation with a brief \"thanks, bye\"."
            : "Once your reservation is confirmed, end the conversation with a brief \"thanks, bye\" — do not keep replying.";

        return
            "You are a guest chatting with the NEmaris restaurant's AI booking assistant. " +
            "Roleplay as a real customer using SMS or chat.\n\n" +
            "WHO YOU ARE TALKING TO:\n" +
            "- The assistant is a CHATBOT. It has no name, no identity, no personal details. Never ask it \"what's your name\" " +
            "or anything about itself. Treat it as a faceless booking system.\n\n" +
            "YOUR DETAILS (share only when the assistant asks):\n" +
            $"- First name: {persona.FirstName}\n" +
            $"- Last name: {persona.LastName}\n" +
            $"- Phone number: {persona.Phone}\n" +
            $"- Party size: {persona.PartySize}\n" +
            $"- Desired arrival: {persona.TimePhrase}\n\n" +
            "STYLE:\n" +
            "- Reply with AT MOST ONE short sentence. No lists, no multiple questions.\n" +
            "- Do NOT volunteer all your details upfront. Answer questions as they come.\n" +
            $"- When the assistant asks for YOUR name, give YOUR name: \"{persona.FirstName} {persona.LastName}\".\n" +
            $"- When the assistant asks for YOUR phone, give YOUR phone exactly as: \"{persona.Phone}\".\n" +
            $"- When the assistant asks for the time, ALWAYS answer with YOUR desired arrival exactly: \"{persona.TimePhrase}\". " +
            "IGNORE any example times the assistant offers (\"tonight at 8pm\", \"tomorrow at 7pm\", etc.) — those are suggestions, " +
            $"not your time. Your time is \"{persona.TimePhrase}\" and nothing else.\n" +
            "- If the assistant lists available tables, accept the first one.\n" +
            "- If the assistant offers a SPECIFIC table by name (e.g. \"F2-T5 is available, would you like to proceed?\"), say yes and confirm — that's an offer, not a refusal.\n" +
            "- Only treat it as no-availability when the assistant explicitly says \"no tables available\", \"fully booked\", or similar. A single-table offer is NOT a refusal.\n" +
            "- If the assistant truly says no tables are available, accept it gracefully — say \"Okay, thanks anyway\" and end the conversation. Do NOT keep insisting on the same time.\n" +
            "- Do NOT break character. Do NOT mention being an AI, simulator, or that you're roleplaying.\n" +
            "- Do NOT write speaker prefixes like \"Guest:\" or \"Me:\". Reply with ONLY your next message.\n\n" +
            $"GOAL: {goal}\n\n/no_think";
    }
}
