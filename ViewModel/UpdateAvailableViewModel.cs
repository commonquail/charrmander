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
    class UpdateAvailableViewModel : AbstractNotifier
    {
        readonly Window _window;

        private RelayCommand _cmdDownload;
        private RelayCommand _cmdClose;

        private Version _curVersion;
        private Version _latestVersion;
        private string _downloadUrl;
        private IEnumerable<XElement> _versionHistory;

        /// <summary>
        /// Creates a new <see cref="UpdateAvailableViewModel"/>. This view
        /// model has no application logic. I.e. it does not perform any update
        /// checking itself. It works strictly with properties set statically
        /// by the caller.
        /// </summary>
        public UpdateAvailableViewModel()
        {
            _window = new UpdateAvailableView(DownloadSafely)
            {
                DataContext = this
            };
            _window.Show();

            void handler(object sender, EventArgs e)
            {
                this.RequestClose -= handler;
                _window.Close();
            }

            this.RequestClose += handler;
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
            get { return _curVersion; }
            set
            {
                if (value != _curVersion)
                {
                    _curVersion = value;
                    RaisePropertyChanged(nameof(CurrentVersion));
                }
            }
        }

        /// <summary>
        /// Returns the latest available version, as supplied to the instance of this class.
        /// </summary>
        public Version LatestVersion
        {
            get { return _latestVersion; }
            set
            {
                if (value != _latestVersion)
                {
                    _latestVersion = value;
                    RaisePropertyChanged(nameof(LatestVersion));
                }
            }
        }

        /// <summary>
        /// Returns the path (URL) to the location of <see cref="LatestVersion"/>, as supplied
        /// to the instance of this class.
        /// This is not of type <see cref="Uri"/> because it's only ever needed as a string.
        /// </summary>
        public string LatestVersionPath
        {
            get { return _downloadUrl; }
            set
            {
                if (value != _downloadUrl)
                {
                    _downloadUrl = value;
                    RaisePropertyChanged(nameof(LatestVersionPath));
                }
            }
        }

        /// <summary>
        /// Returns the version history in the form of Release <see cref="XElement"/>s,
        /// as supplied to the instance of this class.
        /// </summary>
        public IEnumerable<XElement> VersionHistory
        {
            get { return _versionHistory; }
            set
            {
                if (value != _versionHistory)
                {
                    _versionHistory = value;
                    RaisePropertyChanged(nameof(VersionHistory));
                }
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
            get
            {
                if (_cmdDownload == null)
                {
                    _cmdDownload = new RelayCommand(param => this.DownloadLatest());
                }
                return _cmdDownload;
            }
        }

        /// <summary>
        /// Command to exit the application.
        /// </summary>
        public ICommand CommandClose
        {
            get
            {
                if (_cmdClose == null)
                {
                    _cmdClose = new RelayCommand(param => this.OnRequestClose());
                }
                return _cmdClose;
            }
        }

        /// <summary>
        /// Starts the default browser and navigates to <see cref="LatestVersionPath"/>.
        /// </summary>
        private void DownloadLatest()
        {
            DownloadSafely(LatestVersionPath);
        }

        private static void DownloadSafely(string url)
        {
            if (url == null)
            {
                return;
            }

            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
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
