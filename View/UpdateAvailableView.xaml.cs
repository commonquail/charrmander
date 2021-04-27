using System.Diagnostics;
using System.Windows;

namespace Charrmander.View
{
    /// <summary>
    /// Interaction logic for UpdateAvailableView.xaml
    /// </summary>
    public partial class UpdateAvailableView : Window
    {
        public UpdateAvailableView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
