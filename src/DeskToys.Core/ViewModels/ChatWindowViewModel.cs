using System;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DeskToys.Core.Services;

namespace DeskToys.Core.ViewModels;

public partial class ChatWindowViewModel: ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _imageSource;
    [ObservableProperty]
    private string _chatBoxBorderColor = "Transparent";
    [ObservableProperty]
    private string _userMessage = "";
    [ObservableProperty]
    private string _lastRequestId = "";
    [ObservableProperty]
    private bool _messageRequested;
    [ObservableProperty]
    private int _windowPositionX;
    [ObservableProperty]
    private int _windowPositionY;
    [ObservableProperty]
    private bool _chatWindowShown;
    [ObservableProperty]
    private Vector _markdownScrollValue = Vector.Zero;
    [ObservableProperty]
    private string _mdText = "DeskToys AI";
    [ObservableProperty]
    private string _chatBoxOpacity = "0.4";
    [ObservableProperty]
    private bool _interactive;
    [ObservableProperty]
    private bool _followPointer;
    [ObservableProperty]
    private double _chatBoxWidth = 800;
    [ObservableProperty]
    private double _chatBoxHeight = 600;
    [ObservableProperty]
    private bool _chatWithScreenshot;

    partial void OnChatWithScreenshotChanged(bool value)
    {
        if (!value)
        {
            ImageSource = null;
        }
    }

    public void AskAIWithDefaultPrompt(Bitmap? bitmap)
    {
        if (bitmap == null)
        {
            MdText = "No image found";
            return;
        }
        _ = AIChat.Ask(bitmap);
    }
    public void StopAIResponse()
    {
        if (!string.IsNullOrWhiteSpace(LastRequestId))
        {
            AIChat.stopAIResponseStream(LastRequestId);
        }
    }

    public void SendMessage()
    {
        MdText += Environment.NewLine;
        MdText += UserMessage;

        if (ChatWithScreenshot)
        {
            if (Application.Current is App app)
            {
                app.TakeScreenshotWithCallback(bitmap =>
                {
                    ImageSource = bitmap;
                    _ = AIChat.Ask(ImageSource, UserMessage);
                });
            }
        }
        else
        {
            _ = AIChat.Ask(ImageSource, UserMessage);
        }
        
        UserMessage = string.Empty;
    }
    
    public void OnMouseMoved(int mouseX, int mouseY)
    {
        if (!ChatWindowShown)
        {
            return;
        }

        if (!FollowPointer)
        {
            return;
        }
        
        int deltaX = -(int)(ChatBoxWidth / 2);
        int deltaY = 20;
        var localX = mouseX - WindowPositionX;
        var localY = mouseY - WindowPositionY;

        if (Application.Current is App app)
        {
            app.UpdateChatWindowPosition(new PixelPoint(localX + deltaX, localY + deltaY));
        }
    }

    public void UpdateText(string text)
    {
        MdText += text;
    }

    public void ToggleFollowPointer()
    {
        FollowPointer = !FollowPointer;
    }

    public void ScrollMarkdown(Vector vector)
    {
        MarkdownScrollValue += vector;
    }

    partial void OnInteractiveChanged(bool value)
    {
        ChatBoxBorderColor = value ? "Blue" : "Transparent";
        ChatBoxOpacity = value ? "0.5" : "0.4";
    }
}