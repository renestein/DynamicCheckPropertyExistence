using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HCI_LEGACY_REF
{
    static class XmlElementExtensions
    {
        public static dynamic ToDynamic(this XElement element)
        {
            return new XmlELementDynamic(element);
        }
    }
}
