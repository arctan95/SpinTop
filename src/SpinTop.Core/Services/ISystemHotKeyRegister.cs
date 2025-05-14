using SpinTop.Core.Models;

namespace SpinTop.Core.Services;

public interface ISystemHotKeyRegister
{
    bool RegisterHotkey(GlobalHotkey hotkey);
}