using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DbBackup.Core;
using Microsoft.Extensions.Options;
using OpenAI; // For OpenAIClient (base class/client setup)
using OpenAI.Chat; // For Message, Role, ChatRequest, ChatResponse
 // For ChatEndpoint (which you've confirmed you need)

using DbBackup.WebApi.Options; // Your custom OpenAiOptions
using Microsoft.Extensions.Logging;

namespace DbBackup.WebApi.Services
{
    public interface IAiExplanationService
    {
        Task<string> ExplainLogAsync(string runId, CancellationToken ct);
    }

    public class AiExplanationService : IAiExplanationService
    {
        private readonly IStorageProvider _store;
        private readonly OpenAIClient    _api;
        private readonly OpenAiOptions    _opt;
        private readonly ILogger<AiExplanationService> _logger;
        private readonly ChatEndpoint _chatEndpoint;

       // In your AiExplanationService constructor:
public AiExplanationService(
    IOptions<OpenAiOptions> cfg,
    IStorageProvider store,
    ILogger<AiExplanationService> logger)
{
    _opt = cfg.Value;
    _store = store;
    _logger = logger;

    // --- MODIFIED LINE HERE ---
    // Try passing the API key string directly.
    _api = new OpenAIClient(_opt.ApiKey);
    // --- END MODIFIED LINE ---

    _chatEndpoint = new ChatEndpoint(_api);
}

       public async Task<string> ExplainLogAsync(string runId, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(runId))
            {
                _logger.LogWarning("Run ID cannot be null or empty for AI explanation.");
                throw new ArgumentException("Run ID cannot be null or empty");
            }

            await using var stream = await _store.ReadLogAsync(runId, ct);
            using var reader = new StreamReader(stream);
            var logContent = await reader.ReadToEndAsync();

            var lines = logContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                  .TakeLast(200);
            var formattedLog = string.Join(Environment.NewLine, lines);

            if (string.IsNullOrWhiteSpace(formattedLog))
            {
                _logger.LogWarning("Log content for run ID {RunId} is empty or too short to analyze.", runId);
                return "No sufficient log content to analyze.";
            }

            // Create the list of messages for the ChatRequest
            var messages = new List<Message>
            {
                new Message(Role.System, "You are a database backup analysis assistant. Your task is to analyze database backup logs and provide clear, concise explanations of what happened during the backup process. Focus on any errors or warnings that occurred and provide suggestions for improvement."),
                new Message(Role.User, $"Please analyze this database backup log and provide a clear explanation of what happened:\n\n{formattedLog}")
            };

            // --- MODIFIED ChatRequest CREATION ---
            // Based on previous errors, ChatRequest likely takes all these as constructor arguments.
            // The exact order might vary, but this is a common pattern for immutable objects.
            // You might need to confirm the constructor signature with IntelliSense.
            var request = new ChatRequest(
                messages,    // The list of messages
                _opt.Model,  // The model name
                0.7f         // The temperature
                // If there are other required constructor parameters, add them here.
            );
            // --- END MODIFIED ChatRequest CREATION ---


            var maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // --- MODIFIED API CALL HERE ---
                    // Call GetCompletionAsync on the _chatEndpoint instance.
                    var response = await _chatEndpoint.GetCompletionAsync(request, ct);

                    if (response?.Choices is not null && response.Choices.Count > 0)
                    {
                        return response.Choices[0].Message.Content;
                    }
                    else
                    {
                        _logger.LogWarning("AI explanation response for run ID {RunId} was null or had no choices. Attempt {Attempt}/{MaxRetries}", runId, i + 1, maxRetries);
                    }
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning(ex, "Retrying AI explanation request for run ID {RunId} (attempt {Attempt}/{MaxRetries})", runId, i + 1, maxRetries);
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), ct);
                }
            }

            _logger.LogError("Failed to get AI explanation for run ID {RunId} after multiple retries.", runId);
            throw new Exception("Failed to get AI explanation after multiple retries");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI explanation for run ID {RunId}", runId);
            throw;
        }
    }
    }
}