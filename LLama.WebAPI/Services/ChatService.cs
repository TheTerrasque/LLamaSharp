using LLama.WebAPI.Models;

namespace LLama.WebAPI.Services;

public class ChatService
{
    private readonly ChatSession<LLamaModel> _session;

    public ChatService()
    {
        ModelName = "Manticore-13B-Chat-Pyg.ggmlv3.q5_1.bin";
        LLamaModel model = new(new LLamaParams(model: @"C:\temp\models\Manticore-13B-Chat-Pyg.ggmlv3.q5_1.bin", n_ctx: 2048, interactive: true, repeat_penalty: 1.1f, verbose_prompt: false, n_gpu_layers: 17));
        _session = new ChatSession<LLamaModel>(model)
            .WithPromptFile(@"Assets/Assistant.txt")
            .WithAntiprompt(new string[] { "User:", "user:" });
    }

    public string ModelName { get; internal set; }

    public string Send(string text)
    {
        var outputs = _session.Chat("User: " + text);
        var result = "";
        foreach (var output in outputs)
        {
            result += output;
        }

        return result.Replace("Assistant:", "").Replace("assistant:", "").Replace("User:", "").Replace("user:", "").Replace("ASSISTANT:", "").Trim();
    }
}
