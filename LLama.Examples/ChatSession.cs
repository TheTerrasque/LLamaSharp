using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLama.Examples
{
    public class ChatSession
    {
        Random rnd = new Random();
        ChatSession<LLamaModel> _session;
        public ChatSession(string modelPath, string promptFilePath, string[] antiprompt)
        {
            LLamaModel model = new(new LLamaParams(rnd.Next(), model: modelPath, 
                n_ctx: 2048, interactive: true, repeat_penalty: 1.10f, 
                verbose_prompt: false, n_gpu_layers: 17));
            _session = new ChatSession<LLamaModel>(model)
                .WithPromptFile(promptFilePath)
                .WithAntiprompt(antiprompt);
        }


        public void Run()
        {
            Console.Write("\nUser:");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                var question = Console.ReadLine();
                question += "\n";
                Console.ForegroundColor = ConsoleColor.White;
                var outputs = _session.Chat(question, encoding: "UTF-8");
                foreach (var output in outputs)
                {
                    Console.Write(output);
                }
            }
        }
    }
}
