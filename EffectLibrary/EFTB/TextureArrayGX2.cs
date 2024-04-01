using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
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

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
            //texture header (48 bytes)
            ReadBinary(reader);

            //raw data block (GX2B)
            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
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
        }

        private MemoryStream WriteBinary()
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
            }
            return mem;
        }
    }
}
