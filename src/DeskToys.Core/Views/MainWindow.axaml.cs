using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DeskToys.Core.ViewModels;

namespace DeskToys.Core.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    { 
        InitializeComponent();
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindowShown = false;
        }
        base.OnClosing(e);
    }
    
    private void OnAIChatClicked(object? sender, PointerPressedEventArgs e)
    {
        Close();
        if (Application.Current is App app)
        {
            app.OpenChatWindowForInput();
        }
    }
    
}