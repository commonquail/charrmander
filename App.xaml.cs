using System;
using System.Windows;
using Charrmander.View;
using Charrmander.ViewModel;

namespace Charrmander
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var file = "";
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                file = args[1];
            }

            var window = new MainWindow();
            var viewModel = new ViewModelMain(file);
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                window.Close();
            };
            viewModel.RequestClose += handler;

            window.DataContext = viewModel;
            window.Show();
        }
    }
}
