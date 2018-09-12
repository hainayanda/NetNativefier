using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Nativefier.Model;
using Nativefier.Util;

namespace Nativefier.Manager
{
    internal class DiskManager<T> : ICacheManager<T>
    {
        private readonly int MaxCount;
        private readonly string CachePath;
        private LinkedList<string> Index;
        private LinkedList<Pair<string, T>> PendingWrite = new LinkedList<Pair<string, T>>();
        private Pair<string, T> Writing;
        private ConcurrentDictionary<string, Task<T>> PendingRead = new ConcurrentDictionary<string, Task<T>>();
        private ConcurrentDictionary<string, Task> PendingDelete = new ConcurrentDictionary<string, Task>();
        private bool NeedUpdatingIndex = false;
        private bool IsUpdatingIndex = false;

        internal Action WillClearAction;
        internal Action<T> WillRemoveAction;
        internal INativefierSerializer<T> Serializer;

        internal DiskManager(int maxCount, string appName)
        {
            if (maxCount <= 0) throw new ArgumentException("disk count must be greater than 0");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");

            CachePath = Path.Combine(Path.GetTempPath(), appName, typeof(T).Name);
            MaxCount = maxCount;
            Index = ReadIndex();
        }

        internal DiskManager(int maxCount, string appName, string containerName)
        {
            if (maxCount <= 0) throw new ArgumentException("disk count must be greater than 0");
            appName.CheckForNullAndThrow("appName cannot be null");
            appName.CheckIsEmptyAndThrow("appName cannot be empty");
            containerName.CheckForNullAndThrow("containerName cannot be null");
            containerName.CheckIsEmptyAndThrow("containerName cannot be empty");

            CachePath = Path.Combine(Path.GetTempPath(), appName, containerName);
            MaxCount = maxCount;
            Index = ReadIndex();
        }

        public T this[string key] { get => Get(key); set => Put(value, key); }

        public void Clear()
        {
            WillClearAction?.Invoke();
            lock (Index)
            {
                foreach (var key in Index)
                {
                    ConcurentDeleteFor(key);
                }
                Index.Clear();
            }
            ConcurentIndexUpdate();
        }

        public T Get(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            if (!IsExist(key)) return default(T);
            Accessing(key);
            return ConcurentReadFor(key);
        }

        public bool IsExist(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            lock (Index) return Index.Contains(key);
        }

        public void Put(T obj, string forKey)
        {
            forKey.CheckForNullAndThrow("key cannot be null");
            forKey.CheckIsEmptyAndThrow("key cannot be empty");
            obj.CheckForNullAndThrow("object cannot be null");

            ConcurentWriteFor(obj, forKey);
            Accessing(forKey);
        }

        public void Remove(string key)
        {
            key.CheckForNullAndThrow("key cannot be null");
            key.CheckIsEmptyAndThrow("key cannot be empty");

            if (WillRemoveAction != null)
            {
                var obj = SecretlyGet(key);
                if(!EqualityComparer<T>.Default.Equals(obj, default(T))) WillRemoveAction.Invoke(obj);
            }
            ConcurentDeleteFor(key);
            lock (Index) Index.Remove(key);
            ConcurentIndexUpdate();
        }

        private T SecretlyGet(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();
            return ConcurentReadFor(key);
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
                string lastKey;
                lock (Index)
                {
                    lastKey = Index.Last.Value;
                    Index.RemoveLast();
                    count = Index.Count;
                }
                var obj = SecretlyGet(lastKey);
                if (!EqualityComparer<T>.Default.Equals(obj, default(T))) WillRemoveAction?.Invoke(obj);
                ConcurentDeleteFor(lastKey);
            }
            ConcurentIndexUpdate();
        }

        private void ConcurentIndexUpdate()
        {
            if (IsUpdatingIndex)
            {
                NeedUpdatingIndex = true;
                return;
            }
            IsUpdatingIndex = true;
            Task.Run(() =>
            {
                WriteToIndex();
                while (NeedUpdatingIndex)
                {
                    NeedUpdatingIndex = false;
                    WriteToIndex();
                }
                IsUpdatingIndex = false;
            });
        }

