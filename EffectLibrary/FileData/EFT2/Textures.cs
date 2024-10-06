using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EffectLibrary.EFT2
{

    public class TextureInfo : SectionBase
    {
        public override string Magic => "GRTF";

        public TextureDescTable TexDescTable = new TextureDescTable();

        public byte[] BinaryData;

        public BntxFile BntxFile;

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.ChildrenCount != 1)
                throw new Exception();

            //Descriptor
            reader.SeekBegin(StartPosition + this.Header.ChildrenOffset);
            TexDescTable = new TextureDescTable();
            TexDescTable.Read(reader, ptclFile);

            //section contains BNTX
            if (this.Header.Size > 0)
            {
                reader.SeekBegin(StartPosition + this.Header.BinaryOffset);
                BinaryData = reader.ReadBytes((int)this.Header.Size);
                BntxFile = new BntxFile(new MemoryStream(BinaryData));
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
          /*  if (BntxFile != null)
            {
                OrderTextures();

                var mem = new MemoryStream();
                BntxFile.Save(mem);
                this.BinaryData = mem.ToArray();

                this.Header.Size = (uint)BinaryData.Length;
            }*/

            //Descriptor
            this.Header.ChildrenOffset = 32; //descriptors as children
            this.Header.ChildrenCount = 1;

            base.Write(writer, ptclFile);

            WriteChildOffset(writer);
            TexDescTable.Write(writer, ptclFile);

            if (BinaryData?.Length > 0)
            {
                //binary data next with alignment
                writer.AlignBytes(4096);
                WriteBinaryOffset(writer);
                writer.Write(BinaryData);
            }

            writer.AlignBytes(16);
            WriteNextOffset(writer, false);
        }

        public Texture TryGetTexture(ulong id)
        {
            if (TexDescTable == null)
                throw new Exception("Failed to find texture descriptor table!");

            var descPrimIndex = TexDescTable.Descriptors.FindIndex(x => x.ID == id);
            if (descPrimIndex != -1)
            {
                var desc = TexDescTable.Descriptors[descPrimIndex];
                var idx = BntxFile.TextureDict.IndexOf(desc.Name);
                if (idx == -1)
                    throw new Exception($"Failed to find texture {desc.Name} in BNTX!");

                return BntxFile.Textures[idx];
            }

            return null;
        }

        public void AddTexture(ulong id, Texture texture)
        {
            var descPrimIndex = TexDescTable.Descriptors.FindIndex(x => x.ID == id && x.Name == texture.Name);
            //ID already added, skip
            if (descPrimIndex != -1)
                return;

            TexDescTable.Descriptors.Add(new TextureDescTable.Descriptor()
            {
                ID = id,
                Name = texture.Name,
            });

            var idx = BntxFile.TextureDict.IndexOf(texture.Name);
            if (idx != -1)
                return;

            BntxFile.Textures.Add(texture);
            BntxFile.TextureDict.Add(texture.Name);
        }

        public void OrderTextures()
        {
            if (BntxFile == null)
                return;
            //order textures by descriptor
            var textures = BntxFile.Textures.ToList();
            string name = BntxFile.Name;

            this.TexDescTable.Descriptors = this.TexDescTable.Descriptors.OrderBy(x => x.Name).ToList();

            BntxFile.Textures.Clear();
            BntxFile.TextureDict.Clear();

            foreach (var desc in this.TexDescTable.Descriptors)
            {
                var tex = textures.FirstOrDefault(x => x.Name == desc.Name);
                if (tex == null)
                    throw new Exception($"Failed to find texture {desc.Name} in BNTX!");

                BntxFile.Textures.Add(tex);
                BntxFile.TextureDict.Add(tex.Name);
            }
        }
    }

    public class TextureDescTable : SectionBase
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

                //name
                int len = reader.ReadInt32();
                string name = reader.ReadFixedString(len);

                Descriptors.Add(new Descriptor()
                {
                    Name = name,
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
                    long pos = writer.BaseStream.Position;

                    writer.Write(Descriptors[i].ID);
                    writer.Write(0); // next offset
                    writer.Write(Descriptors[i].Name.Length + 1);
                    writer.Write(Encoding.UTF8.GetBytes(Descriptors[i].Name));
                    //Add 2 extra bytes to offset the alignment/padding
                    writer.Write((short)0);

                    writer.AlignBytes(8);

                    if (i < Descriptors.Count - 1) //write next offset
                        writer.WriteOffset(pos + 8, pos);
                }
            }
            return mem;
        }

        public class Descriptor
        {
            public ulong ID;
            public string Name;
        }
    }
}
