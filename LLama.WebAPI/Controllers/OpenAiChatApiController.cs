using LLama.WebAPI.Models;
using LLama.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLama.WebAPI.Controllers
{
    [ApiController]
    [Route("v1/chat")]
    public class OpenAiChatApiController : ControllerBase
    {
        private readonly ChatService _service;
        private readonly ILogger<ChatController> _logger;

        public OpenAiChatApiController(ILogger<ChatController> logger,
            ChatService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("completions")]
        public ChatCompleteResponse SendMessage([FromBody] ChatCompleteRequest input)
        {
            var data = _service.Send(input.Messages.Last().Content);
            return new ChatCompleteResponse
            {
                Choices = new List<Choice> {
                    new Choice {
                        FinishReason = "stop",
                        Index = 0,
                        Message = new Message {
                            Content = data,
                            Role = "agent"
                        }
                    }
                },
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Id = Guid.NewGuid().ToString(),
                Model = _service.ModelName,
                Object = "text_completion",
                Usage = new Usage
                {
                    CompletionTokens = 1,
                    PromptTokens = 1,
                    TotalTokens = 1
                }                
            };
        }
    }
}