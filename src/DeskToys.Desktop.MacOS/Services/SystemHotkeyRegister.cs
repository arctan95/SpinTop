using DeskToys.Core.Models;
using DeskToys.Core.Services;

namespace DeskToys.Desktop.MacOS.Services;

public class SystemHotkeyRegister: ISystemHotKeyRegister
{
    public bool RegisterHotkey(GlobalHotkey hotkey)
    {
        return true;
    }
}