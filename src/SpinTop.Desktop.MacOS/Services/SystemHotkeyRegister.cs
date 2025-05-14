using SpinTop.Core.Models;
using SpinTop.Core.Services;

namespace SpinTop.Desktop.MacOS.Services;

public class SystemHotkeyRegister: ISystemHotKeyRegister
{
    public bool RegisterHotkey(GlobalHotkey hotkey)
    {
        return true;
    }
}