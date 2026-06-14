using Microsoft.Extensions.Caching.Memory;

namespace NEmaris.Application.Services;

public interface IConversationStateStore
{
    ConversationState GetOrCreate(string sessionId);
    void Save(string sessionId, ConversationState state);
}

public class ConversationState
{
    public string? LastResolvedStartTimeUtc { get; set; }
}

public class ConversationStateStore : IConversationStateStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(2);

    private readonly IMemoryCache _cache;

    public ConversationStateStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public ConversationState GetOrCreate(string sessionId)
    {
        return _cache.GetOrCreate(BuildKey(sessionId), entry =>
        {
            entry.SlidingExpiration = Ttl;
            return new ConversationState();
        })!;
    }

    public void Save(string sessionId, ConversationState state)
    {
        _cache.Set(BuildKey(sessionId), state, new MemoryCacheEntryOptions
        {
            SlidingExpiration = Ttl
        });
    }

    private static string BuildKey(string sessionId) => $"chat:{sessionId}";
}
