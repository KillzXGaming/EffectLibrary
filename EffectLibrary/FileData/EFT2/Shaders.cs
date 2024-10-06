using ShaderLibrary;
using System.Reflection.PortableExecutable;
using System.Text;

namespace EffectLibrary.EFT2
{
    public class ShaderInfo : SectionBase
    {
        public override string Magic => "GRSN";

        public ComputeShader ComputeShader = new ComputeShader();

        public byte[] BinaryData;

        public BnshFile BnshFile = new BnshFile();
        public BfshaFile BfshaFile = new BfshaFile();

        public BnshFile.ShaderVariation TryGetShader(int index)
        {
            if (BnshFile.Variations.Count > index && index != -1)
                return BnshFile.Variations[index];

            return null;
        }

        public BnshFile.ShaderVariation TryGetComputeShader(int index) {
            return ComputeShader.TryGetShader(index);
        }
       
        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            //section contains BNSH
            if (this.Header.Size > 0)
            {
                //quick peek at header magic
                SeekFromHeader(reader, this.Header.BinaryOffset);
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

                SeekFromHeader(reader, this.Header.BinaryOffset);
                BinaryData = reader.ReadBytes((int)this.Header.Size);

                switch (magic)
                {
                    case "BNSH":
                        BnshFile = new BnshFile(new MemoryStream(BinaryData));
                        break;
                    case "FSHA":
                        BfshaFile = new BfshaFile(new MemoryStream(BinaryData));
                        BnshFile = BfshaFile.ShaderModels[0].BnshFile;
                        break;
                    default:
                        throw new Exception($"Unsupported shader format with magic {magic}!");
                }
            }

            //compute shader 
            if (this.Header.ChildrenOffset != uint.MaxValue)
            {
                SeekFromHeader(reader, this.Header.ChildrenOffset);
                ComputeShader.Read(reader, ptclFile);
            }
        }

        public void SaveBinary()
        {
            if (BfshaFile != null) //uses bfsha (custom shaders)
            {
                var mem = new MemoryStream();
                BnshFile.Save(mem);
                BinaryData = mem.ToArray();

            }
            else if (BnshFile != null) //else uses bnsh
            {
                var mem = new MemoryStream();
                BnshFile.Save(mem);
                BinaryData = mem.ToArray();
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            SaveBinary();
            this.Header.Size = (uint)BinaryData.Length;

            //Compute shader
            this.Header.ChildrenOffset = uint.MaxValue;
            this.Header.ChildrenCount = 0;

            base.Write(writer, ptclFile);

            if (ComputeShader != null)
            {
                WriteChildOffset(writer);
                ComputeShader.Write(writer, ptclFile);
            }

            if (BinaryData?.Length > 0)
            {
                //binary data next with alignment
                writer.AlignBytes(4096);

                WriteBinaryOffset(writer);
                writer.Write(BinaryData);
            }

            if (ComputeShader != null)
                ComputeShader.WriteData(writer);
        }
    }

    public class ComputeShader : SectionBase
    {
        public override string Magic => "GRSC";

        public byte[] BinaryData;

        public BnshFile BnshFile = new BnshFile();
        public BfshaFile BfshaFile;

        public ComputeShader()
        {
            this.Header.Magic = this.Magic;
            this.Header.ChildrenOffset = uint.MaxValue;
            this.Header.NextSectionOffset = uint.MaxValue;
            this.Header.AttrOffset = uint.MaxValue;
            this.Header.ChildrenCount = 0;
        }

        public BnshFile.ShaderVariation TryGetShader(int index)
        {
            if (BnshFile.Variations.Count > index && index != -1)
                return BnshFile.Variations[index];

            return null;
        }

        public void SaveBinary()
        {
            if (BfshaFile != null) //uses bfsha (custom shaders)
            {
                var mem = new MemoryStream();
                BnshFile.Save(mem);
                BinaryData = mem.ToArray();

            }
            else if (BnshFile != null) //else uses bnsh
            {
                var mem = new MemoryStream();
                BnshFile.Save(mem);
                BinaryData = mem.ToArray();
            }
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            //section contains BNSH
            //quick peek at header magic
            SeekFromHeader(reader, this.Header.BinaryOffset);
            string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

            SeekFromHeader(reader, this.Header.BinaryOffset);
            BinaryData = reader.ReadBytes((int)this.Header.Size);

            switch (magic)
            {
                case "BNSH":
                    BnshFile = new BnshFile(new MemoryStream(BinaryData));
                    break;
                case "FSHA":
                   // File.WriteAllBytes("og.bfsha", BinaryData);
                    BfshaFile = new BfshaFile(new MemoryStream(BinaryData));
                    BnshFile = BfshaFile.ShaderModels[0].BnshFile;
                  //  BfshaFile.Save("new.bfsha");
                    break;
                default:
                    throw new Exception($"Unsupported shader format with magic {magic}!");
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            SaveBinary();
            this.Header.Size = (uint)BinaryData.Length;

            base.Write(writer, ptclFile);
        }

        public void WriteData(BinaryWriter writer)
        {
            //binary data next with alignment
            writer.AlignBytes(4096);

            WriteBinaryOffset(writer);
            writer.Write(BinaryData);
        }
    }
}
