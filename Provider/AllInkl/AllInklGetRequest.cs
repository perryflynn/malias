namespace maliasmgr.Provider.AllInkl
{
    /// <summary>
    /// All-Inkl uses just one single SOAP Endpoint end send JSON to it.
    /// (yes, for real)
    /// This object represents an standard "get data" request.
    /// </summary>
    public class AllInklGetRequest : Data.JsonObject
    {
        public string KasUser { get; set; }
        public string KasAuthType { get; set; } = "session";
        public string KasAuthData { get; set; }
        public string KasRequestType { get; set; }
        public dynamic KasRequestParams { get; set; }
    }
}
