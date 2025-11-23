using System.Windows.Controls;
using Gita.Practice.App.ViewModels;

namespace Gita.Practice.App.Views;

public partial class AudioPlayerControl : UserControl
{
    public AudioPlayerControl()
    {
        InitializeComponent();
        DataContext = new AudioPlayerViewModel(mediaElement);
    }
}
