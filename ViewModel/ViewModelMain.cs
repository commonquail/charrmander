using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Charrmander.Model;
using Charrmander.Util;
using Microsoft.Win32;
using System.IO;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Linq;

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
        private RelayCommand _cmdDeleteCharacter;

        private BackgroundWorker _bgUpdater = new BackgroundWorker();

        private FileInfo _currentFile = null;

        private string _windowTitle = "Charrmander";

        private bool _unsavedChanges = false;

        private ObservableCollection<Character> _characterList;
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

        /// <summary>
        /// Perform this event when the view wishes to close. For instance, dispose the view.
        /// </summary>
        public event EventHandler RequestClose;

        /// <summary>
        /// The title of the main window.
        /// </summary>
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                if (value != _windowTitle)
                {
                    _windowTitle = value;
                    RaisePropertyChanged("WindowTitle");
                }
            }
        }

        /// <summary>
        /// Returns <c>True</c> if there are unsaved changes in <see cref="CharacterList"/>.
        /// Also updates <see cref="WindowTitle"/>.
        /// </summary>
        public bool UnsavedChanges
        {
            get { return _unsavedChanges; }
            set
            {
                if (value != _unsavedChanges)
                {
                    _unsavedChanges = value;
                    WindowTitle = String.Format("{0}{1} - Charrmander",
                        _unsavedChanges ? "*" : string.Empty,
                        _currentFile == null ? "Unnamed" : _currentFile.Name);
                    RaisePropertyChanged("UnsavedChanges");
                }
            }
        }

        /// <summary>
        /// An <see cref="ObservableCollection"/> of <see cref="Character"/> objects loaded by the application.
        /// </summary>
        public ObservableCollection<Character> CharacterList
        {
            get
            {
                if (_characterList == null)
                {
                    CharacterList = new ObservableCollection<Character>();
                }
                return _characterList;
            }
            set
            {
                if (value != _characterList)
                {
                    _characterList = value;
                    _characterList.CollectionChanged += MarkFileDirty;
                    RaisePropertyChanged("CharacterList");
                }
            }
        }

        /// <summary>
        /// The master list of areas, generated at runtime from an embedded XML file.
        /// Compare <see cref="Character.Areas"/> against this.
        /// </summary>
        public ObservableCollection<Area> AreaReferenceList { get; set; }

        /// <summary>
        /// The <see cref="Character"/> in <see cref="CharacterList"/> that is currently selected.
        /// </summary>
        public Character SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                if (value != _selectedCharacter)
                {
                    _selectedCharacter = value;
                    if (SelectedAreaReference != null && _selectedCharacter != null)
                    {
                        SelectedAreaCharacter = null;
                        foreach (Area a in _selectedCharacter.Areas)
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
                            a.PropertyChanged += MarkFileDirty;
                            SelectedCharacter.Areas.Add(a);
                            SelectedAreaCharacter = a;
                        }
                        IsHeartsCompleted = SelectedAreaReference.Hearts == SelectedAreaCharacter.Hearts;
                        IsWaypointsCompleted = SelectedAreaReference.Waypoints == SelectedAreaCharacter.Waypoints;
                        IsPoIsCompleted = SelectedAreaReference.PoIs == SelectedAreaCharacter.PoIs;
                        IsSkillsCompleted = SelectedAreaReference.Skills == SelectedAreaCharacter.Skills;
                        IsVistasCompleted = SelectedAreaReference.Vistas == SelectedAreaCharacter.Vistas;
                        RaisePropertyChanged("Hearts");
                        RaisePropertyChanged("Waypoints");
                        RaisePropertyChanged("PoIs");
                        RaisePropertyChanged("Skills");
                        RaisePropertyChanged("Vistas");
                    }
                    IsCharacterDetailEnabled = value != null;
                    RaisePropertyChanged("SelectedCharacter");
                }
            }
        }

        /// <summary>
        /// The <see cref="Area"/> in <see cref="AreaReferenceList"/> that is currently selected.
        /// </summary>
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
                            a.PropertyChanged += MarkFileDirty;
                            SelectedCharacter.Areas.Add(a);
                            SelectedAreaCharacter = a;
                        }
                        IsHeartsCompleted = SelectedAreaReference.Hearts == SelectedAreaCharacter.Hearts;
                        IsWaypointsCompleted = SelectedAreaReference.Waypoints == SelectedAreaCharacter.Waypoints;
                        IsPoIsCompleted = SelectedAreaReference.PoIs == SelectedAreaCharacter.PoIs;
                        IsSkillsCompleted = SelectedAreaReference.Skills == SelectedAreaCharacter.Skills;
                        IsVistasCompleted = SelectedAreaReference.Vistas == SelectedAreaCharacter.Vistas;
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

        /// <summary>
        /// The <see cref="Area"/> in <see cref="SelectedCharacter.Areas"/> that is currently selected.
        /// </summary>
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

        /// <summary>
        /// Returns <c>True</c> if the application should allow editing of character details.
        /// </summary>
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

        /// <summary>
        /// The number of hearts completed by <see cref="SelectedCharacter"/>.
        /// </summary>
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
                    IsHeartsCompleted = SelectedAreaReference.Hearts == SelectedAreaCharacter.Hearts;
                    RaisePropertyChanged("Hearts");
                }
            }
        }

        /// <summary>
        /// The number of waypoints completed by <see cref="SelectedCharacter"/>.
        /// </summary>
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
                    IsWaypointsCompleted = SelectedAreaReference.Waypoints == SelectedAreaCharacter.Waypoints;
                    RaisePropertyChanged("Waypoints");
                }
            }
        }

        /// <summary>
        /// The number of PoIs completed by <see cref="SelectedCharacter"/>.
        /// </summary>
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
                    IsPoIsCompleted = SelectedAreaReference.PoIs == SelectedAreaCharacter.PoIs;
                    RaisePropertyChanged("PoIs");
                }
            }
        }

        /// <summary>
        /// The number of skills completed by <see cref="SelectedCharacter"/>.
        /// </summary>
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
                    IsSkillsCompleted = SelectedAreaReference.Skills == SelectedAreaCharacter.Skills;
                    RaisePropertyChanged("Skills");
                }
            }
        }

        /// <summary>
        /// The number of vistas completed by <see cref="SelectedCharacter"/>.
        /// </summary>
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
                    IsVistasCompleted = SelectedAreaReference.Vistas == SelectedAreaCharacter.Vistas;
                    RaisePropertyChanged("Vistas");
                }
            }
        }

        /// <summary>
        /// <c>True</c> if <see cref="SelectedCharacter"/> has completed all hearts in <see cref="SelectedArea"/>.
        /// </summary>
        public bool IsHeartsCompleted
        {
            get { return _heartsCompleted; }
            set
            {
                if (value != _heartsCompleted)
                {
                    _heartsCompleted = value;
                    RaisePropertyChanged("IsHeartsCompleted");
                }
            }
        }

        /// <summary>
        /// <c>True</c> if <see cref="SelectedCharacter"/> has completed all waypoints in <see cref="SelectedArea"/>.
        /// </summary>
        public bool IsWaypointsCompleted
        {
            get { return _waypointsCompleted; }
            set
            {
                if (value != _waypointsCompleted)
                {
                    _waypointsCompleted = value;
                    RaisePropertyChanged("IsWaypointsCompleted");
                }
            }
        }

        /// <summary>
        /// <c>True</c> if <see cref="SelectedCharacter"/> has completed all PoIs in <see cref="SelectedArea"/>.
        /// </summary>
        public bool IsPoIsCompleted
        {
            get { return _poisCompleted; }
            set
            {
                if (value != _poisCompleted)
                {
                    _poisCompleted = value;
                    RaisePropertyChanged("IsPoIsCompleted");
                }
            }
        }

        /// <summary>
        /// <c>True</c> if <see cref="SelectedCharacter"/> has completed all skills in <see cref="SelectedArea"/>.
        /// </summary>
        public bool IsSkillsCompleted
        {
            get { return _skillsCompleted; }
            set
            {
                if (value != _skillsCompleted)
                {
                    _skillsCompleted = value;
                    RaisePropertyChanged("IsSkillsCompleted");
                }
            }
        }

        /// <summary>
        /// <c>True</c> if <see cref="SelectedCharacter"/> has completed all vistas in <see cref="SelectedArea"/>.
        /// </summary>
        public bool IsVistasCompleted
        {
            get { return _vistasCompleted; }
            set
            {
                if (value != _vistasCompleted)
                {
                    _vistasCompleted = value;
                    RaisePropertyChanged("IsVistasCompleted");
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

        public ICommand CommandDeleteCharacter
        {
            get
            {
                if (_cmdDeleteCharacter == null)
                {
                    _cmdDeleteCharacter = new RelayCommand(
                        param => this.DeleteCharacter(),
                        param => this.CanDeleteCharacter());
                }
                return _cmdDeleteCharacter;
            }
        }
        #endregion

        private void New()
        {
            Debug.WriteLine("New");
            var c = new Character() { Name = new Guid().ToString() };
            c.PropertyChanged += MarkFileDirty;
            CharacterList.Add(c);
        }

        private void Open()
        {
            Debug.WriteLine("Open");
            if (UnsavedChanges && MessageBox.Show("Unsaved changes. Discard and open?",
                    "Discard changes and open?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            OpenFileDialog open = new OpenFileDialog();
            open.Filter += "Charrmander Character File (.charr)|*.charr";
            if (open.ShowDialog().Value)
            {
                if (_currentFile == null ||
                    _currentFile.FullName != open.FileName ||
                    MessageBox.Show("File already open. Would you like to reload it?",
                        "Reload file?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.ValidationType = ValidationType.Schema;
                        XmlSchemaSet xs = new XmlSchemaSet();
                        xs.Add(Properties.Resources.xNamespace,
                            XmlReader.Create(Application.GetResourceStream(
                                new Uri("Resources/charr.xsd", UriKind.Relative)).Stream));
                        settings.Schemas = xs;
                        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
                        XmlReader r = XmlReader.Create(open.FileName, settings);
                        XDocument doc = XDocument.Load(r);
                        r.Close();
                        var characters = doc.Root.Descendants(CharrElement.Charr + "Character");
                        ObservableCollection<Character> newCharacterList = new ObservableCollection<Character>();
                        foreach (var charr in characters)
                        {
                            Character c = new Character()
                            {
                                Name = charr.Element(CharrElement.Charr + "Name").Value,
                                Profession = charr.Element(CharrElement.Charr + "Profession").Value
                            };
                            try
                            {
                                var areas = charr.Element(CharrElement.Charr + "Areas").Elements(CharrElement.Charr + "Area");
                                foreach (var area in areas)
                                {
                                    Area a = new Area(area.Element(CharrElement.Charr + "Name").Value)
                                    {
                                        Hearts = area.Element(CharrElement.Charr + "Completion").Element(CharrElement.Charr + "Hearts").Value,
                                        Waypoints = area.Element(CharrElement.Charr + "Completion").Element(CharrElement.Charr + "Waypoints").Value,
                                        PoIs = area.Element(CharrElement.Charr + "Completion").Element(CharrElement.Charr + "PoIs").Value,
                                        Skills = area.Element(CharrElement.Charr + "Completion").Element(CharrElement.Charr + "Skills").Value,
                                        Vistas = area.Element(CharrElement.Charr + "Completion").Element(CharrElement.Charr + "Vistas").Value
                                    };
                                    a.PropertyChanged += MarkFileDirty;
                                    c.Areas.Add(a);
                                }
                            }
                            catch (NullReferenceException)
                            {
                                // This character didn't have any area data stored or it was malformed.
                            }
                            c.PropertyChanged += MarkFileDirty;
                            newCharacterList.Add(c);
                        }
                        CharacterList.CollectionChanged -= MarkFileDirty;
                        CharacterList = newCharacterList;
                        _currentFile = new FileInfo(open.FileName);
                        UnsavedChanges = false;
                        //txtInfo.Text = "Opened " + open.FileName;
                        //SetDataContexts();
                    }
                    catch (XmlSchemaValidationException e)
                    {
                        Debug.WriteLine(e.Message);
                        //txtInfo.Text = Properties.Resources.infErrDocValidation;
                    }
                    catch (Exception ex)
                    {
                        //txtInfo.Text = "Open failed: " + ex.Message;
                    }
                }
            }
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new XmlSchemaValidationException("Document not a GW2 Charrmander character file.", e.Exception);
        }

        private void Save()
        {
            Debug.WriteLine("Save from VM");
            if (_currentFile != null && _currentFile.Exists && !_currentFile.IsReadOnly)
            {
                DoSave(_currentFile.FullName);
            }
            else
            {
                SaveAs();
            }
        }

        private bool CanSave()
        {
            Debug.WriteLine("CanSave from VM");
            return !UnsavedChanges;
        }

        private void SaveAs()
        {
            Debug.WriteLine("SaveAs");
            SaveFileDialog save = new SaveFileDialog();

            if (_currentFile == null)
            {
                save.FileName = "GW2 Character List";
            }
            else
            {
                save.FileName = _currentFile.Name;
            }
            save.DefaultExt = ".charr";
            save.Title = "Save GW2 Character List";
            save.Filter += "Charrmander Character File (.charr)|*.charr";
            if (save.ShowDialog().Value)
            {
                DoSave(save.FileName);
            }
        }

        private void DoSave(string fileName)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = false;
            xws.Indent = true;

            try
            {
                using (XmlWriter xw = XmlWriter.Create(fileName, xws))
                {
                    new XDocument(
                        new CharrElement("Charrmander",
                            (CharacterList.Count > 0 ?
                            from c in CharacterList
                            select c.ToXML() : null)
                        )
                    ).Save(xw);
                    _currentFile = new FileInfo(fileName);
                    //UpdateTitle();
                    UnsavedChanges = false;
                }
            }
            catch (Exception e)
            {
                //txtInfo.Text = String.Format(Properties.Resources.infErrSaveFailed, fileName);
                Debug.WriteLine("Error saving: " + e.Message);
            }
        }

        private void OnRequestClose()
        {
            if (UnsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedExitBody,
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

        private void DeleteCharacter()
        {
            Character c = SelectedCharacter;
            if (c != null)
            {
                c.PropertyChanged -= MarkFileDirty;
                foreach (Area a in c.Areas)
                {
                    a.PropertyChanged -= MarkFileDirty;
                }
                CharacterList.Remove(c);
            }
        }

        private bool CanDeleteCharacter()
        {
            return IsCharacterDetailEnabled;
        }

        private void MarkFileDirty(object o, EventArgs err)
        {
            UnsavedChanges = true;
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
