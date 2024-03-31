using ShaderLibrary;

namespace EffectLibrary
{
    public class ShaderInfo : SectionBase
    {
        public override string Magic => "GRSN";

        public ComputeShader ComputeShader = new ComputeShader();

        public byte[] BinaryData;

        public BnshFile BnshFile = new BnshFile();

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
                SeekFromHeader(reader, this.Header.BinaryOffset);
                BinaryData = reader.ReadBytes((int)this.Header.Size);

                BnshFile = new BnshFile(new MemoryStream(BinaryData));
                File.WriteAllBytes("og.bnsh", BinaryData);
                BnshFile.Save("new.bnsh");
            }

            //compute shader 
            if (this.Header.ChildrenOffset != uint.MaxValue)
            {
                SeekFromHeader(reader, this.Header.ChildrenOffset);
                ComputeShader.Read(reader, ptclFile);
            }
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            if (BnshFile != null)
            {
                var mem = new MemoryStream();
                BnshFile.Save(mem);
                BinaryData = mem.ToArray();

                this.Header.Size = (uint)BinaryData.Length;
            }

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

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            //section contains BNSH
            SeekFromHeader(reader, this.Header.BinaryOffset);
            BinaryData = reader.ReadBytes((int)this.Header.Size);

            BnshFile = new BnshFile(new MemoryStream(BinaryData));
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            var mem = new MemoryStream();
            BnshFile.Save(mem);
            BinaryData = mem.ToArray();
            
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
