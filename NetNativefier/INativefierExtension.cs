using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nativefier
{
    public static class INativefierExtension
    {
        public static void AsyncGetAndForget<S, T>(this S self, string key, Action<T> onCompleted) where S : INativefier<T>
        {
            self.AsyncGet(key, onCompleted, true);
        }

        public static void AsyncGetOrFetchAndForget<S, T>(this S self, string key, Action<T> onCompleted) where S : INativefier<T>
        {
            self.AsyncGetOrFetch(key, onCompleted, true);
        }

        public static Task<T> AsyncGet<S, T>(this S self, string key, Action<T> onCompleted) where S : INativefier<T>
        {
            return self.AsyncGet(key, onCompleted, true);
        }

        public static Task<T> AsyncGetOrFetch<S, T>(this S self, string key, Action<T> onCompleted) where S : INativefier<T>
        {
            return self.AsyncGetOrFetch(key, onCompleted, true);
        }

        public static Task<T> AsyncGet<S, T>(this S self, string key) where S : INativefier<T>
        {
            return self.AsyncGet(key, null, true);
        }

        public static Task<T> AsyncGetOrFetch<S, T>(this S self, string key) where S : INativefier<T>
        {
            return self.AsyncGetOrFetch(key, null, true);
        }

        public static Task<T> AsyncGet<S, T>(this S self, string key, bool startTaskImmediately) where S : INativefier<T>
        {
            return self.AsyncGet(key, null, startTaskImmediately);
        }

        public static Task<T> AsyncGetOrFetch<S, T>(this S self, string key, bool startTaskImmediately) where S : INativefier<T>
        {
            return self.AsyncGetOrFetch(key, null, startTaskImmediately);
        }
    }
}
