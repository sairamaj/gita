using System.Windows;
using System.Windows.Controls;
using Gita.Practice.App.ViewModels;

namespace Gita.Practice.App.Views;

public partial class GroupPracticeView : UserControl
{
    public GroupPracticeView()
    {
        InitializeComponent();

        // If DataContext is set later, handle it
        this.DataContextChanged += AudioPlayerControl_DataContextChanged;

        // If DataContext already set by the time Loaded fires, attach then as a fallback
        this.Loaded += (_, _) => TryAttachMediaElement(this.DataContext);
    }

    private void AudioPlayerControl_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        // Clear old VM reference to avoid holding the view alive
        if (e.OldValue is GroupPracticeViewModel oldVm)
            oldVm.MediaElement = null;

        TryAttachMediaElement(e.NewValue);
    }

    private void TryAttachMediaElement(object? dataContext)
    {
        if (dataContext is GroupPracticeViewModel vm)
            vm.MediaElement = mediaElement;
    }
}
