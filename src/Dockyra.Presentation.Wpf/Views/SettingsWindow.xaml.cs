using System.Windows;
using Dockyra.Presentation.Wpf.ViewModels;

namespace Dockyra.Presentation.Wpf.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}

