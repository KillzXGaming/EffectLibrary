using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    [Flags]
    public enum EmitterFlags : int
    {
        ZSort                     = 0x0200,
        ReverseParticleOrder      = 0x0400,
        GPUParticleCalculations = 0x800000,
    };

    public enum RandomSeedMode
    {
        PerEmitter,
        PerEmitterSet,
        Custom,
    }

    public enum ParserType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Uint,
        Int,
        Byte,
        Boolean,
    }

    public enum FileResourceFlags
    {
        HasTexture3 = 0x1,
        HasAlpha1 = 0x2,
        HasGPUBehavior = 0x4,
        UseShaderIndex = 0x8,
        HasNearFarAlpha = 0x10,
        VFXB = 0x20,
    }

    public enum CustomActionCallBackID : uint
    {
        Invalid = 0xFFFFFFFF,
        _0 = 0,
        _1, _2, _3, _4, _5, _6, _7,
        Max = 8,
    };

    public enum CustomShaderCallBackID : uint
    {
        Max = 9,
    }

    public enum FragmentShaderMode
    {
        Normal,
        Refraction,
        Distortion,
    }

    public enum FragmentColorSrc
    {
        RGB,
        A,
    }

    public enum AnimGroupType
    {
        EmissionRatio = 0,
        Lifespan = 1,
        ScaleX = 2,
        ScaleY = 3,
        ScaleZ = 4,
        RotationX = 5,
        RotationY = 6,
        RotationZ = 7,
        PositionX = 8,
        PositionY = 9,
        PositionZ = 10,
        Color0R = 11,
        Color0G = 12,
        Color0B = 13,
        Alpha0 = 14,
        AllDirectionVelocity = 15,
        DirectionVelocity = 16,
        PtclScaleX = 17,
        PtclScaleY = 18,
        Color1R = 19,
        Color1G = 20,
        Color1B = 21,
        EmissionShapeScaleX = 22,
        EmissionShapeScaleY = 23,
        EmissionShapeScaleZ = 24,
        Gravity = 25,
        EmitterColor0R = 38,
        EmitterColor0G = 39,
        EmitterColor0B = 40,
        EmitterColor1R = 41,
        EmitterColor1G = 42,
        EmitterColor1B = 43,
    }

    public enum FilterMode : byte
    {
        Linear,
        Nearest,
    }

    public enum TexturePatternType
    {
        None,
        FitLifespan,
        Clamp,
        Loop,
        Random,
    }

    public enum TextureRepeat
    {
        Repeat_1x1,
        Repeat_2x1,
        Repeat_1x2,
        Repeat_2x2,
    }

    public enum TextureSlot
    {
        _0,
        _1,
        _2,
        _3,
        DepthBuffer,
        FrameBuffer,
        CubeLightMap,
    }

    public enum CpuCore
    {
        _0 = 0,
        _1 = 1,
        _2 = 2,
        Max = 3,
    };

    public enum VertexRotationMode
    {
        None = 0,
        Rotate_X = 1,
        Rotate_Y = 2,
        Rotate_Z = 3,
        Rotate_XYZ = 4,
        Max = 5,
    };

    public enum EmitterType
    {
        Simple,
        Complex,
        Max,
    }

    public enum PtclType
    {
        Simple,
        Complex,
        Child,
        Max,
    }

    public enum MeshType
    {
        Particle,
        Primitive,
        Stripe,
        Max,
    }

    public enum WrapMode : byte
    {
        Mirror,
        Repeat,
        ClampEdge,
        MirrorOnce,
    }

    public enum DisplayFaceType
    {
        Both,
        Font,
        Back,
    }

    public enum PtclFollowType
    {
        SRT = 0,
        None = 1,
        Translate = 2,
        Max = 3,
    };

    public enum ColorSource
    {
        Constant,
        Random,
        Key4Value3,
        Key8,
    }

    public enum ColorMode
    {
        Color0,
        Color0MulTexture,
        Color0MulTextureAddColor1MulInvTexture,
        Color0MulTextureAddColor1,
    }

    public enum AlphaMode
    {
        TextureAlphaMulAlpha0,
        TextureAlphaMinusOneMinusAlpha0Mul2,
        TextureRedMulAlpha0,
        TextureRedMinusOneMinusAlpha0Mul2,
        TextureAlphaMulAlpha0MulAlpha1,
    }

    public enum ShaderType
    {
        Normal = 0,
        UserMacro1 = 1,
        UserMacro2 = 2,
        Max = 3,
    };

    public enum VertexTransformMode
    {
        Billboard,
        PlateXY,
        PlateXZ,
        DirectionalY,
        DirectionalPolygon,
        Stripe,
        ComplexStripe,
        Primitive,
        YBillboard,
    }

    public enum DepthType
    {
        DepthTestNoWriteLequal,
        NoDepthTest,
        DepthTestWriteLequal,
    }

    public enum BlendType
    {
        DefaultAlphaBlend,
        AddTranslucent,
        SubTranslucent,
        Multi,
        Screen,
    }

    public enum AnimationFunctions
    {
        Constant,
        Key4Value3,
        Key8,
        Random,
    }

    public enum EmitPrimitiveType
    {
        Vertex,
        Random,
        EmissionRate,
    }

    public enum EmitterFunctions
    {
        Point,
        Circle,
        CircleDiv,
        CircleFill,
        Sphere,
        SphereDiv,
        SphereDiv64,
        SphereFill,
        Cylinder,
        CylinderFill,
        Box,
        BoxFill,
        Line,
        LineDiv,
        Rectangle,
        Primitive,
        Max,
    }

    [Flags]
    public enum ShaderAvailableAttrib
    {
       Scale = 0x001,
       TexAnim = 0x002,
       SubTexAnim = 0x004,
       WorldPos = 0x008,
       WorldPosDif = 0x010,
       Color0 = 0x020,
       Color1 = 0x040,
       Rot = 0x080,
       EmMat = 0x100,
    }

    public enum ChildFlags
    {
        HasChild          = 0x0001,
        InheritColor0     = 0x0002,
        InheritFlucAlpha  = 0x0004,
        InheritPtclScale  = 0x0008,
        InheritRotation   = 0x0010,
        InheritVelocity   = 0x0020,
        InheritSRT        = 0x0040,
        DrawBeforeParent  = 0x1000,
        InheritColor1     = 0x8000,
        InheritColorScale = 0x10000,
    }

    [Flags]
    public enum ParticleBehavior
    {
        AirResist = 1,
        Gravity = 2,
        Rotation = 4,
        RotationInertia = 8,
        WorldDiff = 0x10,
        ScaleAnim = 0x40,
        Alpha0Anim = 0x80,
        Alpha1Anim = 0x100,
        Color0Anim = 0x200,
        Color1Anim = 0x400,
        UVShiftAnim0 = 0x800,
        UVShiftAnim1 = 0x1000,
        UVShiftAnim2 = 0x2000,
        PatternAnim0 = 0x4000,
        PatternAnim1 = 0x8000,
        PatternAnim2 = 0x40000,
        HasTexture1 = 0x80000,
        HasTexture2 = 0x100000,
        HasTexture3 = 0x200000,

    }

    public enum MatrixRefType
    {
        Emitter,
        Particle,
    }

    public enum GX2TexResFormat
    {
        INVALID = 0x0,
        TCS_R8_G8_B8 = 1,
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
