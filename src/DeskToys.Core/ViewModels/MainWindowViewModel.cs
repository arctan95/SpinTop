using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskToys.Core.ViewModels;

public partial class MainWindowViewModel: ViewModelBase
{
    [ObservableProperty]
    private bool _mainWindowShown;
}