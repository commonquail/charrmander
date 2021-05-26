using Charrmander.Util;
using Charrmander.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Charrmander.ViewModel
{
    internal class UpdateAvailableViewModel : AbstractNotifier
    {
        private readonly Window _window;

        private readonly Version _curVersion = default!;
        private readonly Version _latestVersion = default!;
        private readonly string? _downloadUrl;
        private readonly IEnumerable<XElement> _versionHistory = default!;

        /// <summary>
        /// Creates a new <see cref="UpdateAvailableViewModel"/>. This view
        /// model has no application logic. I.e. it does not perform any update
        /// checking itself. It works strictly with properties set statically
        /// by the caller.
        /// </summary>
        public UpdateAvailableViewModel(
            Version curVersion,
            Version latestVersion,
            string? downloadUrl,
            IEnumerable<XElement> versionHistory)
        {
            _window = new UpdateAvailableView(DownloadSafely)
            {
                DataContext = this
            };
            _window.Show();

            void handler(object? sender, EventArgs e)
            {
                this.RequestClose -= handler;
                _window.Close();
            }

            this.RequestClose += handler;
            this.CurrentVersion = curVersion;
            this.LatestVersion = latestVersion;
            this.LatestVersionPath = downloadUrl;
            this.VersionHistory = versionHistory;
        }

        /// <summary>
        /// Signals this window to close.
        /// </summary>
        public void Close()
        {
            OnRequestClose();
        }

        /// <summary>
        /// Returns the current version, as supplied to the instance of this class.
        /// </summary>
        public Version CurrentVersion
        {
            get => _curVersion;
            private init
            {
                _curVersion = value;
                RaisePropertyChanged(nameof(CurrentVersion));
            }
        }

        /// <summary>
        /// Returns the latest available version, as supplied to the instance of this class.
        /// </summary>
        public Version LatestVersion
        {
            get => _latestVersion;
            init
            {
                _latestVersion = value;
                RaisePropertyChanged(nameof(LatestVersion));
            }
        }

        /// <summary>
        /// Returns the path (URL) to the location of <see cref="LatestVersion"/>, as supplied
        /// to the instance of this class.
        /// This is not of type <see cref="Uri"/> because it's only ever needed as a string.
        /// </summary>
        public string? LatestVersionPath
        {
            get => _downloadUrl;
            init
            {
                _downloadUrl = value;
                RaisePropertyChanged(nameof(LatestVersionPath));
            }
        }

        /// <summary>
        /// Returns the version history in the form of Release <see cref="XElement"/>s,
        /// as supplied to the instance of this class.
        /// </summary>
        public IEnumerable<XElement> VersionHistory
        {
            get => _versionHistory;
            init
            {
                _versionHistory = value;
                RaisePropertyChanged(nameof(VersionHistory));
            }
        }

        /// <summary>
        /// Perform this event when the view wishes to close. For instance, dispose the view.
        /// </summary>
        public event EventHandler RequestClose;

        /// <summary>
        /// Command to start the default browser and download the latest version.
        /// </summary>
        public ICommand CommandDownload
        {
            get => new RelayCommand(_ => this.DownloadLatest());
        }

        /// <summary>
        /// Command to exit the application.
        /// </summary>
        public ICommand CommandClose
        {
            get => new RelayCommand(_ => this.OnRequestClose());
        }

        /// <summary>
        /// Starts the default browser and navigates to <see cref="LatestVersionPath"/>.
        /// </summary>
        private void DownloadLatest()
        {
            DownloadSafely(LatestVersionPath);
        }

        private static void DownloadSafely(string? url)
        {
            if (url == null)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.msgDownloadUpdateFailedBody, url, e.Message),
                    Properties.Resources.msgDownloadUpdateFailedTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Informs the application handler that this window would like to close.
        /// If there are unsaved changes the user is alerted and allowed to abort.
        /// <seealso cref="UnsavedChanges"/>
        /// </summary>
        private void OnRequestClose()
        {
            this.RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
