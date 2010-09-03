using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Xml.Linq;

namespace HCI_LEGACY_REF
{
    class XmlELementDynamic : DynamicObject
    {
        private System.Xml.XmlElement element;


        public XmlELementDynamic(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Element = element;
            ParentElement = element.Parent;
        }
        

        public XElement Element
        {
            get;
            private set;
        }

        public XElement ParentElement
        {
            get;
            private set;

        }
        public override string ToString()
        {
            return Element.ToString();
        }


        public XmlELementDynamic this[int sibblingIndex]
        {
            get
            {
                if (sibblingIndex != 0 && ParentElement == null)
                {
                    throw new ArgumentOutOfRangeException("sibblingIndex");
                }

                if (ParentElement == null)
                {
                    return this;
                }

                XElement sibblingElement = ParentElement.Elements(Element.Name).ElementAt(sibblingIndex);
                
                if (Element == sibblingElement)
                {
                    return this;
                }

                return sibblingElement.ToDynamic();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            result = null;
            XElement innerElement = Element.Element(binder.Name);
            
            if (innerElement != null)
            {
                result = innerElement.ToDynamic();
                return true;
            }
            
            return base.TryGetMember(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Element.Elements()
                         .Select(element => element.Name.LocalName)
                         .Distinct()
                         .OrderBy(name => name);
        }
    }
}
