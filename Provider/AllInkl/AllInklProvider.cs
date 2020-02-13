using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using maliasmgr.Data;
using maliasmgr.Extensions;

namespace maliasmgr.Provider.AllInkl
{
    /// <summary>
    /// Provider implementation for All-Inkl
    /// </summary>
    public sealed class AllInklProvider : Data.IProvider
    {
        /// <summary>
        /// Property Name of the All-Inkl Username in mailias.json Provider Config Array
        /// </summary>
        private const string CFGKEY_USERNAME = "Username";

        /// <summary>
        /// Property Name of the All-Inkl Password (sha1 hash) in mailias.json Provider Config Array
        /// </summary>
        private const string CFGKEY_PASSWORD = "PasswordHash";

        /// <summary>
        /// The API SOAP Endpoint
        /// </summary>
        private const string ALLINKL_SOAPAPI_URL = "https://kasapi.kasserver.com/soap/KasApi.php";

        /// <summary>
        /// The SOAP Endpoint to create an session token
        /// </summary>
        private const string ALLINKL_SOAPAUTH_URL = "https://kasapi.kasserver.com/soap/KasAuth.php";

        /// <summary>
        /// SOAPAction HTTP Header for the API Endpoint
        /// </summary>
        private const string ALLINKL_SOAPAPI_METHOD = "urn:xmethodsKasApi#KasApi";

        /// <summary>
        /// SOAPAction HTTP Header for the session token endpoint
        /// </summary>
        private const string ALLINKL_SOAPAUTH_METHOD = "urn:xmethodsKasApiAuthentication#KasAuth";

        /// <summary>
        /// XPath to extract aliases from a SOAP response
        /// </summary>
        private const string ALLINKL_ALIASES_XPATH = "/SOAP-ENV:Envelope/SOAP-ENV:Body/ns1:KasApiResponse/return/item[key='Response']/value/item[key='ReturnInfo']/value/item";

        /// <summary>
        /// XPath to extract the session token from a SOAP response
        /// </summary>
        private const string ALLINKL_AUTHTOKEN_XPATH = "/SOAP-ENV:Envelope/SOAP-ENV:Body/ns1:KasAuthResponse";

        /// <summary>
        /// XPath to extract a alias source from an alias list
        /// </summary>
        private const string ALLINKL_ALIAS_SOURCE = "item[key='mail_forward_adress']/value";

        /// <summary>
        /// XPath to extract a alias target from an alias list
        /// </summary>
        private const string ALLINKL_ALIAS_TARGET = "item[key='mail_forward_targets']/value";

        /// <summary>
        /// Fault error code for "alias already exists"
        /// </summary>
        private const string ALLINKL_ERROR_ALIASEXISTS = "mail_forward_exists_as_forward";

        /// <summary>
        /// Fault error code for "alias not found"
        /// </summary>
        private const string ALLINKL_ERROR_ALIASNOTFOUND = "mail_forward_not_found_in_kas";

        /// <summary>
        /// Helper to generate the SOAP requests
        /// </summary>
        /// <returns>SOAP request generator</returns>
        private RequestGenerator Generator = new RequestGenerator();

        /// <summary>
        /// The providers key name, used in the mailias.json config
        /// </summary>
        public string ProviderKey => "AllInkl";

        /// <summary>
        /// Apply the configuration parameters to the provider
        /// </summary>
        /// <param name="config">The config object</param>
        public void Configure(MailiasConfig config)
        {
            this.Generator.Username = config.ProviderConfig.Single(cfg => cfg.Key == CFGKEY_USERNAME).Value;
            this.Generator.PasswordHash = config.ProviderConfig.Single(cfg => cfg.Key == CFGKEY_PASSWORD).Value;
        }

        /// <summary>
        /// Create an alias
        /// </summary>
        /// <param name="sourceAddress">The alias mail address</param>
        /// <param name="targetAddress">The target mail address</param>
        /// <returns>Creation result as enum</returns>
        public async Task<CreateResult> CreateAlias(string sourceAddress, string targetAddress)
        {
            await this.EnsureAuthToken();

            using(var webClient = new HttpClient())
            {
                // create request body and execute request
                var requestBody = await this.Generator.CreateAddAliasSoap(sourceAddress, targetAddress);
                var response = await webClient.ExecuteSoapRequest(ALLINKL_SOAPAPI_URL, ALLINKL_SOAPAPI_METHOD, requestBody);
                var responseText = await response.Content.ReadAsStreamAsync();

                // check result
                var document = responseText.ParseXml(out XmlNamespaceManager responseNsMgr);
                var isFailed = document.IsFailure(responseNsMgr, out string errorCode);

                if (isFailed && errorCode == ALLINKL_ERROR_ALIASEXISTS)
                {
                    return CreateResult.AlreadyExists;
                }
                else if (isFailed)
                {
                    return CreateResult.Fail;
                }
                else
                {
                    return CreateResult.Success;
                }
            }
        }

