using System.Text;
using LLama.Interfaces;
using LLama.Extensions;
using Microsoft.Extensions.Options;

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
}

public class BaseChatService
{
    private readonly BaseChatServiceOptions _options;
    private readonly ILanguageModel _model;

    public BaseChatService(IOptions<BaseChatServiceOptions> options)
    {
        _options = options.Value;
        _model = new BaseLLamaModel(new
            LLamaParams(
                model: _options.Model,
                n_ctx: _options.NCtx,
                seed: _options.Seed,
                n_gpu_layers: _options.NGPULayers), 
            _options.Encoding);
    }

    string _formatMessages(List<Message> messages)
    {
        var sb = new StringBuilder();
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