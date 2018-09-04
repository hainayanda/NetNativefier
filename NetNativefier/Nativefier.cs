using Nativefier.Manager;
using Nativefier.Model;
using Nativefier.Util;
using System;
using System.Threading.Tasks;

namespace Nativefier
{
    public class Nativefier<T> : INativefier<T>
    {
        private MemoryManager<T> MemoryManager;
        private DiskManager<T> DiskManager;
        
        public Func<Task<T>> Fetcher;

        private IMemoryCacheDelegate<T> _MemoryDelegate;
        public IMemoryCacheDelegate<T> MemoryDelegate
        {
            get
            {
                return _MemoryDelegate;
            }
            set
            {
                _MemoryDelegate = value;
                if (value != null)
                {
                    MemoryManager.WillClearAction = () =>
                    {
                        value.OnClearMemory(this);
                    };
                    MemoryManager.WillRemoveAction = (T obj) =>
                    {
                        value.OnRemoveMemory(this, obj);
                    };
                }
                else
                {
                    MemoryManager.WillClearAction = null;
                    MemoryManager.WillRemoveAction = null;
                }
            }
        }
        private IDiskCacheDelegate<T> _DiskDelegate;
        public IDiskCacheDelegate<T> DiskDelegate
        {
            get
            {
                return _DiskDelegate;
            }
            set
            {
                _DiskDelegate = value;
                if (value != null)
                {
                    DiskManager.WillClearAction = () =>
                    {
                        _MemoryDelegate.OnClearMemory(this);
                    };
                    DiskManager.WillRemoveAction = (T obj) =>
                    {
                        _MemoryDelegate.OnRemoveMemory(this, obj);
                    };
                }
                else
                {
                    DiskManager.WillClearAction = null;
                    DiskManager.WillRemoveAction = null;
                }
            }
        }

        public Nativefier(int maxCount, string appName)
        {
            if (maxCount < 1) throw new ArgumentException("Nativefier max count must be greater than 1");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");

            MemoryManager = new MemoryManager<T>((maxCount / 2) > 0 ? (maxCount / 2) : 1);
            DiskManager = new DiskManager<T>(maxCount, appName);
        }

        public Nativefier(int maxCount, string appName, string containerName)
        {
            if (maxCount < 1) throw new ArgumentException("Nativefier max count must be greater than 1");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");
            containerName.CheckForNullAndThrow("containerName cannot be null");
            containerName.CheckIsEmptyAndThrow("containerName cannot be empty");

            MemoryManager = new MemoryManager<T>((maxCount / 2) > 0 ? (maxCount / 2) : 1);
            DiskManager = new DiskManager<T>(maxCount, appName, containerName);
        }

        public Nativefier(int diskMaxCount, int memoryMaxCount, string appName)
        {
            if (diskMaxCount < 1) throw new ArgumentException("Nativefier disk max count must be greater than 1");
            if (memoryMaxCount < 1) throw new ArgumentException("Nativefier memory max count must be greater than 1");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");

            MemoryManager = new MemoryManager<T>(memoryMaxCount);
            DiskManager = new DiskManager<T>(diskMaxCount, appName);
        }

        public Nativefier(int diskMaxCount, int memoryMaxCount, string appName, string containerName)
        {
            if (diskMaxCount < 1) throw new ArgumentException("Nativefier disk max count must be greater than 1");
            if (memoryMaxCount < 1) throw new ArgumentException("Nativefier memory max count must be greater than 1");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");
            containerName.CheckForNullAndThrow("containerName cannot be null");
            containerName.CheckIsEmptyAndThrow("containerName cannot be empty");

            MemoryManager = new MemoryManager<T>(memoryMaxCount);
            DiskManager = new DiskManager<T>(diskMaxCount, appName, containerName);
        }

        public T this[string key] { get => Get(key); set => Put(value, key); }

        public Task<T> AsyncGet(string key, Action<T> onCompleted)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            var task = new Task<T>(() =>
            {
                return Get(key);
            });
            task.ContinueWith((Task<T> completedTask) =>
            {
                onCompleted(completedTask.Result);
            });
            task.Start();
            return task;
        }

        public Task<T> AsyncGetOrFetch(string key, Action<T> onCompleted)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");
            var task = new Task<T>(() =>
            {
                var obj = Get(key);
                if(obj == null && Fetcher != null)
                {
                    var fetch = Fetcher?.Invoke();
                    return fetch.Result;
                }
                return obj;
            });
            task.ContinueWith((Task<T> completedTask) =>
            {
                onCompleted(completedTask.Result);
            });
            task.Start();
            return task;
        }

        public T Get(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            var obj = MemoryManager.Get(key);
            if (obj != null) return obj;
            obj = DiskManager.Get(key);
            if (obj != null) MemoryManager.Put(obj, key);
            return obj;
        }

        public void Put(T obj, string forKey)
        {
            forKey.CheckForNullAndThrow("key cannot be null");
            forKey.CheckIsEmptyAndThrow("key cannot be empty");
            obj.CheckForNullAndThrow("object cannot be null");

            MemoryManager.Put(obj, forKey);
            DiskManager.Put(obj, forKey);
        }

        public void Remove(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            MemoryManager.Remove(key);
            DiskManager.Remove(key);
        }

        public void Clear()
        {
            MemoryManager.Clear();
            DiskManager.Clear();
        }

        public bool IsExist(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            if (MemoryManager.IsExist(key)) return true;
            return DiskManager.IsExist(key);
        }
    }
}
