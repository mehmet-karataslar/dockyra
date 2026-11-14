using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dockyra.Contracts.Models;
using Dockyra.Domain.Entities;
using Dockyra.Presentation.Wpf.ViewModels;
using Microsoft.Win32;

namespace Dockyra.Presentation.Wpf.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    private Point _dragStartPoint;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.LoadAsync();
    }

    private void OnGroupsListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void OnGroupsListPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Point currentPos = e.GetPosition(null);
        Vector diff = _dragStartPoint - currentPos;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (FindAncestorItemData<ApplicationGroup>(e.OriginalSource as DependencyObject) is { } group)
        {
            DragDrop.DoDragDrop(GroupsListBox, new DataObject("Dockyra.Group", group), DragDropEffects.Move);
        }
    }

    private async void OnGroupsListDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("Dockyra.Group"))
        {
            return;
        }

        if (e.Data.GetData("Dockyra.Group") is ApplicationGroup sourceGroup &&
            FindAncestorItemData<ApplicationGroup>(e.OriginalSource as DependencyObject) is { } targetGroup)
        {
            await _viewModel.ReorderGroupsByDropAsync(sourceGroup.Id, targetGroup.Id);
        }
    }

    private void OnGroupAppsPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void OnGroupAppsPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Point currentPos = e.GetPosition(null);
        Vector diff = _dragStartPoint - currentPos;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (FindAncestorItemData<ApplicationEntry>(e.OriginalSource as DependencyObject) is { } appEntry)
        {
            DragDrop.DoDragDrop(GroupAppsListBox, new DataObject("Dockyra.AppEntry", appEntry), DragDropEffects.Move);
        }
    }

    private void OnGroupAppsDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("Dockyra.Shortcut") || e.Data.GetDataPresent("Dockyra.AppEntry"))
        {
            e.Effects = e.Data.GetDataPresent("Dockyra.Shortcut") ? DragDropEffects.Copy : DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void OnGroupAppsDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("Dockyra.Shortcut") &&
            e.Data.GetData("Dockyra.Shortcut") is PinnedTaskbarItem shortcut)
        {
            await _viewModel.AddShortcutToSelectedGroupAsync(shortcut);
            return;
        }

        if (e.Data.GetDataPresent("Dockyra.AppEntry") &&
            e.Data.GetData("Dockyra.AppEntry") is ApplicationEntry sourceApp &&
            FindAncestorItemData<ApplicationEntry>(e.OriginalSource as DependencyObject) is { } targetApp)
        {
            await _viewModel.ReorderAppsInSelectedGroupAsync(sourceApp.Id, targetApp.Id);
        }
    }

    private void OnShortcutsPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void OnShortcutsPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Point currentPos = e.GetPosition(null);
        Vector diff = _dragStartPoint - currentPos;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (FindAncestorItemData<PinnedTaskbarItem>(e.OriginalSource as DependencyObject) is { } shortcut)
        {
            DragDrop.DoDragDrop(AvailableShortcutsListBox, new DataObject("Dockyra.Shortcut", shortcut), DragDropEffects.Copy);
        }
    }

    private async void OnBrowseExecutableClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Klasöre Eklenecek Uygulamayı Seçin",
            Filter = "Uygulamalar ve Kısayollar (*.exe;*.lnk)|*.exe;*.lnk",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.NewAppExecutablePath = dialog.FileName;
            _viewModel.NewAppDisplayName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            await _viewModel.AddCustomExecutableToGroupAsync();
        }
    }

    private static T? FindAncestorItemData<T>(DependencyObject? current) where T : class
    {
        while (current is not null)
        {
            if (current is FrameworkElement fe && fe.DataContext is T match)
            {
                return match;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
