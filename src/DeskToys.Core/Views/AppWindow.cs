using System;
using Avalonia.Controls;
using DeskToys.Core.Models;
using DeskToys.Core.Services;
using DeskToys.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DeskToys.Core.Views;

public class AppWindow: Window
{

    private SettingsWindowViewModel? _settingsWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<SettingsWindowViewModel>();

    protected override void OnOpened(EventArgs e)
    {
        DetectScreenSize();
        ConfigureDefaultWindowBehaviors();
        base.OnOpened(e);
    }
    
    private void ConfigureDefaultWindowBehaviors()
    {
        var windowConfigurator = ServiceProviderBuilder.ServiceProvider?.GetService<IWindowConfigurator>();
        if (_settingsWindowViewModel != null)
        {
            windowConfigurator?.ConfigureWindow(this, new WindowBehaviorOptions
            {
                ContentProtection = _settingsWindowViewModel.ContentProtection,
                ExtendToFullScreen = _settingsWindowViewModel.ExtendToFullScreen,
                IgnoreMouseEvents = _settingsWindowViewModel.IgnoreMouseEvents,
            });
        }
    }
    
    
    private void DetectScreenSize()
    {
        var screen = Screens.Primary;
        if (screen != null)
        {
            if (_settingsWindowViewModel != null)
            {
                _settingsWindowViewModel.ScreenMaxWidth = screen.Bounds.Size.Width;
                _settingsWindowViewModel.ScreenMaxHeight = screen.Bounds.Size.Height;
            }
        }
    }
}