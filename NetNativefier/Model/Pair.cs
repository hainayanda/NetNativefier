using System;
using System.Collections.Generic;
using System.Text;

namespace Nativefier.Model
{
    internal class Pair<X, Y>
    {
        internal X _0 { get; }
        internal Y _1 { get; }

        internal Pair(X first, Y second)
        {
            _0 = first;
            _1 = second;
        }
    }
}
