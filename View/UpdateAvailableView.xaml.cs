using System;
using System.Windows;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for UpdateAvailableView.xaml
    /// </summary>
    public partial class UpdateAvailableView : Window
    {
        private readonly Action<string> downloader;

        public UpdateAvailableView(Action<string> Downloader)
        {
            InitializeComponent();
            this.downloader = Downloader;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            downloader(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
