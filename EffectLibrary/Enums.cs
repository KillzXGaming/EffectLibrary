using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public enum ColorType : byte
    {
        Constant,
        Random,
        Animated8Key,
    }

    public enum WrapMode : byte
    {
        Mirror,
        Repeat,
        ClampEdge,
        MirrorOnce,
    }
}
