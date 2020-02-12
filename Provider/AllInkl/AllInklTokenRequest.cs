namespace maliasmgr.Provider.AllInkl
{
    /// <summary>
    /// All-Inkl uses just one single SOAP Endpoint end send JSON to it.
    /// (yes, for real)
    /// This object represents an request to create an session token.
    /// </summary>
    internal sealed class AllInklTokenRequest : Data.JsonObject
    {
        public string KasUser { get; set; }
        public string KasAuthType { get; set; } = "sha1";
        public string KasPassword { get; set; }
        public int SessionLifeTime { get; set; } = 60 * 60 * 6;
        public string SessionUpdateLifeTime { get; set; } = "N";
    }
}
