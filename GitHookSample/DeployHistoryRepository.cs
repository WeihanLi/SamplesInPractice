using System.Collections.Concurrent;

namespace GitHookSample;

public interface IDeployHistoryRepository
{
    void AddDeployHistory(string service, DeployHistory deployHistory);
    DeployHistory[] GetDeployHistory(string service);
    IReadOnlyDictionary<string, DeployHistory[]> GetAllDeployHistory();
}

public class DeployHistoryRepository: IDeployHistoryRepository
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DeployHistory>> _store = new();
    private const int MaxDeployHistoryCount = 10;

    public void AddDeployHistory(string service, DeployHistory deployHistory)
    {
        var svcStore = _store.GetOrAdd(service, _ => new());
        svcStore.Enqueue(deployHistory);
        if (svcStore.Count > MaxDeployHistoryCount)
        {
            svcStore.TryDequeue(out _);
        }
    }
    
    public DeployHistory[] GetDeployHistory(string service)
    {
        if (_store.TryGetValue(service, out var svcStore))
            return svcStore.OrderByDescending(x => x.BeginTime).ToArray();

        return Array.Empty<DeployHistory>();
    }
    
    public IReadOnlyDictionary<string, DeployHistory[]> GetAllDeployHistory()
    {
        return _store.ToDictionary(
            x => x.Key, 
            x => x.Value.OrderByDescending(h => h.BeginTime).ToArray()
            );
    }
}
