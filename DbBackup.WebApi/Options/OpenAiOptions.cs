namespace DbBackup.WebApi.Options;

public class OpenAiOptions
{
    public string ApiKey    { get; init; } = "";
    public string Model     { get; init; } = "gpt-3.5-turbo";
    public int    MaxTokens { get; init; } = 512;
}