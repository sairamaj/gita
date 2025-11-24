using Gita.Practice.App.Models;

namespace Gita.Practice.App.Repository;

public interface IDataRepository
{
    public IEnumerable<Tuple<string,int>> GetAllChapters();
    public Task<Chapter> Get(int chapter);
    Task<string> GetAuditFilePath(int chapter);
}
