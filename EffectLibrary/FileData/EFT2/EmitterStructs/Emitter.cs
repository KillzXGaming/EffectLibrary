using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary.EFT2
{
    public class EmitterData
    {
        public uint Flag;
        public uint RandomSeed;
        public uint Padding1;
        public uint Padding2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        [VersionCheck(VersionCompare.Less, 40)]
        public string Name = null;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        [VersionCheck(VersionCompare.GreaterOrEqual, 40)]
        public string Namev40 = null;

        public EmitterStatic EmitterStatic = new EmitterStatic();
        public EmitterInfo EmitterInfo = new EmitterInfo();
        public EmitterInheritance ChildInheritance = new EmitterInheritance();
        public Emission Emission = new Emission();
        public EmitterShapeInfo ShapeInfo = new EmitterShapeInfo();
        public EmitterRenderState RenderState = new EmitterRenderState();
        public ParticleData ParticleData = new ParticleData();

        [VersionCheck(VersionCompare.Less, 36)]
        public EmitterCombiner Combiner = null;

        [VersionCheck(VersionCompare.Equals, 36)]
        public EmitterCombinerV36 CombinerV36 = null;

        [VersionCheck(VersionCompare.Greater, 40)]
        public EmitterCombinerV40 CombinerV40 = null;

        public ShaderRefInfo ShaderReferences = new ShaderRefInfo();

        public ActionInfo Action = new ActionInfo();

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        [VersionCheck(VersionCompare.Greater, 40)]
        public string DepthMode = null;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
        [VersionCheck(VersionCompare.Greater, 40)]
        public string PassInfo = null;

        public ParticleVelocityInfo ParticleVelocity = new ParticleVelocityInfo();

        [VersionCheck(VersionCompare.GreaterOrEqual, 36)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] UnknownV36 = null;

        public ParticleColor ParticleColor = new ParticleColor();
        public ParticleScale ParticleScale = new ParticleScale();
        public ParticleFlucInfo ParticleFluctuation = new ParticleFlucInfo();

        public TextureSampler Sampler0;
        public TextureSampler Sampler1;
        public TextureSampler Sampler2;

        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureSampler Sampler3;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureSampler Sampler4;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureSampler Sampler5;

        public TextureAnim TextureAnim0;
        public TextureAnim TextureAnim1;
        public TextureAnim TextureAnim2;

        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureAnim TextureAnim3;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureAnim TextureAnim4;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TextureAnim TextureAnim5;

        [VersionCheck(VersionCompare.GreaterOrEqual, 22)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
        public byte[] Reserved = null;

        [IgnoreDataMember]
        public int Order;

        public List<TextureSampler> GetSamplers()
        {
            List<TextureSampler> samplers = new List<TextureSampler>();
            samplers.Add(Sampler0);
            samplers.Add(Sampler1);
            samplers.Add(Sampler2);

            if (Sampler3  != null) //v40 > with 6 samplers total
            {
                samplers.Add(Sampler3);
                samplers.Add(Sampler4);
                samplers.Add(Sampler5);
            }
            return samplers;
        }
    }

    public class EmitterStatic
    {
        public uint Flags1;
        public uint Flags2;
        public uint Flags3;
        public uint Flags4;

        public uint NumColor0Keys;
        public uint NumAlpha0Keys;
        public uint NumColor1Keys;
        public uint NumAlpha1Keys;
        public uint NumScaleKeys;
        public uint NumParamKeys;

        public uint Unknown1;
        public uint Unknown2;

        [VersionCheck(VersionCompare.Greater, 50)]
        public uint NumAnim2Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public uint NumAnim3Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public uint NumAnim4Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public uint NumAnim5Keys;

        public float Color0LoopRate;
        public float Alpha0LoopRate;
        public float Color1LoopRate;
        public float Alpha1LoopRate;
        public float ScaleLoopRate;

        public float Color0LoopRandom;
        public float Alpha0LoopRandom;
        public float Color1LoopRandom;
        public float Alpha1LoopRandom;
        public float ScaleLoopRandom;

        public float Unknown3;
        public float Unknown4;

        public float GravityDirX;
        public float GravityDirY;
        public float GravityDirZ;

        public float GravityScale;

        public float AirRes;

        public float val_0x74;
        public float val_0x78;
        public float val_0x82;

        public float CenterX;
        public float CenterY;

        public float Offset;
        public float Padding;

        public float AmplitudeX;
        public float AmplitudeY;

        public float CycleX;
        public float CycleY;

        public float PhaseRndX;
        public float PhaseRndY;

        public float PhaseInitX;
        public float PhaseInitY;

        public float Coefficient0;
        public float Coefficient1;

        public float val_0xB8;
        public float val_0xBC;

        public TexPatAnim TexPatternAnim0;
        public TexPatAnim TexPatternAnim1;
        public TexPatAnim TexPatternAnim2;

        [VersionCheck(VersionCompare.Greater, 40)]
        public TexPatAnim TexPatternAnim3;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TexPatAnim TexPatternAnim4;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TexPatAnim TexPatternAnim5;

        public TexScrollAnim TexScrollAnim0;
        public TexScrollAnim TexScrollAnim1;
        public TexScrollAnim TexScrollAnim2;

        [VersionCheck(VersionCompare.Greater, 40)]
        public TexScrollAnim TexScrollAnim3;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TexScrollAnim TexScrollAnim4;
        [VersionCheck(VersionCompare.Greater, 40)]
        public TexScrollAnim TexScrollAnim5;

        public float ColorScale;
        public float val_0x364;
        public float val_0x368;
        public float val_0x36A;

        public AnimationKeyTable Color0;
        public AnimationKeyTable Alpha0;
        public AnimationKeyTable Color1;
        public AnimationKeyTable Alpha1;

        public float SoftEdgeParam1;
        public float SoftEdgeParam2;
        public float FresnelAlphaParam1;
        public float FresnelAlphaParam2;
        public float NearDistAlphaParam1;
        public float NearDistAlphaParam2;
        public float FarDistAlphaParam1;
        public float FarDistAlphaParam2;

        public float DecalParam1;
        public float DecalParam2;

        public float AlphaThreshold;
        public float Padding2;

        public float AddVelToScale;
        public float SoftPartcileDist;
        public float SoftParticleVolume;
        public float Padding3;

        public AnimationKeyTable ScaleAnim;
        public AnimationKeyTable ParamAnim;

        [VersionCheck(VersionCompare.Greater, 50)]
        public AnimationKeyTable Anim1Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public AnimationKeyTable Anim2Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public AnimationKeyTable Anim3Keys;
        [VersionCheck(VersionCompare.Greater, 50)]
        public AnimationKeyTable Anim4Keys;

        [VersionCheck(VersionCompare.Greater, 40)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] Unknown6; 

        public float RotateInitX;
        public float RotateInitY;
        public float RotateInitZ;
        public float RotateInitEmpty;

        public float RotateInitRandX;
        public float RotateInitRandY;
        public float RotateInitRandZ;
        public float RotateInitRandEmpty;

        public float RotateAddX;
        public float RotateAddY;
        public float RotateAddZ;
        public float RotateRegist;

        public float RotateAddRandX;
        public float RotateAddRandY;
        public float RotateAddRandZ;
        public float Padding4;

        public float ScaleLimitDistNear;
        public float ScaleLimitDistFar;

        public float Padding5;
        public float Padding6;

        [VersionCheck(VersionCompare.Greater, 40)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] Unknown7;
    }

    public class TexPatAnim
    {
        public float Num;
        public float Frequency;
        public float NumRandom;
        public float Pad;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] Table = new int[32];
    }

    public class TexScrollAnim
    {
        public float ScrollAddX;
        public float ScrollAddY;

        public float ScrollX;
        public float ScrollY;

        public float ScrollRandomX;
        public float ScrollRandomY;

        public float ScaleAddX;
        public float ScaleAddY;

        public float ScaleX;
        public float ScaleY;

        public float ScaleRandomX;
        public float ScaleRandomY;

        public float RotationAdd;
        public float Rotation;
        public float RotationRandom;
        public float RotationType;

        public float UVScaleX;
        public float UVScaleY;

        public float UVDivX;
        public float UVDivY;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class AnimationKeyTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public AnimationKey[] Keys = new AnimationKey[8];
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class AnimationKey
    {
        public float X;
        public float Y;
        public float Z;
        public float Time; //ratio 0.0 -> 1.0
    }


    public class EmitterInfo
    {
        public byte IsParticleDraw;
        public byte SortType;
        public byte CalcType;
        public byte FollowType;
        public byte IsFadeEmit;
        public byte IsFadeAlphaFade;
        public byte IsScaleFade;
        public byte RandomSeedType;
        public byte IsUpdateMatrixByEmit;
        public byte TestAlways;
        public byte InterpolateEmissionAmount;
        public byte IsAlphaFadeIn;
        public byte IsScaleFadeIn;
        public byte padding1;
        public byte padding2;
        public byte padding3;

        public uint RandomSeed;
        public uint DrawPath; //render pass
        public int AlphaFadeTime;
        public int FadeInTime;
        public float TransX;
        public float TransY;
        public float TransZ;
        public float TransRandX;
        public float TransRandY;
        public float TransRandZ;
        public float RotateX;
        public float RotateY;
        public float RotateZ;
        public float RotateRandX;
        public float RotateRandY;
        public float RotateRandZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;

        public float Color0R;
        public float Color0G;
        public float Color0B;
        public float Color0A;

        public float Color1R;
        public float Color1G;
        public float Color1B;
        public float Color1A;

        public float EmissionRangeNear;
        public float EmissionRangeFar;
        public float EmissionRatioFar;
    }


    public class EmitterInheritance
    {
        public byte Velocity;
        public byte Scale;
        public byte Rotate;
        public byte ColorScale;
        public byte Color0;
        public byte Color1;
        public byte Alpha0;
        public byte Alpha1;
        public byte DrawPath;
        public byte PreDraw;
        public byte Alpha0EachFrame;
        public byte Alpha1EachFrame;
        public byte EnableEmitterParticle;
        public byte padding1;
        public byte padding2;
        public byte padding3;

        [VersionCheck(VersionCompare.Greater, 40)]
        public ulong UnknownV40;

        public float VelocityRate;
        public float ScaleRate;
    }


    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class Emission
    {
        public bool isOneTime;
        public bool IsWorldGravity;
        public bool val_0x3;
        public bool val_0x4;
        public uint Start;
        public uint Timing;
        public uint Duration;
        public float Rate;
        public float RateRandom;
        public int Interval;
        public float IntervalRandom;
        public float PositionRandom;
        public float GravityScale;
        public float GravityDirX;
        public float GravityDirY;
        public float GravityDirZ;
        public float EmitterDistUnit;
        public float EmitterDistMin;
        public float EmitterDistMax;
        public float EmitterDistMarg;
        public int EmitterDistParticlesMax;
    }

    public class EmitterShapeInfo
    {
        public byte VolumeType;
        public byte SweepStartRandom;
        public byte ArcType;
        public byte IsVolumeLatitudeEnabled;
        public byte VolumeTblIndex;
        public byte VolumeTblIndex64;
        public byte VolumeLatitudeDir;
        public byte IsGpuEmitter;
        public float SweepLongitude;
        public float SweepLatitude;
        public float SweepStart;
        public float VolumeSurfacePosRand;
        public float CaliberRatio;
        public float LineCenter;
        public float LineLength;
        public float VolumeRadiusX;
        public float VolumeRadiusY;
        public float VolumeRadiusZ;
        public float VolumeFormScaleX;
        public float VolumeFormScaleY;
        public float VolumeFormScaleZ;
        public int PrimEmitType;
        public ulong PrimitiveIndex;
        public int numDivideCircle;
        public int numDivideCircleRandom;
        public int numDivideLine;
        public int numDivideLineRandom;

        [VersionCheck(VersionCompare.Less, 40)]
        public byte IsOnAnotherBinaryVolumePrimitive;
        [VersionCheck(VersionCompare.Less, 40)]
        public byte padding1;
        [VersionCheck(VersionCompare.Less, 40)]
        public byte padding2;
        [VersionCheck(VersionCompare.Less, 40)]
        public byte padding3;
        [VersionCheck(VersionCompare.Less, 40)]
        public uint padding4;
    }

    public class EmitterRenderState
    {
        public bool IsBlendEnable;
        public bool IsDepthTest;
        public byte DepthFunc;
        public bool IsDepthMask;

        public bool IsAlphaTest;
        public byte AlphaFunc;
        public byte BlendType;
        public byte DisplaySide;

        public float AlphaThreshold;
        public uint padding;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class ParticleData
    {
        public bool InfiniteLife; //Always display
        public bool val_0x1;
        public byte BillboardType;
        public byte RotType;
        public byte OffsetType;
        public bool RotRevRandX;
        public bool RotRevRandY;
        public bool RotRevRandZ;
        public bool IsRotateX;
        public bool IsRotateY;
        public byte IsRotateZ;
        public byte PrimitiveScaleType;
        public byte IsTextureCommonRandom;
        public byte ConnectPtclScaleAndZOffset;
        public byte EnableAvoidZFighting;
        public byte val_0xF;
        public int Life;
        public int LifeRandom;
        public float MomentumRandom;
        public uint PrimitiveVertexInfoFlags;
        public ulong PrimitiveID;
        public ulong PrimitiveExID;
        public bool LoopColor0;
        public bool LoopAlpha0;
        public bool LoopColor1;
        public bool LoopAlpha1;
        public bool ScaleLoop;
        public bool LoopRandomColor0;
        public bool LoopRandomAlpha0;
        public bool LoopRandomColor1;
        public bool LoopRandomAlpha1;
        public bool ScaleLoopRandom;
        public byte prim_flag1;
        public byte prim_flag2;

        [VersionCheck(VersionCompare.Less, 50)]
        public int Color0LoopRate;
        [VersionCheck(VersionCompare.Less, 50)]
        public int Alpha0LoopRate;
        [VersionCheck(VersionCompare.Less, 50)]
        public int Color1LoopRate;
        [VersionCheck(VersionCompare.Less, 50)]
        public int Alpha1LoopRate;
        [VersionCheck(VersionCompare.Less, 50)]
        public int ScaleLoopRate;

        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short Color0LoopRate16;
        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short Alpha0LoopRate16;
        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short Color1LoopRate16;
        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short Alpha1LoopRate16;
        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short ScaleLoopRate16;

    }

    public class EmitterCombinerV36
    {
        public byte ColorCombinerProcess;
        public byte AlphaCombinerProcess;
        public byte Texture1ColorBlend;
        public byte Texture2ColorBlend;

        public byte PrimitiveColorBlend;
        public byte Texture1AlphaBlend;
        public byte Texture2AlphaBlend;
        public byte PrimitiveAlphaBlend;
    }

    public class EmitterCombinerV40
    {
        public byte ColorCombinerProcess;
        public byte AlphaCombinerProcess;
        public byte Texture1ColorBlend;
        public byte Texture2ColorBlend;

        public byte PrimitiveColorBlend;
        public byte Texture1AlphaBlend;
        public byte Texture2AlphaBlend;
        public byte PrimitiveAlphaBlend;

        public byte TexColor0InputType;
        public byte TexColor1InputType;
        public byte TexColor2InputType;
        public byte TexAlpha0InputType;

        public byte TexAlpha1InputType;
        public byte TexAlpha2InputType;
        public byte PrimitiveColorInputType;
        public byte PrimitiveAlphaInputType;

        //Likely combiner color/alpha modes for added textures 3, 4, 5
        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public short Padding; 

        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public uint Padding2;

        [VersionCheck(VersionCompare.GreaterOrEqual, 50)]
        public uint Padding3;
    }

    public class EmitterCombiner
    {
        public byte ColorCombinerProcess;
        public byte AlphaCombinerProcess;
        public byte Texture1ColorBlend;
        public byte Texture2ColorBlend;

        public byte PrimitiveColorBlend;
        public byte Texture1AlphaBlend;
        public byte Texture2AlphaBlend;
        public byte PrimitiveAlphaBlend;

        public byte TexColor0InputType;
        public byte TexColor1InputType;
        public byte TexColor2InputType;
        public byte TexAlpha0InputType;

        public byte TexAlpha1InputType;
        public byte TexAlpha2InputType;
        public byte PrimitiveColorInputType;
        public byte PrimitiveAlphaInputType;

        public byte ShaderType;
        public byte ApplyAlpha;
        public byte IsDistortionByCameraDistance;
        public byte padding1;

        public uint padding2;
    }

    public class ShaderRefInfo
    {
        public byte Type;
        public byte val_0x2;
        public byte val_0x3;
        public byte val_0x4;


        public int ShaderIndex;
        public int ComputeShaderIndex; 
        public int UserShaderIndex1;
        public int UserShaderIndex2; 
        public int CustomShaderIndex;

        [VersionCheck(VersionCompare.Less, 50)]
        public ulong CustomShaderFlag;

        [VersionCheck(VersionCompare.Less, 50)]
        public ulong CustomShaderSwitch;

        [VersionCheck(VersionCompare.Less, 22)]
        public ulong Unknown1;

        public int ExtraShaderIndex2;

        public int val_0x34;

        [VersionCheck(VersionCompare.Greater, 50)]
        public ulong Unknown2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UserShaderDefine1 = new byte[16];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UserShaderDefine2 = new byte[16];

    }

    public class ActionInfo
    {
        public uint ActionIndex;

        [VersionCheck(VersionCompare.Greater, 40)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] 
        public uint[] Unknown; //maybe in action section

    }

    public class ParticleVelocityInfo
    {
        public float AllDirection;
        public float DesignatedDirScale;
        public float DesignatedDirX;
        public float DesignatedDirY;
        public float DesignatedDirZ;
        public float DiffusionDirAngle;
        public float XZDiffusion;
        public float DiffusionX;
        public float DiffusionY;
        public float DiffusionZ;

        public float VelRandom;
        public float EmVelInherit;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class ParticleColor
    {
        public byte IsSoftParticle;
        public byte IsFresnelAlpha;
        public byte IsNearDistAlpha;
        public byte IsFarDistAlpha;
        public byte IsDecal;
        public byte val_0x5;
        public byte val_0x6;
        public byte val_0x7;

        public ColorType Color0Type;
        public ColorType Color1Type;
        public ColorType Alpha0Type;
        public ColorType Alpha1Type;

        public float Color0R;
        public float Color0G;
        public float Color0B;
        public float Alpha0;
        public float Color1R;
        public float Color1G;
        public float Color1B;
        public float Alpha1;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class ParticleScale
    {
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float ScaleRandomX;
        public float ScaleRandomY;
        public float ScaleRandomZ;

        public byte EnableScalingByCameraDistNear;
        public byte EnableScalingByCameraDistFar;
        public byte EnableAddScaleY;
        public byte EnableLinkFovyToScaleValue;

        public float ScaleMin;
        public float ScaleMax;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class ParticleFlucInfo
    {
        public byte IsApplyAlpha;
        public byte IsApplayScale;
        public byte IsApplayScaleY;
        public byte IsWaveType;

        public byte IsPhaseRandomX;
        public byte IsPhaseRandomY;
        public byte padding1;
        public byte padding2;

        public uint padding3;
    }


    public class TextureSampler
    {
        public ulong TextureID;

        public WrapMode WrapU = WrapMode.Mirror;
        public WrapMode WrapV = WrapMode.Mirror;
        public byte Filter = 0;
        public byte isSphereMap;

        public float MaxLOD = 15.0f;
        public float LODBias = 0.0f;

        public byte MipLevelLimit;
        public byte IsDensityFixedU;
        public byte IsDensityFixedV;
        public byte IsSquareRgb;

        [VersionCheck(VersionCompare.Less, 50)]
        public byte IsOnAnotherBinary;
        [VersionCheck(VersionCompare.Less, 50)]
        public byte padding1;
        [VersionCheck(VersionCompare.Less, 50)]
        public byte padding2;
        [VersionCheck(VersionCompare.Less, 50)]
        public byte padding3;

        [VersionCheck(VersionCompare.Less, 50)]
        public uint padding4;
    }

    public class TextureAnim
    {
        public byte PatternAnimType;
        public bool IsScroll;
        public bool IsRotate;
        public bool IsScale;

        public byte Repeat;
        public byte InvRandU;
        public byte InvRandV;
        public byte IsPatAnimLoopRandom;

        public byte UvChannel;
        public byte IsCrossfade;
        public byte padding1;
        public byte padding2;

        public uint padding3;
    }
}
