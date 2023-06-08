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