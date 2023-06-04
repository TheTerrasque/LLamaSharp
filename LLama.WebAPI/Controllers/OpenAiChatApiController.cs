using System.Text.Json;
using LLama.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LLama.WebAPI.Controllers
{
    public class LowerCaseNamingPolicy : JsonNamingPolicy
    {
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        return name.ToLower();
    }
    }

    [ApiController]
    [Route("v1/chat")]
    public class OpenAiChatApiController : ControllerBase
    {
        private readonly BaseChatService _service;
        private Serilog.ILogger _log = Log.ForContext<OpenAiChatApiController>();

        public OpenAiChatApiController(BaseChatService service)
        {
            _service = service;
        }



        private async Task<EmptyResult> handleStreaming(List<Message> messages) {
            _log.Information("Handling streaming request");

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            //await Response.Body.FlushAsync();

            //var responseStream = Response.Body;
            var cancellationToken = HttpContext.RequestAborted;

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = new LowerCaseNamingPolicy(), WriteIndented = false };

            try
            {
                foreach (var data in _service.ProcessRequestStreamResponse(messages, cancellationToken))
                {
                    var delta = new ChatCompletionDelta
                    {
                        Choices = new DeltaChoice[] {
                            new DeltaChoice {
                                Delta = new Delta {
                                    Role = "assistant",
                                    Content = data
                                },
                                FinishReason = "",
                                Index = 0
                            }
                        },
                        Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        Id = Guid.NewGuid().ToString(),
                        Model = "MysteryModel",
                        Object = "text_completion"
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(delta, jsonOptions);
                    await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                }
                await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
                //await _sendStreamData(responseStream, "[DONE]", cancellationToken);

                return new EmptyResult();

            }
            catch (OperationCanceledException)
            {
                _log.Information("Client disconnected from SSE.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error while streaming SSE.");
            }
            finally
            {
                //responseStream.Close();
            }
            return new EmptyResult();
        }

        private async Task _sendStreamData(Stream responseStream, string data, CancellationToken cancellationToken)
        {
            _log.Debug("Sending SSE data: {sseData}", data);
            byte[] initialEventBytes = System.Text.Encoding.UTF8.GetBytes("data: "+data+"\n\n");
            await responseStream.WriteAsync(initialEventBytes, 0, initialEventBytes.Length, cancellationToken);
            await responseStream.FlushAsync(cancellationToken);
        }

        [HttpPost("completions")]
        public async Task<IActionResult> SendMessage([FromBody] ChatCompleteRequest input)
        {
            _log.Debug("Received completions request {@input}", input);
            var cancellation = Request.HttpContext.RequestAborted;

            if (input.Stream)
            {
                return await handleStreaming(input.Messages);
            }

            var data = _service.ProcessRequest(input.Messages, cancellation);

            var result = new ChatCompleteResponse
            {
                Choices = new List<Choice> {
                    new Choice {
                        FinishReason = "stop",
                        Index = 0,
                        Message = new Message {
                            Content = data,
                            Role = "assistant"
                        }
                    }
                },
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Id = Guid.NewGuid().ToString(),
                Model = "MysteryModel",
                Object = "text_completion",
                Usage = new Usage
                {
                    CompletionTokens = -1,
                    PromptTokens = -1,
                    TotalTokens = -1
                }                
            };
            return Ok(result);
        }
    }
}