using System;
using Avalonia.Input;
using SharpHook.Native;

namespace DeskToys.Core.Utilities;

public class KeyConvertor
{
    public static KeyModifiers ToKeyModifiers(ModifierMask mask)
    {
        KeyModifiers result = KeyModifiers.None;

        if ((mask & ModifierMask.Ctrl) != 0)
            result |= KeyModifiers.Control;

        if ((mask & ModifierMask.Shift) != 0)
            result |= KeyModifiers.Shift;

        if ((mask & ModifierMask.Alt) != 0)
            result |= KeyModifiers.Alt;

        if ((mask & ModifierMask.Meta) != 0)
            result |= KeyModifiers.Meta;

        return result;
    }

    public static Key ToKey(KeyCode keyCode)
    {
        if (Enum.TryParse<Key>(keyCode.ToString().Substring(2), out var result))
            return result;
        return Key.None;
    }
}