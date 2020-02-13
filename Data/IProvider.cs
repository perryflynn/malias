using System.Collections.Generic;
using System.Threading.Tasks;

namespace maliasmgr.Data
{
    /// <summary>
    /// Each provider needs to implement this interface
    /// </summary>
    public interface IProvider
    {
        string ProviderKey { get; }
        void Configure(Data.MailiasConfig config);
        Task<IList<Data.Alias>> GetAliases();
        Task<IList<Data.Alias>> GetAliases(MailiasConfig config);
        Task<Data.CreateResult> CreateAlias(string sourceAddress, string targetAddress);
        Task<Data.DeleteResult> DeleteAliasAddress(string fullEmailAddress);
    }
}
