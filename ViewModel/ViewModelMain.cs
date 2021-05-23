using Charrmander.Model;
using Charrmander.Util;
using Charrmander.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Charrmander.ViewModel
{
    internal class ViewModelMain : AbstractNotifier, IViewModel, IDisposable
    {
        #region Fields

        private RelayCommand _cmdNew = default!;
        private RelayCommand _cmdOpen = default!;
        private RelayCommand _cmdSave = default!;
        private RelayCommand _cmdSaveAs = default!;
        private RelayCommand _cmdClose = default!;
        private RelayCommand _cmdCheckUpdate = default!;
        private RelayCommand _cmdDeleteCharacter = default!;
        private RelayCommand _cmdRegisterExtensions = default!;
        private RelayCommand _cmdCompleteArea = default!;
        private RelayCommand _cmdCompletionOverview = default!;

        private readonly BackgroundWorker _bgUpdater = new();

        private UpdateAvailableViewModel _updateViewModel = default!;
        private CompletionOverviewView _completionOverview = default!;

        private FileInfo? _currentFile;

        private string _windowTitle = "Charrmander";

        private readonly Version _curVersion = new(1, 21, 0, 0);

        private string _statusBarUpdateCheck = default!;

        private bool _unsavedChanges = false;

        /// <summary>
        /// The source collection for <see cref="SortedCharacterList"/>, as an
        /// <see cref="ObservableCollection"/> to propagate both element and
        /// collection change events.
        /// </summary>
        private readonly ObservableCollection<Character> _characterList = new();

        private Character? _selectedCharacter;
        private Area? _selectedAreaReference;
        private Area? _selectedAreaCharacter;

        private bool _isCharacterDetailEnabled = false;

        private Visibility _isBiographyVisible = Visibility.Collapsed;

        private readonly IDictionary<string, object> _biographyOptionsProfession;
        private readonly IReadOnlyList<string> _biographyOptionsPersonality;
        private readonly IDictionary<string, IDictionary<string, IReadOnlyList<string>>> _biographyOptionsRace;

        private IReadOnlyList<string>?
            _selectedBiographyOptionsProfession,
            _selectedBiographyOptionsPersonality,
            _selectedBiographyOptionsRaceFirst,
            _selectedBiographyOptionsRaceSecond,
            _selectedBiographyOptionsRaceThird;

        #endregion

        public ViewModelMain()
        {
            _characterList.CollectionChanged += MarkFileDirty;
            // Ensure Source is set immediately, so any change events from the
            // source collection propagates all the way through.
            SortedCharacterList.Source = _characterList;

            var doc = XDocument.Load(XmlReader.Create(
                App.GetPackResourceStream("Resources/Areas.xml").Stream));

            AreaReferenceList = doc.Root!.Elements(Area.XmlNamespace + "Area")
                .Select(Area.ReferenceAreaFromXML)
                .ToList();

            ReferenceAreaNames = AreaReferenceList.Select(a => a.Name).ToHashSet();

            static bool IsRequiredForWorldCompletion(Area a) => a.ParticipatesInWorldCompletion;
            WorldCompletionAreaNames = AreaReferenceList
                .Where(IsRequiredForWorldCompletion)
                .Select(a => a.Name)
                .ToHashSet();

            SortedAreas = CollectionViewSource.GetDefaultView(AreaReferenceList);
            SortedAreas.SortDescriptions.Add(
                new SortDescription(nameof(Area.Name), ListSortDirection.Ascending));
            SortedAreas.Filter = (item) => !ShowOnlyRequiredForWorldCompletion
                || ((item is Area a) && IsRequiredForWorldCompletion(a));

            var races = XDocument.Load(XmlReader.Create(
                App.GetPackResourceStream("Resources/Races.xml").Stream)).Root!.Elements("Race");

            var biographies = XDocument.Load(XmlReader.Create(
                App.GetPackResourceStream("Resources/Biographies.xml").Stream));

            _biographyOptionsProfession = new Dictionary<string, object>(8);
            foreach (XElement xe in biographies.Root!.Element("Professions")!.Elements())
            {
                // Rangers need an extra level of nesting becauseo of a racial
                // dependency.
                if (xe.Name.LocalName == "Ranger")
                {
                    var d = new Dictionary<string, IReadOnlyList<string>>();
                    foreach (var race in races)
                    {
                        string key = race.Element("Name")!.Value;
                        d[key] = xe.Element(key)!.Elements()
                            .Select(bo => bo.Value).ToList();
                    }
                    _biographyOptionsProfession[xe.Name.LocalName] = d;
                }
                else
                {
                    _biographyOptionsProfession[xe.Name.LocalName] =
                        xe.Elements().Select(e => e.Value).ToList();
                }
            }

            _biographyOptionsPersonality =
                biographies.Root!.Element("Personalities")!.Elements()
                    .Select(p => p.Value).ToList();

            _biographyOptionsRace = new Dictionary<string, IDictionary<string, IReadOnlyList<string>>>(5);
            foreach (XElement xe in biographies.Root!.Element("Races")!.Elements())
            {
                var d = new Dictionary<string, IReadOnlyList<string>>(3);
                foreach (var choice in xe.Elements())
                {
                    d[choice.Name.LocalName] = choice.Elements().Select(c => c.Value).ToList();
                }
                _biographyOptionsRace[xe.Name.LocalName] = d;
            }

            _bgUpdater.DoWork += UpdateWorker_DoWork;
            _bgUpdater.RunWorkerCompleted += UpdateWorker_RunWorkerCompleted;
        }

        /// <summary>
        /// Perform this event when the view wishes to close. For instance, dispose the view.
        /// </summary>
        public event EventHandler? RequestClose;

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
                    RaisePropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public string CurrentVersion
        {
            get { return _curVersion.ToString(); }
        }

        /// <summary>
        /// A string value indicating the result of performing an update check.
        /// </summary>
        public string StatusBarUpdateCheck
        {
            get { return _statusBarUpdateCheck; }
            set
            {
                if (value != _statusBarUpdateCheck)
                {
                    _statusBarUpdateCheck = value;
                    RaisePropertyChanged(nameof(StatusBarUpdateCheck));
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
                    _updateViewModel?.Close();
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
                    RaisePropertyChanged(nameof(UnsavedChanges));
                }
                WindowTitle = String.Format(Properties.Resources.wnWindowTitle,
                    _unsavedChanges ? "*" : string.Empty,
                    _currentFile == null ? "Unnamed" : Path.GetFileName(_currentFile.Name));
            }
        }

        /// <summary>
        /// The view of the character list, sorted by default on
        /// <see cref="Character.DefaultSortOrder"/>, and backed by
        /// <see cref="_characterList"/>.
        /// </summary>
        public CollectionViewSource SortedCharacterList { get; } = new CollectionViewSource
        {
            IsLiveSortingRequested = true,
            SortDescriptions =
            {
                new SortDescription(nameof(Character.DefaultSortOrder), ListSortDirection.Ascending),
            },
        };

        /// <summary>
        /// The master list of areas, generated at runtime from an embedded XML file.
        /// Compare <see cref="Character.Areas"/> against this.
        /// </summary>
        /// <seealso cref="SortedAreas"/>
        /// <seealso cref="ReferenceAreaNames"/>
        internal IReadOnlyList<Area> AreaReferenceList { get; }

        /// <summary>
        /// A sorted view of <see cref="AreaReferenceList"/>.
        /// </summary>
        public ICollectionView SortedAreas { get; }

        /// <summary>
        /// The name of every recognized area.
        /// </summary>
        /// <see cref="AreaReferenceList"/>
        private IReadOnlySet<string> ReferenceAreaNames { get; }

        internal IReadOnlySet<string> WorldCompletionAreaNames { get; }

        private bool _showOnlyRequiredForWorldCompletion;
        public bool ShowOnlyRequiredForWorldCompletion
        {
            get => _showOnlyRequiredForWorldCompletion;
            set
            {
                if (_showOnlyRequiredForWorldCompletion == value) return;
                _showOnlyRequiredForWorldCompletion = value;
                RaisePropertyChanged(nameof(ShowOnlyRequiredForWorldCompletion));
                SortedAreas.Refresh();
            }
        }

        /// <summary>
        /// The <see cref="Character"/> in <see cref="CharacterList"/> that is currently selected.
        /// </summary>
        public Character? SelectedCharacter
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
                            /// Necessary to get the text fields to update when
                            /// switching character, else they will remain
                            /// bound to the previous character.
                            SelectedAreaCharacter = null;
                        }
                        ChangedAreaOrCharacter();
                    }
                    IsCharacterDetailEnabled = value != null;
                    RaisePropertyChanged(nameof(SelectedCharacter));
                }
            }
        }

        /// <summary>
        /// The <see cref="Area"/> in <see cref="AreaReferenceList"/> that is currently selected.
        /// </summary>
        public Area? SelectedAreaReference
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
                    RaisePropertyChanged(nameof(SelectedAreaReference));
                }
            }
        }

        /// <summary>
        /// The <see cref="Area"/> in <see cref="SelectedCharacter.Areas"/> that is currently selected.
        /// </summary>
        public Area? SelectedAreaCharacter
        {
            get { return _selectedAreaCharacter; }
            set
            {
                if (value != _selectedAreaCharacter)
                {
                    _selectedAreaCharacter = value;
                    RaisePropertyChanged(nameof(SelectedAreaCharacter));
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
                    RaisePropertyChanged(nameof(IsCharacterDetailEnabled));
                }
            }
        }

        #region Biography

        /// <summary>
        /// <c>Visibility.Visible</c> if the biography options should be shown,
        /// <c>Visibility.Collapsed</c> otherwise.
        /// </summary>
        public Visibility IsBiographyVisible
        {
            get { return _isBiographyVisible; }
            set
            {
                if (value != _isBiographyVisible)
                {
                    _isBiographyVisible = value;
                    RaisePropertyChanged(nameof(IsBiographyVisible));
                }
            }
        }

        /// <summary>
        /// The profession biography options.
        /// </summary>
        public IReadOnlyList<string>? BiographyOptionsProfession
        {
            get { return _selectedBiographyOptionsProfession; }
            private set
            {
                if (value != _selectedBiographyOptionsProfession)
                {
                    _selectedBiographyOptionsProfession = value;
                    RaisePropertyChanged(nameof(BiographyOptionsProfession));
                }
            }
        }

        /// <summary>
        /// The personality biography options.
        /// </summary>
        public IReadOnlyList<string>? BiographyOptionsPersonality
        {
            get { return _selectedBiographyOptionsPersonality; }
            private set
            {
                if (value != _selectedBiographyOptionsPersonality)
                {
                    _selectedBiographyOptionsPersonality = value;
                    RaisePropertyChanged(nameof(BiographyOptionsPersonality));
                }
            }
        }

        /// <summary>
        /// The biography options for the first racial choice.
        /// </summary>
        public IReadOnlyList<string>? BiographyOptionsRaceFirst
        {
            get { return _selectedBiographyOptionsRaceFirst; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceFirst)
                {
                    _selectedBiographyOptionsRaceFirst = value;
                    RaisePropertyChanged(nameof(BiographyOptionsRaceFirst));
                }
            }
        }

        /// <summary>
        /// The biography options for the second racial choice.
        /// </summary>
        public IReadOnlyList<string>? BiographyOptionsRaceSecond
        {
            get { return _selectedBiographyOptionsRaceSecond; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceSecond)
                {
                    _selectedBiographyOptionsRaceSecond = value;
                    RaisePropertyChanged(nameof(BiographyOptionsRaceSecond));
                }
            }
        }

        /// <summary>
        /// The biography options for the third racial choice.
        /// </summary>
        public IReadOnlyList<string>? BiographyOptionsRaceThird
        {
            get { return _selectedBiographyOptionsRaceThird; }
            private set
            {
                if (value != _selectedBiographyOptionsRaceThird)
                {
                    _selectedBiographyOptionsRaceThird = value;
                    RaisePropertyChanged(nameof(BiographyOptionsRaceThird));
                }
            }
        }

        #endregion //Biography

        #region Area Completion

        /// <summary>
        /// The number of hearts completed by <see cref="SelectedCharacter"/>.
        /// </summary>
        public string Hearts
        {
            get
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null)
                {
                    return SelectedAreaCharacter.Hearts;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null
                    && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Hearts = value;
                    RaisePropertyChanged(nameof(HeartIcon));
                    RaisePropertyChanged(nameof(Hearts));
                    UpdateAreaState(SelectedAreaReference, SelectedCharacter);
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
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null)
                {
                    return SelectedAreaCharacter.Waypoints;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null
                    && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Waypoints = value;
                    RaisePropertyChanged(nameof(Waypoints));
                    RaisePropertyChanged(nameof(WaypointIcon));
                    UpdateAreaState(SelectedAreaReference, SelectedCharacter);
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
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null)
                {
                    return SelectedAreaCharacter.PoIs;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null
                    && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.PoIs = value;
                    RaisePropertyChanged(nameof(PoIs));
                    RaisePropertyChanged(nameof(PoIIcon));
                    UpdateAreaState(SelectedAreaReference, SelectedCharacter);
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
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null)
                {
                    return SelectedAreaCharacter.Skills;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null
                    && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Skills = value;
                    RaisePropertyChanged(nameof(Skills));
                    RaisePropertyChanged(nameof(SkillIcon));
                    UpdateAreaState(SelectedAreaReference, SelectedCharacter);
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
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null)
                {
                    return SelectedAreaCharacter.Vistas;
                }
                return string.Empty;
            }
            set
            {
                if (SelectedCharacter != null
                    && SelectedAreaReference != null
                    && SelectedAreaCharacter != null
                    && !string.IsNullOrWhiteSpace(value))
                {
                    SelectedAreaCharacter.Vistas = value;
                    RaisePropertyChanged(nameof(Vistas));
                    RaisePropertyChanged(nameof(VistaIcon));
                    UpdateAreaState(SelectedAreaReference, SelectedCharacter);
                }
            }
        }

        /// <summary>
        /// Gets the name of the completion icon, minus extension, to use for
        /// heats.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the completion icon, minus extension, to use for
        /// waypoints.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the completion icon, minus extension, to use for
        /// points of interest.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the completion icon, minus extension, to use for
        /// skill challenges.
        /// </summary>
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

        /// <summary>
        /// Gets the name of the completion icon, minus extension, to use for
        /// vistas.
        /// </summary>
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

        #endregion

        #region ICommand Implementations
        /// <summary>
        /// Command to create a new character.
        /// </summary>
        public ICommand CommandNewCharacter =>
            _cmdNew ??= new RelayCommand(_ => NewCharacter());

        /// <summary>
        /// Command to open a character file.
        /// </summary>
        public ICommand CommandOpen =>
            _cmdOpen ??= new RelayCommand(_ => Open(null));

        /// <summary>
        /// Command to save the current character file.
        /// </summary>
        public ICommand CommandSave =>
            _cmdSave ??= new RelayCommand(_ => Save(), _ => UnsavedChanges);

        /// <summary>
        /// Command to save the current character file at a specified location.
        /// </summary>
        public ICommand CommandSaveAs =>
            _cmdSaveAs ??= new RelayCommand(_ => SaveAs());

        /// <summary>
        /// Command to exit the application.
        /// </summary>
        public ICommand CommandExit =>
            _cmdClose ??= new RelayCommand(_ => OnRequestClose());

        /// <summary>
        /// Command to check for updates.
        /// </summary>
        public ICommand CommandCheckUpdate =>
            _cmdCheckUpdate ??= new RelayCommand(_ => CheckUpdate());

        /// <summary>
        /// Command to delete a character.
        /// </summary>
        public ICommand CommandDeleteCharacter =>
            _cmdDeleteCharacter ??= new RelayCommand(
                _ => DeleteCharacter(),
                _ => CanDeleteCharacter());

        /// <summary>
        /// Command to register the file extension.
        /// </summary>
        public ICommand CommandRegisterExtension =>
            _cmdRegisterExtensions ??= new RelayCommand(
                _ => RegisterExtension());

        /// <summary>
        /// Command to mark the selected area as completed.
        /// </summary>
        public ICommand CommandCompleteArea =>
            _cmdCompleteArea ??= new RelayCommand(
                _ => MarkAreaCompleted(),
                _ => CanMarkAreaCompleted());

        /// <summary>
        /// Command to mark the selected area as completed.
        /// </summary>
        public ICommand CommandCompletionOverview =>
            _cmdCompletionOverview ??= new RelayCommand(
                _ => ShowCompletionOverview(),
                _ => CanShowCompletionOverview());

        #endregion

        /// <summary>
        /// Creates a new character.
        /// </summary>
        private void NewCharacter()
        {
            var c = new Character(this);
            // When adding a new character, set its default sort order to be 1 greater than the current max.
            // This enables the following invariant: if the current sort order is by descending age,
            // that sort order is maintained.
            var currentMaxSortOrder = _characterList.Select(character => character.DefaultSortOrder).DefaultIfEmpty(0).Max();
            c.DefaultSortOrder = 1 + currentMaxSortOrder;
            _characterList.Add(c);
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
        /// <returns>True if <paramref name="filePath"/> was attempted opened,
        /// even if unsuccessfully; false otherwise.</returns>
        public bool Open(string? filePath)
        {
            if (UnsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedOpenBody,
                    Properties.Resources.msgUnsavedOpenTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return false;
            }

            if (filePath == null)
            {
                var open = new OpenFileDialog();
                open.Filter += Properties.Resources.cfgFileFilter;
                if (open.ShowDialog() ?? false)
                {
                    filePath = open.FileName;
                }
            }
            if (!string.IsNullOrWhiteSpace(filePath) &&
                File.Exists(filePath) &&
                (_currentFile == null || _currentFile.FullName != filePath ||
                MessageBox.Show(Properties.Resources.msgReloadFileBody,
                    Properties.Resources.msgReloadFileTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                DoOpen(filePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs the file reading and parsing of the file specified by
        /// <see cref="filePath"/>. If there is an error processing the file
        /// the current state is unchanged.
        /// </summary>
        /// <param name="filePath">The path of the file to open</param>
        private void DoOpen(string filePath)
        {
            var settings = new XmlReaderSettings();
            var schemas = new XmlSchemaSet();
            settings.CloseInput = true;
            schemas.Add(Properties.Resources.xNamespace,
                XmlReader.Create(App.GetPackResourceStream(
                    Properties.Resources.cfgXsdpath).Stream, settings));

            settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationCallBack;

            using XmlReader r = XmlReader.Create(filePath, settings);
            try
            {
                // Load and parse the file. Only if load and parse succeed
                // should the file handle be updated.
                var doc = XDocument.Load(r);
                Parse(doc);
                _currentFile = new FileInfo(filePath);
                UnsavedChanges = false;
            }
            catch (XmlSchemaValidationException ex)
            {
                ShowError(Properties.Resources.msgOpenFailedValidationTitle, String.Format(
                    Properties.Resources.msgOpenFailedParsingBody,
                    ex.InnerException?.Message ?? string.Empty));
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

        /// <summary>
        /// Parses the supplied document into the model.
        /// </summary>
        /// <param name="doc">The document to parse.</param>
        private void Parse(XDocument doc)
        {
            // XML already schema-validated.
            var characters = doc.Root!.CElements("Character");

            // Replace the contents of the character list with the soon to be
            // loaded content. Preserve the original collection instance to
            // avoid breaking the binding (and resetting the current sort
            // setting).
            // https://stackoverflow.com/a/44198356/482758
            _characterList.Clear();

            foreach (var charr in characters)
            {
                var c = new Character(this)
                {
                    Name = charr.CElement("Name").Value,
                    Race = charr.CElement("Race").Value,
                    Profession = charr.CElement("Profession").Value
                };

                _ = int.TryParse(charr.CElement("Level").Value, out int level);
                c.Level = level;

                _ = int.TryParse(charr.CElement("DefaultSortOrder").Value, out int defaultSortOrder);
                c.DefaultSortOrder = Math.Max(defaultSortOrder, Character.MinSortOrder);

                // Biography choices.
                var biographies = charr.CElement("Biographies");
                c.BiographyProfession = biographies.CElement("Profession").Value;
                c.BiographyPersonality = biographies.CElement("Personality").Value;
                c.BiographyRaceFirst = biographies.CElement("RaceFirst").Value;
                c.BiographyRaceSecond = biographies.CElement("RaceSecond").Value;
                c.BiographyRaceThird = biographies.CElement("RaceThird").Value;

                var unlockedSpecializations = charr
                    .CElement("UnlockedEliteSpecializations")
                    .CElements("Specialization");
                foreach (var unlockedSpec in unlockedSpecializations)
                {
                    foreach (var specialization in c.EliteSpecializations)
                    {
                        // O(n*m) but 0 <= n <= m = 2 .
                        if (unlockedSpec.Value == specialization.Name)
                        {
                            specialization.Unlocked = true;
                            break; // goto unlockedSpec
                        }
                    }
                }

                var storyChoices = charr.CElement("StoryChoices");
                c.Order = storyChoices.CElement("Order").Value;
                c.RacialSympathy = storyChoices.CElement("RacialSympathy").Value;
                c.RetributionAlly = storyChoices.CElement("RetributionAlly").Value;
                c.GreatestFear = storyChoices.CElement("GreatestFear").Value;
                c.PlanOfAttack = storyChoices.CElement("PlanOfAttack").Value;

                // Crafting disciplines.
                var craftingDisciplines = charr.CElement("CraftingDisciplines");
                foreach (var discipline in c.CraftingDisciplines)
                {
                    _ = int.TryParse(craftingDisciplines.CElement(discipline.Name).CElement("Level").Value,
                        out int craftLevel);
                    discipline.Level = craftLevel;
                }

                // World completion.
                _ = bool.TryParse(charr.CElement("HasWorldCompletion").Value, out bool worldCompleted);
                c.HasWorldCompletion = worldCompleted;

                // Area completion.
                var areas = charr.CElement("Areas").CElements("Area");
                foreach (var area in areas)
                {
                    var a = new Area(area.CElement("Name").Value)
                    {
                        Hearts = area.CElement("Completion").CElement("Hearts").Value,
                        Waypoints = area.CElement("Completion").CElement("Waypoints").Value,
                        PoIs = area.CElement("Completion").CElement("PoIs").Value,
                        Skills = area.CElement("Completion").CElement("Skills").Value,
                        Vistas = area.CElement("Completion").CElement("Vistas").Value
                    };
                    a.PropertyChanged += MarkFileDirty;

                    if (ReferenceAreaNames.Contains(a.Name))
                    {
                        c.AreaByName[a.Name] = a;
                    }
                }

                LoadStorylineWithActs(charr, "Lw2", c.Lw2Acts);
                LoadStorylineWithActs(charr, "HoT", c.HoTActs);
                LoadStorylineWithActs(charr, "KotT", c.KotTActs);
                LoadStorylineWithActs(charr, "Lw3", c.Lw3Acts);
                LoadStorylineWithActs(charr, "PoF", c.PoFActs);
                LoadStorylineWithActs(charr, "Lw4", c.Lw4Acts);
                LoadStorylineWithActs(charr, "Tis", c.TisActs);

                foreach (var cd in c.Dungeons)
                {
                    foreach (var ld in charr.CElement("Dungeons").CElements("Dungeon"))
                    {
                        if (cd.Name == ld.CElement("Name").Value)
                        {
                            _ = bool.TryParse(ld.CElement("StoryCompleted").Value, out bool completed);
                            cd.StoryCompleted = completed;
                            break;
                        }
                    }
                }

                _ = int.TryParse(charr.CElement("FractalTier").Value, out int fractalTier);
                fractalTier = Math.Max(fractalTier, Character.FractalTierMin);
                if (fractalTier > c.FractalTier)
                {
                    c.FractalTier = fractalTier;
                }

                c.Notes = charr.CElement("Notes").Value;

                // All done.
                _characterList.Add(c);
            }

            SelectFirstCharacter();
        }

        private void SelectFirstCharacter()
        {
            SelectedCharacter = SortedCharacterList.View.Cast<Character>().FirstOrDefault();
        }

        private static void LoadStorylineWithActs(XElement charr, string storyline, IReadOnlyList<Act> acts)
        {
            var chapterByNameByActName = new Dictionary<string, Dictionary<string, Chapter>>(acts.Count);
            foreach (var act in acts)
            {
                var chapterByName = new Dictionary<string, Chapter>(act.Chapters.Count);
                foreach (var chapter in act.Chapters)
                {
                    chapterByName[chapter.Name.ToLower()] = chapter;
                }
                chapterByNameByActName[act.Name.ToLower()] = chapterByName;
            }

            var storyChapters = charr.CElement("StoryChapters");
            var xe = storyChapters.CElement(storyline);
            foreach (var ld in xe.CDescendants("Chapter"))
            {
                var actName = ld.Parent!.Parent!.CElement("Name").Value.ToLower();
                var chapterName = ld.CElement("Name").Value.ToLower();
                if (chapterName.Equals("calm in the storm"))
                {
                    // "Calm in the Storm" was a placeholder story step in
                    // TIS: Champions between chapters. When a new chapter
                    // came out the step was "moved"; it could not be
                    // meaningfully completed. Just remove it altogether.
                    continue;
                }
                _ = bool.TryParse(ld.CElement("Completed").Value, out bool completed);
                // "Scion & Champion>"
                if (chapterName.EndsWith(">"))
                {
                    chapterName = chapterName[0..^1];
                }
                chapterByNameByActName[actName][chapterName].ChapterCompleted = completed;
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
        private static void ValidationCallBack(object? sender, ValidationEventArgs e)
        {
            throw new XmlSchemaValidationException(Properties.Resources.msgOpenFailedValidationBody, e.Exception);
        }

        /// <summary>
        /// Save the current file if it has unsaved changes.
        /// <seealso cref="UnsavedChanges"/>
        /// </summary>
        private void Save()
        {
            if (_currentFile?.Exists == true && !_currentFile.IsReadOnly)
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
            var save = new SaveFileDialog();

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
            if (save.ShowDialog() ?? false)
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
            var xws = new XmlWriterSettings
            {
                OmitXmlDeclaration = false,
                Indent = true
            };

            using XmlWriter xw = XmlWriter.Create(filePath, xws);
            new XDocument(
                new CharrElement("Charrmander",
                    _characterList.Count > 0 ?
                    from c in _characterList
                    select c.ToXML() : null
                )
            ).Save(xw);
            _currentFile = new FileInfo(filePath);
            UnsavedChanges = false;
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

        /// <summary>
        /// Returns <c>true</c> if user was prompted to close and said no,
        /// <c>false</c> either if user wasn't prompted or if user said yes.
        /// </summary>
        /// <returns><c>True</c> if user wants to abort closing.</returns>
        public bool AbortClosing()
        {
            return UnsavedChanges && MessageBox.Show(Properties.Resources.msgUnsavedExitBody,
                Properties.Resources.msgUnsavedExitTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No) == MessageBoxResult.No;
        }

        /// <summary>
        /// Starts the background process checking for updates.
        /// </summary>
        internal void CheckUpdate()
        {
            if (!_bgUpdater.IsBusy)
            {
                StatusBarUpdateCheck = Properties.Resources.suUpdateCheckInProgress;
                _bgUpdater.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Removes <see cref="SelectedCharacter"/> from
        /// <see cref="CharacterList"/>. If there are more characters in the
        /// list, selects the first one.
        /// Responsible for detaching events.
        /// </summary>
        private void DeleteCharacter()
        {
            Character? c = SelectedCharacter;

            if (c != null)
            {
                _characterList.Remove(c);
                c.Dispose();
            }

            SelectFirstCharacter();
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
        private static void RegisterExtension()
        {
            if (!OperatingSystem.IsWindows())
            {
                MessageBox.Show(
                    Properties.Resources.msgOsUnsupportedBody,
                    Properties.Resources.msgOsUnsupportedTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(Properties.Resources.msgRegisterExtensionBody,
                Properties.Resources.msgRegisterExtensionTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            string exePath = Environment.GetCommandLineArgs()[0];
            CurrentUserFileAssoc(Properties.Resources.cfgFileExtension, "Charrmander");
            CurrentUserFileAssoc("Charrmander", "Charrmander GW2 Character File");
            CurrentUserFileAssoc(@"Charrmander\shell\open\command", $"\"{exePath}\" \"%L\"");
            CurrentUserFileAssoc(@"Charrmander\DefaultIcon", exePath + ",0");
        }

        /// <summary>
        /// Updates all completion items of the selected area to be equal to
        /// values of the reference area. In short, an area is marked as
        /// completed.
        /// </summary>
        private void MarkAreaCompleted()
        {
            var selectedAreaCharacter = SelectedAreaCharacter;
            var selectedAreaReference = SelectedAreaReference;
            if (selectedAreaCharacter != null && selectedAreaReference != null)
            {
                selectedAreaCharacter.Hearts = selectedAreaReference.Hearts;
                selectedAreaCharacter.Waypoints = selectedAreaReference.Waypoints;
                selectedAreaCharacter.PoIs = selectedAreaReference.PoIs;
                selectedAreaCharacter.Skills = selectedAreaReference.Skills;
                selectedAreaCharacter.Vistas = selectedAreaReference.Vistas;
            }

            // Call this to signal an update of the relevant UI components.
            ChangedAreaOrCharacter();
        }

        /// <summary>
        /// If <see cref="SelectedAreaCharacter"/> and
        /// <see cref="SelectedAreaReference"/> are not null the selected area
        /// can be marked completed for the selected character.
        /// </summary>
        /// <returns><c>True</c> if an area can be marked as completed.</returns>
        private bool CanMarkAreaCompleted()
        {
            return SelectedAreaCharacter != null && SelectedAreaReference != null;
        }

        /// <summary>
        /// Gets all characters that have been formally named.
        /// </summary>
        /// <returns>Returns all named characters.</returns>
        private IEnumerable<Character> GetNamedCharacters()
        {
            return SortedCharacterList.View.Cast<Character>()
                .Where(c => !string.IsNullOrWhiteSpace(c.Name));
        }

        /// <summary>
        /// Open a window that shows an overview of all characters' area
        /// completion state. This does not show the state for unnamed
        /// characters as these cannot exist in the game world.
        /// </summary>
        private void ShowCompletionOverview()
        {
            var t = typeof(string);
            var table = new DataTable();
            var column = new DataColumn("Area", t);
            table.Columns.Add(column);

            _completionOverview?.Close();

            var namedCharacters = GetNamedCharacters().ToList();
            foreach (var c in namedCharacters)
            {
                table.Columns.Add(new DataColumn(c.Name, t));
            }

            _completionOverview = new CompletionOverviewView(table, WorldCompletionAreaNames);
            _completionOverview.Show();

            var areas = from a in AreaReferenceList
                        orderby a.Name
                        select new Area(a.Name)
                        {
                            Hearts = a.Hearts,
                            Waypoints = a.Waypoints,
                            PoIs = a.PoIs,
                            Skills = a.Skills,
                            Vistas = a.Vistas
                        };

            foreach (var a in areas)
            {
                var row = table.NewRow();
                row["Area"] = a.Name;
                foreach (var character in namedCharacters)
                {
                    UpdateAreaState(a, character);
                    row[character.Name] = a.State;
                }
                table.Rows.Add(row);
            }
        }

        /// <summary>
        /// Determines whether or not the area completion overview can be
        /// shown. The completion overview can be shown if at least one named
        /// character exists in <see cref="CharacterList"/>.
        /// </summary>
        /// <returns><c>True</c> if the completion overview can be
        /// shown.</returns>
        /// <seealso cref="GetNamedCharacters"/>
        private bool CanShowCompletionOverview()
        {
            return GetNamedCharacters().Any();
        }

        /// <summary>
        /// Signalled when a property of the current file was changed.
        /// The parameters are not used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MarkFileDirty(object? sender, EventArgs e)
        {
            UnsavedChanges = true;
        }

        /// <summary>
        /// Updates the biography options to match with
        /// <see cref="SelectedCharacter.Race"/> and
        /// <see cref="SelectedCharacter.Profession"/>
        /// <seealso cref="BiographyOptionsProfession"/>
        /// <seealso cref="BiographyOptionsPersonality"/>
        /// <seealso cref="BiographyOptionsRaceFirst"/>
        /// <seealso cref="BiographyOptionsRaceSecond"/>
        /// <seealso cref="BiographyOptionsRaceThird"/>
        /// </summary>
        public void UpdateBiographyOptions()
        {
            string? charProfession = SelectedCharacter?.Profession;
            string? charRace = SelectedCharacter?.Race;
            if (string.IsNullOrEmpty(charProfession) || string.IsNullOrEmpty(charRace))
            {
                IsBiographyVisible = Visibility.Collapsed;
                BiographyOptionsProfession = null;
                BiographyOptionsPersonality = null;
                BiographyOptionsRaceFirst = null;
                BiographyOptionsRaceSecond = null;
                BiographyOptionsRaceThird = null;
            }
            else
            {
                IsBiographyVisible = Visibility.Visible;
                var profOption = _biographyOptionsProfession[charProfession];
                BiographyOptionsProfession = profOption switch
                {
                    // Ranger requires special handling. It has a race
                    // dependency and so is nested one level further.
                    IDictionary<string, IReadOnlyList<string>> ranger => ranger[charRace],
                    // Non-rangers all have simple lists of options.
                    IReadOnlyList<string> other => other,
                    _ => throw new ArgumentException("" + profOption),
                };

                // Personality (this one is constant).
                BiographyOptionsPersonality = _biographyOptionsPersonality;

                // Race.
                var raceOption = _biographyOptionsRace[charRace];
                BiographyOptionsRaceFirst = raceOption["First"];
                BiographyOptionsRaceSecond = raceOption["Second"];
                BiographyOptionsRaceThird = raceOption["Third"];
            }
        }

        /// <summary>
        /// Wrapper for displaying a <see cref="MessageBox"/>.
        /// </summary>
        /// <param name="caption">The message box caption.</param>
        /// <param name="body">The message box message.</param>
        private static void ShowError(string caption, string body)
        {
            MessageBox.Show(body, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Updates the <see cref="Area.State"/> of the specified area to match
        /// the state of the corresponding area in
        /// <see cref="SelectedCharacter.Areas"/>. This is necessary because it
        /// does not hold that
        /// <c>AreaReferenceList \ SelectedCharacter.Areas == Ø</c>
        /// The state therefore has to be gotten from <c>AreaReferenceList</c>,
        /// which is always current, and thus serializing the value doesn't
        /// achieve anything.
        /// </summary>
        /// <seealso cref="Area.State"/>
        /// <param name="referenceArea">The area whose <c>State</c> to
        /// update.</param>
        /// <param name="c">The character whose completion state to get</param>
        private static void UpdateAreaState(Area referenceArea, Character c)
        {
            if (c.AreaByName.TryGetValue(referenceArea.Name, out Area? ca))
            {
                if (referenceArea.Hearts == ca.Hearts &&
                    referenceArea.Waypoints == ca.Waypoints &&
                    referenceArea.PoIs == ca.PoIs &&
                    referenceArea.Skills == ca.Skills &&
                    referenceArea.Vistas == ca.Vistas)
                {
                    if (referenceArea.Hearts != string.Empty)
                    {
                        referenceArea.State = CompletionState.Completed;
                    }
                }
                else
                {
                    if (ca.Hearts == "0"
                        && ca.Waypoints == "0"
                        && ca.PoIs == "0"
                        && ca.Skills == "0"
                        && ca.Vistas == "0")
                    {
                        referenceArea.State = CompletionState.NotBegun;
                    }
                    else
                    {
                        referenceArea.State = CompletionState.Begun;
                    }
                }
            }
            else
            {
                referenceArea.State = CompletionState.NotBegun;
            }
        }

        /// <summary>
        /// When a character or area was selected from their respective lists,
        /// this method is called and makes the necessary property changes.
        /// </summary>
        private void ChangedAreaOrCharacter()
        {
            if (SelectedCharacter == null)
            {
                return;
            }

            /// An area was selected. Find the matching area the selected
            /// character has already encountered, or create a new one if it
            /// doesn't exist yet.
            if (SelectedAreaReference != null)
            {
                if (SelectedCharacter.AreaByName.TryGetValue(
                    SelectedAreaReference.Name,
                    out Area? knownArea))
                {
                    SelectedAreaCharacter = knownArea;
                }
                if (SelectedAreaCharacter == null
                    || SelectedAreaCharacter.Name != SelectedAreaReference.Name)
                {
                    var newArea = new Area(SelectedAreaReference.Name);
                    newArea.PropertyChanged += MarkFileDirty;
                    SelectedCharacter.AreaByName[newArea.Name] = newArea;
                    SelectedAreaCharacter = newArea;
                }
            }

            /// Area states in the reference list can be updated without having
            /// and area selected.
            foreach (var ra in AreaReferenceList)
            {
                UpdateAreaState(ra, SelectedCharacter);
            }

            RaisePropertyChanged(nameof(Hearts));
            RaisePropertyChanged(nameof(Waypoints));
            RaisePropertyChanged(nameof(PoIs));
            RaisePropertyChanged(nameof(Skills));
            RaisePropertyChanged(nameof(Vistas));

            RaisePropertyChanged(nameof(HeartIcon));
            RaisePropertyChanged(nameof(WaypointIcon));
            RaisePropertyChanged(nameof(PoIIcon));
            RaisePropertyChanged(nameof(SkillIcon));
            RaisePropertyChanged(nameof(VistaIcon));

            UpdateStoryChapterCompletion();
            // Forcefully update the story chapter summary.
            // Here, in chronological order of release to match typical eye
            // movement.
            RaisePropertyChanged(nameof(HasCompletedLw2));
            RaisePropertyChanged(nameof(HasCompletedHoT));
            RaisePropertyChanged(nameof(HasCompletedKotT));
            RaisePropertyChanged(nameof(HasCompletedLw3));
            RaisePropertyChanged(nameof(HasCompletedPoF));
            RaisePropertyChanged(nameof(HasCompletedLw4));
            RaisePropertyChanged(nameof(HasCompletedTis));
        }

        /// <summary>
        /// Starts downloading update notes in the background, passing them to
        /// <c>e.Result</c> when finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">This event is passed to
        /// <see cref="UpdateWorker_RunWorkerCompleted"/>.</param>
        private void UpdateWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            using var tr = new XmlTextReader(Properties.Resources.cfgUpdateCheckUri);
            // We don't want to deal with the namespace for the version
            // history file, we're not validating it anyway.
            tr.Namespaces = false;
            e.Result = XDocument.Load(tr);
        }

        /// <summary>
        /// Called when the background updater finishes downloading update notes,
        /// and displays a dialog with information about available updates and
        /// the option to go to the location of a new update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">This event is passed from <see cref="UpdateWorker_DoWork"/>.</param>
        private void UpdateWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is InvalidOperationException)
                {
                    StatusBarUpdateCheck = String.Format(
                        Properties.Resources.suUpdateCheckFailed404,
                        Properties.Resources.cfgUpdateCheckUri, e.Error.Message);
                }
                else
                {
                    StatusBarUpdateCheck = String.Format(
                        Properties.Resources.suUpdateCheckFailedUnknown, e.Error.Message);
                }
            }
            else if (!e.Cancelled)
            {
                XDocument? doc = e.Result as XDocument;
                var publishedReleases = doc?.Root?.Element("Public");
                if (publishedReleases == null)
                {
                    StatusBarUpdateCheck = Properties.Resources.suUpdateCheckFailedReading;
                    return;
                }

                XElement? latest = publishedReleases.Element("Latest")?.Element("Release");
                string? newVersionValue = latest?.Element("Version")?.Value;
                if (newVersionValue == null)
                {
                    StatusBarUpdateCheck = Properties.Resources.suUpdateCheckFailedReading;
                    return;
                }

                var newVersion = new Version(newVersionValue);
                if (newVersion.IsNewerThan(_curVersion))
                {
                    StatusBarUpdateCheck = Properties.Resources.suUpdateCheckNewVersion;
                    UpdateWindow = new UpdateAvailableViewModel(
                        _curVersion,
                        newVersion,
                        latest?.Element("DownloadUrl")?.Value,
                        publishedReleases.Descendants("Release"));
                }
                else
                {
                    StatusBarUpdateCheck = Properties.Resources.suUpdateCheckNoUpdates;
                }
            }
        }

        /// <summary>
        /// Recalculates completion of all story chapters.
        /// </summary>
        internal void UpdateStoryChapterCompletion()
        {
            Character? selectedCharacter = SelectedCharacter;
            if (selectedCharacter == null)
            {
                return;
            }
            HasKeyLw2 = selectedCharacter.Lw2Acts[5].Chapters[2].ChapterCompleted;
            HasKeyHoT = selectedCharacter.HoTActs[2].Chapters[0].ChapterCompleted;
            // Chronologically reverse order of release date.
            // Assumption: characters are more likely to record new completions
            // of newer content, e.g. because they've already completed all old
            // content.
            HasCompletedTis = CalculateStoryChapterCompletion(selectedCharacter.TisActs);
            HasCompletedLw4 = CalculateStoryChapterCompletion(selectedCharacter.Lw4Acts);
            HasCompletedPoF = CalculateStoryChapterCompletion(selectedCharacter.PoFActs);
            HasCompletedLw3 = CalculateStoryChapterCompletion(selectedCharacter.Lw3Acts);
            HasCompletedKotT = CalculateStoryChapterCompletion(selectedCharacter.KotTActs);
            HasCompletedHoT = CalculateStoryChapterCompletion(selectedCharacter.HoTActs);
            HasCompletedLw2 = CalculateStoryChapterCompletion(selectedCharacter.Lw2Acts);
        }

        internal void TrimSelectedCharacterName()
        {
            Character? selectedCharacter = SelectedCharacter;
            if (selectedCharacter == null)
            {
                return;
            }
            selectedCharacter.Name = selectedCharacter.Name.Trim();
        }

        private static CompletionState CalculateStoryChapterCompletion(IReadOnlyList<Act> acts)
        {
            if (acts.SelectMany(a => a.Chapters).All(c => c.ChapterCompleted))
            {
                return CompletionState.Completed;
            }

            if (acts.SelectMany(a => a.Chapters).Any(c => c.ChapterCompleted))
            {
                return CompletionState.Begun;
            }

            return CompletionState.NotBegun;
        }

        private CompletionState _hasCompletedLw2;

        public CompletionState HasCompletedLw2
        {
            get { return _hasCompletedLw2; }
            private set
            {
                if (value != _hasCompletedLw2)
                {
                    _hasCompletedLw2 = value;
                    RaisePropertyChanged(nameof(HasCompletedLw2));
                }
            }
        }

        private CompletionState _hasCompletedHoT;

        public CompletionState HasCompletedHoT
        {
            get { return _hasCompletedHoT; }
            private set
            {
                if (value != _hasCompletedHoT)
                {
                    _hasCompletedHoT = value;
                    RaisePropertyChanged(nameof(HasCompletedHoT));
                }
            }
        }

        private CompletionState _hasCompletedKotT;

        public CompletionState HasCompletedKotT
        {
            get { return _hasCompletedKotT; }
            private set
            {
                if (value != _hasCompletedKotT)
                {
                    _hasCompletedKotT = value;
                    RaisePropertyChanged(nameof(HasCompletedKotT));
                }
            }
        }

        private CompletionState _hasCompletedLw3;

        public CompletionState HasCompletedLw3
        {
            get { return _hasCompletedLw3; }
            private set
            {
                if (value != _hasCompletedLw3)
                {
                    _hasCompletedLw3 = value;
                    RaisePropertyChanged(nameof(HasCompletedLw3));
                }
            }
        }

        private CompletionState _hasCompletedPoF;

        public CompletionState HasCompletedPoF
        {
            get { return _hasCompletedPoF; }
            private set
            {
                if (value != _hasCompletedPoF)
                {
                    _hasCompletedPoF = value;
                    RaisePropertyChanged(nameof(HasCompletedPoF));
                }
            }
        }

        private CompletionState _hasCompletedLw4;

        public CompletionState HasCompletedLw4
        {
            get { return _hasCompletedLw4; }
            private set
            {
                if (value != _hasCompletedLw4)
                {
                    _hasCompletedLw4 = value;
                    RaisePropertyChanged(nameof(HasCompletedLw4));
                }
            }
        }

        private CompletionState _hasCompletedTis;

        public CompletionState HasCompletedTis
        {
            get { return _hasCompletedTis; }
            private set
            {
                if (value != _hasCompletedTis)
                {
                    _hasCompletedTis = value;
                    RaisePropertyChanged(nameof(HasCompletedTis));
                }
            }
        }

        private bool _hasKeyLw2;

        public bool HasKeyLw2
        {
            get { return _hasKeyLw2; }
            private set
            {
                if (value != _hasKeyLw2)
                {
                    _hasKeyLw2 = value;
                    RaisePropertyChanged(nameof(HasKeyLw2));
                }
            }
        }

        private bool _hasKeyHoT;

        public bool HasKeyHoT
        {
            get { return _hasKeyHoT; }
            private set
            {
                if (value != _hasKeyHoT)
                {
                    _hasKeyHoT = value;
                    RaisePropertyChanged(nameof(HasKeyHoT));
                }
            }
        }

        /// <summary>
        /// <see cref="IDisposable"/> implementation. CLoses all secondary
        /// windows.
        /// </summary>
        public void Dispose()
        {
            _bgUpdater.Dispose();
            _updateViewModel?.Close();
            _completionOverview?.Close();
        }

        /// <summary>
        /// Sets a per-user file type association.
        /// If <paramref name="subKeyName"/> is a file extension, <paramref name="defaultValue"/> should be a <c>ProgID</c>.
        /// Otherwise, the first path component of <paramref name="subKeyName"/> should be a <c>ProgId</c>,
        /// with <paramref name="defaultValue"/> its value.
        /// </summary>
        /// <param name="subKeyName">The per-user file association sub-key whose "default" value to set</param>
        /// <param name="defaultValue">The "default" value of <c>subKeyName</c></param>
        [SupportedOSPlatform("windows")]
        private static void CurrentUserFileAssoc(string subKeyName, string defaultValue)
        {
            // Location of per-user file association: https://stackoverflow.com/a/69863/482758
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + subKeyName))
            {
                if (key == null)
                {
                    ShowError(Properties.Resources.msgRegisterExtensionFailedTitle,
                        String.Format(Properties.Resources.msgRegisterExtensionFailedBody, key));
                    return;
                }
                key.SetValue("", defaultValue);
            }

            const long SHCNE_ASSOCCHANGED = 0x08000000;
            const uint SHCNF_IDLIST = 0x0000;
            const uint SHCNF_FLUSHNOWAIT = 0x2000;
            NativeMethods.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST | SHCNF_FLUSHNOWAIT, IntPtr.Zero, IntPtr.Zero);
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        internal static extern void SHChangeNotify(
            long wEventId,
            uint uFlags,
            IntPtr dwItem1,
            IntPtr dwItem2);
    }
}
