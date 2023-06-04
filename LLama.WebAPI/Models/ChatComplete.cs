using System.Collections.Generic;

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public class ChatCompleteRequest
{
    public string Model { get; set; }
    public List<Message> Messages { get; set; }
    public int Max_tokens { get; set; } = 2000;
    public bool Stream { get; set; } = false;

}

public class Choice
{
    public int Index { get; set; }
    public Message Message { get; set; }
    public string? FinishReason { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

public class ChatCompleteResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public List<Choice> Choices { get; set; }
    public Usage Usage { get; set; }
    public string? Model { get; set; }
}

public class ChatCompletionDelta
{
    public DeltaChoice[] Choices { get; set; }
    public long Created { get; set; }
    public string Id { get; set; }
    public string Model { get; set; }
    public string Object { get; set; }
}

public class DeltaChoice
{
    public Delta Delta { get; set; }
    public string FinishReason { get; set; }
    public int Index { get; set; }
}

public class Delta
{
    public string Content { get; set; }
    public string Role { get; set; }
}