using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefier
{
    public interface INativefierSerializer<T>
    {
        byte[] Serialize(T obj);
        T Deserialize(byte[] bytes);
    }

    public abstract class NativefierStringSerializer<T> : INativefierSerializer<T>
    {
        public T Deserialize(byte[] bytes)
        {
            var str = StringEncode.GetString(bytes);
            if (str == null) return default(T);
            return DeserializeFromString(str);
        }

        public byte[] Serialize(T obj)
        {
            var str = SerializeToString(obj);
            if (str == null) return new byte[0];
            var bytes = StringEncode.GetBytes(str);
            return bytes;
        }

        public abstract Encoding StringEncode { get; }
        public abstract string SerializeToString(T obj);
        public abstract T DeserializeFromString(string str);
    }
}
