using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DeskToys.Core.Services;
using DeskToys.Core.Utilities;
using DeskToys.Core.ViewModels;

namespace DeskToys.Core.Views;

public partial class SettingsWindow: Window
{
    
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnTextBoxFocused(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.Watermark = string.Empty;
            textBox.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                string functionName = StringExtensions.ToSnakeCase(textBox.Name!);
                GlobalHotkeyRecorder.StartRecording(hotkey =>
                {
                    var key = KeyConvertor.ToKey(hotkey.Key);
                    var modifiers = KeyConvertor.ToKeyModifiers(hotkey.Modifier);
                    if ( key != Key.None && modifiers != KeyModifiers.None)
                    {
                        viewModel.RecordHotKey(functionName, hotkey);
                        Dispatcher.UIThread.InvokeAsync(() => textBox.Text = PlatformKeyGestureConverter.ToPlatformString(new KeyGesture(key, modifiers)));
                    }
                });
            }
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
    }
    
    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.Watermark = "Press shortcut";
            GlobalHotkeyRecorder.StopRecording();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            viewModel.SettingsWindowShown = false;
        }
        base.OnClosing(e);
    }
}