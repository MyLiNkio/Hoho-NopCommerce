using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Nop.Plugin.Customization.CertificatesManager.Services;

public partial interface IMulticertificateAttributeService
{
    string SetMulticertificateAttributes(string attributesXml, MulticertificateAttributes attributes);
    MulticertificateAttributes GetMulticertificateAttributesFromXML(string attributesXml);
}

public partial class MulticertificateAttributeService : IMulticertificateAttributeService
{
    #region Utilities
    private static void SetXmlElement(XmlDocument xmlDoc, XmlElement multicertificateElement, string nodeName, string value)
    {
        var resValue = value?.Trim();
        if (string.IsNullOrEmpty(resValue))
            return;

        var element = xmlDoc.CreateElement(nodeName);
        element.InnerText = resValue;
        multicertificateElement.AppendChild(element);
    }
    #endregion

    public string SetMulticertificateAttributes(string attributesXml, MulticertificateAttributes attributes)
    {
        var result = string.Empty;
        try
        {
            var xmlDoc = new XmlDocument();
            if (string.IsNullOrEmpty(attributesXml))
            {
                var element1 = xmlDoc.CreateElement("Attributes");
                xmlDoc.AppendChild(element1);
            }
            else
                xmlDoc.LoadXml(attributesXml);

            var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

            var multicertificateElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo");
            if (multicertificateElement == null)
            {
                multicertificateElement = xmlDoc.CreateElement("MulticertificateInfo");
                rootElement.AppendChild(multicertificateElement);
            }

            SetXmlElement(xmlDoc, multicertificateElement, "RecipientFirstName", attributes.RecipientFirstName);
            SetXmlElement(xmlDoc, multicertificateElement, "RecipientLastName", attributes.RecipientLastName);
            SetXmlElement(xmlDoc, multicertificateElement, "RecipientEmail", attributes.RecipientEmail);

            SetXmlElement(xmlDoc, multicertificateElement, "SenderFirstName", attributes.SenderFirstName);
            SetXmlElement(xmlDoc, multicertificateElement, "SenderLastName", attributes.SenderLastName);
            SetXmlElement(xmlDoc, multicertificateElement, "SenderEmail", attributes.SenderEmail);

            SetXmlElement(xmlDoc, multicertificateElement, "Message", attributes.Message);

            if (attributes.SendAnonymously)
                SetXmlElement(xmlDoc, multicertificateElement, "SendAnonymously", attributes.SendAnonymously.ToString());

            if (attributes.SendAtUTC.HasValue)
                SetXmlElement(xmlDoc, multicertificateElement, "SendAtUTC", attributes.SendAtUTC.Value.ToString("o", CultureInfo.InvariantCulture));

            result = xmlDoc.OuterXml;
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }

        return result;
    }

    public virtual MulticertificateAttributes GetMulticertificateAttributesFromXML(string attributesXml)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(attributesXml);

            var attributes = new MulticertificateAttributes
            {
                RecipientFirstName = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/RecipientFirstName"))?.InnerText,
                RecipientLastName = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/RecipientLastName"))?.InnerText,
                RecipientEmail = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/RecipientEmail"))?.InnerText,

                SenderFirstName = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/SenderFirstName"))?.InnerText,
                SenderLastName = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/SenderLastName"))?.InnerText,
                SenderEmail = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/SenderEmail"))?.InnerText,

                Message = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/Message"))?.InnerText,
            };

            var sendAtUTCText = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/SendAtUTC"))?.InnerText;
            if (!string.IsNullOrEmpty(sendAtUTCText))
            {
                var parsed = DateTime.TryParse(sendAtUTCText, null, DateTimeStyles.RoundtripKind, out var value);
                if (parsed)
                    attributes.SendAtUTC = value.ToUniversalTime();
            }

            var sendAnonymuslyText = ((XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/MulticertificateInfo/SendAnonymously"))?.InnerText;
            if (!string.IsNullOrEmpty(sendAnonymuslyText))
            {
                var parsed = Boolean.TryParse(sendAnonymuslyText, out var value);
                if (parsed)
                    attributes.SendAnonymously = value;
            }

            return attributes;
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }
        return null;
    }
}

public class MulticertificateAttributes
{
    public string RecipientFirstName { get; set; }
    public string RecipientLastName { get; set; }

    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }

    public string RecipientEmail { get; set; }
    public string SenderEmail { get; set; }

    public string Message { get; set; }

    public DateTime? SendAtUTC { get; set; }

    public bool SendAnonymously { get; set; }
}
