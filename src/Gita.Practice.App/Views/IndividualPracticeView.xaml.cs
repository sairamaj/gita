using Gita.Practice.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Gita.Practice.App.Views
{
    /// <summary>
    /// Interaction logic for IndividualPracticeView.xaml
    /// </summary>
    public partial class IndividualPracticeView : UserControl
    {
        public IndividualPracticeView()
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
            if (e.OldValue is IndividualPracticeViewModel oldVm)
                oldVm.MediaElement = null;

            TryAttachMediaElement(e.NewValue);
        }

        private void TryAttachMediaElement(object? dataContext)
        {
            if (dataContext is IndividualPracticeViewModel vm)
                vm.MediaElement = mediaElement;
        }
    }
}
