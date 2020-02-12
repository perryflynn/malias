using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Text;
using System.Threading;

namespace maliasmgr.Extensions
{
    /// <summary>
    /// Extensions for easier handling of SOAP data.
    /// Why exists this class?
    /// Because I was not able to get the dotnet-svcutil working with the all-inkl endpoints.
    /// </summary>
    public static class SoapExtensions
    {
        /// <summary>
        /// Default xml declaration header
        /// </summary>
        /// <returns>An XML 1.0 header with utf-8 encoding</returns>
        private static readonly XDeclaration xmlDeclaration = new XDeclaration("1.0", "utf-8", null);

        /// <summary>
        /// Default xml writer settings
        /// </summary>
        /// <returns>Settings to serialize XML without header, idented and with utf-8 encoding</returns>
        private static readonly XmlWriterSettings xmlSettings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
            Async = true,
            Encoding = Encoding.UTF8,
            Indent = true
        };

        /// <summary>
        /// Execute an soap request via HttpClient
        /// </summary>
        /// <param name="client">A http client instance</param>
        /// <param name="url">SOAP Endpoint</param>
        /// <param name="soapAction">SOAPAction header content</param>
        /// <param name="xml">Request XML body</param>
        /// <returns>The HTTP Response</returns>
        public static async Task<HttpResponseMessage> ExecuteSoapRequest(
            this HttpClient client,
            string url,
            string soapAction,
            string xml)
        {
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(xml)
            };

            message.Headers.Add("SOAPAction", soapAction);
            return await client.SendAsync(message);
        }

        /// <summary>
        /// Parse SOAP XML and auto-detect custom namespaces and expose them as XML Namespace Manager
        /// </summary>
        /// <param name="stream">XML String/Stream Reader</param>
        /// <param name="namespaceManager">Contains all detected custom SOAP namespaces</param>
        /// <returns>Parsed XML Document</returns>
        public static XDocument ParseXml(this TextReader stream, out XmlNamespaceManager namespaceManager)
        {
            XmlReader reader = XmlReader.Create(stream);
            var document = XDocument.Load(reader);

            // add default SOAP namespace
            namespaceManager = new XmlNamespaceManager(reader.NameTable);
            namespaceManager.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");

            // add additional namespaces defined in the <SOAP-ENV:Envelope />
            var nsattributes = document
                .XPathSelectElement("/SOAP-ENV:Envelope", namespaceManager)
                .Attributes()
                .Where(attr => attr.Name.LocalName != "SOAP-ENV")
                .Where(attr => attr.Name.NamespaceName == "http://www.w3.org/2000/xmlns/");

            foreach(var nsattribute in nsattributes)
            {
                namespaceManager.AddNamespace(nsattribute.Name.LocalName, nsattribute.Value);
            }

            return document;
        }

        /// <summary>
        /// Parse SOAP XML and auto-detect custom namespaces and expose them as XML Namespace Manager
        /// </summary>
        /// <param name="xml">XML string</param>
        /// <param name="namespaceManager">Contains all detected custom SOAP namespaces</param>
        /// <returns>Parsed XML Document</returns>
        public static XDocument ParseXml(this string xml, out XmlNamespaceManager namespaceManager)
        {
            return ParseXml(new StringReader(xml), out namespaceManager);
        }

        /// <summary>
        /// Parse SOAP XML and auto-detect custom namespaces and expose them as XML Namespace Manager
        /// </summary>
        /// <param name="xml">XML Stream</param>
        /// <param name="namespaceManager">Contains all detected custom SOAP namespaces</param>
        /// <returns>Parsed XML Document</returns>
        public static XDocument ParseXml(this Stream xml, out XmlNamespaceManager namespaceManager)
        {
            return ParseXml(new StreamReader(xml), out namespaceManager);
        }

        /// <summary>
        /// I was not able to generate an utf-8 XML string.
        /// This method generates the XML without an xml header and add the
        /// header manually.
        /// </summary>
        /// <param name="document">An XML Document</param>
        /// <returns>Serialized XML as string</returns>
        public static async Task<string> ToFullString(this XDocument document)
        {
            var test = document.Declaration.ToString();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(xmlDeclaration.ToString());

            using (TextWriter textWriter = new StringWriter(builder))
            using(var xmlWriter = XmlWriter.Create(textWriter, xmlSettings))
            {
                await document.SaveAsync(xmlWriter, CancellationToken.None);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Checks if a SOAP Response is a failure
        /// </summary>
        /// <param name="document">XML Document</param>
        /// <param name="namespaceManager">Namespace Manager of the XML Document</param>
        /// <param name="errorCode">Exposes the Error Code found in the document</param>
        /// <returns></returns>
        public static bool IsFailure(this XDocument document, XmlNamespaceManager namespaceManager, out string errorCode)
        {
            var error = document.XPathSelectElement("/SOAP-ENV:Envelope/SOAP-ENV:Body/SOAP-ENV:Fault/faultstring", namespaceManager);
            if (error == null) {
                errorCode = null;
                return false;
            }

            errorCode = error.Value;
            return true;
        }
    }
}
