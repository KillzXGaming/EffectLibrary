using BfresLibrary;
using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public class PrimitiveList : SectionBase
    {
        public override string Magic => "PRMA";

        public List<Primtitive> Primtitives = new List<Primtitive>();

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.ChildrenCount > 0)
            {
                reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
                for (int i = 0; i < this.Header.ChildrenCount; i++)
                {
                    var prim = new Primtitive();
                    prim.Read(reader, ptclFile);
                    Primtitives.Add(prim);
                }
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            this.Header.NextSectionOffset = 32;
            this.Header.AttrOffset = uint.MaxValue;
            this.Header.BinaryOffset = uint.MaxValue;
            this.Header.ChildrenCount = (ushort)Primtitives.Count;

            base.Write(writer, ptclFile);

            long start_pos = writer.BaseStream.Position;

            if (Primtitives.Count > 0)
            {
                WriteChildOffset(writer);
                WriteBinaryOffset(writer);
                WriteList(Primtitives, writer, ptclFile);
            }

            long end_pos = writer.BaseStream.Position;


            this.WriteSectionSize(writer, end_pos - start_pos);
            this.WriteNextOffset(writer, false);
        }
    }

    public class Primtitive : SectionBase
    {
        public override string Magic => "PRIM";

        public ulong PrimitiveID;

        public Attribute Positions = new Attribute();
        public Attribute Normals = new Attribute();
        public Attribute Tangents = new Attribute();
        public Attribute Colors = new Attribute();
        public Attribute TexCoords0 = new Attribute();
        public Attribute TexCoords1 = new Attribute();

        public List<int> Indices = new List<int>();

        public override void Read(BinaryReader reader, PtclFile header)
        {
            base.Read(reader, header);

            SeekFromHeader(reader, this.Header.BinaryOffset);
            ReadBinary(reader);
        }

        public override void Write(BinaryWriter writer, PtclFile header)
        {
            base.Write(writer, header);

            WriteBinaryOffset(writer);

            long pos = writer.BaseStream.Position;
            WriterBinary(writer);

            var size = writer.BaseStream.Position - pos;

            this.WriteSectionSize(writer, (int)size);
        }

        private void ReadBinary(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;

            PrimitiveID = reader.ReadUInt64();
            int num_positions = reader.ReadInt32();
            int num_positions_elements = reader.ReadInt32();
            int num_normals = reader.ReadInt32();
            int num_normals_elements = reader.ReadInt32();
            int num_tangents = reader.ReadInt32();
            int num_tangents_elements = reader.ReadInt32();
            int num_colors = reader.ReadInt32();
            int num_num_colors_elements = reader.ReadInt32();
            int num_texCoords0 = reader.ReadInt32();
            int num_texCoords0_elements = reader.ReadInt32();
            int num_texCoords1 = reader.ReadInt32();
            int num_texCoords1_elements = reader.ReadInt32();
            int num_indices = reader.ReadInt32();
            uint position_offset = reader.ReadUInt32();
            uint normal_offset = reader.ReadUInt32();
            uint tangent_offset = reader.ReadUInt32();
            uint colors_offset = reader.ReadUInt32();
            uint texCoords_offset = reader.ReadUInt32();
            uint index_buffer_offset = reader.ReadUInt32();

            Attribute ReadAttribute(int count, int num_elements, uint offset)
            {
                Attribute att = new Attribute();
                att.Buffer = new float[count * 4];
                att.Count = count;
                att.ElementCount = num_elements;

                reader.SeekBegin(pos + offset);
                for (int i = 0; i < count * 4; i++)
                    att.Buffer[i] = reader.ReadSingle();

                return att;
            }

            this.Positions = ReadAttribute(num_positions, num_positions_elements, position_offset);
            this.Normals = ReadAttribute(num_normals, num_normals_elements, normal_offset);
            this.Tangents = ReadAttribute(num_tangents, num_tangents_elements, tangent_offset);
            this.Colors = ReadAttribute(num_colors, num_num_colors_elements, colors_offset);

            var texCoord_size = (uint)(num_texCoords0 * 16);

            this.TexCoords0 = ReadAttribute(num_texCoords0, num_texCoords0_elements, texCoords_offset);
            this.TexCoords1 = ReadAttribute(num_texCoords1, num_texCoords1_elements, texCoords_offset + texCoord_size);

            reader.SeekBegin(pos + index_buffer_offset);
            Indices = reader.ReadInt32s((int)num_indices).ToList();
        }

        private void WriterBinary(BinaryWriter writer)
        {
            var pos = writer.BaseStream.Position;

            writer.Write(PrimitiveID);
            writer.Write(this.Positions.Count);
            writer.Write(this.Positions.GetElementCount());
            writer.Write(this.Normals.Count);
            writer.Write(this.Normals.GetElementCount());
            writer.Write(this.Tangents.Count);
            writer.Write(this.Tangents.GetElementCount());
            writer.Write(this.Colors.Count);
            writer.Write(this.Colors.GetElementCount());
            writer.Write(this.TexCoords0.Count);
            writer.Write(this.TexCoords0.GetElementCount());
            writer.Write(this.TexCoords1.Count);
            writer.Write(this.TexCoords1.GetElementCount());
            writer.Write(this.Indices.Count);

            long ofs_pos = writer.BaseStream.Position;

            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            if (this.PtclHeader.Header.VFXVersion >= 21)
                writer.Write(0);

            void WriteAttribute(Attribute attr, int index)
            {
                if (attr.Buffer.Length == 0)
                    return;

                writer.AlignBytes(0x40, 0xCC);
                writer.WriteOffset(ofs_pos + index * sizeof(uint), pos);
                writer.Write(attr.Buffer);
            }

            WriteAttribute(this.Positions, 0);
            WriteAttribute(this.Normals, 1);
            WriteAttribute(this.Tangents, 2);
            WriteAttribute(this.Colors, 3);
            WriteAttribute(this.TexCoords0, 4);
            //data directly after
            writer.Write(this.TexCoords1.Buffer);

            //indices
            writer.AlignBytes(0x40, 0xCC);
            writer.WriteOffset(ofs_pos + 5 * sizeof(uint), pos);
            writer.Write(this.Indices.ToArray());
        }

        public class Attribute
        {
            public int Count;
            public int ElementCount;
            public float[] Buffer = new float[0];

            public int GetElementCount() => ElementCount;
        }
    }

    public class PrimitiveInfo : SectionBase
    {
        public override string Magic => "G3PR";

        public PrimitiveDescTable PrimDescTable = new PrimitiveDescTable();

        public ResFile ResFile;

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.ChildrenCount != 1)
                throw new Exception();

            //Descriptor
            SeekFromHeader(reader, this.Header.ChildrenOffset);
            PrimDescTable = new PrimitiveDescTable();
            PrimDescTable.Read(reader, ptclFile);

            //section contains BFRES
            if (this.Header.BinaryOffset != uint.MaxValue)
            {
                SeekFromHeader(reader, this.Header.BinaryOffset);
                LoadBinary(reader);
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            var bin_data = GetBinaryData();

            this.Header.Size = (uint)bin_data.Length;

            //Descriptor
            this.Header.ChildrenOffset = 32; //descriptors as children
            this.Header.ChildrenCount = 1;

            base.Write(writer, ptclFile);

            WriteChildOffset(writer);
            PrimDescTable.Write(writer, ptclFile);


            if (bin_data.Length > 0)
            {
                //binary data next with alignment
                writer.AlignBytes(4096);
                WriteBinaryOffset(writer);
                writer.Write(bin_data);
            }

            this.WriteNextOffset(writer, false);
        }

        public void LoadBinary(BinaryReader reader)
        {
            var binData = reader.ReadBytes((int)this.Header.Size);
            ResFile = new ResFile(new MemoryStream(binData));
        }

        public byte[] GetBinaryData()
        {
            if (ResFile == null) return new byte[0];

            var mem = new MemoryStream();
            ResFile.Save(mem);
            return mem.ToArray();
        }

        public void AddPrimitive(ulong id, Model model)
        {
            //model is empty, skip
            var descPrimIndex = PrimDescTable.Descriptors.FindIndex(x => x.ID == id);
            //ID already added, skip
            if (descPrimIndex != -1)
                return;

            //compute indices
            sbyte pos_idx = -1;
            sbyte nrm_idx = -1;
            sbyte tan_idx = -1;
            sbyte col_idx = -1;
            sbyte uv0_idx = -1;
            sbyte uv1_idx = -1;

            if (model.Shapes.Count > 0)
            {
                //Note all shapes seem to use the same attributes
                var attr = model.VertexBuffers[0].Attributes;

                if (attr.ContainsKey("_p0")) pos_idx = (sbyte)attr.IndexOf("_p0");
                if (attr.ContainsKey("_n0")) nrm_idx = (sbyte)attr.IndexOf("_n0");
                if (attr.ContainsKey("_t0")) tan_idx = (sbyte)attr.IndexOf("_t0");
                if (attr.ContainsKey("_c0")) col_idx = (sbyte)attr.IndexOf("_c0");
                if (attr.ContainsKey("_u0")) uv0_idx = (sbyte)attr.IndexOf("_u0");
                if (attr.ContainsKey("_u1")) uv1_idx = (sbyte)attr.IndexOf("_u1");
            }

            PrimDescTable.Descriptors.Add(new PrimitiveDescTable.Descriptor()
            {
                ID = id,
                PositionIndex = pos_idx,
                NormalIndex = nrm_idx,
                TangentIndex = tan_idx,
                ColorIndex = col_idx,
                TexCoord0Index = uv0_idx,
                TexCoord1Index = uv1_idx,
            });

            if (!ResFile.Models.ContainsKey(model.Name))
                ResFile.Models.Add(model.Name, model);
        }

        public Model TryGetModel(ulong id)
        {
            if (PrimDescTable == null)
                throw new Exception("Failed to find primitive descriptor table!");

            var descPrimIndex = PrimDescTable.Descriptors.FindIndex(x => x.ID == id);
            if (descPrimIndex != -1)
                return ResFile.Models[descPrimIndex];

            return null;
        }
    }

    public class PrimitiveDescTable : SectionBase
    {
        public List<Descriptor> Descriptors = new List<Descriptor>();

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
            ReadBinary(reader);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            var binaryData = WriteBinary().ToArray();

            this.Header.Size = (uint)binaryData.Length;
            this.Header.NextSectionOffset = (uint)binaryData.Length + 48;

            base.Write(writer, ptclFile);

            WriteBinaryOffset(writer);
            writer.Write(binaryData);

            writer.AlignBytes(16);
            this.WriteNextOffset(writer, false);
        }

        private void ReadBinary(BinaryReader reader)
        {
            var end_pos = StartPosition + this.Header.BinaryOffset + this.Header.Size;
            while (end_pos > reader.BaseStream.Position)
            {
                long pos = reader.BaseStream.Position;

                ulong id = reader.ReadUInt64();
                uint next_offset = reader.ReadUInt32(); //to next descriptor
                reader.ReadUInt32(); //always 8
                var indices = reader.ReadSbytes(6);
                ushort padding = reader.ReadUInt16();

                Descriptors.Add(new Descriptor()
                {
                    PositionIndex = indices[0],
                    NormalIndex = indices[1],
                    TangentIndex = indices[2],
                    ColorIndex = indices[3],
                    TexCoord0Index = indices[4],
                    TexCoord1Index = indices[5],
                    Padding = padding,
                    ID = id,
                });

                if (next_offset == 0)
                    break;

                reader.SeekBegin(pos + next_offset);
            }
        }

        private MemoryStream WriteBinary()
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
                for (int i = 0; i < Descriptors.Count; i++)
                {
                    writer.Write(Descriptors[i].ID);
                    writer.Write(i == Descriptors.Count - 1 ? 0 : 24); // next offset
                    writer.Write(8); // always 8?
                    writer.Write(Descriptors[i].PositionIndex);
                    writer.Write(Descriptors[i].NormalIndex);
                    writer.Write(Descriptors[i].TangentIndex);
                    writer.Write(Descriptors[i].ColorIndex);
                    writer.Write(Descriptors[i].TexCoord0Index);
                    writer.Write(Descriptors[i].TexCoord1Index);
                    writer.Write(Descriptors[i].Padding);
                }
            }
            return mem;
        }

        public class Descriptor
        {
            public ulong ID;
            public sbyte PositionIndex;
            public sbyte NormalIndex;
            public sbyte TangentIndex;
            public sbyte ColorIndex;
            public sbyte TexCoord0Index;
            public sbyte TexCoord1Index;
            public ushort Padding;
        }
    }
}
