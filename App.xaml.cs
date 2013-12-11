using System;
using System.Windows;
using System.ComponentModel;
using Charrmander.View;
using Charrmander.ViewModel;

namespace Charrmander
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ?
                args.Name.Substring(0, args.Name.IndexOf(',')) :
                args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources"))
            {
                return null;
            }

            System.Resources.ResourceManager rm =
                new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources",
                    System.Reflection.Assembly.GetExecutingAssembly());

            byte[] bytes = (byte[])rm.GetObject(dllName);

            return System.Reflection.Assembly.Load(bytes);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var file = "";
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                file = args[1];
            }

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
            var viewModel = new ViewModelMain(file);
            EventHandler menuExitHandler = null;
            menuExitHandler = delegate { window.Close(); };
            viewModel.RequestClose += menuExitHandler;

            CancelEventHandler xButtonHandler = null;
            xButtonHandler = delegate(object o, CancelEventArgs ce)
            {
                if (viewModel.AbortClosing()) { ce.Cancel = true; }
                else
                {
                    viewModel.Dispose();
                    viewModel.RequestClose -= menuExitHandler;
                    window.Closing -= xButtonHandler;
                }
            };
            window.Closing += xButtonHandler;
            window.DataContext = viewModel;
            window.Show();
            viewModel.CheckUpdate();
        }
    }
}
