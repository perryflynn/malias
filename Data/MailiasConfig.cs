using System.Collections.Generic;

namespace maliasmgr.Data
{
    /// <summary>
    /// Application configuration
    /// Will stored as ~/malias.json
    /// </summary>
    public sealed class MailiasConfig
    {
        public string MailDomain { get; set; }
        public string TargetAddress { get; set; }
        public string Prefix { get; set; }
        public int UniqeIdLength { get; set; }
        public string Provider { get; set; }
        public IList<MailiasConfigProperty> ProviderConfig { get; set; }
    }
}
