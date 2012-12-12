using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Charrmander.Model;
using Charrmander.Util;

namespace Charrmander.ViewModel
{
    class ViewModelMain : AbstractNotifier
    {
        private Character _selectedCharacter;

        private Area _selectedAreaReference;
        private Area _selectedAreaCharacter;

        private bool _heartsCompleted = false;
        private bool _waypointsCompleted = false;
        private bool _poisCompleted = false;
        private bool _skillsCompleted = false;
        private bool _vistasCompleted = false;


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
                    OnPropertyChanged("SelectedCharacter");
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
                        OnPropertyChanged("Hearts");
                        OnPropertyChanged("Waypoints");
                        OnPropertyChanged("PoIs");
                        OnPropertyChanged("Skills");
                        OnPropertyChanged("Vistas");
                    }
                    OnPropertyChanged("SelectedAreaReference");
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
                    OnPropertyChanged("SelectedAreaCharacter");
                }
            }
        }

        private bool _isCharacterDetailEnabled = false;
        public bool IsCharacterDetailEnabled
        {
            get { return _isCharacterDetailEnabled; }
            set
            {
                if (value != _isCharacterDetailEnabled)
                {
                    _isCharacterDetailEnabled = value;
                    OnPropertyChanged("IsCharacterDetailEnabled");
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
                    OnPropertyChanged("Hearts");
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
                    OnPropertyChanged("Waypoints");
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
                    OnPropertyChanged("PoIs");
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
                    OnPropertyChanged("Skills");
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
                    OnPropertyChanged("Vistas");
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
                    OnPropertyChanged("HeartsCompleted");
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
                    OnPropertyChanged("WaypointsCompleted");
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
                    OnPropertyChanged("PoIsCompleted");
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
                    OnPropertyChanged("SkillsCompleted");
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
                    OnPropertyChanged("VistasCompleted");
                }
            }
        }
        #endregion // Area Completion

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
        }
    }
}
