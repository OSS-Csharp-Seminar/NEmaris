namespace NEmaris.Application.Configuration;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
    public int MaxToolIterations { get; set; } = 6;
    public double Temperature { get; set; } = 0.2;
}
