using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
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
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = typeof(App).Namespace + '.'
                + new AssemblyName(args.Name).Name;

            if (assemblyName.EndsWith(".resources"))
            {
                return null;
            }

            var dllArchive = assemblyName + ".dll.gz";

            using (var zipped = new GZipStream(
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(dllArchive) ?? Stream.Null,
                CompressionMode.Decompress))
            {
                using (var outstream = new MemoryStream())
                {
                    CopyTo(zipped, outstream);
                    return Assembly.Load(outstream.GetBuffer());
                }
            }
        }

        private static void CopyTo(Stream source, Stream destination)
        {
            Debug.Assert(source != null);
            Debug.Assert(destination != null);

            var buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
            }
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
            viewModel.CheckUpdate();
            window.Show();
        }
    }
}
