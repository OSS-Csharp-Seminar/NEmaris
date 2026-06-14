using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NEmaris.Application.Configuration;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services;

public class ChatService : IChatService
{
    private static string BuildSystemPrompt(DateTime nowUtc, TimeZoneInfo guestTimeZone)
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, guestTimeZone);
        var offset = guestTimeZone.GetUtcOffset(nowUtc);
        var offsetLabel = (offset >= TimeSpan.Zero ? "+" : "-") + offset.ToString(@"hh\:mm");
        var isUtc = guestTimeZone.Equals(TimeZoneInfo.Utc);

        var nowUtcIso = nowUtc.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
        var inFiveUtcIso = nowUtc.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ss") + "Z";

        var timeContext = isUtc
            ? $"The current date and time is {nowUtcIso} (UTC). " +
              "The guest's timezone was not supplied, so treat all clock times as UTC. "
            : $"The current time is {nowLocal:yyyy-MM-dd HH:mm} in the guest's timezone " +
              $"({guestTimeZone.Id}, offset {offsetLabel}), which is {nowUtcIso} in UTC. " +
              "When the guest gives a clock time like \"17:00\", \"7pm\", or \"tonight at 8\", interpret it as " +
              $"their LOCAL time and convert it to UTC by subtracting the offset ({offsetLabel}) before passing " +
              "startTime to any tool. Example: guest says \"17:00 tomorrow\" with offset +02:00 → " +
              "startTime = (tomorrow's date)T15:00:00Z. Tools always expect UTC. ";

        return
        $"You are the reservations assistant for the NEmaris restaurant. " +
        "Help guests check availability, create, find, update, and cancel reservations using the provided tools.\n\n" +
        timeContext +
        "\n\nFIRST TURN — PARSE THE GUEST'S OPENER FOR FIELDS THEY ALREADY GAVE.\n" +
        "Before responding, scan the guest's message for: partySize, time phrase, firstName, lastName, phone. " +
        "A time phrase ALWAYS counts as a provided startTime — these phrases ARE the time, even when terse:\n" +
        "  • \"now\" / \"right now\"\n" +
        "  • \"in N minutes\" / \"in N hours\" — any integer N. \"in 5 minutes\", \"in 30 minutes\", \"in 90 minutes\", \"in 2 hours\", etc.\n" +
        "  • \"in half an hour\" / \"in an hour\" / \"in a few minutes\"\n" +
        "  • Hedged variants with \"about\", \"around\", \"roughly\" — treat as exact\n" +
        "  • \"tonight at 8pm\" / \"tomorrow at 7pm\" / \"next Friday at 19:00\"\n" +
        "If the guest's first message contains a phrase from this list, your FIRST action MUST be a single call to " +
        "resolve_time. Do NOT write \"I'd be happy to help\". Do NOT write \"To get started, I need to know\". " +
        "Do NOT enumerate what you need. Do NOT ask \"what time?\" or \"when would you like?\". Do NOT respond with prose. " +
        "The time IS in the guest's message — call the tool. Treat the absence of a tool call as a bug.\n\n" +
        "WORKED EXAMPLE.\n" +
        "  Guest: \"Hi, I'd like to reserve a table for 4 in 90 minutes.\"\n" +
        "  Correct first action: tool call resolve_time(offsetMinutes=90). No prose. No \"I'd be happy to help\".\n" +
        "  Incorrect: any text reply asking for the time, listing example times, or saying \"I need to know\".\n\n" +
        "Resolving times — DO NOT DO THE TIME ARITHMETIC YOURSELF. Use the resolve_time tool instead.\n" +
        "Whenever the guest gives any time phrase (relative or absolute), call resolve_time FIRST. It returns a " +
        "field called startTimeUtc — copy that string VERBATIM into get_available_tables, create_reservation, " +
        "or any other tool that needs startTime. Do not recompute the UTC value, do not adjust it, do not pass a " +
        "different value to different tools in the same conversation. The same startTimeUtc must appear in every " +
        "tool call that references that booking.\n" +
        "How to call resolve_time:\n" +
        "- Relative phrases (\"now\", \"in 5 minutes\", \"in 90 minutes\", \"in an hour\", \"in 2 hours\", " +
        "\"in half an hour\", \"in a few minutes\", with hedges like \"about\"/\"around\"/\"roughly\"): pass " +
        "offsetMinutes as an integer. \"now\" = 0, \"in 5 minutes\" = 5, \"in half an hour\" = 30, \"in an hour\" " +
        "= 60, \"in 90 minutes\" = 90, \"in 2 hours\" = 120. Treat hedges as exact.\n" +
        "- Absolute clock phrases (\"tonight at 8pm\", \"tomorrow at 7pm\", \"next Friday at 19:00\", \"today at " +
        "noon\"): pass localClock = {day, hour, minute}. day is \"today\", \"tomorrow\", or a weekday name. " +
        "hour is 0-23 (8pm=20, noon=12, midnight=0). minute defaults to 0.\n" +
        "These phrases are COMPLETE — do NOT ask the guest \"what date?\", \"could you confirm the time?\", or " +
        "echo the time back as a question. Call resolve_time, then proceed.\n" +
        "If the guest gives ONLY a clock time with NO day reference (\"7pm\" alone), ask whether they mean today or tomorrow.\n\n" +
        "Act, don't confirm — when you have everything you need to call a tool (partySize + startTime for availability; " +
        "all required fields for create_reservation), CALL THE TOOL in this turn. Do not write a message that " +
        "rephrases what the guest said, asks them to \"please confirm\", or lists the values back for verification " +
        "before acting. Verification text is reserved for AFTER a tool returns — e.g. the create_reservation confirmation. " +
        "The only valid reasons to send a text-only reply are: (a) a required field is missing, or (b) a tool just returned and you are relaying its result to the guest.\n\n" +
        "Reservation conversation order — follow these steps in this exact sequence. Do NOT skip ahead and " +
        "do NOT call any tool until the step's requirement is met:\n" +
        "1. Ask for partySize ONLY if the guest's messages contain no party size. A number like \"4\" or \"for 6 people\" " +
        "counts as provided.\n" +
        "2. Ask for the desired startTime ONLY if the guest's messages contain NO time phrase at all. \"In 5 minutes\", " +
        "\"in 90 minutes\", \"in 2 hours\", \"tonight at 8\", \"tomorrow at 7pm\", \"now\" — every one of these is a " +
        "PROVIDED startTime. If the guest said any of them, do NOT ask \"for when?\" — call resolve_time directly. " +
        "Never invent a default time, but also never re-ask for a time the guest already gave.\n" +
        "3. Call get_available_tables with partySize and startTime, then show the list.\n" +
        "4. Let the guest pick a tableNumber from the list.\n" +
        "5. Ask for firstName, lastName, and phone if any are missing.\n" +
        "6. Call create_reservation. Only after it returns a confirmation may you tell the guest the booking is done.\n\n" +
        "Required fields before calling create_reservation:\n" +
        "- firstName, lastName, phone, partySize, tableNumber, startTime, durationMinutes (default 90)\n" +
        "- You may ONLY use firstName, lastName, and phone values that the guest has typed in THEIR OWN messages " +
        "in this conversation. If the guest has not explicitly given a name or phone, you MUST ask them. " +
        "Do not invent, guess, autocomplete, or fall back to generic example values. " +
        "Specifically forbidden: \"John\", \"Jane\", \"Doe\", \"Smith\", \"John Doe\", \"Jane Doe\", " +
        "\"John Smith\", \"Test\", \"Anonymous\", \"Guest\", \"Customer\", and any phone like " +
        "\"(123) 456-7890\", \"123-456-7890\", \"1234567890\", \"555-1234\", \"555-555-5555\", " +
        "or anything that is not a real number the guest typed.\n" +
        "- Also forbidden: empty values, \"unknown\", \"N/A\", \"none\", \"null\", \"<nil>\", or repeated punctuation. " +
        "If the guest gives only a full name, split it into firstName and lastName yourself; if they give only one name, ask for the other.\n" +
        "- Ask for the missing fields ONE OR TWO AT A TIME in plain language. Do not write a confirmation, summary, " +
        "or \"Confirming: ...\" line until the guest has supplied real values for firstName, lastName, AND phone in their messages.\n" +
        "- If create_reservation returns an error mentioning \"Already collected:\", read those values and re-call " +
        "create_reservation with the SAME values for every field EXCEPT the one the guest just replaced. Never reset " +
        "fields the guest already provided.\n\n" +
        "Tool usage:\n" +
        "- BEFORE calling get_available_tables, you MUST know the guest's desired startTime. A relative phrase like " +
        "\"now\", \"in 5 minutes\", \"in an hour\", \"tonight at 8\", \"tomorrow at 7pm\" already gives you everything " +
        "you need — resolve it against the clock above and call the tool. Do NOT ask the guest for the date when they " +
        "have already given a relative phrase. Only ask if the guest has not stated any time at all. Do not invent a " +
        "startTime such as \"tonight\" or \"19:00\" if the guest did not say it. Party size is required too — ask if it is missing.\n" +
        "- ALWAYS call get_available_tables before create_reservation, then pass a tableNumber from its result.\n" +
        "- When update_reservation changes time or table, call get_available_tables first to confirm the new slot is free.\n" +
        "- If get_available_tables returns available=true, tell the guest YES; if false, tell them NO. Never contradict the tool.\n" +
        "- find_my_reservations requires BOTH phone and last name. If count=0, say you couldn't find a matching reservation — " +
        "do NOT speculate about which detail was wrong, and do NOT echo the phone or name back.\n" +
        "- If find_my_reservations returns a reservation with status=\"Seated\", the guests are already at the table. " +
        "Do NOT call cancel_reservation or update_reservation for it. Tell the guest their party is already seated " +
        "and ask them to flag down their server for changes. Do not apologize repeatedly — just explain once.\n" +
        "- For cancellations and updates the guest MUST provide their phone. BEFORE calling cancel_reservation or " +
        "update_reservation, you MUST first call find_my_reservations (in this turn, or use the result from a prior " +
        "turn in this same conversation) and pass the EXACT startTime value that find_my_reservations returned. " +
        "Do NOT resolve the relative phrase yourself, do NOT pull the time from your earlier confirmation message, " +
        "and do NOT use a wall-clock minute like \"19:28\" — the stored time has seconds and must be copied verbatim " +
        "from the find result. If you have not yet called find_my_reservations in this conversation, call it FIRST " +
        "and only then call cancel_reservation or update_reservation.\n" +
        "- HARD RULE — must call a tool: when the guest asks to cancel, update, find, or look up a reservation, " +
        "you MUST call a tool in this turn before replying. NEVER write \"I couldn't find\", \"I cannot cancel\", " +
        "\"the reservation doesn't exist\", \"there is no record\", or any similar denial from text alone. Those " +
        "statements are ONLY valid AFTER find_my_reservations returned count=0 in this turn OR cancel_reservation " +
        "threw not_found in this turn. Until a tool has actually run in the CURRENT turn, you have no authority to " +
        "claim a reservation is missing — call the tool first, then reply based on its real result.\n" +
        "- HARD RULE — refuse past times: the clock at the top of this prompt is authoritative. If the startTime " +
        "you would resolve for the guest is earlier than that clock (i.e. already in the past), do NOT call " +
        "get_available_tables and do NOT call create_reservation. Instead, tell the guest that time has already " +
        "passed and ask if they meant the same time tomorrow (or some future time). Only proceed once the guest " +
        "confirms a startTime that is strictly in the future.\n" +
        "- Time arguments are full ISO 8601 datetimes like \"2026-05-10T19:00:00\". " +
        "Prefer durationMinutes (e.g. 90) over endTime; never pass a bare time like \"19:00\".\n\n" +
        "Talking to the guest:\n" +
        "- After a tool call, base your reply ONLY on what the tool returned. If the tool returned tableNumber=\"T2\" and startTime " +
        "ending in 19:00, say \"T2\" and \"7pm\" — never substitute a different table or time.\n" +
        "- When create_reservation returns a \"confirmation\" field, your reply MUST contain that confirmation string " +
        "verbatim. Do not paraphrase the date, times, table, or party size — copy the confirmation sentence as-is. " +
        "You may add a brief friendly closing after it, but never alter the confirmation itself.\n" +
        "- NEVER claim you have reserved, booked, confirmed, cancelled, updated, or saved anything unless you " +
        "actually called the corresponding tool (create_reservation, cancel_reservation, update_reservation) in " +
        "this turn AND it returned a success result. Do not write phrases like \"I've reserved\", \"booked\", " +
        "\"your reservation is confirmed\", \"all set\", \"I'll cancel it for you now\", \"I'm going to cancel it\", " +
        "or \"the cancellation has gone through\" unless the tool has already run in THIS turn and returned. " +
        "Announcing intent (\"I'll cancel for you\") is forbidden — call the tool first, then report what it " +
        "returned. If the tool hasn't returned yet, you have nothing to announce.\n" +
        "- Refer to tables by tableNumber (e.g. \"T2\"), never by any internal id. " +
        "Refer to reservations by table + date/time. NEVER mention numeric ids or reservation numbers. " +
        "If asked for a reservation number, say we identify reservations by phone and time.\n" +
        "- Do not promise things outside this system — no confirmation emails, SMS, phone calls, or reminders. " +
        "Only confirm what a tool actually did.\n" +
        "- Never promise to check, look up, or do something \"in a moment\". If you need fresh data, call the " +
        "tool in the SAME turn and answer from its result. Do not produce filler like \"let me check\", " +
        "\"one moment\", or \"I'll get back to you\".\n" +
        "- Be concise and friendly.\n\n/no_think";
    }

    private readonly IOllamaClient _ollama;
    private readonly IEnumerable<IChatTool> _tools;
    private readonly OllamaOptions _options;
    private readonly IRequestTimeZoneContext _timeZoneContext;
    private readonly IConversationStateStore _stateStore;

    public ChatService(
        IOllamaClient ollama,
        IEnumerable<IChatTool> tools,
        IOptions<OllamaOptions> options,
        IRequestTimeZoneContext timeZoneContext,
        IConversationStateStore stateStore)
    {
        _ollama = ollama;
        _tools = tools;
        _options = options.Value;
        _timeZoneContext = timeZoneContext;
        _stateStore = stateStore;
    }

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken)
    {
        _timeZoneContext.Set(request.TimeZone);

        var toolsByName = _tools.ToDictionary(t => t.Name, StringComparer.Ordinal);
        var toolDefinitions = _tools.Select(t => new OllamaToolDefinition
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.ParameterSchema
        }).ToList();

        var messages = new List<OllamaChatMessage>
        {
            new() { Role = "system", Content = BuildSystemPrompt(DateTime.UtcNow, _timeZoneContext.TimeZone) }
        };

        foreach (var msg in request.Messages)
        {
            messages.Add(new OllamaChatMessage
            {
                Role = msg.Role,
                Content = msg.Content
            });
        }

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId) ? null : request.SessionId.Trim();
        var sessionState = sessionId is not null ? _stateStore.GetOrCreate(sessionId) : new ConversationState();
        var lastResolvedStartTimeUtc = sessionState.LastResolvedStartTimeUtc;
        var toolNamesCalled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var phonesProvided = ExtractPhonesProvided(request.Messages);
        var wordsProvided = ExtractWordsProvided(request.Messages);
        var reservationsChanged = false;

        for (var iteration = 0; iteration < _options.MaxToolIterations; iteration++)
        {
            var assistant = await _ollama.ChatAsync(messages, toolDefinitions, cancellationToken);
            assistant.Content = StripThinking(assistant.Content);

            if (assistant.ToolCalls is null || assistant.ToolCalls.Count == 0)
                return new ChatResponseDto
                {
                    Reply = GuardHallucinatedSuccess(assistant.Content, toolNamesCalled),
                    ReservationsChanged = reservationsChanged
                };

            messages.Add(assistant);

            foreach (var call in assistant.ToolCalls)
            {
                toolNamesCalled.Add(call.Name);
                var effectiveArguments = call.Arguments;
                string? overrideNote = null;

                if (NeedsStartTime(call.Name) && lastResolvedStartTimeUtc is not null)
                {
                    var llmPassed = TryGetStringProperty(call.Arguments, "startTime");
                    if (!string.Equals(llmPassed, lastResolvedStartTimeUtc, StringComparison.Ordinal))
                    {
                        effectiveArguments = OverrideStartTime(call.Arguments, lastResolvedStartTimeUtc);
                        overrideNote = $" [override: startTime {llmPassed ?? "<missing>"} → {lastResolvedStartTimeUtc}]";
                    }
                }

                if (DiagnosticsEnabled)
                    Console.WriteLine($"[chat-diag] tool={call.Name} args={effectiveArguments.GetRawText()}{overrideNote}");

                string result;
                if (call.Name == "create_reservation" && !IsPhoneFromGuest(effectiveArguments, phonesProvided))
                {
                    var llmPhone = TryGetStringProperty(effectiveArguments, "phone") ?? "<missing>";
                    result = $"{{\"error\":\"invalid\",\"message\":\"Phone number '{llmPhone}' was not provided by the guest in this conversation. Ask the guest for their actual phone number before creating the reservation.\"}}";
                    if (DiagnosticsEnabled)
                        Console.WriteLine($"[chat-diag] guard: rejected hallucinated phone '{llmPhone}' on create_reservation");
                }
                else if (call.Name == "create_reservation" && FindHallucinatedNameField(effectiveArguments, wordsProvided) is { } badField)
                {
                    var llmName = TryGetStringProperty(effectiveArguments, badField) ?? "<missing>";
                    var humanLabel = badField == "firstName" ? "first name" : "last name";
                    result = $"{{\"error\":\"invalid\",\"message\":\"{humanLabel.Substring(0,1).ToUpper()}{humanLabel.Substring(1)} '{llmName}' was not provided by the guest in this conversation. Ask the guest for their actual {humanLabel} before creating the reservation.\"}}";
                    if (DiagnosticsEnabled)
                        Console.WriteLine($"[chat-diag] guard: rejected hallucinated {humanLabel} '{llmName}' on create_reservation");
                }
                else
                {
                    result = await ExecuteToolAsync(toolsByName, call.Name, effectiveArguments, cancellationToken);
                    if (IsMutatingTool(call.Name) && !IsErrorResult(result))
                        reservationsChanged = true;
                }

                if (DiagnosticsEnabled)
                    Console.WriteLine($"[chat-diag] tool={call.Name} result={Truncate(result, 400)}");

                if (call.Name == "resolve_time")
                {
                    var newUtc = ExtractStartTimeUtc(result);
                    if (newUtc is not null)
                    {
                        lastResolvedStartTimeUtc = newUtc;
                        sessionState.LastResolvedStartTimeUtc = newUtc;
                        if (sessionId is not null)
                            _stateStore.Save(sessionId, sessionState);
                    }
                }

                messages.Add(new OllamaChatMessage
                {
                    Role = "tool",
                    Content = result
                });
            }
        }

        return new ChatResponseDto
        {
            Reply = "I wasn't able to complete that request. Please try rephrasing or contact the restaurant directly.",
            ReservationsChanged = reservationsChanged
        };
    }

    private static bool IsMutatingTool(string toolName) =>
        toolName is "create_reservation" or "cancel_reservation" or "update_reservation";

    private static bool IsErrorResult(string resultJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(resultJson);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("error", out _);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> ExecuteToolAsync(
        IReadOnlyDictionary<string, IChatTool> toolsByName,
        string name,
        JsonElement arguments,
        CancellationToken cancellationToken)
    {
        if (!toolsByName.TryGetValue(name, out var tool))
            return $"{{\"error\":\"Unknown tool '{name}'.\"}}";

        try
        {
            return await tool.ExecuteAsync(arguments, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            return $"{{\"error\":\"not_found\",\"message\":\"{Escape(ex.Message)}\"}}";
        }
        catch (InvalidOperationException ex)
        {
            return $"{{\"error\":\"invalid\",\"message\":\"{Escape(ex.Message)}\"}}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"unexpected\",\"message\":\"{Escape(ex.Message)}\"}}";
        }
    }

    private static bool NeedsStartTime(string toolName) =>
        toolName is "get_available_tables" or "create_reservation";

    private static string? TryGetStringProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        if (!element.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }

    private static JsonElement OverrideStartTime(JsonElement original, string startTimeUtc)
    {
        var node = JsonNode.Parse(original.GetRawText()) as JsonObject ?? new JsonObject();
        node["startTime"] = startTimeUtc;
        using var doc = JsonDocument.Parse(node.ToJsonString());
        return doc.RootElement.Clone();
    }

    private static string? ExtractStartTimeUtc(string toolResultJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(toolResultJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            if (!doc.RootElement.TryGetProperty("startTimeUtc", out var prop)) return null;
            return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static readonly bool DiagnosticsEnabled =
        string.Equals(
            Environment.GetEnvironmentVariable("CHAT_DIAGNOSTICS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";

    private static readonly Regex ThinkBlock = new(@"<think>[\s\S]*?</think>", RegexOptions.Compiled);
    private static readonly Regex DanglingThinkClose = new(@"\A[\s\S]*?</think>\s*", RegexOptions.Compiled);

    private static string StripThinking(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        var stripped = ThinkBlock.Replace(content, string.Empty);
        stripped = DanglingThinkClose.Replace(stripped, string.Empty);
        return stripped.TrimStart();
    }

    private static readonly string[] CancellationSuccessPhrases =
    {
        "successfully cancelled",
        "has been cancelled",
        "cancellation successful",
        "cancellation confirmed",
        "cancellation complete",
        "now free"
    };

    private static readonly string[] BookingSuccessPhrases =
    {
        "reservation confirmed for",
        "your reservation is confirmed",
        "your booking is confirmed"
    };

    private static string GuardHallucinatedSuccess(string content, HashSet<string> toolNamesCalled)
    {
        if (string.IsNullOrEmpty(content)) return content;

        if (ContainsAny(content, CancellationSuccessPhrases) && !toolNamesCalled.Contains("cancel_reservation"))
        {
            if (DiagnosticsEnabled)
                Console.WriteLine("[chat-diag] guard: rewriting hallucinated cancellation success");
            return "I'm sorry — I wasn't able to actually cancel that reservation. Could you please confirm the reservation details and try again?";
        }

        if (ContainsAny(content, BookingSuccessPhrases) && !toolNamesCalled.Contains("create_reservation"))
        {
            if (DiagnosticsEnabled)
                Console.WriteLine("[chat-diag] guard: rewriting hallucinated booking confirmation");
            return "I'm sorry — I wasn't able to actually create that reservation. Could you please confirm the details and try again?";
        }

        return content;
    }

    private static bool ContainsAny(string content, string[] phrases)
    {
        foreach (var phrase in phrases)
        {
            if (content.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static readonly Regex PhonePattern = new(@"\+?\d[\d\s\-\(\)]{6,}\d", RegexOptions.Compiled);

    private static HashSet<string> ExtractPhonesProvided(IEnumerable<ChatMessageDto> messages)
    {
        var phones = new HashSet<string>(StringComparer.Ordinal);
        foreach (var msg in messages)
        {
            if (!string.Equals(msg.Role, "user", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrEmpty(msg.Content)) continue;
            foreach (Match m in PhonePattern.Matches(msg.Content))
            {
                var normalized = PhoneNormalizer.Normalize(m.Value);
                var digitsOnly = new string(normalized.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length >= 7)
                    phones.Add(normalized);
            }
        }
        return phones;
    }

    private static bool IsPhoneFromGuest(JsonElement args, HashSet<string> phonesProvided)
    {
        var llmPhone = TryGetStringProperty(args, "phone");
        if (string.IsNullOrWhiteSpace(llmPhone)) return true;
        var normalized = PhoneNormalizer.Normalize(llmPhone);
        if (string.IsNullOrEmpty(normalized)) return true;
        return phonesProvided.Contains(normalized);
    }

    private static readonly Regex WordPattern = new(@"[\p{L}]{2,}", RegexOptions.Compiled);

    private static HashSet<string> ExtractWordsProvided(IEnumerable<ChatMessageDto> messages)
    {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var msg in messages)
        {
            if (!string.Equals(msg.Role, "user", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrEmpty(msg.Content)) continue;
            foreach (Match m in WordPattern.Matches(msg.Content))
            {
                words.Add(m.Value);
            }
        }
        return words;
    }

    private static string? FindHallucinatedNameField(JsonElement args, HashSet<string> wordsProvided)
    {
        if (!IsNameFromGuest(TryGetStringProperty(args, "firstName"), wordsProvided))
            return "firstName";
        if (!IsNameFromGuest(TryGetStringProperty(args, "lastName"), wordsProvided))
            return "lastName";
        return null;
    }

    private static bool IsNameFromGuest(string? name, HashSet<string> wordsProvided)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        var parts = name.Split(new[] { ' ', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.Length < 2) continue;
            if (!wordsProvided.Contains(part)) return false;
        }
        return true;
    }
}
