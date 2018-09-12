using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativefierTest.Model
{
    public class TestModelBytes
    {
        public byte[] Content { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is TestModelBytes model)) return false;
            if (model.Content == null && Content == null) return true;
            else if ((model.Content != null && Content == null) || (model.Content == null && Content != null)) return false;
            else if (model.Content.Length != Content.Length) return false;
            foreach (var member in Content)
            {
                if (!model.Content.Contains(member)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 1997410482 + EqualityComparer<byte[]>.Default.GetHashCode(Content);
        }
    }
}
