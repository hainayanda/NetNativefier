using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefier.Model
{
    public interface IMemoryCacheDelegate<T>
    {
        void OnClearMemory(INativefier<T> cache);
        void OnRemoveMemory(INativefier<T> cache, T forObj);
    }
}
