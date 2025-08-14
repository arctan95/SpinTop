using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SpinTop.Core.Services;

namespace SpinTop.Core.ViewModels;

public partial class ChatWindowViewModel: ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _imageSource;
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
    private Vector _markdownScrollValue = Vector.Zero;
    [ObservableProperty]
    private string _mdText = "What can I help with?";
    [ObservableProperty]
    private string _chatBoxOpacity = "0.4";
    [ObservableProperty]
    private bool _followPointer;
    [ObservableProperty]
    private double _chatBoxWidth;
    [ObservableProperty]
    private double _chatBoxHeight;
    [ObservableProperty]
    private bool _chatWithScreenshot;
    [ObservableProperty]
    private bool _readClipboardImage;
    [ObservableProperty]
    private bool _contentProtection = true;
    [ObservableProperty]
    private bool _overlayWindow = true;
    [ObservableProperty]
    private bool _ignoreMouseEvents = true;
    
    partial void OnChatWithScreenshotChanged(bool value)
    {
        if (!value)
        {
            ImageSource = null;
        }
        else
        {
            ReadClipboardImage = false;
        }
    }

    partial void OnReadClipboardImageChanged(bool value)
    {
        if (value)
        {
            ChatWithScreenshot = false;
        }
    }

    public async Task AskWithImage(Bitmap? bitmap)
    {
        Bitmap? image = bitmap;
        if (image == null)
        {
            if (Application.Current is App app)
            {
                if (ChatWithScreenshot)
                {
                    image = await app.TakeScreenshotAsync();
                }
                else if (ReadClipboardImage)
                {
                    image = await app.LoadClipboardImageAsync();
                }
            }
        }
        ImageSource = image;
        await AIChat.Ask(image, UserMessage);
        ImageSource = null;
    }
    
    public void StopAIResponse()
    {
        if (!string.IsNullOrWhiteSpace(LastRequestId))
        {
            AIChat.StopAIResponseStream(LastRequestId);
        }
    }

    public async Task SendMessage()
    {
        MdText += Environment.NewLine + Environment.NewLine + UserMessage;
        await AskWithImage(ImageSource);
    }
    
    public void OnMouseMoved(int mouseX, int mouseY)
    {

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

    public void ScrollMarkdown(Vector offset)
    {
        // FIXME: find a way to fix unlimited scrolling
        var newX = MarkdownScrollValue.X + offset.X;
        var newY = MarkdownScrollValue.Y + offset.Y;
        MarkdownScrollValue = new Vector(newX, newY);
    }

    public void ShowMissingApiKeyHint()
    {
        MdText = "Please configure your AI provider's API key in the settings.";
    }
}