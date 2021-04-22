using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Charrmander.Util
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns <c>True</c> if this <see cref="Version"/> instance is
        /// strictly newer than <c>other</c>.
        /// </summary>
        /// <param name="v">The <see cref="Version"/> instance being
        /// checked.</param>
        /// <param name="other">The <see cref="Version"/> instance being
        /// checked against.</param>
        /// <returns><c>True</c> if <c>v</c> is newer than <c>other</c> or
        /// <c>other</c> is <c>null</c>.</returns>
        public static bool IsNewerThan(this Version v, Version other)
        {
            return v.CompareTo(other) > 0;
        }

        /// <summary>
        /// Wrapper method for <see cref="XElement.Element(XName)"/>, returning
        /// the desired element or a new, empty element to avoid the need for
        /// constant <c>null</c> checks.
        /// </summary>
        /// <param name="xe">The <see cref="XElement"/> containing the child of
        /// interest.</param>
        /// <param name="name">The name of the child of <c>xe</c> we're
        /// interested in.</param>
        /// <returns>The child of <c>xe</c> identified by <c>name</c> or
        /// <c>new XElement(name);</c></returns>
        public static XElement CElement(this XElement xe, string name)
        {
            return xe.Element(CharrElement.Charr + name) ?? new CharrElement(name);
        }

        /// <summary>
        /// Wrapper method for <see cref="XElement.Elements(XName)"/>,
        /// returning the desired elements or a new zero-length array to avoid
        /// the need for constant <c>null</c> checks.
        /// </summary>
        /// <param name="xe">The <see cref="XElement"/> containing the children
        /// of interest.</param>
        /// <param name="name">The name of the children of <c>xe</c> we're
        /// interested in.</param>
        /// <returns>The children of <c>xe</c> identified by <c>name</c> or
        /// <c>new CharrElement[0];</c></returns>
        public static IEnumerable<XElement> CElements(this XElement xe, string name)
        {
            var ce = xe.Elements(CharrElement.Charr + name);
            if (ce == null)
            {
                ce = Array.Empty<CharrElement>();
            }
            return ce;
        }

        /// <summary>
        /// Wrapper method for <see cref="XContainer.Descendants(XName)"/>,
        /// returning the desired descendant elements or a new zero-length
        /// array to avoid the need for constant <c>null</c> checks.
        /// </summary>
        /// <param name="xc">The <see cref="XContainer"/> containing the
        /// descendants of interest.</param>
        /// <param name="name">The name of the children of <c>xc</c> we're
        /// interested in.</param>
        /// <returns>The descendants of <c>xc</c> identified by <c>name</c> or
        /// <c>new CharrElement[0];</c></returns>
        public static IEnumerable<XElement> CDescendants(this XContainer xc, string name)
        {
            var cd = xc.Descendants(CharrElement.Charr + name);
            if (cd == null)
            {
                cd = Array.Empty<CharrElement>();
            }
            return cd;
        }
    }
}
