using System.Windows;
using System.Windows.Controls;
using Dockyra.Presentation.Wpf.ViewModels;

namespace Dockyra.Presentation.Wpf.Views;

public partial class FolderFlyoutControl : UserControl
{
    public FolderFlyoutControl()
    {
        InitializeComponent();
    }

    private void OnClosePopupButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is FolderPopupViewModel vm)
        {
            vm.IsOpen = false;
        }
    }
}
