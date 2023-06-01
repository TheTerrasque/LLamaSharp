using LLama;
using LLama.Examples;
using Serilog;

string GetModelPath(string prompt = "Please input your model path: ")
{
    bool complete = false;
    string modelPath = "";
    while (!complete)
    {
        Console.Write(prompt);
        modelPath = Console.ReadLine();
        // check if path exists
        if (!System.IO.File.Exists(modelPath))
        {
            Console.WriteLine("Cannot find the model file. Please input again.");
            continue;
        }
        complete = true;
    }
    return modelPath;
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Console.WriteLine("================LLamaSharp Examples==================\n");

Console.WriteLine("Please input a number to choose an example to run:");
Console.WriteLine("0: Run a chat session.");
Console.WriteLine("1: Run a LLamaModel to chat.");
Console.WriteLine("2: Quantize a model.");
Console.WriteLine("3: Get the embeddings of a message.");
Console.WriteLine("4: Run a LLamaModel with instruct mode.");
Console.WriteLine("5: Load and save state of LLamaModel.");
Console.WriteLine("6: LlamaBaseModel example.");


while (true)
{
    Console.Write("\nYour choice: ");
    int choice = int.Parse(Console.ReadLine());

    if (choice == 0)
    {
        var modelPath = GetModelPath();
        ChatSession chat = new(modelPath, "Assets/chat-with-bob.txt", new string[] { "User:", "user:" });
        chat.Run();
    }
    else if (choice == 1)
    {
        var modelPath = GetModelPath();
        ChatWithLLamaModel chat = new(modelPath, "Assets/chat-with-bob.txt", new string[] { "User:" });
        chat.Run();
    }
    else if (choice == 2) // quantization
    {
        var inputPath = GetModelPath();
        var outputPath = GetModelPath("Please input your output model path: ");
        Console.Write("Please input the quantize type (one of q4_0, q4_1, q5_0, q5_1, q8_0): ");
        var quantizeType = Console.ReadLine();
        Quantize q = new Quantize();
        q.Run(inputPath, outputPath, quantizeType);
    }
    else if (choice == 3) // get the embeddings only
    {
        var modelPath = GetModelPath();
        GetEmbeddings em = new GetEmbeddings(modelPath);
        Console.Write("Please input the text: ");
        var text = Console.ReadLine();
        em.Run(text);
    }
    else if (choice == 4) // instruct mode
    {
        var modelPath = GetModelPath();
        InstructMode im = new InstructMode(modelPath, "Assets/alpaca.txt");
        Console.WriteLine("Here's a simple example for using instruct mode. You can input some words and let AI " +
            "complete it for you. For example: Write a story about a fox that wants to make friend with human. No less than 200 words.");
        im.Run();
    }
    else if (choice == 5) // load and save state
    {
        var modelPath = GetModelPath();
        Console.Write("Please input your state file path: ");
        var statePath = Console.ReadLine();
        SaveAndLoadState sals = new(modelPath, File.ReadAllText(@"D:\development\llama\llama.cpp\prompts\alpaca.txt"));
        sals.Run("Write a story about a fox that wants to make friend with human. No less than 200 words.");
        sals.SaveState(statePath);
        sals.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // create a new model to load the state.
        SaveAndLoadState sals2 = new(modelPath, "");
        sals2.LoadState(statePath);
        sals2.Run("Tell me more things about the fox in the story you told me.");
    }
    else if (choice == 6) {
        var modelPath = GetModelPath();
        var llamaparams = new LLamaParams(model: modelPath);
        var model = new BaseLLamaModel(llamaparams);
        var chat = new BaseChatSession(model, 
            new ChatMetadata()
                .WithPromptFromFile("Assets/chat-with-bob.txt")
                .SetAssistantName("Bob"));

        Console.WriteLine("Here's a simple example for using BaseLLamaModel. You can input some words and let AI " +
            "complete it for you. For example: Write a story about a fox that wants to make friend with human. No less than 200 words.");
        
        while (true)
        {
            Console.Write("Your input: ");
            var input = Console.ReadLine();
            foreach (var item in chat.Chat(input))
            {
                Console.Write(item);
            }
        }
    }
    else
    {
        Console.WriteLine("Cannot parse your choice. Please select again.");
        continue;
    }
    break;
}