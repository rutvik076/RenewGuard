using Anthropic;
using Microsoft.Extensions.AI;

namespace RenewGuard.Infrastructure.Extraction;

public static class ChatClientFactory
{
    public static IChatClient Create(string model, string apiKey)
    {
        var client = new AnthropicClient { ApiKey = apiKey };
        return client.AsIChatClient(model);
    }
}
