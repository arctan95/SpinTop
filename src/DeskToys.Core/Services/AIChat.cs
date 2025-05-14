using System;
using System.ClientModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DeskToys.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace DeskToys.Core.Services;

public class AIChat
{
    private static MainWindowViewModel? _mainWindowViewModel;
    private static SettingsWindowViewModel? _settingsViewModel;
    private static ChatClient? chatClient;

    static AIChat()
    {
        _mainWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<MainWindowViewModel>();
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

    private static async Task Ask(params ChatMessage[] messages)
    {
        AsyncCollectionResult<StreamingChatCompletionUpdate>? completionUpdates = chatClient?.CompleteChatStreamingAsync(messages);

        if (_mainWindowViewModel != null)
        {
            _mainWindowViewModel.MdText = "[AI]: ";
            if (completionUpdates != null)
            {
                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        string text = completionUpdate.ContentUpdate[0].Text;
                        _mainWindowViewModel.UpdateText(text);
                    }
                }
            }
        }
    }

    public static async Task Ask(Bitmap? bitmap, string? question = null)
    {
        var text = ChatMessageContentPart.CreateTextPart(question ?? _settingsViewModel?.UserPrompt);
        var sysText = ChatMessageContentPart.CreateTextPart(_settingsViewModel?.SystemPrompt);
        ChatMessageContentPart? image = null;
        if (bitmap != null)
        {
            image =  ChatMessageContentPart.CreateImagePart(ToBinaryData(bitmap), "image/png");
        }
        
        ChatMessage sysMessage = new SystemChatMessage(sysText);
        ChatMessage message = new UserChatMessage(text, image);
        await Ask(sysMessage, message);
    }

    private static BinaryData ToBinaryData(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        return BinaryData.FromBytes(stream.ToArray());
    }
}