        /// <summary>
        /// Delete existing alias address
        /// </summary>
        /// <param name="fullEmailAddress">Full alias address to delete</param>
        /// <returns>The request result</returns>
        public async Task<DeleteResult> DeleteAliasAddress(string fullEmailAddress)
        {
            await this.EnsureAuthToken();

            using(var webClient = new HttpClient())
            {
                // create request body and execute request
                var requestBody = await this.Generator.CreateDeleteAliasSoap(fullEmailAddress);
                var response = await webClient.ExecuteSoapRequest(ALLINKL_SOAPAPI_URL, ALLINKL_SOAPAPI_METHOD, requestBody);
                var responseText = await response.Content.ReadAsStreamAsync();

                // check result
                var document = responseText.ParseXml(out XmlNamespaceManager responseNsMgr);
                var isFailed = document.IsFailure(responseNsMgr, out string errorCode);

                if (isFailed && errorCode == ALLINKL_ERROR_ALIASNOTFOUND)
                {
                    return DeleteResult.NotExists;
                }
                else if (isFailed)
                {
                    return DeleteResult.Fail;
                }
                else
                {
                    return DeleteResult.Success;
                }
            }
        }

        /// <summary>
        /// Get aliases list
        /// </summary>
        /// <returns>List of existing aliases</returns>
        public async Task<IList<Alias>> GetAliases()
        {
            await this.EnsureAuthToken();

            using(var webClient = new HttpClient())
            {
                // create request body and execute request
                var requestBody = await this.Generator.CreateGetAliasesSoap();
                var response = await webClient.ExecuteSoapRequest(ALLINKL_SOAPAPI_URL, ALLINKL_SOAPAPI_METHOD, requestBody);
                var content = await response.Content.ReadAsStreamAsync();

                // parse result
                var document = content.ParseXml(out XmlNamespaceManager namespaceManager);

                return document.XPathSelectElements(ALLINKL_ALIASES_XPATH, namespaceManager)
                    .Select(node => new Data.Alias()
                    {
                        SourceAddress = node.XPathSelectElement(ALLINKL_ALIAS_SOURCE).Value,
                        TargetAddresses = node.XPathSelectElement(ALLINKL_ALIAS_TARGET).Value?.Split(',')
                    })
                    .OrderBy(alias => alias.Domain)
                    .ThenBy(alias => alias.IsCatchAll ? 0 : 1)
                    .ThenBy(alias => alias.SourceAddress)
                    .ToList();
            }
        }

        /// <summary>
        /// Get aliases which was created by this application
        /// </summary>
        /// <param name="config">mailias.json configuration</param>
        /// <returns>Aliases which match the prefix and domain</returns>
        public async Task<IList<Data.Alias>> GetAliases(MailiasConfig config)
        {
            return (await this.GetAliases())
                .Where(a =>
                    (string.IsNullOrWhiteSpace(config.Prefix) || a.LocalPart.StartsWith(config.Prefix + ".")) &&
                    a.Domain == config.MailDomain &&
                    a.TargetAddresses.Any(ta => ta == config.TargetAddress)
                )
                .ToList();
        }

        /// <summary>
        /// Ensure authentication
        /// </summary>
        /// <returns>An async task to wait for</returns>
        private async Task EnsureAuthToken()
        {
            if (string.IsNullOrWhiteSpace(this.Generator.SessionToken))
            {
                using(var webClient = new HttpClient())
                {
                    // Create and execute the request
                    var xmlBody = await this.Generator.CreateAuthSoap();
                    var response = await webClient.ExecuteSoapRequest(ALLINKL_SOAPAUTH_URL, ALLINKL_SOAPAUTH_METHOD, xmlBody);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Extract the auth token
                    var responseDocument = responseContent.ParseXml(out XmlNamespaceManager responseNsMgr);
                    this.Generator.SessionToken = responseDocument.XPathSelectElement(ALLINKL_AUTHTOKEN_XPATH, responseNsMgr).Value;
                }
            }
        }
    }
}
