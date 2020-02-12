using System.Collections.Generic;
using System.Linq;

namespace maliasmgr.Data
{
    /// <summary>
    /// Represents an existing email alias
    /// </summary>
    public class Alias
    {
        public string SourceAddress { get; set; }
        public IList<string> TargetAddresses { get; set; }
        public bool IsCatchAll => this.SourceAddress.StartsWith("@");
        public string LocalPart => this.SourceAddress.Split('@').First();
        public string Domain => this.SourceAddress.Split('@').Last();
    }
}
