using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml;
using maliasmgr.Extensions;
using System.IO;
using System.Linq;
using System.Dynamic;

namespace maliasmgr.Provider.AllInkl
{
    /// <summary>
    /// Helper class to generate SOAP reuqests.
    /// Why exists this class?
    /// Because I was not able to get the dotnet-svcutil working with the all-inkl endpoints.
    /// </summary>
    internal sealed class RequestGenerator
    {
        /// <summary>
        /// File which contains the auth token request
        /// </summary>
        const string REQ_AUTH = "requests/all-inkl/get-authtoken.xml";

        /// <summary>
        /// XPath to add JSON Auth data into the SOAP XML
        /// </summary>
        const string REQ_AUTH_XPATH = "/SOAP-ENV:Envelope/SOAP-ENV:Body/ns1:KasAuth/Params";

        /// <summary>
        /// Action to get mail aliases list
        /// </summary>
        const string REQ_GETALIASES_NAME = "get_mailforwards";

        /// <summary>
        /// File which contains the general get request
        /// </summary>
        const string REQ_ACTION = "requests/all-inkl/action.xml";

        /// <summary>
        /// XPath to add JSON get data into the SOAP request
        /// </summary>
        const string REQ_ACTION_XPATH = "/SOAP-ENV:Envelope/SOAP-ENV:Body/ns1:KasApi/Params";

        /// <summary>
        /// Action create a new alias
        /// </summary>
        const string REQ_ADDALIAS_NAME = "add_mailforward";

        /// <summary>
        /// Action delete an existing alias
        /// </summary>
        const string REQ_DELALIAS_NAME = "delete_mailforward";

        /// <summary>
        /// All-Inkl Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// All-Inkl sha-1 hashed password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// API Session token
        /// </summary>
        public string SessionToken { get; set; }

        public string CreateAuthJson()
        {
            return new AllInklTokenRequest()
            {
                KasUser = this.Username,
                KasPassword = this.PasswordHash
            }
            .ToJSON();
        }

        /// <summary>
        /// Create SOAP to get an session token
        /// </summary>
        /// <returns>SOAP XML String</returns>
        public async Task<string> CreateAuthSoap()
        {
            var requestDocument = File.OpenText(REQ_AUTH).ParseXml(out XmlNamespaceManager requestNsMgr);

            var node = requestDocument.XPathSelectElement(REQ_AUTH_XPATH, requestNsMgr);
            node.Value = this.CreateAuthJson();

            return await requestDocument.ToFullString();
        }

        /// <summary>
        /// Create a general SOAP get request
        /// </summary>
        /// <param name="getAction">The GET Action</param>
        /// <param name="properties">Optional Properties</param>
        /// <returns>SOAP XML String</returns>
        public string CreateGetRequest(string getAction, dynamic properties)
        {
            return new AllInklGetRequest()
            {
                KasUser = this.Username,
                KasAuthData = this.SessionToken,
                KasRequestType = getAction,
                KasRequestParams = properties
            }
            .ToJSON();
        }

        /// <summary>
        /// Create a general SOAP get request
        /// </summary>
        /// <param name="getAction">The GET Action</param>
        /// <returns>SOAP XML String</returns>
        public string CreateGetRequest(string getAction)
        {
            return this.CreateGetRequest(getAction, new object());
        }

        /// <summary>
        /// Create an SOAP Request which returns all aliases
        /// </summary>
        /// <returns>SOAP XML String</returns>
        public async Task<string> CreateGetAliasesSoap()
        {
            var requestDocument = File.OpenText(REQ_ACTION).ParseXml(out XmlNamespaceManager requestNsMgr);

            var node = requestDocument.XPathSelectElement(REQ_ACTION_XPATH, requestNsMgr);
            node.Value = this.CreateGetRequest(REQ_GETALIASES_NAME);

            return await requestDocument.ToFullString();
        }

        /// <summary>
        /// Create an SOAP Request which creates an new alias
        /// </summary>
        /// <param name="source">New alias address</param>
        /// <param name="target">Alias target address</param>
        /// <returns>SOAP XML String</returns>
        public async Task<string> CreateAddAliasSoap(string source, string target)
        {
            dynamic properties = new ExpandoObject();
            properties.local_part = source.Split('@').First();
            properties.domain_part = source.Split('@').Last();
            properties.target_0 = target;

            var requestDocument = File.OpenText(REQ_ACTION).ParseXml(out XmlNamespaceManager requestNsMgr);

            var node = requestDocument.XPathSelectElement(REQ_ACTION_XPATH, requestNsMgr);
            node.Value = this.CreateGetRequest(REQ_ADDALIAS_NAME, properties);

            return await requestDocument.ToFullString();
        }

        /// <summary>
        /// Create an SOAP Request which deletes an alias
        /// </summary>
        /// <param name="aliasAddress">The alias to delete</param>
        /// <returns>SOAP XML String</returns>
        public async Task<string> CreateDeleteAliasSoap(string aliasAddress)
        {
            dynamic properties = new ExpandoObject();
            properties.mail_forward = aliasAddress;

            var requestDocument = File.OpenText(REQ_ACTION).ParseXml(out XmlNamespaceManager requestNsMgr);

            var node = requestDocument.XPathSelectElement(REQ_ACTION_XPATH, requestNsMgr);
            node.Value = this.CreateGetRequest(REQ_DELALIAS_NAME, properties);

            return await requestDocument.ToFullString();
        }
    }
}
