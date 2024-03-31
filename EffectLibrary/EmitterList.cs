using BfresLibrary;
using ShaderLibrary;
using Syroot.BinaryData;
using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public class EmitterList : SectionBase
    {
        public List<EmitterSet> EmitterSets = new List<EmitterSet>();

        public override string Magic => "ESTA";

        public EmitterList()
        {
            this.Header.Magic = this.Magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
            for (int i = 0; i < this.Header.ChildrenCount; i++)
            {
                var emitterSet = new EmitterSet();
                emitterSet.Read(reader, ptclFile);
                EmitterSets.Add(emitterSet);
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            this.Header.ChildrenCount = (ushort)EmitterSets.Count;

            base.Write(writer, ptclFile);

            WriteChildOffset(writer);
            WriteList(EmitterSets, writer, ptclFile);

            WriteSectionSize(writer);

            writer.AlignBytes(16);
            this.WriteNextOffset(writer, false);
        }

        public EmitterSet GetEmitterSet(string name)
        {
            return this.EmitterSets.FirstOrDefault(x => x.Name == name);
        }
    }

    public class EmitterSet : SectionBase
    {
        public List<Emitter> Emitters = new List<Emitter>();

        public string Name { get; set; }

        public override string Magic => "ESET";

        public EmitterSet()
        {
            this.Header.Magic = Magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.Magic != Magic)
                throw new Exception();

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
            ReadBinary(reader);

            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
            for (int i = 0; i < this.Header.ChildrenCount; i++)
            {
                var emitter = new Emitter(this);
                emitter.Read(reader, ptclFile);
                Emitters.Add(emitter);

                emitter.Data.Order = i;
            }

            if (Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(StartPosition + Header.NextSectionOffset);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            this.Header.ChildrenCount = (ushort)Emitters.Count;

            base.Write(writer, ptclFile);

            WriteBinaryOffset(writer);
            WriteBinary(writer);

            WriteChildOffset(writer);
            WriteList(Emitters, writer, ptclFile);

            WriteSectionSize(writer);
        }

        private void ReadBinary(BinaryReader reader)
        {
            reader.ReadBytes(16); //padding
            Name = reader.ReadFixedString(64);
        }

        private void WriteBinary(BinaryWriter writer)
        {
            writer.Write(new byte[16]);

            long pos = writer.BaseStream.Position;

            writer.Write(Encoding.UTF8.GetBytes(Name));

            writer.Seek((int)pos + 64, SeekOrigin.Begin);

            writer.Write(this.Emitters.Count + this.Emitters.Sum(x => x.Children.Count));
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
        }
    }


    public class Emitter : SectionBase
    {
        public override string Magic => "EMTR";

        public byte[] BinaryData;

        public string Name { get; set; }

        public EmitterData Data = new EmitterData();

        public List<Emitter> Children = new List<Emitter>();

        public List<EmitterSubSection> SubSections = new List<EmitterSubSection>();

        public EmitterSet EmitterSet;

        public Emitter(EmitterSet emitterSet)
        {
            this.EmitterSet = emitterSet;
            this.Header.Magic = Magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.Magic != Magic)
                throw new Exception();

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);

            //size. 
            var end = this.Header.AttrOffset != uint.MaxValue ? this.Header.AttrOffset : this.Header.Size;
            var size = end - this.Header.BinaryOffset;
            BinaryData = reader.ReadBytes((int)size);

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
            ReadBinary(reader);

            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
            for (int i = 0; i < this.Header.ChildrenCount; i++)
            {
                var sect = new Emitter(EmitterSet);
                sect.Read(reader, ptclFile);
                Children.Add(sect);

                sect.Data.Order = i;
            }


            if (this.Header.AttrOffset != uint.MaxValue)
            {
                reader.SeekBegin(StartPosition + this.Header.AttrOffset);
                //Sub sections
                while (true)
                {
                    EmitterSubSection sect = new();
                    sect.Read(reader, ptclFile);
                    SubSections.Add(sect);

                    //end
                    if (sect.Header.NextSectionOffset == uint.MaxValue)
                        break;
                }
            }

            if (Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(StartPosition + Header.NextSectionOffset);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            base.Write(writer, ptclFile);

            writer.AlignBytes(256);
            WriteBinaryOffset(writer);

            var pos = writer.BaseStream.Position;
            writer.Write(BinaryData);

            //sub section
            if (SubSections.Count > 0)
            {
                writer.AlignBytes(8);
                WriteSubSectionOffset(writer);
                WriteList(SubSections, writer, ptclFile);
            }

            var end_pos = writer.BaseStream.Position;

            writer.Seek((int)pos, SeekOrigin.Begin);
            WriteBinary(writer);

            writer.Seek((int)end_pos, SeekOrigin.Begin);

            WriteSectionSize(writer);

            if (this.Children.Count > 0)
            {
                WriteChildOffset(writer);
                WriteList(Children, writer, ptclFile);
            }
        }

        private void ReadBinary(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;

            Data.Flag = reader.ReadUInt32();
            Data.RandomSeed = reader.ReadUInt32();

            reader.ReadBytes(8);
            Name = reader.ReadFixedString(64);

            long spos = reader.BaseStream.Position;

            reader.SeekBegin(pos + 0x868);
            Data.VolumePrimitveID = reader.ReadUInt64();

            int offset = 0;

            reader.SeekBegin(pos + 0x50 + offset);
            Data.EmitterStatic = reader.ReadStruct<EmitterStatic>();

            if (this.PtclHeader.Header.VFXVersion >= 36)
                offset += 64;

            reader.SeekBegin(pos + 0x7F0 + offset);
            reader.BaseStream.Read(Utils.AsSpan(ref Data.Emission));

            reader.SeekBegin(pos + 0x8A8 + offset);
            reader.BaseStream.Read(Utils.AsSpan(ref Data.ParticleData));


            reader.SeekBegin(pos + 0x910 + offset);
            Data.ShaderReferences = reader.ReadStruct<ShaderReferences>();

            reader.SeekBegin(pos + 0x99C + offset);
            reader.BaseStream.Read(Utils.AsSpan(ref Data.ParticleColor));
            reader.BaseStream.Read(Utils.AsSpan(ref Data.ParticleScale));
            
            reader.SeekBegin(pos + 0x9F8 + offset);

            if (PtclHeader.Header.VFXVersion >= 37)
                reader.SeekBegin(spos + 2472);
            else if (PtclHeader.Header.VFXVersion > 21)
                reader.SeekBegin(spos + 2464);
            else
                reader.SeekBegin(spos + 2472);

            for (int i = 0; i < 3; i++)
            {
                EmitterSampler samplerInfo = new EmitterSampler();
                samplerInfo.Read(reader, this.PtclHeader);
                Data.Samplers[i] = samplerInfo;
            }
        }

        private void WriteBinary(BinaryWriter writer)
        {
            var pos = writer.BaseStream.Position;
            writer.Write(this.Data.Flag);
            writer.Write(this.Data.RandomSeed);
            writer.Write(new byte[8]);
            writer.Write(Encoding.UTF8.GetBytes(Name));
            writer.AlignBytes(64);

            writer.Seek((int)pos + 0x50, SeekOrigin.Begin);
            writer.WriteStruct(Data.EmitterStatic);

            writer.Seek((int)pos + 2152, SeekOrigin.Begin);
            writer.Write(Data.VolumePrimitveID);

            writer.Seek((int)pos + 0x7F0, SeekOrigin.Begin);
            writer.BaseStream.Write(Utils.AsSpan(ref Data.Emission));


            writer.Seek((int)pos + 0x8A8, SeekOrigin.Begin);
            writer.BaseStream.Write(Utils.AsSpan(ref Data.ParticleData));

            writer.Seek((int)pos + 0x910, SeekOrigin.Begin);
            writer.WriteStruct(Data.ShaderReferences);

            writer.Seek((int)pos + 0x99C, SeekOrigin.Begin);
            writer.BaseStream.Write(Utils.AsSpan(ref Data.ParticleColor));
            writer.BaseStream.Write(Utils.AsSpan(ref Data.ParticleScale));


            int offset = 0;
            if (this.PtclHeader.Header.VFXVersion >= 37)
                offset = -8;


            writer.Seek((int)pos + 2552, SeekOrigin.Begin);
            for (int i = 0; i < 3; i++)
            {
                if (Data.Samplers[i] != null)
                    Data.Samplers[i].Write(writer, PtclHeader);
            }
        }

        public Texture GetTextureBinary(EmitterSampler sampler)
        {
            return this.PtclHeader.Textures.TryGetTexture(sampler.TextureID);
        }

        public Model GetVolumeModelBinary()
        {
            return this.PtclHeader.Primitives.TryGetModel(this.Data.VolumePrimitveID);
        }

        public Model GetModelBinary()
        {
            return this.PtclHeader.Primitives.TryGetModel(this.Data.ParticleData.PrimitiveID);
        }

        public Model GetModelExtraBinary()
        {
            return this.PtclHeader.Primitives.TryGetModel(this.Data.ParticleData.PrimitiveExID);
        }

        public BnshFile.ShaderVariation GetShaderBinary()
        {
            return this.PtclHeader.Shaders.TryGetShader(this.Data.ShaderReferences.ShaderIndex);
        }

        public BnshFile.ShaderVariation GetUserShaderBinary()
        {
            if (this.Data.ShaderReferences.ShaderIndex == this.Data.ShaderReferences.UserShaderIndex1)
                return null;

            return this.PtclHeader.Shaders.TryGetShader(this.Data.ShaderReferences.UserShaderIndex1);
        }

        public BnshFile.ShaderVariation GetComputeShaderBinary()
        {
            if (Data.ShaderReferences.ComputeShaderIndex == -1) return null;

            return this.PtclHeader.Shaders.TryGetComputeShader(this.Data.ShaderReferences.ComputeShaderIndex);
        }
    }

    public class EmitterData
    {
        public uint Flag;
        public uint RandomSeed;

        public EmitterStatic EmitterStatic = new EmitterStatic();
        public Emission Emission = new Emission();
        public ParticleData ParticleData = new ParticleData();
        public ParticleColor ParticleColor = new ParticleColor();
        public ParticleScale ParticleScale = new ParticleScale();
        public ShaderReferences ShaderReferences = new ShaderReferences();

        public EmitterSampler[] Samplers = new EmitterSampler[3];

        public ulong VolumePrimitveID = ulong.MaxValue;

        public int Order = 0;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
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
        public float Color0LoopRate;
        public float Alpha0LoopRate;
        public float Color1LoopRate;
        public float Alpha1LoopRate;
        public float ScaleLoopRate;
        public bool Color0LoopRandom;
        public bool Alpha0LoopRandom;
        public bool Color1LoopRandom;
        public bool Alpha1LoopRandom;
        public bool ScaleLoopRandom;
        public uint Unknown3;
        public uint Unknown4;

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

        public float val_0x90;
        public float val_0x94;

        public float val_0x98;
        public float val_0x112;

        public float val_0xA0;
        public float val_0xA4;

        public float val_0xA8;
        public float val_0xA12;

        public float val_0xB0;
        public float val_0xB4;

        public float val_0xB8;
        public float val_0xBC;

        public TexPatAnim TexPatternAnim0;
        public TexPatAnim TexPatternAnim1;
        public TexPatAnim TexPatternAnim2;

        public TexScrollAnim TexScrollAnim0;
        public TexScrollAnim TexScrollAnim1;
        public TexScrollAnim TexScrollAnim2;

        public float ColorScale;
        public float val_0x364;
        public float val_0x368;
        public float val_0x36A;

        public AnimationKeyTable Color0;
        public AnimationKeyTable Alpha0;
        public AnimationKeyTable Color1;
        public AnimationKeyTable Alpha1;
    }


    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct TexPatAnim
    {
        public float Num;
        public float Frequency;
        public float NumRandom;
        public float Pad;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] Table;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct TexScrollAnim
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
    public struct AnimationKeyTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public AnimationKey[] Keys;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct AnimationKey
    {
        public float X;
        public float Y;
        public float Z;
        public float Time; //ratio 0.0 -> 1.0
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ParticleData
    {
        public bool InfiniteLife; //Always display
        public bool val_0x1;
        public byte BillboardType;
        public byte val_0x3;
        public bool val_0x4;
        public bool val_0x5;
        public byte val_0x6;
        public byte val_0x7;
        public bool val_0x8;
        public bool val_0x9;
        public byte val_0xA;
        public byte val_0xB;
        public byte val_0xC;
        public byte val_0xD;
        public byte val_0xE;
        public byte val_0xF;
        public int Life;
        public int LifeRandom;
        public float MomentumRandom;
        public uint val_0x1C;
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
        public bool prim_flag1;
        public bool prim_flag2;
        public int Color0LoopRate;
        public int Alpha0LoopRate;
        public int Color1LoopRate;
        public int Alpha1LoopRate;
        public int ScaleLoopRate;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct ParticleColor
    {
        public byte val_0x0;
        public byte val_0x1;
        public byte val_0x2;
        public byte val_0x3;
        public byte val_0x4;
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
    public struct ParticleScale
    {
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float ScaleRandomX;
        public float ScaleRandomY;
        public float ScaleRandomZ;

        public byte val_0x18;
        public byte val_0x19;
        public byte val_0x1A;
        public byte val_0x1B;

        public float ScaleMin;
        public float ScaleMax;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct Emission
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

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public class ShaderReferences
    {
        public byte Type;
        public byte val_0x2;
        public byte val_0x3;
        public byte val_0x4;

        public int ShaderIndex; //primary shader index
        public int ComputeShaderIndex; //seems to index the compute shader list

        public int UserShaderIndex1; //User shader
        public int val_0x10; //0

        public int UserShaderIndex2; //A second user shader (used in SMO)
        public int val_0x18; //0

        public int val_0x14; //0
        public int val_0x1C; //0
        public int val_0x20; //0
        public int val_0x24; //0
        public int val_0x28; //0

        public int ExtraShaderIndex2; //another shader?

        public int val_0x34;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public ushort[] Params;

        public override string ToString()
        {
            return $"{ShaderIndex} {ComputeShaderIndex} {UserShaderIndex1} ";
        }
    }

    public class EmitterSampler
    {
        public ulong TextureID;

        public WrapMode WrapU = WrapMode.Mirror;
        public WrapMode WrapV = WrapMode.Mirror;

        public short Filter = 0;

        public float MaxLOD = 15.0f;
        public float LODBias = 0.0f;

        public uint Flags = 0;
        public uint Unknown0 = 0;
        public uint Unknown1 = 0;

        public void Read(BinaryReader reader, PtclFile file)
        {
            TextureID = reader.ReadUInt64();
            WrapU = (WrapMode)reader.ReadByte();
            WrapV = (WrapMode)reader.ReadByte();
            Filter = reader.ReadInt16();
            MaxLOD = reader.ReadSingle();
            LODBias = reader.ReadSingle();
            Flags = reader.ReadUInt32();

            if (file.Header.VFXVersion >= 0x15)
            {
                Unknown0 = reader.ReadUInt32();
                Unknown1 = reader.ReadUInt32();
            }
        }

        public void Write(BinaryWriter writer, PtclFile file)
        {
            writer.Write(TextureID);
            writer.Write((byte)WrapU);
            writer.Write((byte)WrapV);
            writer.Write(Filter);
            writer.Write(MaxLOD);
            writer.Write(LODBias);
            writer.Write(Flags);

            if (file.Header.VFXVersion >= 0x15)
            {
                writer.Write(Unknown0);
                writer.Write(Unknown1);
            }
        }
    }

    public class EmitterSubSection : SectionBase
    {
        public byte[] Data;

        public EmitterSubSection() { }

        public EmitterSubSection(string magic)
        {
            this.Header.Magic = magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(this.StartPosition + this.Header.BinaryOffset);

            //read entire section
            Data = reader.ReadBytes((int)(this.Header.Size - this.Header.BinaryOffset));

            //goto next section
            if (Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(StartPosition + Header.NextSectionOffset);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            this.Header.Size = (uint)Data.Length + 32;

            base.Write(writer, ptclFile);

            WriteBinaryOffset(writer);
            writer.Write(Data);
        }
    }
}
