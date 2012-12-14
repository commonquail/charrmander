using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Charrmander.Util
{
    public class CharrElement : XElement
    {
        public static XNamespace Charr = Properties.Resources.xNamespace;

        public CharrElement(string elementName, params Object[] content)
            : base(Charr + elementName, content)
        {
        }
        public CharrElement(string elementName, Object content)
            : base(Charr + elementName, content)
        {
        }
    }
}
