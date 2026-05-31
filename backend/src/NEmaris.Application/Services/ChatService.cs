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

        var timeContext = isUtc
            ? $"The current date and time is {nowUtc:yyyy-MM-ddTHH:mm:ss}Z (UTC). " +
              "The guest's timezone was not supplied, so treat all clock times as UTC. "
            : $"The current time is {nowLocal:yyyy-MM-dd HH:mm} in the guest's timezone " +
              $"({guestTimeZone.Id}, offset {offsetLabel}), which is {nowUtc:yyyy-MM-ddTHH:mm:ss}Z in UTC. " +
              "When the guest gives a clock time like \"17:00\", \"7pm\", or \"tonight at 8\", interpret it as " +
              $"their LOCAL time and convert it to UTC by subtracting the offset ({offsetLabel}) before passing " +
              "startTime to any tool. Example: guest says \"17:00 tomorrow\" with offset +02:00 → " +
              "startTime = (tomorrow's date)T15:00:00Z. Tools always expect UTC. ";

        return
        $"You are the reservations assistant for the NEmaris restaurant. " +
        "Help guests check availability, create, find, update, and cancel reservations using the provided tools.\n\n" +
        timeContext +
        "Use this clock to resolve relative dates and times like \"tomorrow\", \"next Friday\", \"now\", \"in 5 minutes\", or \"tonight\". " +
        "Never guess the date or time — if you are unsure, use this one.\n\n" +
        "Reservation conversation order — follow these steps in this exact sequence. Do NOT skip ahead and " +
        "do NOT call any tool until the step's requirement is met:\n" +
        "1. Ask for partySize if missing.\n" +
        "2. Ask for the desired startTime (\"for when?\") if missing — you MUST have an explicit guest-provided time " +
        "before step 3. Never invent a default time.\n" +
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
        "- BEFORE calling get_available_tables, you MUST know the guest's desired startTime — either as an absolute " +
        "time (\"tonight 7pm\", \"tomorrow at noon\") or a relative one (\"now\", \"in 10 minutes\"). If the guest has " +
        "not stated a time, ASK them first. Do not invent a startTime such as \"tonight\" or \"19:00\" if the guest " +
        "did not say it. Party size is required too — ask if it is missing.\n" +
        "- ALWAYS call get_available_tables before create_reservation, then pass a tableNumber from its result.\n" +
        "- When update_reservation changes time or table, call get_available_tables first to confirm the new slot is free.\n" +
        "- If get_available_tables returns available=true, tell the guest YES; if false, tell them NO. Never contradict the tool.\n" +
        "- find_my_reservations requires BOTH phone and last name. If count=0, say you couldn't find a matching reservation — " +
        "do NOT speculate about which detail was wrong, and do NOT echo the phone or name back.\n" +
        "- For cancellations and updates the guest MUST provide their phone. Identify the reservation by phone PLUS the " +
        "current startTime (taken from find_my_reservations).\n" +
        "- Time arguments are full ISO 8601 datetimes like \"2026-05-10T19:00:00\". " +
        "Prefer durationMinutes (e.g. 90) over endTime; never pass a bare time like \"19:00\".\n\n" +
        "Talking to the guest:\n" +
        "- After a tool call, base your reply ONLY on what the tool returned. If the tool returned tableNumber=\"T2\" and startTime " +
        "ending in 19:00, say \"T2\" and \"7pm\" — never substitute a different table or time.\n" +
        "- When create_reservation returns a \"confirmation\" field, your reply MUST contain that confirmation string " +
        "verbatim. Do not paraphrase the date, times, table, or party size — copy the confirmation sentence as-is. " +
        "You may add a brief friendly closing after it, but never alter the confirmation itself.\n" +
        "- NEVER claim you have reserved, booked, confirmed, or saved anything unless you actually called " +
        "create_reservation in this turn AND it returned a confirmation field. If you have not called the tool, " +
        "do not write phrases like \"I've reserved\", \"booked\", \"your reservation is confirmed\", or \"all set\". " +
        "Instead, say what is still missing and ask the guest for it.\n" +
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

    public ChatService(
        IOllamaClient ollama,
        IEnumerable<IChatTool> tools,
        IOptions<OllamaOptions> options,
        IRequestTimeZoneContext timeZoneContext)
    {
        _ollama = ollama;
        _tools = tools;
        _options = options.Value;
        _timeZoneContext = timeZoneContext;
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

        for (var iteration = 0; iteration < _options.MaxToolIterations; iteration++)
        {
            var assistant = await _ollama.ChatAsync(messages, toolDefinitions, cancellationToken);
            assistant.Content = StripThinking(assistant.Content);

            if (assistant.ToolCalls is null || assistant.ToolCalls.Count == 0)
                return new ChatResponseDto { Reply = assistant.Content };

            messages.Add(assistant);

            foreach (var call in assistant.ToolCalls)
            {
                var result = await ExecuteToolAsync(toolsByName, call, cancellationToken);
                messages.Add(new OllamaChatMessage
                {
                    Role = "tool",
                    Content = result
                });
            }
        }

        return new ChatResponseDto
        {
            Reply = "I wasn't able to complete that request. Please try rephrasing or contact the restaurant directly."
        };
    }

    private static async Task<string> ExecuteToolAsync(
        IReadOnlyDictionary<string, IChatTool> toolsByName,
        OllamaToolCall call,
        CancellationToken cancellationToken)
    {
        if (!toolsByName.TryGetValue(call.Name, out var tool))
            return $"{{\"error\":\"Unknown tool '{call.Name}'.\"}}";

        try
        {
            return await tool.ExecuteAsync(call.Arguments, cancellationToken);
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

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static readonly Regex ThinkBlock = new(@"<think>[\s\S]*?</think>", RegexOptions.Compiled);

    private static string StripThinking(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        return ThinkBlock.Replace(content, string.Empty).TrimStart();
    }
}
