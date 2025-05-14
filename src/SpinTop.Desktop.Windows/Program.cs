using System;
using Avalonia;
using SpinTop.Core;
using SpinTop.Core.Services;
using SpinTop.Core.ViewModels;
using SpinTop.Desktop.Windows.Services;

namespace SpinTop.Desktop.Windows;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
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
            .With(new Win32PlatformOptions());
}