        private void WriteToIndex()
        {
            var indexPath = Path.Combine(CachePath, "index.idx");
            using (FileStream indexFile = new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter indexWriter = new StreamWriter(indexFile))
                {
                    string[] indexArray;
                    lock (Index)
                    {
                        indexArray = new string[Index.Count];
                        Index.CopyTo(indexArray, 0);
                    }
                    foreach (var key in indexArray)
                    {
                        indexWriter.WriteLine(key);
                    }
                    indexWriter.Dispose();
                }
                indexFile.Close();
            }
        }

        private LinkedList<string> ReadIndex()
        {
            var result = new LinkedList<string>();
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
                return result;
            }
            var indexPath = Path.Combine(CachePath, "index.idx");
            if (!File.Exists(indexPath)) return result;
            using (FileStream stream = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        result.AddLast(line);
                        line = reader.ReadLine();
                    }
                    reader.Dispose();
                }
                stream.Close();
                return result;
            }
        }

        private string GetObjectPathFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            return Path.Combine(CachePath, key + ".ch");
        }

        private void ConcurentDeleteFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            if (PendingDelete.ContainsKey(key)) return;
            var task = new Task(() =>
            {
                Pair<string, T>[] pendingArray;
                lock (PendingWrite)
                {
                    pendingArray = new Pair<string, T>[PendingWrite.Count];
                    PendingWrite.CopyTo(pendingArray, 0);
                }
                foreach (var pair in pendingArray)
                {
                    if (pair._0 == key)
                    {
                        lock (PendingWrite)
                        {
                            if(PendingWrite.Contains(pair)) PendingWrite.Remove(pair);
                        }
                    }
                }
                try
                {
                    DeleteFromDiskFor(key);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });
            task.ContinueWith((finishedTask) =>
            {
                PendingDelete.TryRemove(key, out var _);
            });
            PendingDelete[key] = task;
            task.Start();
        }

        private T ConcurentReadFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            var fromPending = GetFromPendingFor(key);
            if (!EqualityComparer<T>.Default.Equals(fromPending, default(T))) return fromPending;
            var task = new Task<T>(() =>
            {
                try
                {
                    return GetFromDiskFor(key);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return default(T);
                }
            });
            PendingRead[key] = task;
            task.ContinueWith((finishedTask) =>
            {
                if (PendingRead.ContainsKey(key)) PendingRead.TryRemove(key, out var _);
            });
            task.Start();
            return task.Result;

        }

        private T GetFromPendingFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            var fromWritingTask = new Task<T>(() =>
            {
                var writingPair = Writing;
                if (writingPair != null)
                    if (writingPair._0 == key) return writingPair._1;

                Pair<string, T>[] pendingArray;
                lock (PendingWrite)
                {
                    pendingArray = new Pair<string, T>[PendingWrite.Count];
                    PendingWrite.CopyTo(pendingArray, 0);
                }
                foreach (var pair in pendingArray)
                {
                    if (pair._0 == key) return pair._1;
                }
                return default(T);
            });
            PendingRead.TryGetValue(key, out var fromReadingTask);
            fromWritingTask.Start();
            var writeResult = fromWritingTask.Result;
            if (!EqualityComparer<T>.Default.Equals(writeResult, default(T))) return writeResult;
            else if (fromReadingTask != null) return fromReadingTask.Result;
            else return default(T);
            
        }

        private void ConcurentWriteFor(T obj, string forKey)
        {
            forKey.CheckForNullAndThrow();
            forKey.CheckIsEmptyAndThrow();
            obj.CheckForNullAndThrow();

            if (Writing != null) {
                Pair<string, T>[] pendingArray;
                lock (PendingWrite)
                {
                    pendingArray = new Pair<string, T>[PendingWrite.Count];
                    PendingWrite.CopyTo(pendingArray, 0);
                }
                foreach (var pair in pendingArray)
                {
                    if (pair._0 == forKey)
                    {
                        lock (PendingWrite) PendingWrite.Remove(pair);
                    }
                }
                lock (PendingWrite) PendingWrite.AddLast(new Pair<string, T>(forKey, obj));
                return;
            }
            Writing = new Pair<string, T>(forKey, obj);
            Task.Run(() =>
            {
                try
                {
                    WriteToDisk(obj, forKey);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                int count;
                lock (PendingWrite) count = PendingWrite.Count;
                while (count > 0)
                {
                    Pair<string, T> pair;
                    lock (PendingWrite)
                    {
                        pair = PendingWrite.First.Value;
                        PendingWrite.RemoveFirst();
                    }
                    Writing = pair;
                    try
                    {
                        WriteToDisk(pair._1, pair._0);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    lock (PendingWrite) count = PendingWrite.Count;
                }
                Writing = null;
            });
        }

        private void WriteToDisk(T obj, string forKey)
        {
            forKey.CheckForNullAndThrow();
            forKey.CheckIsEmptyAndThrow();
            obj.CheckForNullAndThrow();

            if(Serializer != null)
            {
                WriteToDiskUsing(Serializer, obj, forKey);
                return;
            }
            var objPath = GetObjectPathFor(forKey);
            var formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(objPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, obj);
                stream.Close();
            }
        }

        private void WriteToDiskUsing(INativefierSerializer<T> serializer, T obj, string forKey)
        {
            forKey.CheckForNullAndThrow();
            forKey.CheckIsEmptyAndThrow();
            obj.CheckForNullAndThrow();
            serializer.CheckForNullAndThrow();

            var objPath = GetObjectPathFor(forKey);
            var bytes = serializer.Serialize(obj);
            File.WriteAllBytes(objPath, bytes);
        }

        private void DeleteFromDiskFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            var objPath = GetObjectPathFor(key);
            if(File.Exists(objPath)) File.Delete(objPath);
        }

        private T GetFromDiskFor(string key)
        {
            key.CheckForNullAndThrow();
            key.CheckIsEmptyAndThrow();

            var objPath = GetObjectPathFor(key);
            if (!File.Exists(objPath)) return default(T);

            if(Serializer != null)
            {
                var bytes = File.ReadAllBytes(objPath);
                return Serializer.Deserialize(bytes);
            }
            var formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(objPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(0, SeekOrigin.Begin);
                T obj = (T)formatter.Deserialize(stream);
                stream.Close();
                return obj;
            }
        }
    }
}
