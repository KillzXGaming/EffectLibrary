using BfresLibrary;
using Newtonsoft.Json;
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

namespace EffectLibrary.EFT2
{
    public class EmitterList : SectionBase
    {
        public List<EmitterSet> EmitterSets = new List<EmitterSet>();

        public override string Magic => "ESTA";

        public EmitterList()
        {
            Header.Magic = Magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + Header.ChildrenOffset);
            for (int i = 0; i < Header.ChildrenCount; i++)
            {
                var emitterSet = new EmitterSet();
                emitterSet.Read(reader, ptclFile);
                EmitterSets.Add(emitterSet);
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            Header.ChildrenCount = (ushort)EmitterSets.Count;

            base.Write(writer, ptclFile);

            WriteChildOffset(writer);
            WriteList(EmitterSets, writer, ptclFile);

            WriteSectionSize(writer);

            writer.AlignBytes(16);
            WriteNextOffset(writer, false);
        }

        public EmitterSet GetEmitterSet(string name)
        {
            return EmitterSets.FirstOrDefault(x => x.Name == name);
        }
    }

    public class EmitterSet : SectionBase
    {
        public List<Emitter> Emitters = new List<Emitter>();

        public string Name { get; set; }

        public override string Magic => "ESET";

        public uint Unknown1;
        public uint Unknown2;

        public uint Unknown3;
        public uint Unknown4;
        public uint Unknown5;
        public uint Unknown6;

        public EmitterSet()
        {
            Header.Magic = Magic;

        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (Header.Magic != Magic)
                throw new Exception();

            reader.SeekBegin(StartPosition + Header.BinaryOffset);
            ReadBinary(reader);

            reader.SeekBegin(StartPosition + Header.ChildrenOffset);
            for (int i = 0; i < Header.ChildrenCount; i++)
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
            Header.ChildrenCount = (ushort)Emitters.Count;

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
            reader.ReadUInt32(); //emitter count
            reader.ReadUInt32(); //0
            reader.ReadUInt32(); //0
            reader.ReadUInt32(); //0
            if (PtclHeader.Header.VFXVersion >= 0x16)
            {
                Unknown1 = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt32();
            }
            if (PtclHeader.Header.VFXVersion >= 0x24)
            {
                Unknown3 = reader.ReadUInt32();
                Unknown4 = reader.ReadUInt32();
                Unknown5 = reader.ReadUInt32();
                Unknown6 = reader.ReadUInt32();
            }
        }

