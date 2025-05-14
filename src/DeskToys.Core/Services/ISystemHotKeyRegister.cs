using DeskToys.Core.Models;

namespace DeskToys.Core.Services;

public interface ISystemHotKeyRegister
{
    bool RegisterHotkey(GlobalHotkey hotkey);
}