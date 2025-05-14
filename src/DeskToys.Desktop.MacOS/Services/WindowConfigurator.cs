using DeskToys.Core.Services;
using ObjCRuntime;

namespace DeskToys.Desktop.MacOS.Services;

public class WindowConfigurator: IWindowConfigurator
{

    public void SetIgnoresMouseEvents(IntPtr handle, bool ignoreMouseEvents)
    {
        var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
        if (nsWindow != null)
        {
            nsWindow.IgnoresMouseEvents = ignoreMouseEvents;
        }
    }

    public void ExtendToFullScreen(IntPtr handle, bool extendToFullScreen)
    {
        if (extendToFullScreen)
        {
            var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
            if (nsWindow != null)
            {
                nsWindow.Level = NSWindowLevel.PopUpMenu;
            }
        }
    }

    public void SetContentProtection(IntPtr handle, bool contentProtection)
    {
        if (contentProtection)
        {
            var nsWindow = Runtime.GetNSObject<NSWindow>(handle);
            if (nsWindow != null)
            {
                nsWindow.SharingType = NSWindowSharingType.None;
            }
        }
    }
}