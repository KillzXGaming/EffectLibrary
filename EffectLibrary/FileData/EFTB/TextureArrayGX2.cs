using BfresLibrary.WiiU;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary.EFT2
{
    public class TextureArrayGX2 : SectionBase
    {
        public override string Magic => "TEXA"; //Texture array

        public List<TextureGX2> Textures = new List<TextureGX2>();

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);

            for (int i = 0; i < this.Header.ChildrenCount; i++)
            {
                TextureGX2 tex = new TextureGX2();
                tex.Read(reader, ptclFile);
                Textures.Add(tex);
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {

        }
    }

    public class TextureGX2 : SectionBase
    {
        public override string Magic => "TEXR"; //Texture Resource

        public GX2Binary GX2Bin = new GX2Binary();

        public ushort Width;
        public ushort Height;

        public uint val0x4;
        public uint val0x10;
        public uint val0x18;
        public uint val0x1C;
        public uint val0x20;

        public uint TileMode;
        public uint MipCount;
        public uint CompSel;
        public GX2SurfaceFormat SurfFormat;
        public byte[] data;
        public uint TextureID;
        public byte[] Padding = new byte[7];

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
            //texture header (48 bytes)
            ReadBinary(reader);

            //raw data block (GX2B)
            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
            GX2Bin.Read(reader, ptclFile);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            var binaryData = WriteBinary().ToArray();

            this.Header.Size = (uint)binaryData.Length;

            base.Write(writer, ptclFile);

            WriteBinaryOffset(writer);
            writer.Write(binaryData);

            writer.AlignBytes(16);
            this.WriteNextOffset(writer, false);
        }

        private void ReadBinary(BinaryReader reader)
        {
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            val0x4 = reader.ReadUInt32();
            CompSel = reader.ReadUInt32();
            MipCount = reader.ReadUInt32();
            val0x10 = reader.ReadUInt32();
            TileMode = reader.ReadUInt32();
            val0x18 = reader.ReadUInt32();
            val0x1C = reader.ReadUInt32();
            val0x20 = reader.ReadUInt32();
            TextureID = reader.ReadUInt32();
            SurfFormat = (GX2SurfaceFormat)reader.ReadByte();
            Padding = reader.ReadBytes(7);
        }

        private MemoryStream WriteBinary()
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write((ushort)Width);
                writer.Write((ushort)Height);
                writer.Write(val0x4);
                writer.Write(CompSel);
                writer.Write(MipCount);
                writer.Write(val0x10);
                writer.Write(TileMode);
                writer.Write(val0x18);
                writer.Write(val0x1C);
                writer.Write(val0x20);
                writer.Write(TextureID);
                writer.Write((byte)SurfFormat);
                writer.Write(Padding);
            }
            return mem;
        }

        public class GX2Binary : SectionBase
        {
            public override string Magic => "GX2B"; //GX2 Raw image data

            public byte[] Data;

            public override void Read(BinaryReader reader, PtclFile ptclFile)
            {
                base.Read(reader, ptclFile);

                reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
                Data = reader.ReadBytes((int)this.Header.Size);
            }

            public override void Write(BinaryWriter writer, PtclFile ptclFile)
            {
                this.Header.Size = (uint)Data.Length;

                base.Write(writer, ptclFile);

                WriteBinaryOffset(writer);
                writer.Write(Data);

                writer.AlignBytes(16);
                this.WriteNextOffset(writer, false);
            }
        }
    }
}
