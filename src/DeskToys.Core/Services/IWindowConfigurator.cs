using System;
using Avalonia.Controls;
using DeskToys.Core.Models;

namespace DeskToys.Core.Services;

public interface IWindowConfigurator
{
    public void ConfigureWindow(Window window, WindowBehaviorOptions windowBehaviorOptions)
    {
        if (window.TryGetPlatformHandle()?.Handle is { } handle)
        {
            ExtendToFullScreen(handle, windowBehaviorOptions.ExtendToFullScreen);
            SetIgnoresMouseEvents(handle, windowBehaviorOptions.IgnoreMouseEvents);
            SetContentProtection(handle, windowBehaviorOptions.ContentProtection);
        }
    }
    
    public void SetIgnoresMouseEvents(IntPtr window, bool ignoreMouseEvents);
    public void ExtendToFullScreen(IntPtr window, bool extendToFullScreen);
    public void SetContentProtection(IntPtr window, bool contentProtection);
}