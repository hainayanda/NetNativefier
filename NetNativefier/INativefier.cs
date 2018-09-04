using Nativefier.Manager;
using Nativefier.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nativefier
{
    public interface INativefier<T> : ICacheManager<T>
    {
        Func<Task<T>> Fetcher { get; set; }
        IMemoryCacheDelegate<T> MemoryDelegate { get; set; }
        IDiskCacheDelegate<T> DiskDelegate { get; set; }
        Task<T> AsyncGet(string key, Action<T> onCompleted, bool startTaskImmediately);
        Task<T> AsyncGetOrFetch(string key, Action<T> onCompleted, bool startTaskImmediately);
    }
}
