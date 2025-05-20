using System;
using Avalonia.Controls;
using DeskToys.Core.Models;
using DeskToys.Core.ViewModels;

namespace DeskToys.Core.Views;

public partial class MainWindow : Window
{
    
    public MainWindow()
    { 
        InitializeComponent();
        Activated += OnActivated;
        Deactivated += OnDeActivated;
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Interactive = true;
            ForceFocusUserPromptInput();
        }
    }
    
    private void OnDeActivated(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Interactive = false;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        ConfigureWindowBehaviors();
        DetectScreenSize();
        base.OnOpened(e);
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
            Position = screen.Bounds.Position;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ScreenMaxWidth = screen.Bounds.Size.Width;
                viewModel.ScreenMaxHeight = screen.Bounds.Size.Height;
                viewModel.WindowPositionX = screen.Bounds.Position.X;
                viewModel.WindowPositionY = screen.Bounds.Position.Y;
                viewModel.MouseX = (screen.Bounds.Size.Width - viewModel.ChatBoxWidth) / 2.0;
                viewModel.MouseY = screen.Bounds.Size.Height / 8.0;
            }
        }
    }

    private void ConfigureWindowBehaviors()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ConfigureWindowBehaviors(this, new WindowBehaviorOptions
            {
                ContentProtection = viewModel.ContentProtection,
                ExtendToFullScreen = viewModel.ExtendToFullScreen,
                IgnoreMouseEvents = viewModel.IgnoreMouseEvents,
            });
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindowShown = false;
            viewModel.Interactive = false;
        }
        base.OnClosing(e);
    }
    
}