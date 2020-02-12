namespace maliasmgr.Data
{
    /// <summary>
    /// Configuration Property for a provider.
    /// Is used in MaliasConfig to implementing dynamic key-value pairs.
    /// </summary>
    public sealed class MailiasConfigProperty
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
