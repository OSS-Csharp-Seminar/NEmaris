using Microsoft.Extensions.Options;
using NEmaris.Application.Configuration;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services;

public class ChatService : IChatService
{
    private static string BuildSystemPrompt(DateTime nowUtc) =>
        $"You are the reservations assistant for the NEmaris restaurant. " +
        "Help guests check availability, create, find, update, and cancel reservations using the provided tools.\n\n" +
        $"Today's date is {nowUtc:yyyy-MM-dd} (UTC). Use it to resolve relative dates like \"tomorrow\" or \"next Friday\". " +
        "Never guess the date — if you are unsure, use this one.\n\n" +
        "Required fields before calling create_reservation:\n" +
        "- firstName, lastName, phone, partySize, tableNumber, startTime, durationMinutes (default 90)\n" +
        "- If ANY of these is missing or unclear, ASK the guest. Do not call the tool with empty, made-up, or " +
        "placeholder values like \"unknown\", \"N/A\", \"none\", \"null\", \"<nil>\", or repeated punctuation. " +
        "If the guest gives only a full name, split it into firstName and lastName yourself; if they give only one name, ask for the other.\n\n" +
        "Tool usage:\n" +
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
        "- Refer to tables by tableNumber (e.g. \"T2\"), never by any internal id. " +
        "Refer to reservations by table + date/time. NEVER mention numeric ids or reservation numbers. " +
        "If asked for a reservation number, say we identify reservations by phone and time.\n" +
        "- Do not promise things outside this system — no confirmation emails, SMS, phone calls, or reminders. " +
        "Only confirm what a tool actually did.\n" +
        "- Be concise and friendly.";

    private readonly IOllamaClient _ollama;
    private readonly IEnumerable<IChatTool> _tools;
    private readonly OllamaOptions _options;

    public ChatService(
        IOllamaClient ollama,
        IEnumerable<IChatTool> tools,
        IOptions<OllamaOptions> options)
    {
        _ollama = ollama;
        _tools = tools;
        _options = options.Value;
    }

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken)
    {
        var toolsByName = _tools.ToDictionary(t => t.Name, StringComparer.Ordinal);
        var toolDefinitions = _tools.Select(t => new OllamaToolDefinition
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.ParameterSchema
        }).ToList();

        var messages = new List<OllamaChatMessage>
        {
            new() { Role = "system", Content = BuildSystemPrompt(DateTime.UtcNow) }
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
}
