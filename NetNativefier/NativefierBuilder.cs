using Nativefier.Model;
using System;
using System.Threading.Tasks;

namespace Nativefier
{
    public class NativefierBuilder<T>
    {
        private int DiskMaxCount, MemoryMaxCount;
        private string AppName, ContainerName;
        private Func<Task<T>> Fetcher;
        private IMemoryCacheDelegate<T> MemoryDelegate;
        private IDiskCacheDelegate<T> DiskDelegate;

        public NativefierBuilder()
        {
        }

        public NativefierBuilder<T> SetFetcher(Func<Task<T>> fetcher)
        {
            Fetcher = fetcher;
            return this;
        }

        public NativefierBuilder<T> SetAppName(string name)
        {
            AppName = name;
            return this;
        }

        public NativefierBuilder<T> SetContainerName(string name)
        {
            ContainerName = name;
            return this;
        }

        public NativefierBuilder<T> SetDiskDelegate(IDiskCacheDelegate<T> dDelegate)
        {
            DiskDelegate = dDelegate;
            return this;
        }

        public NativefierBuilder<T> SetMemoryDelegate(IMemoryCacheDelegate<T> mDelegate)
        {
            MemoryDelegate = mDelegate;
            return this;
        }

        public NativefierBuilder<T> SetMaxCount(int count)
        {
            DiskMaxCount = count;
            MemoryMaxCount = count / 2;
            return this;
        }

        public NativefierBuilder<T> SetDiskMaxCount(int count)
        {
            DiskMaxCount = count;
            return this;
        }

        public NativefierBuilder<T> SetMemoryMaxCount(int count)
        {
            MemoryMaxCount = count;
            return this;
        }

        public INativefier<T> Build()
        {
            INativefier<T> result;
            if (ContainerName == null && AppName != null) result = new Nativefier<T>(DiskMaxCount, MemoryMaxCount, AppName);
            else if (AppName != null) result = new Nativefier<T>(DiskMaxCount, MemoryMaxCount, AppName, ContainerName);
            else throw new MissingMemberException("Missing ContainerName");
            result.Fetcher = Fetcher;
            result.MemoryDelegate = MemoryDelegate;
            result.DiskDelegate = DiskDelegate;
            return result;
        }

    }
}
