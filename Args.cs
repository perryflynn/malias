using CommandLine;

namespace maliasmgr
{
    /// <summary>
    /// Command line parser arguments
    /// </summary>
    public class Args
    {
        [Option('c', "create", MetaValue = "amazon", Default = null, Required = false, HelpText = "What do you want to do?")]
        public string CreateName { get; set; }

        [Option('d', "delete", MetaValue = "a.amazon.fj292@example.com", Default = null, Required = false, HelpText = "Alias to delete")]
        public string DeleteAlias { get; set; }

        [Option('l', "list", Default = false, Required = false, HelpText = "List existing aliases")]
        public bool List { get; set; }

        [Option('i', "info", Default = false, Required = false, HelpText = "Show current configuration")]
        public bool Info { get; set; }

        [Option('f', "force", Default = false, Required = false, HelpText = "Force the current operation")]
        public bool Force { get; set; }

        [Option("delete-existing", Default = false, Required = false, HelpText = "Delete existing alias with the same name")]
        public bool DeleteExisting { get; set; }

        [Option("silent", Default = false, Required = false, HelpText = "No output")]
        public bool Silent { get; set; }
    }
}
