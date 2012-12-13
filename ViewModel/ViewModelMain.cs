﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Charrmander.Model;
using Charrmander.Util;

namespace Charrmander.ViewModel
{
    class ViewModelMain : AbstractNotifier
    {
        private RelayCommand _cmdNew;
        private RelayCommand _cmdOpen;
        private RelayCommand _cmdSave;
        private RelayCommand _cmdSaveAs;
        private RelayCommand _cmdClose;
        private RelayCommand _cmdCheckUpdate;

        private BackgroundWorker _bgUpdater = new BackgroundWorker();

        private bool _unsavedChanges = false;

        private Character _selectedCharacter;
        private Area _selectedAreaReference;
        private Area _selectedAreaCharacter;

        private bool _isCharacterDetailEnabled = false;

        private bool _heartsCompleted = false;
        private bool _waypointsCompleted = false;
        private bool _poisCompleted = false;
        private bool _skillsCompleted = false;
        private bool _vistasCompleted = false;

        public ViewModelMain()
        {
            this.CharacterList = new ObservableCollection<Character>();
            CharacterList.Add(new Character() { Name = "Bob", Profession = "Necromancer" });

            AreaReferenceList = new ObservableCollection<Area>();

            var doc = XDocument.Load(XmlReader.Create(Application.GetResourceStream(
                new Uri("Resources/Areas.xml", UriKind.Relative)).Stream));

            foreach (XElement xe in doc.Root.Elements("Area"))
            {
                AreaReferenceList.Add(new Area(xe.Element("Name").Value)
                {
                    Hearts = xe.Element("Hearts").Value,
                    Waypoints = xe.Element("Waypoints").Value,
                    PoIs = xe.Element("PoIs").Value,
                    Skills = xe.Element("Skills").Value,
                    Vistas = xe.Element("Vistas").Value
                });
            }
            _bgUpdater.DoWork += UpdateWorker_DoWork;
            _bgUpdater.RunWorkerCompleted += UpdateWorker_RunWorkerCompleted;
        }

        public event EventHandler RequestClose;

        public ObservableCollection<Character> CharacterList { get; set; }

        public ObservableCollection<Area> AreaReferenceList { get; set; }