        private void WriteBinary(BinaryWriter writer)
        {
            writer.Write(new byte[16]);

            long pos = writer.BaseStream.Position;

            writer.Write(Encoding.UTF8.GetBytes(Name));

            writer.Seek((int)pos + 64, SeekOrigin.Begin);

            writer.Write(Emitters.Count + Emitters.Sum(x => x.Children.Count));
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            if (PtclHeader.Header.VFXVersion >= 0x16)
            {
                writer.Write(Unknown1);
                writer.Write(Unknown2);
            }
            if (PtclHeader.Header.VFXVersion >= 0x24)
            {
                writer.Write(Unknown3);
                writer.Write(Unknown4);
                writer.Write(Unknown5);
                writer.Write(Unknown6);
            }
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
            EmitterSet = emitterSet;
            Header.Magic = Magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (Header.Magic != Magic)
                throw new Exception();

            reader.SeekBegin(StartPosition + Header.BinaryOffset);

            //size. 
            var end = Header.AttrOffset != uint.MaxValue ? Header.AttrOffset : Header.Size;
            var size = end - Header.BinaryOffset;
            BinaryData = reader.ReadBytes((int)size);

            reader.SeekBegin(StartPosition + Header.BinaryOffset);
            ReadBinary(reader);

            reader.SeekBegin(StartPosition + Header.ChildrenOffset);
            for (int i = 0; i < Header.ChildrenCount; i++)
            {
                var sect = new Emitter(EmitterSet);
                sect.Read(reader, ptclFile);
                Children.Add(sect);

                sect.Data.Order = i;
            }


            if (Header.AttrOffset != uint.MaxValue)
            {
                reader.SeekBegin(StartPosition + Header.AttrOffset);
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
            Header.ChildrenCount = (ushort)Children.Count;

            base.Write(writer, ptclFile);

            writer.AlignBytes(256);
            WriteBinaryOffset(writer);

            var pos = writer.BaseStream.Position;
            writer.Write(BinaryData);

            //sub section
            if (SubSections.Count > 0)
            {
                writer.AlignBytes(4);
                WriteSubSectionOffset(writer);
                WriteList(SubSections, writer, ptclFile);
            }

            var end_pos = writer.BaseStream.Position;

            writer.Seek((int)pos, SeekOrigin.Begin);
            WriteBinary(writer);

            writer.Seek((int)end_pos, SeekOrigin.Begin);

            WriteSectionSize(writer);

            if (Children.Count > 0)
            {
                writer.AlignBytes(4);

                WriteChildOffset(writer);
                WriteList(Children, writer, ptclFile);
            }
        }

        private void ReadBinary(BinaryReader reader)
        {
            Data = PtclSerialize.Serialize(reader, new EmitterData(), PtclHeader.Header.VFXVersion, reader.BaseStream.Position);
            Name = Data.Name;
            if (!string.IsNullOrEmpty(Data.Namev40))
                Name = Data.Namev40;
        }

        private void VerifyDump()
        {
            File.WriteAllBytes("og.bin", BinaryData);
            File.WriteAllText("test.json", ToJson());

            var mem = new MemoryStream();
            using (var wr = new BinaryWriter(mem))
            {
                PtclSerialize.Deserialize(wr, Data, PtclHeader.Header.VFXVersion);
            }
            File.WriteAllBytes("saved.bin", mem.ToArray());
            throw new Exception();
        }

        public string ToJson()
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                },
            };
            return JsonConvert.SerializeObject(Data, Formatting.Indented, jsonsettings);
        }

        private void WriteBinary(BinaryWriter writer)
        {
            var pos = writer.BaseStream.Position;
            PtclSerialize.Deserialize(writer, Data, PtclHeader.Header.VFXVersion);
        }

        public Texture GetTextureBinary(TextureSampler sampler)
        {
            return PtclHeader.Textures.TryGetTexture(sampler.TextureID);
        }

        public Model GetVolumeModelBinary()
        {
            return PtclHeader.Primitives.TryGetModel(Data.ShapeInfo.PrimitiveIndex);
        }

        public Model GetModelBinary()
        {
            return PtclHeader.Primitives.TryGetModel(Data.ParticleData.PrimitiveID);
        }

        public Model GetModelExtraBinary()
        {
            return PtclHeader.Primitives.TryGetModel(Data.ParticleData.PrimitiveExID);
        }

        public BnshFile.ShaderVariation GetShaderBinary()
        {
            return PtclHeader.Shaders.TryGetShader(Data.ShaderReferences.ShaderIndex);
        }

        public BnshFile.ShaderVariation GetUser1ShaderBinary()
        {
            if (Data.ShaderReferences.ShaderIndex == Data.ShaderReferences.UserShaderIndex1)
                return null;

            return PtclHeader.Shaders.TryGetShader(Data.ShaderReferences.UserShaderIndex1);
        }

        public BnshFile.ShaderVariation GetUser2ShaderBinary()
        {
            if (Data.ShaderReferences.UserShaderIndex2 == 0)
                return null;

            return PtclHeader.Shaders.TryGetShader(Data.ShaderReferences.UserShaderIndex2);
        }

        public BnshFile.ShaderVariation GetComputeShaderBinary()
        {
            if (Data.ShaderReferences.ComputeShaderIndex == -1) return null;

            return PtclHeader.Shaders.TryGetComputeShader(Data.ShaderReferences.ComputeShaderIndex);
        }
    }

    public class EmitterSubSection : SectionBase
    {
        public byte[] Data;

        public EmitterSubSection() { }

        public EmitterSubSection(string magic)
        {
            Header.Magic = magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + Header.BinaryOffset);

            //read entire section
            Data = reader.ReadBytes((int)(Header.Size - Header.BinaryOffset));

            //goto next section
            if (Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(StartPosition + Header.NextSectionOffset);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            base.Write(writer, ptclFile);

            WriteBinaryOffset(writer);
            writer.Write(Data);

            WriteSectionSize(writer);
        }
    }
}
