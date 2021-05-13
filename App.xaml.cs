using Charrmander.View;
using Charrmander.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Shell;

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

            /* Setup some close event handlers.
             * First, if the menu Exit option is selected, call the [X] button
             * event handler.
             * Next, in the [X] button event handler, allow the user to abort
             * closing. If proceed, tear down everything and remove the close
             * event handlers.
             * Set in here instead of view model because I need access to view
             * model, window, and handler references.
             */
            var window = new MainWindow();
            var viewModel = new ViewModelMain();
            void menuExitHandler(object sender, EventArgs e) => window.Close();
            void xButtonHandler(object o, CancelEventArgs ce)
            {
                if (viewModel.AbortClosing()) { ce.Cancel = true; }
                else
                {
                    viewModel.Dispose();
                    viewModel.RequestClose -= menuExitHandler;
                    window.Closing -= xButtonHandler;
                }
            }

            viewModel.RequestClose += menuExitHandler;
            window.Closing += xButtonHandler;
            window.DataContext = viewModel;
            viewModel.CheckUpdate();

            string[] args = Environment.GetCommandLineArgs();
            string file = args.Length == 2 ? args[1] : null;
            if (!string.IsNullOrWhiteSpace(file) && viewModel.Open(file))
            {
                RecordRecentFile(file);
            }

            window.Show();
        }

        internal static StreamResourceInfo GetPackResourceStream(string localPath)
        {
            return GetResourceStream(
                new Uri("pack://application:,,,/Charrmander;component/" + localPath,
                UriKind.Absolute));
        }

        private static void RecordRecentFile(string filePath)
        {
            var jp = new JumpPath
            {
                Path = filePath
            };
            var jl = JumpList.GetJumpList(Application.Current);
            JumpList.AddToRecentCategory(jp);
            jl.Apply();
        }
    }
}
