using System;
using Avalonia;
using Avalonia.Controls;
using SpinTop.Core.Models;
using SpinTop.Core.ViewModels;

namespace SpinTop.Core.Views;

public partial class ChatWindow : Window
{
    public ChatWindow()
    { 
        InitializeComponent();
        Activated += OnActivated;
        Deactivated += OnDeActivated;
        Closing += OnClosing;
        Resized += OnResized;
    }

    private void OnResized(object? sender, WindowResizedEventArgs e)
    {
        if (DataContext is ChatWindowViewModel vm)
        {
            vm.ChatBoxWidth = e.ClientSize.Width;
            vm.ChatBoxHeight = e.ClientSize.Height;
        }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (DataContext is ChatWindowViewModel vm)
        {
            vm.ChatBoxBorderColor = "Blue";
            vm.ChatBoxOpacity = "0.5";
            ForceFocusUserPromptInput();
        }
    }
    
    private void OnDeActivated(object? sender, EventArgs e)
    {
        if (DataContext is ChatWindowViewModel vm)
        {
            vm.ChatBoxBorderColor = vm.IgnoreMouseEvents ? "Transparent" : "Green";
            vm.ChatBoxOpacity = "0.4";
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        DetectScreenSize();
        ConfigureWindow();
        base.OnOpened(e);
    }

    private void ConfigureWindow()
    {
        if (Application.Current is App app)
        {
            if (DataContext is ChatWindowViewModel vm)
            {
                app.ConfigureWindowBehaviors(this, new WindowBehaviorOptions
                {
                    ContentProtection = vm.ContentProtection,
                    OverlayWindow = vm.OverlayWindow,
                    IgnoreMouseEvents = vm.IgnoreMouseEvents
                });
            }
        }
    }

    private void ForceFocusUserPromptInput()
    {
        UserPrompt.Focus();
    }
    
    private void DetectScreenSize()
    {
        var screen = Screens.Primary;
        if (screen != null)
        {
            if (DataContext is ChatWindowViewModel viewModel)
            {
                viewModel.WindowPositionX = screen.Bounds.Position.X;
                viewModel.WindowPositionY = screen.Bounds.Position.Y;
            }
        }
    }
    
}