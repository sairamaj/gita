using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gita.Practice.App.Services
{
    // Attached property service that executes an ICommand when the TreeView's selected item changes.
    public static class TreeViewSelectionService
    {
        public static readonly DependencyProperty SelectedItemChangedCommandProperty = DependencyProperty.RegisterAttached(
            "SelectedItemChangedCommand",
            typeof(ICommand),
            typeof(TreeViewSelectionService),
            new PropertyMetadata(null, OnSelectedItemChangedCommandChanged));

        public static void SetSelectedItemChangedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(SelectedItemChangedCommandProperty, value);
        }

        public static ICommand? GetSelectedItemChangedCommand(DependencyObject element)
        {
            return (ICommand?)element.GetValue(SelectedItemChangedCommandProperty);
        }

        private static void OnSelectedItemChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                }
            }
        }

        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object?> e)
        {
            if (sender is TreeView treeView)
            {
                var command = GetSelectedItemChangedCommand(treeView);
                if (command != null)
                {
                    var parameter = e.NewValue;
                    if (command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                    }
                }
            }
        }
    }
}
