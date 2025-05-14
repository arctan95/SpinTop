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
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (DataContext is ChatWindowViewModel vm)
        {
            vm.Interactive = true;
            ForceFocusUserPromptInput();
        }
    }
    
    private void OnDeActivated(object? sender, EventArgs e)
    {
        if (DataContext is ChatWindowViewModel vm)
        {
            vm.Interactive = false;
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
    

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is ChatWindowViewModel viewModel)
        {
            viewModel.Interactive = false;
        }
        base.OnClosing(e);
    }
}