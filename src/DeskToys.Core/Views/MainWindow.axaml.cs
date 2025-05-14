using System;
using Avalonia.Controls;
using DeskToys.Core.Models;
using DeskToys.Core.Services;
using DeskToys.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DeskToys.Core.Views;

public partial class MainWindow : Window
{
    
    public MainWindow()
    { 
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        ConfigureWindowBehaviors();
        DetectScreenSize();
        base.OnOpened(e);
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
            }
        }
    }

    private void ConfigureWindowBehaviors()
    {
        IWindowConfigurator? windowConfigurator = ServiceProviderBuilder.ServiceProvider?.GetService<IWindowConfigurator>();
        if (windowConfigurator != null)
        {
            windowConfigurator.ConfigureWindow(
                this,
                new WindowBehaviorOptions
                {
                    ContentProtection = true,
                    ExtendToFullScreen = true,
                    IgnoreMouseEvents = true
                });
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindowShown = false;
        }
        base.OnClosing(e);
    }

}