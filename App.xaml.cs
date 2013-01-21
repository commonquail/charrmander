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

            var window = new MainWindow();
            var viewModel = new ViewModelMain(file);
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.Dispose();
                viewModel.RequestClose -= handler;
                window.Close();
            };
            viewModel.RequestClose += handler;

            window.DataContext = viewModel;
            window.Show();
            viewModel.CheckUpdate();
        }
    }
}
