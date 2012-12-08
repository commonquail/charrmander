﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Charrmander.Extensions;
using System.Collections.ObjectModel;

namespace Charrmander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Character> _characters = new ObservableCollection<Character>();

        private BackgroundWorker _bgUpdater = new BackgroundWorker();

        private bool _unsavedChanges = false;

        public MainWindow()
        {
            InitializeComponent();

            _bgUpdater.DoWork += UpdateWorker_DoWork;
            _bgUpdater.RunWorkerCompleted += UpdateWorker_RunWorkerCompleted;

            lstCharacters.ItemsSource = _characters;
            _characters.Add(new Character() { Name = "Bob", Profession = "Necromancer" });
        }

        private void Click_CheckUpdates(object sender, RoutedEventArgs e)
        {
            if (!_bgUpdater.IsBusy)
            {
                _bgUpdater.RunWorkerAsync();
            }
        }

        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            XDocument doc = XDocument.Load(Properties.Resources.cfgUpdateCheckUri);
            e.Result = doc;
        }

        private void UpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is InvalidOperationException)
                {
                    MessageBox.Show(
                        String.Format(Properties.Resources.msgUpdateCheckFailedBody404,
                        Properties.Resources.cfgUpdateCheckUri, e.Error.Message),
                        Properties.Resources.msgUpdateCheckFailedTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(
                        String.Format(Properties.Resources.msgUpdateCheckFailedBodyUnknown, e.Error.Message),
                        Properties.Resources.msgUpdateCheckFailedTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Debug.WriteLine("Update check failed");
            }
            else if (e.Cancelled)
            { }
            else
            {
                XDocument doc = (XDocument)e.Result;
                XElement latest = doc.Root.Element("Latest");
                Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                Version newVersion = new Version(latest.Element("Version").Value);
                if (newVersion.IsNewerThan(curVersion))
                {
                    Debug.WriteLine("New version available");
                    bool download = MessageBox.Show(
                        String.Format("New version available: {0}. Open default browser and download?", newVersion),
                        "Update Check: " + curVersion,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                    if (download)
                    {
                        Process.Start(latest.Element("DownloadUrl").Value);
                    }
                }
                else
                {
                    Debug.WriteLine("No new version available");
                }
            }
        }

        private void Command_New(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("New");
            _characters.Add(new Character() { Name = new Guid().ToString() });
        }

        private void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open");
        }

        private void Command_Save(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save");
        }

        private void Command_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("SaveAs");
        }

        private void Command_Exit(object sender, ExecutedRoutedEventArgs e)
        {
            if (_unsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedExitBody,
                Properties.Resources.msgUnsavedExitTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            Close();
        }

        private void CanExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_unsavedChanges;
        }
    }
}
