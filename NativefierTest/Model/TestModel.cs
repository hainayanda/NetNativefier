using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativefierTest.Model
{
    public class TestModel
    {
        public string Str { get; set; }
        public int Integer { get; set; }
        public double Dbl { get; set; }
        public SubTestModel Sub { get; set; }

        public TestModel(string str, int integer, double dbl, SubTestModel sub)
        {
            Str = str;
            Integer = integer;
            Dbl = dbl;
            Sub = sub;
        }

        public static bool operator ==(TestModel left, TestModel right)
        {
            if (left == null && right != null) return false;
            else if (left != null && right == null) return false;
            else if (left == null && right == null) return true;
            return left.Equals(right);
        }

        public static bool operator !=(TestModel left, TestModel right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            var model = obj as TestModel;
            return model != null &&
                   Str == model.Str &&
                   Integer == model.Integer &&
                   Dbl == model.Dbl &&
                   EqualityComparer<SubTestModel>.Default.Equals(Sub, model.Sub);
        }

        public override int GetHashCode()
        {
            var hashCode = -1939694401;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Str);
            hashCode = hashCode * -1521134295 + Integer.GetHashCode();
            hashCode = hashCode * -1521134295 + Dbl.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SubTestModel>.Default.GetHashCode(Sub);
            return hashCode;
        }
    }

    public class SubTestModel
    {
        public string Str { get; set; }
        public int Integer { get; set; }
        public double Dbl { get; set; }

        public SubTestModel(string str, int integer, double dbl)
        {
            Str = str;
            Integer = integer;
            Dbl = dbl;
        }

        public static bool operator== (SubTestModel left, SubTestModel right)
        {
            if (left == null && right != null) return false;
            else if (left != null && right == null) return false;
            else if (left == null && right == null) return true;
            return left.Equals(right);
        }

        public static bool operator!= (SubTestModel left, SubTestModel right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is SubTestModel model &&
                   Str == model.Str &&
                   Integer == model.Integer &&
                   Dbl == model.Dbl;
        }

        public override int GetHashCode()
        {
            var hashCode = 531747578;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Str);
            hashCode = hashCode * -1521134295 + Integer.GetHashCode();
            hashCode = hashCode * -1521134295 + Dbl.GetHashCode();
            return hashCode;
        }
    }
}
