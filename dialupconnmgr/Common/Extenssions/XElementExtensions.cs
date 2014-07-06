using System;
using System.Xml.Linq;

public static class XElementExtensions
{
    public static string GetElementValue(this XElement e, XName elementName)
    {
        var element = e.Element(elementName);
        if (element == null)
        {
            return String.Empty;
        }
        return element.Value;
    }

    public static string GetAttributeValue(this XElement e, XName attributeName)
    {
        var attribute = e.Attribute(attributeName);
        if (attribute == null)
        {
            return String.Empty;
        }
        return attribute.Value;
    }

    //public static void SetElementValue(this XElement e, XName elementName, object value)
    //{
    //    var element = e.Element(elementName);
    //    if (element != null)
    //    {
    //        element.SetValue(value);
    //    }
    //}

    //public static void SetAttributeValue(this XElement e, XName attributeName, object value)
    //{
    //    var attribute = e.Attribute(attributeName);
    //    if (attribute != null)
    //    {
    //        attribute.SetValue(value);
    //    }
    //}
}