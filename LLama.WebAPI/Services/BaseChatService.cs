using System.Text;
using LLama.Interfaces;
using LLama.Extensions;
using LLama.Models;
using Microsoft.Extensions.Options;
using Serilog;
using HandlebarsDotNet;

namespace LLama.WebAPI.Services;

public class BaseChatServiceOptions {
    public const string ChatServiceOptionsSection = "ChatService";
    public string Model { get; set; } = @"Asets\Models\model.bin";
    public int NCtx { get; set; } = 2048;
    public int Seed { get; set; } = -1;
    public int NGPULayers { get; set; } = 0;
    public string Encoding { get; set; } = "UTF-8";
    public string Prompt { get; set; } = "";
    public string PromptFile { get; set; } = "";

    /// <summary>
    /// Handlebars template for system messages
    /// </summary>
    public string SystemFormat { get; set; } = "{{#capitalize user}}: {{message}}\n";
    /// <summary>
    /// Handlebars template for user messages
    /// </summary>
    public string UserFormat { get; set; } = "{{#capitalize user}}: {{message}}\n";

    /// <summary>
    /// Handlebars template for assistant messages
    /// </summary>
    public string AssistantFormat { get; set; } = "{{#capitalize user}}: {{#if message}}{{message}}\n{{/if}}";

    public string[] BreakOn { get; set; } = new string[] { "user:"};
    public bool TreatEOSAsNewline { get; set; } = true;
    public LLamaSamplingParams SamplingParams { get; set; } = new LLamaSamplingParams();
}

public class TemplateInput
{
    private HandlebarsTemplate<object, object> _template;

    public TemplateInput(string template)
    {
        _template = Handlebars.Compile(template);
    }

    public string Render(string user, string message)
    {
        return _template(new{user, message});
    }
}

public class BaseChatService
{
    private readonly BaseChatServiceOptions _options;
    private readonly ILanguageModel _model;
    private readonly Dictionary<string, TemplateInput> _templates;
    Serilog.ILogger _log = Log.ForContext<BaseChatService>();

    public BaseChatService(IOptions<BaseChatServiceOptions> options)
    {
        if (options.Value.Model == null || options.Value.Model == "")
        {
            throw new ArgumentNullException(nameof(options.Value.Model));
        }
        if (options.Value.Seed < 0)
        {
            options.Value.Seed = new Random().Next();
        }
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

        Handlebars.RegisterHelper("capitalize", (writer, context, parameters) => 
        {
            if (parameters.Length != 1)
            {
                throw new HandlebarsException("{{capitalize}} helper must have exactly one parameter");
            }
            writer.WriteSafeString(parameters[0].ToString().Capitalize());
        });

        _templates = new Dictionary<string, TemplateInput>() {
            {"system", new TemplateInput(_options.SystemFormat)},
            {"user", new TemplateInput(_options.UserFormat)},
            {"assistant", new TemplateInput(_options.AssistantFormat)}
        };
        
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
            if (!_templates.ContainsKey(message.Role))
            {
                log.Warning("Unknown role {role}", message.Role);
                continue;
            }
            sb.Append(_templates[message.Role].Render(message.Role, message.Content));
        }
        sb.Append("Assistant: ");
        return sb.ToString();
    }

    private string _removeEnding(string text) {
        foreach (var ending in _options.BreakOn)
        {
            if (text.Trim().ToLower().EndsWith("\n" + ending.ToLower()))
            {
                text = text.Remove(text.Length - ending.Length).Trim();
                break;
            }
        }
        return text;
    }

    public string ProcessRequest(List<Message> messages, CancellationToken? ct = null)
    {
        var text = "";
        foreach (var token in ProcessRequestStreamResponse(messages))
        {
            if (ct != null && ct.Value.IsCancellationRequested) break;
            text += token;
        }
        return _removeEnding(text);
    }

    public IEnumerable<string> ProcessRequestStreamResponse(List<Message> messages, CancellationToken? ct = null)
    {
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken cti = source.Token;
        var text = "";
        foreach (var entry in _model.Generate(_formatMessages(messages), cti, _options.SamplingParams))
        {
            if (ct != null && ct.Value.IsCancellationRequested)
            {
                source.Cancel();
                break;
            }
            if (_options.BreakOn.Any(x => text.Trim().ToLower().EndsWith(x)))
            {
              source.Cancel();
              break;
            }
            text += entry;
            yield return entry;
        };
    }    
}