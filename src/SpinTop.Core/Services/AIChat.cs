using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SpinTop.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace SpinTop.Core.Services;

public class AIChat
{
    private static ChatWindowViewModel? _chatWindowViewModel;
    private static SettingsWindowViewModel? _settingsViewModel;
    private static ChatClient? chatClient;
    private static readonly Dictionary<string, CancellationTokenSource> Cancellations = new();

    static AIChat()
    {
        _chatWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<ChatWindowViewModel>();
        _settingsViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<SettingsWindowViewModel>();
        if (_settingsViewModel is { Endpoint: not null, ApiKey: not null })
        {
            OpenAIClientOptions options =  new OpenAIClientOptions
            {
                Endpoint = new Uri(_settingsViewModel.Endpoint)
            };
            var openAiClient = new OpenAIClient(new ApiKeyCredential(_settingsViewModel.ApiKey), options);
            chatClient = openAiClient.GetChatClient(_settingsViewModel.Model);
        }
    }

    public static void StopAIResponseStream(string requestId)
    {
        if (!string.IsNullOrEmpty(requestId) && Cancellations.ContainsKey(requestId))
        {
            var  cancellationTokenSource = Cancellations[requestId];
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested && _chatWindowViewModel != null)
            {
                cancellationTokenSource.Cancel();
                _chatWindowViewModel.MessageRequested = false;
                Cancellations.Remove(requestId);
            }
        }
    }

    private static async Task Ask(string requestId, params ChatMessage[] messages)
    {
        var cts = new CancellationTokenSource();
        var cancelToken = cts.Token;
        cancelToken.ThrowIfCancellationRequested();
        Cancellations.Add(requestId, cts);
        AsyncCollectionResult<StreamingChatCompletionUpdate>? completionUpdates = chatClient?.CompleteChatStreamingAsync(messages, null, cancelToken);

        if (_chatWindowViewModel != null)
        {
            _chatWindowViewModel.LastRequestId = requestId;
            _chatWindowViewModel.MessageRequested = true;
            _chatWindowViewModel.UserMessage = string.Empty;
            _chatWindowViewModel.MdText = "[AI]: ";
            if (completionUpdates != null)
            {
                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        string text = completionUpdate.ContentUpdate[0].Text;
                        _chatWindowViewModel.UpdateText(text);
                    }

                    if (completionUpdate.FinishReason == ChatFinishReason.Stop)
                    {
                        _chatWindowViewModel.MessageRequested = false;
                        Cancellations.Remove(requestId);
                    }
                }
            }
        }
    }
    
    public static async Task Ask(Bitmap? bitmap = null, string? question = null)
    {
        var userPrompt = question ?? _settingsViewModel?.UserPrompt;
        var systemPrompt = _settingsViewModel?.SystemPrompt;

        if (string.IsNullOrWhiteSpace(userPrompt) && bitmap == null)
        {
            Console.WriteLine("Both question and image are null. Nothing to send.");
            return;
        }

        var userParts = new List<ChatMessageContentPart>();

        if (!string.IsNullOrWhiteSpace(userPrompt))
        {
            userParts.Add(ChatMessageContentPart.CreateTextPart(userPrompt));
        }

        if (bitmap != null)
        {
            var imageData = ToBinaryData(bitmap);
            userParts.Add(ChatMessageContentPart.CreateImagePart(imageData, "image/png"));
        }

        var userMessage = new UserChatMessage(userParts.ToArray());
        var sysPart = ChatMessageContentPart.CreateTextPart(systemPrompt);
        var systemMessage = new SystemChatMessage(sysPart);
        var requestId = Guid.NewGuid().ToString();
        await Ask(requestId, systemMessage, userMessage);
    }

    private static BinaryData ToBinaryData(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        return BinaryData.FromBytes(stream.ToArray());
    }
}