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
using Charrmander.View;
using Microsoft.Win32;
using System.IO;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Linq;
using BrendanGrant.Helpers.FileAssociation;
using System.Collections;

namespace Charrmander.ViewModel
{
    class ViewModelMain : AbstractNotifier, IViewModel
    {
        #region Fields

        private RelayCommand _cmdNew;
        private RelayCommand _cmdOpen;
        private RelayCommand _cmdSave;
        private RelayCommand _cmdSaveAs;
        private RelayCommand _cmdClose;
        private RelayCommand _cmdCheckUpdate;
        private RelayCommand _cmdDeleteCharacter;
        private RelayCommand _cmdRegisterExtensions;

        private BackgroundWorker _bgUpdater = new BackgroundWorker();
        private UpdateAvailableViewModel _updateViewModel;

        private FileInfo _currentFile = null;

        private string _windowTitle = "Charrmander";

        private bool _unsavedChanges = false;

        private ObservableCollection<Character> _characterList;
        private Character _selectedCharacter;
        private Area _selectedAreaReference;
        private Area _selectedAreaCharacter;

        private bool _isCharacterDetailEnabled = false;

        private IDictionary<string, object> _biographyOptionsProfession;
        private ObservableCollection<string> _biographyOptionsPersonality;
        private IDictionary<string, IDictionary<string, ObservableCollection<string>>> _biographyOptionsRace;

        private ObservableCollection<string>
            _selectedBiographyOptionsProfession,
            _selectedBiographyOptionsPersonality,
            _selectedBiographyOptionsRaceFirst,
            _selectedBiographyOptionsRaceSecond,
            _selectedBiographyOptionsRaceThird;

        #endregion

