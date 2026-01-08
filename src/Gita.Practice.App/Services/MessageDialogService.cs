using System.Windows;
using System.Windows.Media;
using Gita.Practice.App.Views;

namespace Gita.Practice.App.Services;

public class MessageDialogService : IMessageDialogService
{
    public void ShowError(string message, string? title = null)
    {
        var dialog = new MessageDialog
        {
            DialogTitle = title ?? "Error",
            Message = message,
            IconText = "✕",
            IconColor = new SolidColorBrush(Color.FromRgb(232, 17, 35)), // Red
            ButtonText = "OK",
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    public void ShowInformation(string message, string? title = null)
    {
        var dialog = new MessageDialog
        {
            DialogTitle = title ?? "Information",
            Message = message,
            IconText = "ℹ",
            IconColor = new SolidColorBrush(Color.FromRgb(0, 120, 212)), // Blue
            ButtonText = "OK",
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    public void ShowWarning(string message, string? title = null)
    {
        var dialog = new MessageDialog
        {
            DialogTitle = title ?? "Warning",
            Message = message,
            IconText = "⚠",
            IconColor = new SolidColorBrush(Color.FromRgb(255, 185, 0)), // Orange
            ButtonText = "OK",
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }

    public bool ShowQuestion(string message, string? title = null)
    {
        var dialog = new MessageDialog
        {
            DialogTitle = title ?? "Question",
            Message = message,
            IconText = "?",
            IconColor = new SolidColorBrush(Color.FromRgb(0, 120, 212)), // Blue
            ButtonText = "OK",
            Owner = Application.Current.MainWindow
        };
        return dialog.ShowDialog() == true;
    }
}

