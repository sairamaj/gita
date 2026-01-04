using System.Windows;
using System.Windows.Media.Imaging;

namespace Gita.Practice.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set the icon programmatically - using Krishna and Arjuna image
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/krishna-arjuna-icon.png", UriKind.Absolute);
            var streamResourceInfo = Application.GetResourceStream(uri);
            if (streamResourceInfo != null)
            {
                var bitmapDecoder = BitmapDecoder.Create(streamResourceInfo.Stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                Icon = bitmapDecoder.Frames[0];
            }
        }
        catch (System.Exception)
        {
            // Icon loading failed, continue without icon
        }
    }
}