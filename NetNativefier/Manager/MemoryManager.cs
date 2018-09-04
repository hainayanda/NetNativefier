using Nativefier.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Nativefier.Manager
{
    internal class MemoryManager<T> : ICacheManager<T>
    {

        private ConcurrentDictionary<string, T> Cache = new ConcurrentDictionary<string, T>();
        private LinkedList<string> Index = new LinkedList<string>();
        private readonly int MaxCount;

        internal Action WillClearAction;
        internal Action<T> WillRemoveAction;

        internal MemoryManager(int maxCount)
        {
            if (maxCount <= 0) throw new ArgumentException("memory count must be greater than 0");
            MaxCount = maxCount;
        }

        public T this[string key] { get => Get(key); set => Put(value, key); }

        public void Clear()
        {
            WillClearAction?.Invoke();
            lock(Index) Index.Clear();
            Cache.Clear();
        }

        public T Get(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            if (!IsExist(key)) return default(T);
            Accessing(key);
            return Cache[key];
        }

        public bool IsExist(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            return Cache.ContainsKey(key);
        }

        public void Put(T obj, string forKey)
        {
            forKey.CheckForNullAndThrow("key cannot be null");
            forKey.CheckIsEmptyAndThrow("key cannot be empty");
            obj.CheckForNullAndThrow("object cannot be null");

            Accessing(forKey);
            Cache[forKey] = obj;
        }

        public void Remove(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            lock (Index) if (Index.Contains(key)) Index.Remove(key);

            if (Cache.ContainsKey(key))
            {
                var obj = Cache[key];
                WillRemoveAction?.Invoke(obj);
                Cache.TryRemove(key, out var _);
            }
        }

        private void Accessing(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            int count;
            lock (Index)
            {
                if (Index.Contains(key)) Index.Remove(key);
                Index.AddFirst(key);
                count = Index.Count;
            }
            while (count > MaxCount)
            {
                var lastKey = Index.Last.Value;
                if (Cache.ContainsKey(lastKey))
                {
                    var obj = Cache[lastKey];
                    WillRemoveAction?.Invoke(obj);
                    Cache.TryRemove(lastKey, out var _);
                }
                lock (Index)
                {
                    Index.RemoveLast();
                    count = Index.Count;
                }
            }
        }
    }
}
