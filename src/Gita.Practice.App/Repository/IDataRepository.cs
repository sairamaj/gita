using Gita.Practice.App.Models;

namespace Gita.Practice.App.Repository
{
    internal interface IDataRepository
    {
        public Task<Chapter> Get(int chapter);
        Task<string> GetAuditFilePath(int chapter);
    }
}
