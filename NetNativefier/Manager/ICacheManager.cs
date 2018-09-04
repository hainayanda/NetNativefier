using System;

namespace Nativefier.Manager
{
    public interface ICacheManager<T>
    {

        T this[string key] { get; set; }
        T Get(string key);
        void Put(T obj, string forKey);
        void Remove(string key);
        void Clear();
        bool IsExist(string key);
    }

}
