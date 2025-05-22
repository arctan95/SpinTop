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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DeskToys.Core.Views;
using CommunityToolkit.Mvvm.Input;
using DeskToys.Core.Models;
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
    private ChatWindowViewModel? _chatWindowViewModel;
    private SettingsWindowViewModel? _settingsWindowViewModel;
    private MainWindowViewModel? _mainWindowViewModel;
    private ConfigService? _configService;
    private Window? _chatWindow;
    private Window? _shortcutHintWindow;
    private SparkleUpdater? _sparkle;
    
    public ICommand SettingsCommand { get; }
    
    public ICommand CheckUpdatesCommand { get; }
    public ICommand MainCommand { get; }
    public ICommand QuitCommand { get; }
    
    public App()
    {
        QuitCommand = new RelayCommand(QuitApplication);
        SettingsCommand = new RelayCommand(ShowSettingsWindow);
        MainCommand = new RelayCommand(OpenMainWindow);
        CheckUpdatesCommand = new AsyncRelayCommand(CheckUpdates);
        _chatWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<ChatWindowViewModel>();
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
        GlobalHotkeyManager.BindHotkey("hide_chat_window", ModifierMask.None, KeyCode.VcEscape,  HideChatWindow);
        
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
        FunctionRegistry.RegisterFunction("open_chat_window", ToggleShowChatWindow);
        FunctionRegistry.RegisterFunction("take_screenshot", TakeScreenshot);
        FunctionRegistry.RegisterFunction("start_over", StartOver);
        FunctionRegistry.RegisterFunction("ask_ai", AskAI);
        FunctionRegistry.RegisterFunction("quit_app", QuitApplication);
        FunctionRegistry.RegisterFunction("click_through", ToggleClickThrough);
    }
    
    public void ToggleClickThrough()
    {
        if (_chatWindow != null && _settingsWindowViewModel != null)
        {
            _settingsWindowViewModel.IgnoreMouseEvents = !_settingsWindowViewModel.IgnoreMouseEvents;;
            _settingsWindowViewModel.ConfigureWindowBehaviors(_chatWindow, new WindowBehaviorOptions
            {
                ContentProtection = _settingsWindowViewModel.ContentProtection,
                ExtendToFullScreen = _settingsWindowViewModel.ExtendToFullScreen,
                IgnoreMouseEvents = _settingsWindowViewModel.IgnoreMouseEvents,
            });
        }
    }

    public void UpdateChatWindowPosition(PixelPoint newPosition)
    {
        if (_chatWindow != null && _chatWindowViewModel != null)
        {
            Dispatcher.UIThread.InvokeAsync(() => { _chatWindow.Position = newPosition; });
        }
    }


    private void AskAI()
    {
        _chatWindowViewModel?.AskAIWithDefaultPrompt(_chatWindowViewModel.ImageSource);
    }

    public void TakeScreenshotWithCallback(Action<Bitmap?> callback)
    {
        IScreenCapturer? screenCapturer = ServiceProviderBuilder.ServiceProvider?.GetService<IScreenCapturer>();
        screenCapturer?.CaptureScreen(1920, 1080, callback);
    }

    public void TakeScreenshot()
    {
        TakeScreenshotWithCallback(bitmap =>
        {
            if (_chatWindowViewModel != null)
                _chatWindowViewModel.ImageSource = bitmap;
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
        if (_sparkle != null)
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
        if (_sparkle != null)
        {
            await _sparkle.StartLoop(_settingsWindowViewModel?.AutoCheckForUpdates ?? true);
        }
    }
    public void OpenMainWindow()
    {
        if (_mainWindowViewModel is { MainWindowShown: false })
        {
            var window = new MainWindow
            {
                DataContext = _mainWindowViewModel
            };
            window.Show();
            _mainWindowViewModel.MainWindowShown = true;
        }
    }

    public void OpenChatWindowForInput()
    {
        if (_chatWindowViewModel != null)
        {
            if (_chatWindowViewModel is { ChatWindowShown: true, Interactive: true })
            {
                return;
            }
            if (!_chatWindowViewModel.ChatWindowShown)
            {
                ShowChatWindow(true);
            }
            ForceActivateChatWindow();
                        
            _chatWindowViewModel.Interactive = true;
        }
    }
    public void ShowChatWindow()
    {
        ShowChatWindow(false);
    }
    public void ShowChatWindow(bool forceActivated)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_chatWindowViewModel != null)
            {
                if (_chatWindow == null)
                {
                    _chatWindow = new ChatWindow
                    {
                        Topmost = true,
                        CanResize = false,
                        ShowInTaskbar = false,
                        ShowActivated = forceActivated,
                        Background = Brushes.Transparent,
                        TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
                        SystemDecorations = SystemDecorations.None,
                        DataContext = _chatWindowViewModel
                    };
                }
                
                if (_shortcutHintWindow == null)
                {
                    _shortcutHintWindow = new ShortcutHintWindow
                    {
                        Topmost = true,
                        CanResize = false,
                        ShowInTaskbar = false,
                        ShowActivated = false,
                        Background = Brushes.Transparent,
                        TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
                        SystemDecorations = SystemDecorations.None,
                        DataContext = _chatWindowViewModel
                    };
                }
                
                _chatWindowViewModel.ChatWindowShown = true;
            }
            
            _chatWindow?.Show();
            _shortcutHintWindow?.Show();
        });
    }
    
    protected void HideChatWindow()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _chatWindow?.Close();
            _shortcutHintWindow?.Close();
            _chatWindow = null;
            _shortcutHintWindow = null;
        });
    }
    
    public void ToggleShowChatWindow()
    {
        if (_chatWindowViewModel is { ChatWindowShown: true })
        {
            HideChatWindow();
        }
        else
        {
            ShowChatWindow();
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
            if (_chatWindowViewModel != null)
            {
                _chatWindowViewModel.MdText = "DeskToys AI";
                _chatWindowViewModel.ImageSource = null;
                _chatWindowViewModel.UserMessage = string.Empty;
                _chatWindowViewModel.StopAIResponse();
            }
        });
    }
    public void ForceActivateChatWindow()
    {
        if (_chatWindow != null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _chatWindow.Activate();
            });
        }
    }
    
    public void QuitApplication()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _lifetime?.Shutdown();
        });
    }
}