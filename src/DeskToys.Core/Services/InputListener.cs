using System;
using System.Diagnostics;
using Avalonia;
using DeskToys.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using SharpHook.Native;

namespace DeskToys.Core.Services;

public class InputListener
{
    private static MainWindowViewModel? _mainWindowViewModel;
    private static IGlobalHook? _globalHook;
    private static DateTime _lastAltTime = DateTime.MinValue;
    private const int DoubleTapThresholdMs = 300;

    
    static InputListener()
    {
        _globalHook = new TaskPoolGlobalHook(1, GlobalHookType.All, null, true);
        _globalHook.KeyPressed += GlobalHotkeyManager.OnKeyPressed;
        _globalHook.KeyPressed += GlobalHotkeyRecorder.OnKeyPressed;
        _globalHook.KeyPressed += ScrollOnArrow;
        _globalHook.MouseMoved += OnMouseMoved;
        _globalHook.MouseDragged += OnMouseMoved;
        _mainWindowViewModel = ServiceProviderBuilder.ServiceProvider?.GetRequiredService<MainWindowViewModel>();
    }
    
    
    public static void SetupSystemHook()
    {
        try
        {
            _globalHook?.RunAsync();
            Debug.WriteLine("Hook started.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting hook: {ex.Message}");
        }
    }
    
    
    static void OnMouseMoved(object? sender, MouseHookEventArgs e)
    {
        _mainWindowViewModel?.OnMouseMoved(e.Data.X, e.Data.Y);
    }

    static void ScrollOnArrow(object? sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode is KeyCode.VcLeftAlt or KeyCode.VcRightAlt)
        {
            var now = DateTime.Now;
            var timeDelta = now - _lastAltTime;
            
            if (timeDelta.TotalMilliseconds < DoubleTapThresholdMs)
            {
                _mainWindowViewModel?.ToggleFollowPointer();
                _mainWindowViewModel?.ToggleMdScrollable();
            }
            _lastAltTime = now;
        }
        
        if (_mainWindowViewModel is { MdScrollable: true })
        {
            if (e.RawEvent.Mask.HasFlag(ModifierMask.LeftCtrl) && e.RawEvent.Mask.HasFlag(ModifierMask.LeftAlt))
            {
                double delta = 25;
                Vector offset = e.Data.KeyCode switch
                {
                    KeyCode.VcUp => new Vector(0, -delta),
                    KeyCode.VcDown => new Vector(0, delta),
                    KeyCode.VcLeft => new Vector(-delta, 0),
                    KeyCode.VcRight => new Vector(delta, 0),
                    _ => Vector.Zero
                };

                ScrollMarkdown(offset);
            }
           
        }
    }
    
    
    static void ScrollMarkdown(Vector offset)
    {
        _mainWindowViewModel?.ScrollMarkdown(offset);
    }

    
}