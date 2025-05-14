using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using DeskToys.Core.Views;
using CommunityToolkit.Mvvm.Input;
using DeskToys.Core.Services;
using DeskToys.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using NetSparkleUpdater;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using SharpHook.Native;

namespace DeskToys.Core;

public partial class App : Application
{
    
    private IClassicDesktopStyleApplicationLifetime? _lifetime;
    private MainWindowViewModel? _mainWindowViewModel;
    private SettingsWindowViewModel? _settingsWindowViewModel;
    private ConfigService? _configService;
    private Window? _mainWindow;
    private SparkleUpdater? _sparkle;
    
    public ICommand SettingsCommand { get; }
    
    public ICommand CheckUpdatesCommand { get; }
    public ICommand MainCommand { get; }
    public ICommand QuitCommand { get; }
    
    public App()
    {
        QuitCommand = new RelayCommand(QuitApplication);
        SettingsCommand = new RelayCommand(ShowSettingsWindow);
        MainCommand = new RelayCommand(ShowMainWindow);
        CheckUpdatesCommand = new AsyncRelayCommand(CheckUpdates);
        _mainWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<MainWindowViewModel>();
        _settingsWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<SettingsWindowViewModel>();
        _configService = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<ConfigService>();
        DataContext = this;
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InputListener.SetupSystemHook();
        RegisterFunctions();
        RegisterGlobalHotkeys();
        SetUpUpdater();
    }
    
    private void RegisterGlobalHotkeys()
    {
        // predefined global hotkeys
        GlobalHotkeyManager.BindHotkey("hide_main_window", ModifierMask.None, KeyCode.VcEscape,  HideMainWindow);
        
        foreach (var keyValuePair in FunctionRegistry._functionBindings)
        {
            var functionName = keyValuePair.Key;
            var function = keyValuePair.Value;
            var hotkey = _configService?.Get<ushort[]>("control."+functionName);
            if (hotkey is { Length: > 1 })
            {
                GlobalHotkeyManager.BindHotkey(
                    functionName,
                    (ModifierMask)hotkey[0],
                    (KeyCode)hotkey[1],
                    function);
            }
        }
    }
    
    private void RegisterFunctions()
    {
        FunctionRegistry.RegisterFunction("open_main_window", ToggleShowMainWindow);
        FunctionRegistry.RegisterFunction("take_screenshot", TakeScreenshot);
        FunctionRegistry.RegisterFunction("start_over", StartOver);
        FunctionRegistry.RegisterFunction("ask_question", AskQuestion);
        FunctionRegistry.RegisterFunction("quit_app", QuitApplication);
    }

    private void AskQuestion()
    {
        _mainWindowViewModel?.AskQuestion(_mainWindowViewModel.ImageSource);
    }

    private void TakeScreenshot()
    {
        IScreenCapturer? screenCapturer = ServiceProviderBuilder.ServiceProvider?.GetService<IScreenCapturer>();
        screenCapturer?.CaptureScreen(1920, 1080, (bitmap) =>
        {
            if (_mainWindowViewModel != null)
                _mainWindowViewModel.ImageSource = bitmap;
        });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _lifetime = desktop;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task CheckUpdates()
    {
        await _sparkle.CheckForUpdatesAtUserRequest();
    }


    private void SetUpUpdater()
    {
        var osName = OperatingSystem.IsMacOS() ? "macos" :
            OperatingSystem.IsWindows() ? "windows" :
            "unknown";
        var url = $"https://desktoys.github.io/DeskToys/publish/{osName}/appcast.xml";
        using var iconStream = AssetLoader.Open(new Uri("avares://DeskToys.Core/Assets/software-update-available.ico"));
        using var pubStream = AssetLoader.Open(new Uri("avares://DeskToys.Core/Assets/app_update.pub"));
        using var reader = new StreamReader(pubStream, Encoding.UTF8);
		
        _sparkle = new CustomSparkleUpdater(url, new Ed25519Checker(SecurityMode.Strict, reader.ReadToEnd()))
        {
            UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(new WindowIcon(iconStream)),
            LogWriter = new LogWriter(LogWriterOutputMode.Console),
            CheckServerFileName = false
        };
        var dler = new WebRequestAppCastDataDownloader(_sparkle.LogWriter) { TrustEverySSLConnection = true };
        _sparkle.AppCastDataDownloader = dler;
        StartSparkle();
    }
    
    private async void StartSparkle()
    {
        await _sparkle.StartLoop(_settingsWindowViewModel?.AutoCheckForUpdates ?? true);
    }
    
    protected void ShowMainWindow()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow
                {
                    Focusable = false,
                    Topmost = true,
                    CanResize = false,
                    IsHitTestVisible = false,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Background = Brushes.Transparent,
                    TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
                    SystemDecorations = SystemDecorations.None,
                    DataContext = _mainWindowViewModel
                };
            }
            _mainWindow.Show();
            if (_mainWindowViewModel != null)
                _mainWindowViewModel.MainWindowShown = true;
        });
    }
    
    protected void HideMainWindow()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainWindow?.Close();
            _mainWindow = null;
        });
    }
    
    public void ToggleShowMainWindow()
    {
        if (_mainWindowViewModel is { MainWindowShown: true })
        {
            HideMainWindow();
        }
        else
        {
            ShowMainWindow();
        }
    }

    public void ShowSettingsWindow()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_settingsWindowViewModel is { SettingsWindowShown: false })
            {
                var window = new SettingsWindow
                {
                    Topmost = true,
                    DataContext = _settingsWindowViewModel
                };
                window.Show();
                _settingsWindowViewModel.SettingsWindowShown = true;
            }
        });
    }

    public void StartOver()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_mainWindowViewModel != null)
            {
                _mainWindowViewModel.MdText = "Ask me anything";
                _mainWindowViewModel.ImageSource = null;
            }
        });
    }
    
    public void QuitApplication()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _lifetime?.Shutdown();
        });
    }
}