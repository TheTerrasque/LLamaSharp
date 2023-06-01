using System.Text;
using LLama.Interfaces;
using LLama.Extensions;
using Microsoft.Extensions.Options;
using Serilog;

namespace LLama.WebAPI.Services;

public class BaseChatServiceOptions {
    public const string ChatServiceOptionsSection = "ChatService";
    public string Model { get; set; } = @"C:\temp\models\Manticore-13B-Chat-Pyg.ggmlv3.q5_1.bin";
    public int NCtx { get; set; } = 2048;
    public int Seed { get; set; } = 53453721;
    public int NGPULayers { get; set; } = 17;
    public string Encoding { get; set; } = "UTF-8";
    public string Prompt { get; set; } = "";
    public string PromptFile { get; set; } = "";
    public string[] BreakOn { get; set; } = new string[] { "User:", "user:" };
    public bool TreatEOSAsNewline { get; set; } = true;
}

public class BaseChatService
{
    private readonly BaseChatServiceOptions _options;
    private readonly ILanguageModel _model;
    Serilog.ILogger _log = Log.ForContext<BaseChatService>();

    public BaseChatService(IOptions<BaseChatServiceOptions> options)
    {
        _log.Debug("Creating BaseChatService with options {@options}", options.Value);
        _options = options.Value;
        _model = new BaseLLamaModel(new
            LLamaParams(
                model: _options.Model,
                n_ctx: _options.NCtx,
                seed: _options.Seed,
                eos_to_newline: _options.TreatEOSAsNewline,
                n_gpu_layers: _options.NGPULayers), 
            _options.Encoding);
    }

    string _formatMessages(List<Message> messages)
    {
        var log = _log.ForContext("Function", "_formatMessages");

        var sb = new StringBuilder();
        sb.Append(_options.Prompt);
        if (_options.PromptFile != "" && System.IO.File.Exists(_options.PromptFile))
        {
            log.Debug("Appending prompt file {promptFile}", _options.PromptFile);
            sb.Append(System.IO.File.ReadAllText(_options.PromptFile));
        }
        foreach (var message in messages)
        {
            if (message.Role != null && message.Role != "" && message.Role != "system")
            {
                sb.Append(message.Role.Capitalize());
                sb.Append(": ");
            }
            sb.Append(message.Content);
            sb.Append("\n");
        }
        sb.Append("Assistant: ");
        return sb.ToString();
    }

    public string ProcessRequest(List<Message> messages)
    {
        var text = "";
        foreach (var token in ProcessRequestStreamResponse(messages))
        {
            text += token;
        }
        text = text.Remove(text.Length - 5).Trim();
        return text;
    }

    public IEnumerable<string> ProcessRequestStreamResponse(List<Message> messages)
    {
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken ct = source.Token;
        var text = "";
        foreach (var entry in _model.Generate(_formatMessages(messages), ct))
        {
            if (_options.BreakOn.Any(x => text.Trim().EndsWith(x)))
            {
              source.Cancel();
              break;
            }
            text += entry;
            yield return entry;
        };
    }    
}