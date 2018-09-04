using Nativefier.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nativefier
{
    public interface INativefier<T> : ICacheManager<T>
    {
        Task<T> AsyncGet(string key, Action<T> onCompleted);
        Task<T> AsyncGetOrFetch(string key, Action<T> onCompleted);
    }
}
