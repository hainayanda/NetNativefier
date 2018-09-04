using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefier.Util
{
    internal static class Extension
    {
        internal static void CheckForNullAndThrow<T>(this T self)
        {
            if (self == null) throw new ArgumentNullException();
        }

        internal static void CheckForNullAndThrow<T>(this T self, string message)
        {
            if (self == null) throw new ArgumentNullException(message ?? "");
        }

        internal static void CheckIsEmptyAndThrow(this string self)
        {
            if (self == "") throw new ArgumentException();
        }

        internal static void CheckIsEmptyAndThrow(this string self, string message)
        {
            if (self == "") throw new ArgumentException(message ?? "");
        }
    }
}
