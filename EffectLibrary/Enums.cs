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

    public enum GX2SurfaceFormat : byte
    {
        INVALID = 0x0,
        TCS_R8_G8_B8_A8 = 2,
        T_BC1_UNORM = 3,
        T_BC1_SRGB = 4,
        T_BC2_UNORM = 5,
        T_BC2_SRGB = 6,
        T_BC3_UNORM = 7,
        T_BC3_SRGB = 8,
        T_BC4_UNORM = 9,
        T_BC4_SNORM = 10,
        T_BC5_UNORM = 11,
        T_BC5_SNORM = 12,
        TC_R8_UNORM = 13,
        TC_R8_G8_UNORM = 14,
        TCS_R8_G8_B8_A8_UNORM = 15,
        TCS_R5_G6_B5_UNORM = 25,
    };
}