        public ViewModelMain(string filePath)
        {
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

            var races = XDocument.Load(XmlReader.Create(Application.GetResourceStream(
                new Uri("Resources/Races.xml", UriKind.Relative)).Stream)).Root.Elements("Race");

            var biographies = XDocument.Load(XmlReader.Create(Application.GetResourceStream(
                new Uri("Resources/Biographies.xml", UriKind.Relative)).Stream));

            _biographyOptionsProfession = new Dictionary<string, object>(8);
            foreach (XElement xe in biographies.Root.Element("Professions").Elements())
            {
                if (xe.Name.LocalName == "Ranger")
                {
                    var d = new Dictionary<string, ObservableCollection<string>>();
                    foreach (var race in races)
                    {
                        string key = race.Element("Name").Value;
                        d[key] = new ObservableCollection<string>(
                            from bo in xe.Element(key).Elements() select bo.Value);
                    }
                    _biographyOptionsProfession[xe.Name.LocalName] = d;
                }
                else
                {
                    _biographyOptionsProfession[xe.Name.LocalName] = new ObservableCollection<string>(
                        from e in xe.Elements() select e.Value);
                }
            }

            _biographyOptionsPersonality = new ObservableCollection<string>(
                from p in biographies.Root.Element("Personalities").Elements() select p.Value);

            _biographyOptionsRace = new Dictionary<string, IDictionary<string, ObservableCollection<string>>>(5);
            foreach (XElement xe in biographies.Root.Element("Races").Elements())
            {
                var d = new Dictionary<string, ObservableCollection<string>>(3);
                foreach (var choice in xe.Elements())
                {
                    d[choice.Name.LocalName] = new ObservableCollection<string>(
                        from c in choice.Elements() select c.Value);
                }
                _biographyOptionsRace[xe.Name.LocalName] = d;
            }

            _bgUpdater.DoWork += UpdateWorker_DoWork;
            _bgUpdater.RunWorkerCompleted += UpdateWorker_RunWorkerCompleted;

            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                DoOpen(Path.GetFullPath(filePath));
            }
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
        /// This view model displays when a new update is available.
        /// </summary>
        private UpdateAvailableViewModel UpdateWindow
        {
            get { return _updateViewModel; }
            set
            {
                if (value != _updateViewModel)
                {
                    if (_updateViewModel != null)
                    {
                        _updateViewModel.Close();
                    }
                    _updateViewModel = value;
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
                    RaisePropertyChanged("UnsavedChanges");
                }
                WindowTitle = String.Format(Properties.Resources.wnWindowTitle,
                    _unsavedChanges ? "*" : string.Empty,
                    _currentFile == null ? "Unnamed" : Path.GetFileName(_currentFile.Name));
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
                    if (_characterList != null)
                    {
                        _characterList.CollectionChanged -= MarkFileDirty;
                    }
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
                    if (_selectedCharacter != null)
                    {
                        if (SelectedAreaReference != null)
                        {
                            SelectedAreaCharacter = null;
                            ChangedAreaOrCharacter();
                        }
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
                        ChangedAreaOrCharacter();
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

        public ObservableCollection<string> BiographyOptionsProfession
        {
            get { return _selectedBiographyOptionsProfession; }
            private set
            {
                if (value != _selectedBiographyOptionsProfession)
                {
                    _selectedBiographyOptionsProfession = value;
                    RaisePropertyChanged("BiographyOptionsProfession");
                }
            }
        }

        public ObservableCollection<string> BiographyOptionsPersonality
        {
            get { return _selectedBiographyOptionsPersonality; }
            private set
            {
                if (value != _selectedBiographyOptionsPersonality)
                {
                    _selectedBiographyOptionsPersonality = value;
                    RaisePropertyChanged("BiographyOptionsPersonality");
                }
            }
        }

        public ObservableCollection<string> BiographyOptionsRaceFirst
        {
            get { return _selectedBiographyOptionsRaceFirst; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceFirst)
                {
                    _selectedBiographyOptionsRaceFirst = value;
                    RaisePropertyChanged("BiographyOptionsRaceFirst");
                }
            }
        }

        public ObservableCollection<string> BiographyOptionsRaceSecond
        {
            get { return _selectedBiographyOptionsRaceSecond; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceSecond)
                {
                    _selectedBiographyOptionsRaceSecond = value;
                    RaisePropertyChanged("BiographyOptionsRaceSecond");
                }
            }
        }

        public ObservableCollection<string> BiographyOptionsRaceThird
        {
            get { return _selectedBiographyOptionsRaceThird; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceThird)
                {
                    _selectedBiographyOptionsRaceThird = value;
                    RaisePropertyChanged("BiographyOptionsRaceThird");
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
                    RaisePropertyChanged("HeartIcon");
                    RaisePropertyChanged("Hearts");
                }
            }
        }

        public string HeartIcon
        {
            get
            {
                if (SelectedAreaReference != null && SelectedAreaReference.Hearts == Hearts)
                {
                    return "HeartDone";
                }
                return "Heart";
            }
        }

        public string WaypointIcon
        {
            get
            {
                if (SelectedAreaReference != null && SelectedAreaReference.Waypoints == Waypoints)
                {
                    return "WaypointDone";
                }
                return "Waypoint";
            }
        }

        public string PoIIcon
        {
            get
            {
                if (SelectedAreaReference != null && SelectedAreaReference.PoIs == PoIs)
                {
                    return "PoIDone";
                }
                return "PoI";
            }
        }

        public string SkillIcon
        {
            get
            {
                if (SelectedAreaReference != null && SelectedAreaReference.Skills == Skills)
                {
                    return "SkillDone";
                }
                return "Skill";
            }
        }

        public string VistaIcon
        {
            get
            {
                if (SelectedAreaReference != null && SelectedAreaReference.Vistas == Vistas)
                {
                    return "VistaDone";
                }
                return "Vista";
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
                    RaisePropertyChanged("Waypoints");
                    RaisePropertyChanged("WaypointIcon");
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
                    RaisePropertyChanged("PoIs");
                    RaisePropertyChanged("PoIIcon");
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
                    RaisePropertyChanged("Skills");
                    RaisePropertyChanged("SkillIcon");
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
                    RaisePropertyChanged("Vistas");
                    RaisePropertyChanged("VistaIcon");
                }
            }
        }

