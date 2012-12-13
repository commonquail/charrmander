using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Charrmander.Util;
using System.Xml.Linq;
using Charrmander.Properties;

namespace Charrmander.Model
{
    class Character : AbstractNotifier
    {
        private string _name;
        private string _profession;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string Profession
        {
            get { return _profession; }
            set
            {
                if (value != _profession)
                {
                    _profession = value;
                    RaisePropertyChanged("Profession");
                }
            }
        }

        public ObservableCollection<Area> Areas { get; set; }

        public Character()
        {
            Areas = new ObservableCollection<Area>();
            /*
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            XmlSchemaSet xs = new XmlSchemaSet();
            xs.Add(Resources.XNamespace,
                XmlReader.Create(Application.GetResourceStream(
                    new Uri("cv.xsd", UriKind.Relative)).Stream));
            settings.Schemas = xs;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            XmlReader r = XmlReader.Create(docPath, settings);
            XDocument doc = XDocument.Load(r);
            r.Close();
            Load(doc);
            */
        }

        public XDocument ToXML()
        {
            return new XDocument(
                new CharrElement("Charrmander",
                    new CharrElement("Character",
                        new CharrElement("Name", Name),
                        new CharrElement("Profession", Profession),
                        (Areas.Count > 0 ?
                        new CharrElement("Areas",
                            from a in Areas
                            select new CharrElement("Area",
                                new CharrElement("Name", a.Name),
                                new CharrElement("Hearts", a.Hearts),
                                new CharrElement("Waypoints", a.Waypoints),
                                new CharrElement("PoIs", a.PoIs),
                                new CharrElement("Skills", a.Skills),
                                new CharrElement("Vistas", a.Vistas)
                            )
                        ) : null)
                    )
                )
            );
        }

        private static XNamespace _charr = Resources.xNamespace;
        private class CharrElement : XElement
        {
            public CharrElement(string elementName, params Object[] content)
                : base(_charr + elementName, content)
            {
            }
            public CharrElement(string elementName, Object content)
                : base(_charr + elementName, content)
            {
            }
        }
    }
}
