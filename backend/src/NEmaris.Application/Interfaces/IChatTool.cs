using System.Text.Json;

namespace NEmaris.Application.Interfaces;

public interface IChatTool
{
    string Name { get; }
    string Description { get; }
    object ParameterSchema { get; }
    Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken);
}
