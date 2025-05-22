using DeskToys.Core.Services;
using ObjCRuntime;

namespace DeskToys.Desktop.MacOS.Services;

public class WindowConfigurator: IWindowConfigurator
{

    public void SetIgnoresMouseEvents(IntPtr handle, bool ignoreMouseEvents)
    {
        NSApplication.SharedApplication.InvokeOnMainThread(() =>
        {
            var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
            if (nsWindow != null)
            {
                nsWindow.IgnoresMouseEvents = ignoreMouseEvents;
            }
        });
    }

    public void SetOverlayWindow(IntPtr handle, bool extendToFullScreen)
    {
        if (extendToFullScreen)
        {
            NSApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
                if (nsWindow != null)
                {
                    nsWindow.Level = NSWindowLevel.PopUpMenu;
                    nsWindow.StyleMask = NSWindowStyle.Borderless;
                }
            });
        }
    }

    public void SetContentProtection(IntPtr handle, bool contentProtection)
    {
        if (contentProtection)
        {
            NSApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
                if (nsWindow != null)
                {
                    nsWindow.SharingType = NSWindowSharingType.None;
                }
            });
        }
    }
}