        public Character SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                if (value != _selectedCharacter)
                {
                    _selectedCharacter = value;
                    IsCharacterDetailEnabled = value != null;
                    RaisePropertyChanged("SelectedCharacter");
                }
            }
        }

        public Area SelectedAreaReference
        {
            get { return _selectedAreaReference; }
            set
            {
                if (value != _selectedAreaReference)
                {
                    _selectedAreaReference = value;
                    if (SelectedCharacter != null)
                    {
                        foreach (Area a in SelectedCharacter.Areas)
                        {
                            if (a.Name == SelectedAreaReference.Name)
                            {
                                SelectedAreaCharacter = a;
                                break;
                            }
                        }
                        if (SelectedAreaCharacter == null
                            || SelectedAreaCharacter.Name != SelectedAreaReference.Name)
                        {
                            var a = new Area(SelectedAreaReference.Name);
                            SelectedCharacter.Areas.Add(a);
                            SelectedAreaCharacter = a;
                        }
                        HeartsCompleted = SelectedAreaReference.Hearts == SelectedAreaCharacter.Hearts;
                        WaypointsCompleted = SelectedAreaReference.Waypoints == SelectedAreaCharacter.Waypoints;
                        PoIsCompleted = SelectedAreaReference.PoIs == SelectedAreaCharacter.PoIs;
                        SkillsCompleted = SelectedAreaReference.Skills == SelectedAreaCharacter.Skills;
                        VistasCompleted = SelectedAreaReference.Vistas == SelectedAreaCharacter.Vistas;
                        RaisePropertyChanged("Hearts");
                        RaisePropertyChanged("Waypoints");
                        RaisePropertyChanged("PoIs");
                        RaisePropertyChanged("Skills");
                        RaisePropertyChanged("Vistas");
                    }
                    RaisePropertyChanged("SelectedAreaReference");
                }
            }
        }

        public Area SelectedAreaCharacter
        {
            get { return _selectedAreaCharacter; }
            set
            {
                if (value != _selectedAreaCharacter)
                {
                    _selectedAreaCharacter = value;
                    RaisePropertyChanged("SelectedAreaCharacter");
                }
            }
        }

        public bool IsCharacterDetailEnabled
        {
            get { return _isCharacterDetailEnabled; }
            set
            {
                if (value != _isCharacterDetailEnabled)
                {
                    _isCharacterDetailEnabled = value;
                    RaisePropertyChanged("IsCharacterDetailEnabled");
                }
            }
        }

        #region Area Completion
        public string Hearts
        {
            get
            {
                if (SelectedCharacter != null && SelectedAreaReference != null)
                {
                    return SelectedAreaCharacter.Hearts;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedAreaCharacter != null && value != SelectedAreaCharacter.Hearts && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Hearts = value;
                    HeartsCompleted = SelectedAreaReference.Hearts == SelectedAreaCharacter.Hearts;
                    RaisePropertyChanged("Hearts");
                }
            }
        }

        public string Waypoints
        {
            get
            {
                if (SelectedCharacter != null && SelectedAreaReference != null)
                {
                    return SelectedAreaCharacter.Waypoints;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedAreaCharacter != null && value != SelectedAreaCharacter.Waypoints && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Waypoints = value;
                    WaypointsCompleted = SelectedAreaReference.Waypoints == SelectedAreaCharacter.Waypoints;
                    RaisePropertyChanged("Waypoints");
                }
            }
        }

        public string PoIs
        {
            get
            {
                if (SelectedCharacter != null && SelectedAreaReference != null)
                {
                    return SelectedAreaCharacter.PoIs;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedAreaCharacter != null && value != SelectedAreaCharacter.PoIs && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.PoIs = value;
                    PoIsCompleted = SelectedAreaReference.PoIs == SelectedAreaCharacter.PoIs;
                    RaisePropertyChanged("PoIs");
                }
            }
        }

        public string Skills
        {
            get
            {
                if (SelectedCharacter != null && SelectedAreaReference != null)
                {
                    return SelectedAreaCharacter.Skills;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedAreaCharacter != null && value != SelectedAreaCharacter.Skills && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Skills = value;
                    SkillsCompleted = SelectedAreaReference.Skills == SelectedAreaCharacter.Skills;
                    RaisePropertyChanged("Skills");
                }
            }
        }

        public string Vistas
        {
            get
            {
                if (SelectedCharacter != null && SelectedAreaReference != null)
                {
                    return SelectedAreaCharacter.Vistas;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedAreaCharacter != null && value != SelectedAreaCharacter.Vistas && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Vistas = value;
                    VistasCompleted = SelectedAreaReference.Vistas == SelectedAreaCharacter.Vistas;
                    RaisePropertyChanged("Vistas");
                }
            }
        }

        public bool HeartsCompleted
        {
            get { return _heartsCompleted; }
            set
            {
                if (value != _heartsCompleted)
                {
                    _heartsCompleted = value;
                    RaisePropertyChanged("HeartsCompleted");
                }
            }
        }

        public bool WaypointsCompleted
        {
            get { return _waypointsCompleted; }
            set
            {
                if (value != _waypointsCompleted)
                {
                    _waypointsCompleted = value;
                    RaisePropertyChanged("WaypointsCompleted");
                }
            }
        }

        public bool PoIsCompleted
        {
            get { return _poisCompleted; }
            set
            {
                if (value != _poisCompleted)
                {
                    _poisCompleted = value;
                    RaisePropertyChanged("PoIsCompleted");
                }
            }
        }

        public bool SkillsCompleted
        {
            get { return _skillsCompleted; }
            set
            {
                if (value != _skillsCompleted)
                {
                    _skillsCompleted = value;
                    RaisePropertyChanged("SkillsCompleted");
                }
            }
        }

        public bool VistasCompleted
        {
            get { return _vistasCompleted; }
            set
            {
                if (value != _vistasCompleted)
                {
                    _vistasCompleted = value;
                    RaisePropertyChanged("VistasCompleted");
                }
            }
        }
        #endregion // Area Completion

        #region ICommand Implementations
        public ICommand CommandNew
        {
            get
            {
                if (_cmdNew == null)
                {
                    _cmdNew = new RelayCommand(param => this.New());
                }
                return _cmdNew;
            }
        }

        public ICommand CommandOpen
        {
            get
            {
                if (_cmdOpen == null)
                {
                    _cmdOpen = new RelayCommand(param => this.Open());
                }
                return _cmdOpen;
            }
        }

        public ICommand CommandSave
        {
            get
            {
                if (_cmdSave == null)
                {
                    _cmdSave = new RelayCommand(param => this.Save(), param => this.CanSave());
                }
                return _cmdSave;
            }
        }

        public ICommand CommandSaveAs
        {
            get
            {
                if (_cmdSaveAs == null)
                {
                    _cmdSaveAs = new RelayCommand(param => this.SaveAs());
                }
                return _cmdSaveAs;
            }
        }

        public ICommand CommandExit
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

        public ICommand CommandCheckUpdate
        {
            get
            {
                if (_cmdCheckUpdate == null)
                {
                    _cmdCheckUpdate = new RelayCommand(param => this.CheckUpdate());
                }
                return _cmdCheckUpdate;
            }
        }
#endregion

        private void New()
        {
            Debug.WriteLine("New");
            CharacterList.Add(new Character() { Name = new Guid().ToString() });
        }

        private void Open()
        {
            Debug.WriteLine("Open");
        }

        private void Save()
        {
            Debug.WriteLine("Save from VM");
        }

        private bool CanSave()
        {
            Debug.WriteLine("CanSave from VM");
            return !_unsavedChanges;
        }

        private void SaveAs()
        {
            Debug.WriteLine("SaveAs");
        }

        private void OnRequestClose()
        {
            if (_unsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedExitBody,
                Properties.Resources.msgUnsavedExitTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            EventHandler handler = this.RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void CheckUpdate()
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
    }
}
