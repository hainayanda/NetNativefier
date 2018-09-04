using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefier.Model
{
    public interface IDiskCacheDelegate<T>
    {
        void OnClearDisk(INativefier<T> cache);
        void OnRemoveDisk(INativefier<T> cache, T forObj);
    }
}
