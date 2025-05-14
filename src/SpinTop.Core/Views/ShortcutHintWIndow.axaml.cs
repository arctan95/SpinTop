using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SpinTop.Core.Models;

namespace SpinTop.Core.Views;

public partial class ShortcutHintWindow : Window
{
    public ShortcutHintWindow()
    {
        InitializeComponent();
    }

    private void PositionWindowAtLeftBottom()
    {
        var screen = Screens.Primary;
        if (screen != null)
        {
            Position = new PixelPoint(10, screen.Bounds.Y + screen.Bounds.Bottom - (int)Height - 10);
        }
    }
    
    protected override void OnOpened(EventArgs e)
    {
        ConfigureWindow();
        PositionWindowAtLeftBottom();
        base.OnOpened(e);
    }

    private void ConfigureWindow()
    {
        if (Application.Current is App app)
        {
            app.ConfigureWindowBehaviors(this, new WindowBehaviorOptions
            {
                ContentProtection = true, OverlayWindow = true, IgnoreMouseEvents = true
            });
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        Opacity = 1;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Opacity = 0;
    }
}