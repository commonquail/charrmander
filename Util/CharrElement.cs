using System;
using System.Xml.Linq;

namespace Charrmander.Util
{
    public class CharrElement : XElement
    {
        public static XNamespace Charr { get; set; } = Properties.Resources.xNamespace;

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
