using System;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DeskToys.Core.Services;

namespace DeskToys.Core.ViewModels;

public partial class MainWindowViewModel: ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _imageSource;
    [ObservableProperty]
    private double _maxChatBoxWidth = 800;
    [ObservableProperty]
    private double _maxChatBoxHeight = 400;

    [ObservableProperty]
    private string _chatBoxBorderColor = "Transparent";
    [ObservableProperty]
    private int _screenMaxWidth;
    [ObservableProperty]
    private int _screenMaxHeight;
    [ObservableProperty]
    private double _mouseX;
    [ObservableProperty]
    private double _mouseY;
    [ObservableProperty]
    private int _windowPositionX;
    [ObservableProperty]
    private int _windowPositionY;
    [ObservableProperty]
    private bool _isChatBoxVisible;
    [ObservableProperty]
    private bool _mainWindowShown;
    [ObservableProperty]
    private Vector _markdownScrollValue = Vector.Zero;
    [ObservableProperty]
    private bool _mdScrollable;
    [ObservableProperty]
    private string _mdText = "Ask me anything";
    [ObservableProperty]
    private string _chatBoxOpacity = "0.6";
    private bool _followPointer = true;

    public void AskQuestion(Bitmap? bitmap)
    {
        if (bitmap == null)
        {
            MdText = "No image found";
            return;
        }
        _ = AIChat.Ask(bitmap);
    }
    
    public void OnMouseMoved(int mouseX, int mouseY)
    {
        if (!MainWindowShown)
        {
            return;
        }

        if (!IsChatBoxVisible)
        {
            IsChatBoxVisible = true;
        }

        if (!_followPointer)
        {
            return;
        }
        
        int deltaX = 20;
        int deltaY = 20;
        var localX = mouseX - WindowPositionX;
        var localY = mouseY - WindowPositionY;
        
        if (localX + deltaX + MaxChatBoxWidth / 2 > ScreenMaxWidth)
            deltaX = (int)(-MaxChatBoxWidth / 2 - deltaX);

        if (localY + deltaY + MaxChatBoxHeight / 2 > ScreenMaxHeight)
            deltaY = (int)(-MaxChatBoxHeight / 2 - deltaY);
        MouseX = Math.Clamp(localX + deltaX, (int)0, (int)ScreenMaxWidth);
        MouseY = Math.Clamp(localY + deltaY, (int)0, (int)ScreenMaxHeight);
    }

    public void UpdateText(string text)
    {
        MdText += text;
    }

    public void ToggleFollowPointer()
    {
        _followPointer = !_followPointer;
    }

    public void ToggleMdScrollable()
    {
        MdScrollable = !MdScrollable;
    }

    public void ScrollMarkdown(Vector vector)
    {
        MarkdownScrollValue += vector;
    }

    partial void OnMdScrollableChanged(bool value)
    {
        ChatBoxBorderColor = value ? "Blue" : "Transparent";
        ChatBoxOpacity = value ? "0.5" : "0.6";
    }
}