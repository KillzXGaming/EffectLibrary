using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public class VersionCheck : Attribute
    {
        public int Version;
        public VersionCompare Compare;

        public int Version2;
        public VersionCompare Compare2;

        public VersionCheck(VersionCompare compare, int version)
        {
            Compare = compare;
            Version = version;
        }

        public VersionCheck(VersionCompare compare, int version, VersionCompare compare2, int version2)
        {
            Compare = compare;
            Version = version;
            Compare2 = compare2;
            Version2 = version2;
        }

        public bool IsValid(int v)
        {
            if (Version2 != 0)
            {
                return CompareCheck(this.Compare, this.Version, v) &&
                       CompareCheck(this.Compare2, this.Version2, v);
            }
            return CompareCheck(this.Compare, this.Version, v);
        }

        private bool CompareCheck(VersionCompare c, int dst, int src)
        {
            switch (c)
            {
                case VersionCompare.Greater: return src > dst;
                case VersionCompare.GreaterOrEqual: return src >= dst;
                case VersionCompare.Less: return src < dst;
                case VersionCompare.Equals: return src == dst;
            }
            return true;
        }
    }

    public enum VersionCompare
    {
        Less,
        LessOrEqual,
        GreaterOrEqual,
        Greater,
        Equals,
    }
}
