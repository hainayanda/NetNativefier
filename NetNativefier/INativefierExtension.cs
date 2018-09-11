using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nativefier
{
    public static class INativefierExtension
    {
        public static void AsyncGetAndForget<T>(this INativefier<T> self, string key, Action<T> onCompleted)
        {
            self.AsyncGet(key, onCompleted, true);
        }

        public static void AsyncGetOrFetchAndForget<T>(this INativefier<T> self, string key, Action<T> onCompleted)
        {
            self.AsyncGetOrFetch(key, onCompleted, true);
        }

        public static Task<T> AsyncGet<T>(this INativefier<T> self, string key, Action<T> onCompleted)
        {
            return self.AsyncGet(key, onCompleted, true);
        }

        public static Task<T> AsyncGetOrFetch<T>(this INativefier<T> self, string key, Action<T> onCompleted)
        {
            return self.AsyncGetOrFetch(key, onCompleted, true);
        }

        public static Task<T> AsyncGet<T>(this INativefier<T> self, string key)
        {
            return self.AsyncGet(key, null, true);
        }

        public static Task<T> AsyncGetOrFetch<T>(this INativefier<T> self, string key)
        {
            return self.AsyncGetOrFetch(key, null, true);
        }

        public static Task<T> AsyncGet<T>(this INativefier<T> self, string key, bool startTaskImmediately)
        {
            return self.AsyncGet(key, null, startTaskImmediately);
        }

        public static Task<T> AsyncGetOrFetch<T>(this INativefier<T> self, string key, bool startTaskImmediately)
        {
            return self.AsyncGetOrFetch(key, null, startTaskImmediately);
        }
    }
}
