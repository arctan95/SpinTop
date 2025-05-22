using System;
using Avalonia;

namespace DeskToys.Core.Views;

public partial class ShortcutHintWindow : AppWindow
{
    public ShortcutHintWindow()
    {
        InitializeComponent();
        PositionWindowAtLeftBottom();
    }

    private void PositionWindowAtLeftBottom()
    {
        var screen = Screens.Primary;
        if (screen != null)
        {
            Position = new PixelPoint(screen.Bounds.X + 10, screen.Bounds.Y + 10);
        }
    }

    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        PositionWindowAtLeftBottom();
    }
}