﻿using SpinTop.Desktop.MacOS.Services;
using Avalonia;
using SpinTop.Core;
using SpinTop.Core.Services;
using SpinTop.Core.ViewModels;

namespace SpinTop.Desktop.MacOS;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        NSApplication.Init();
        new ServiceProviderBuilder()
            .AddSingleton<IScreenCapturer, ScreenCapturer>()
            .AddSingleton<IWindowConfigurator, WindowConfigurator>()
            .AddSingleton<ISystemHotKeyRegister, SystemHotkeyRegister>()
            .AddSingleton<ConfigService>()
            .AddSingleton<ChatWindowViewModel>()
            .AddSingleton<SettingsWindowViewModel>()
            .Build();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .With(new MacOSPlatformOptions() { ShowInDock = false });
}
