using System.Collections.Concurrent;

namespace Domain.Entities;

public class RequestProgress
{
    private readonly ConcurrentDictionary<int, int> progress = new();
    private int _nextId;

    public int OpenChannel()
    {
        return _nextId;
    }

    public void CloseChannel(int id)
    {
        _nextId = id;
    }
}