        #endregion

        #region ICommand Implementations
        /// <summary>
        /// Command to create a new character.
        /// </summary>
        public ICommand CommandNewCharacter
        {
            get
            {
                if (_cmdNew == null)
                {
                    _cmdNew = new RelayCommand(param => this.NewCharacter());
                }
                return _cmdNew;
            }
        }

        /// <summary>
        /// Command to open a character file.
        /// </summary>
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

        /// <summary>
        /// Command to save the current character file.
        /// </summary>
        public ICommand CommandSave
        {
            get
            {
                if (_cmdSave == null)
                {
                    _cmdSave = new RelayCommand(param => this.Save(), param => this.UnsavedChanges);
                }
                return _cmdSave;
            }
        }

        /// <summary>
        /// Command to save the current character file at a specified location.
        /// </summary>
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

        /// <summary>
        /// Command to exit the application.
        /// </summary>
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

        /// <summary>
        /// Command to check for updates.
        /// </summary>
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

        /// <summary>
        /// Command to delete a character.
        /// </summary>
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

        /// <summary>
        /// Command to register the file extension.
        /// </summary>
        public ICommand CommandRegisterExtension
        {
            get
            {
                if (_cmdRegisterExtensions == null)
                {
                    _cmdRegisterExtensions = new RelayCommand(param => this.RegisterExtension());
                }
                return _cmdRegisterExtensions;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new character.
        /// </summary>
        private void NewCharacter()
        {
            var c = new Character(this);
            CharacterList.Add(c);
            if (SelectedCharacter == null)
            {
                SelectedCharacter = c;
            }
        }

        /// <summary>
        /// Attempts to open the file specified by <c>filePath</c>. If <c>filePath</c>
        /// is <c>null</c> an <see cref="OpenFileDialog"/> is displayed for the user
        /// to select a file. If <see cref="UnsavedChanges"/> is <c>True</c> a
        /// <see cref="MessageBox"/> asks if the user wants to proceed with loading
        /// the file. The same thing happens if <c>filePath</c> is already open.
        /// </summary>
        /// <param name="filePath">The path to the file to open.</param>
        public void Open(string filePath = null)
        {
            if (UnsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedOpenBody,
                    Properties.Resources.msgUnsavedOpenTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            if (filePath == null)
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter += Properties.Resources.cfgFileFilter;
                if (open.ShowDialog().Value)
                {
                    filePath = open.FileName;
                }
            }
            if (filePath != null &&
                (_currentFile == null || _currentFile.FullName != filePath ||
                MessageBox.Show(Properties.Resources.msgReloadFileBody,
                    Properties.Resources.msgReloadFileTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                DoOpen(filePath);
            }
        }

        /// <summary>
        /// Performs the file reading and parsing of the file specified by
        /// <see cref="filePath"/>. If there is an error processing the file
        /// the current state is unchanged.
        /// </summary>
        /// <param name="filePath">The path of the file to open</param>
        private void DoOpen(string filePath)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            XmlSchemaSet xs = new XmlSchemaSet();
            settings.CloseInput = true;
            xs.Add(Properties.Resources.xNamespace,
                XmlReader.Create(Application.GetResourceStream(
                    new Uri(Properties.Resources.cfgXsdpath, UriKind.Relative)).Stream, settings));

            settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = xs;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            XDocument doc = null;
            using (XmlReader r = XmlReader.Create(filePath, settings))
            {
                try
                {
                    // Load and parse the file. Only if load and parse succeed
                    // should the file handle be updated.
                    doc = XDocument.Load(r);
                    Parse(doc);
                    _currentFile = new FileInfo(filePath);
                    UnsavedChanges = false;
                }
                catch (XmlSchemaValidationException ex)
                {
                    Debug.WriteLine(ex.Message);
                    ShowError(Properties.Resources.msgOpenFailedValidationTitle, ex.Message);
                }
                catch (XmlException ex)
                {
                    ShowError(Properties.Resources.msgOpenFailedParsingTitle,
                        String.Format(Properties.Resources.msgOpenFailedParsingBody,
                        ex.Message));
                }
                catch (FileNotFoundException)
                {
                    ShowError(Properties.Resources.msgOpenFailedNoFileTitle,
                        Properties.Resources.msgOpenFailedNoFileBody);
                }
            }
        }

        /// <summary>
        /// Parses the supplied document into the model.
        /// </summary>
        /// <param name="doc">The document to parse.</param>
        private void Parse(XDocument doc)
        {
            var characters = doc.Root.CElements("Character");
            ObservableCollection<Character> newCharacterList = new ObservableCollection<Character>();

            foreach (var charr in characters)
            {
                Character c = new Character(this)
                {
                    Name = charr.CElement("Name").Value,
                    Race = charr.CElement("Race").Value,
                    Profession = charr.CElement("Profession").Value
                };

                // Biography choices.
                var biographies = charr.CElement("Biographies");
                c.BiographyProfession = biographies.CElement("Profession").Value;
                c.BiographyPersonality = biographies.CElement("Personality").Value;
                c.BiographyRaceFirst = biographies.CElement("RaceFirst").Value;
                c.BiographyRaceSecond = biographies.CElement("RaceSecond").Value;
                c.BiographyRaceThird = biographies.CElement("RaceThird").Value;

                // Crafting disciplines.
                var craftingDisciplines = charr.CElement("CraftingDisciplines");
                foreach (var discipline in c.CraftingDisciplines)
                {
                    discipline.Level = craftingDisciplines.CElement(discipline.Name).CElement("Level").Value;
                }

                // Area completion.
                var areas = charr.CElement("Areas").CElements("Area");
                foreach (var area in areas)
                {
                    Area a = new Area(area.CElement("Name").Value)
                    {
                        Hearts = area.CElement("Completion").CElement("Hearts").Value,
                        Waypoints = area.CElement("Completion").CElement("Waypoints").Value,
                        PoIs = area.CElement("Completion").CElement("PoIs").Value,
                        Skills = area.CElement("Completion").CElement("Skills").Value,
                        Vistas = area.CElement("Completion").CElement("Vistas").Value
                    };
                    c.Areas.Add(a);
                }

                // All done.
                newCharacterList.Add(c);
            }
            CharacterList = newCharacterList;
            if (CharacterList.Count > 0)
            {
                SelectedCharacter = CharacterList[0];
            }
        }

        /// <summary>
        /// This callback gets around the problem of XML warnings, raised when
        /// an XML file does not contain a matching schema definition, not
        /// failing validation. It does so by throwing a new
        /// <see cref="XmlSchemaValidationException"/> containing the original
        /// one.
        /// </summary>
        /// <param name="sender">The culprit causing the warning or error,
        /// for instance an XML element or a text element.</param>
        /// <param name="e">Contains information about the type of validation
        /// error that occurred.</param>
        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new XmlSchemaValidationException(Properties.Resources.msgOpenFailedValidationBody, e.Exception);
        }

        /// <summary>
        /// Save the current file if it has unsaved changes.
        /// <seealso cref="UnsavedChanges"/>
        /// </summary>
        private void Save()
        {
            if (_currentFile != null && _currentFile.Exists && !_currentFile.IsReadOnly)
            {
                DoSave(_currentFile.FullName);
            }
            else
            {
                SaveAs();
            }
        }

        /// <summary>
        /// Opens a dialog letting the user specify where to save the current file,
        /// irrespective of <see cref="UnsavedChanges"/>.
        /// </summary>
        private void SaveAs()
        {
            SaveFileDialog save = new SaveFileDialog();

            if (_currentFile == null)
            {
                save.FileName = Properties.Resources.cfgFileName;
            }
            else
            {
                save.FileName = _currentFile.Name;
            }
            save.DefaultExt = Properties.Resources.cfgFileExtension;
            save.Filter += Properties.Resources.cfgFileFilter;
            if (save.ShowDialog().Value)
            {
                DoSave(save.FileName);
            }
        }

        /// <summary>
        /// Performs the XML processing and file writing when saving.
        /// </summary>
        /// <param name="filePath">The file path to write to; doesn't have to exist.</param>
        private void DoSave(string filePath)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = false;
            xws.Indent = true;

            using (XmlWriter xw = XmlWriter.Create(filePath, xws))
            {
                new XDocument(
                    new CharrElement("Charrmander",
                        (CharacterList.Count > 0 ?
                        from c in CharacterList
                        select c.ToXML() : null)
                    )
                ).Save(xw);
                _currentFile = new FileInfo(filePath);
                UnsavedChanges = false;
            }
        }

        /// <summary>
        /// Informs the application handler that this window would like to close.
        /// If there are unsaved changes the user is alerted and allowed to abort.
        /// <seealso cref="UnsavedChanges"/>
        /// </summary>
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

        /// <summary>
        /// Starts the background process checking for updates.
        /// </summary>
        private void CheckUpdate()
        {
            if (!_bgUpdater.IsBusy)
            {
                _bgUpdater.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Removes <see cref="SelectedCharacter"/> from <see cref="CharacterList"/>.
        /// Responsible for detaching events.
        /// </summary>
        private void DeleteCharacter()
        {
            Character c = SelectedCharacter;

            if (c != null)
            {
                CharacterList.Remove(c);
                c.Dispose();
            }
        }

        /// <summary>
        /// Returns <c>True</c> if <see cref="DeleteCharacter"/> can be called.
        /// </summary>
        /// <returns><c>True</c> when a <see cref="SelectedCharacter"/> is set.</returns>
        private bool CanDeleteCharacter()
        {
            return IsCharacterDetailEnabled;
        }

        /// <summary>
        /// Registers this application's file extension with this application.
        /// If an association already exists that association is deleted.
        /// This application is defined by the current path of the executable,
        /// meaning this has to be re-run if the executable is moved.
        /// </summary>
        private void RegisterExtension()
        {
            if (MessageBox.Show(Properties.Resources.msgRegisterExtensionBody,
                Properties.Resources.msgRegisterExtensionTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            string exePath = Environment.GetCommandLineArgs()[0];
            FileAssociationInfo fai = new FileAssociationInfo(Properties.Resources.cfgFileExtension);
            if (fai.Exists)
            {
                fai.Delete();
            }
            fai.Create("Charrmander", PerceivedTypes.None, "text/xml", new string[] { "notepad.exe" });

            ProgramAssociationInfo pai = new ProgramAssociationInfo(fai.ProgID);
            if (pai.Exists)
            {
                pai.Delete();
            }
            pai.Create("Charrmander GW2 Character File.", new ProgramVerb("Open", exePath + " %1"));
            pai.DefaultIcon = new ProgramIcon(exePath);
        }

        /// <summary>
        /// Signalled when a property of the current file was changed.
        /// The parameters are not used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MarkFileDirty(object sender, EventArgs e)
        {
            UnsavedChanges = true;
        }

        public void UpdateBiographyOptions()
        {
            if (SelectedCharacter == null || string.IsNullOrEmpty(SelectedCharacter.Profession)
                || string.IsNullOrEmpty(SelectedCharacter.Race))
            {
                BiographyOptionsProfession = null;
                BiographyOptionsPersonality = null;
                BiographyOptionsRaceFirst = null;
                BiographyOptionsRaceSecond = null;
                BiographyOptionsRaceThird = null;
            }
            else
            {
                // Non-rangers all have simple lists of options.
                var nonRanger = _biographyOptionsProfession[SelectedCharacter.Profession]
                    as ObservableCollection<string>;
                if (nonRanger != null)
                {
                    BiographyOptionsProfession = nonRanger;
                }
                else
                {
                    // Ranger requires special handling. It has a race
                    // dependency and so is nested one level further.
                    var ranger = _biographyOptionsProfession[SelectedCharacter.Profession]
                        as IDictionary<string, ObservableCollection<string>>;
                    if (ranger != null)
                    {
                        BiographyOptionsProfession = ranger[SelectedCharacter.Race];
                    }
                }

                // Personality (this one is constant).
                BiographyOptionsPersonality = _biographyOptionsPersonality;

                // Race.
                BiographyOptionsRaceFirst = _biographyOptionsRace[SelectedCharacter.Race]["First"];
                BiographyOptionsRaceSecond = _biographyOptionsRace[SelectedCharacter.Race]["Second"];
                BiographyOptionsRaceThird = _biographyOptionsRace[SelectedCharacter.Race]["Third"];
            }
        }
        /// <summary>
        /// Wrapper for displaying a <see cref="MessageBox"/>.
        /// </summary>
        /// <param name="caption">The message box caption.</param>
        /// <param name="body">The message box message.</param>
        /// <param name="severity">A <see cref="MessageBoxImage"/> indicating
        /// the severity, default <c>MessageBoxImage.Error</c></param>
        private void ShowError(string caption, string body, MessageBoxImage severity = MessageBoxImage.Error)
        {
            MessageBox.Show(body, caption, MessageBoxButton.OK, severity);
        }

        /// <summary>
        /// When a character or area was selected from their respective lists,
        /// this method is called and makes the necessary property changes.
        /// </summary>
        private void ChangedAreaOrCharacter()
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

            RaisePropertyChanged("Hearts");
            RaisePropertyChanged("Waypoints");
            RaisePropertyChanged("PoIs");
            RaisePropertyChanged("Skills");
            RaisePropertyChanged("Vistas");

            RaisePropertyChanged("HeartIcon");
            RaisePropertyChanged("WaypointIcon");
            RaisePropertyChanged("PoIIcon");
            RaisePropertyChanged("SkillIcon");
            RaisePropertyChanged("VistaIcon");
        }

        /// <summary>
        /// Starts downloading update notes in the background, passing them to
        /// <c>e.Result</c> when finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">This event is passed to <see cref="UpdateWorker_RunWorkerCompleted"/>.</param>
        private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            XDocument doc = XDocument.Load(Properties.Resources.cfgUpdateCheckUri);
            e.Result = doc;
        }

        /// <summary>
        /// Called when the background updater finishes downloading update notes,
        /// and displays a dialog with information about available updates and
        /// the option to go to the location of a new update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">This event is passed from <see cref="UpdateWorker_DoWork"/>.</param>
        private void UpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is InvalidOperationException)
                {
                    ShowError(Properties.Resources.msgUpdateCheckFailedTitle,
                        String.Format(Properties.Resources.msgUpdateCheckFailedBody404,
                        Properties.Resources.cfgUpdateCheckUri, e.Error.Message));
                }
                else
                {
                    ShowError(Properties.Resources.msgUpdateCheckFailedTitle,
                        String.Format(Properties.Resources.msgUpdateCheckFailedBodyUnknown, e.Error.Message));
                }
            }
            else if (e.Cancelled)
            { }
            else
            {
                try
                {
                    XDocument doc = (XDocument)e.Result;
                    XElement latest = doc.Root.Element("Latest").Element("Release");
                    Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    Version newVersion = new Version(latest.Element("Version").Value);
                    if (newVersion.IsNewerThan(curVersion))
                    {
                        UpdateWindow = new UpdateAvailableViewModel();
                        UpdateWindow.CurrentVersion = curVersion;
                        UpdateWindow.LatestVersion = newVersion;
                        UpdateWindow.LatestVersionPath = latest.Element("DownloadUrl").Value;
                        UpdateWindow.VersionHistory = doc.Root.Descendants("Release");
                    }
                    else
                    {
                        ShowError(Properties.Resources.msgUpdateCheckNoUpdatesBody,
                            Properties.Resources.msgUpdateCheckNoUpdatesTitle,
                            MessageBoxImage.Information);
                    }
                }
                catch (NullReferenceException)
                {
                    ShowError(Properties.Resources.msgUpdateCheckFailedTitle,
                        Properties.Resources.msgUpdateCheckFailedBodyReading);
                }
            }
        }
    }
}
