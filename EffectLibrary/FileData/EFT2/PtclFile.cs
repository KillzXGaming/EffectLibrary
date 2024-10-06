using System.Globalization;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;

namespace EffectLibrary.EFT2
{
    public class PtclFile
    {
        public BinaryHeader Header;

        public string Name;

        public EmitterList EmitterList = new EmitterList();
        public TextureInfo Textures = new TextureInfo();
        public ShaderInfo Shaders = new ShaderInfo();
        public PrimitiveInfo Primitives = new PrimitiveInfo();
        public PrimitiveList PrimitiveList = new PrimitiveList();
        public SectionDefault TRMA = new SectionDefault();

        //EFTB
        public TextureArrayGX2 TexturesGX2 = new TextureArrayGX2();

        public PtclFile()
        {
            Header = new BinaryHeader();
        }

        public PtclFile(string filePath)
        {
            Read(File.OpenRead(filePath));
        }

        public PtclFile(Stream stream)
        {
            Read(stream);
        }

        public void Save(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Write(fs);
            }
        }

        public void Save(Stream stream)
        {
            Write(stream);
        }

        private void Read(Stream stream)
        {
            var reader = stream.AsBinaryReader();
            stream.Read(Utils.AsSpan(ref Header));
            Name = reader.ReadFixedString(32);

            reader.SeekBegin(Header.BlockOffset);

            //Go through all sections
            while (reader.BaseStream.Position < Header.FileSize)
            {
                long pos = reader.BaseStream.Position;

                ReadSection(reader);

                reader.SeekBegin(pos + 12);
                uint nextSectionOffset = reader.ReadUInt32();

                if (nextSectionOffset != uint.MaxValue)
                    reader.SeekBegin(pos + nextSectionOffset);
                else
                    break;
            }
        }

        private void Write(Stream stream)
        {
            var writer = stream.AsBinaryWriter();
            stream.Write(Utils.AsSpan(ref Header));

            //Name (32 bytes total, used in BOTW/TOTK)
            writer.WriteZeroTerminatedString(Name);

            writer.Seek(64, SeekOrigin.Begin);

            //write each section
            EmitterList?.Write(writer, this);
            Textures?.Write(writer, this);
            PrimitiveList?.Write(writer, this);
          //  TRMA?.Write(writer, this);
            Primitives?.Write(writer, this);
            Shaders?.Write(writer, this);

            //set file size
            using (writer.BaseStream.TemporarySeek(28, SeekOrigin.Begin))
            {
                writer.Write((uint)writer.BaseStream.Length);
            }
        }

        private SectionBase ReadSection(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;

            string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            reader.BaseStream.Seek(-4, SeekOrigin.Current);

            if (!SectionTypes.ContainsKey(magic))
                throw new Exception($"Unknown section {magic}");

            var section = (SectionBase)Activator.CreateInstance(SectionTypes[magic]);
            section.Read(reader, this);

            //Apply each section
            switch (magic)
            {
                case "ESTA": EmitterList = (EmitterList)section; break;
                case "GRTF": Textures = (TextureInfo)section; break;
                case "PRMA": PrimitiveList = (PrimitiveList)section; break;
                case "G3PR": Primitives = (PrimitiveInfo)section; break;
                case "GRSN": Shaders = (ShaderInfo)section; break;
                case "TRMA": TRMA = (SectionDefault)section; break;
                case "EFTB": TexturesGX2 = (TextureArrayGX2)section; break;
                default:
                    throw new Exception($"Section {magic} not supported!");
            }

            //go to next section
            if (section.Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(pos + section.Header.NextSectionOffset);

            return section;
        }

        static Dictionary<string, Type> SectionTypes = new Dictionary<string, Type>()
        {
            { "ESTA", typeof(EmitterList) },
            { "ESET", typeof(EmitterSet) },
            { "EMTR", typeof(Emitter) },

            { "GRTF", typeof(TextureInfo) },
            { "GTNT", typeof(TextureDescTable) },

            { "TEXA", typeof(TextureArrayGX2) }, //Used by EFTB

            { "PRMA", typeof(PrimitiveList) },

            { "G3PR", typeof(PrimitiveInfo) },
            { "G3NT", typeof(PrimitiveDescTable) },

            { "GRSN", typeof(ShaderInfo) },
            { "GRSC", typeof(ComputeShader) },

            { "TRMA", typeof(SectionDefault) }
        };
    }

    public class SectionBase
    {
        public virtual string Magic { get; }

        internal long StartPosition;
        internal SectionHeader Header = new SectionHeader();
        internal PtclFile PtclHeader;

        public SectionBase()
        {
            this.Header.NextSectionOffset = uint.MaxValue;
            this.Header.AttrOffset = uint.MaxValue;
            this.Header.BinaryOffset = uint.MaxValue;
            this.Header.ChildrenOffset = uint.MaxValue;
        }

        public virtual void Read(BinaryReader reader, PtclFile ptclFile)
        {
            StartPosition = reader.BaseStream.Position;

            PtclHeader = ptclFile;
            Header = new SectionHeader();
            reader.BaseStream.Read(Utils.AsSpan(ref Header));
        }

        public virtual void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            this.PtclHeader = ptclFile;

            Header.ChildrenOffset = uint.MaxValue;
            Header.NextSectionOffset = uint.MaxValue;

            StartPosition = writer.BaseStream.Position;
            writer.BaseStream.Write(Utils.AsSpan(ref Header));
        }

        public void SeekFromHeader(BinaryReader reader, long pos)
        {
            reader.SeekBegin(StartPosition + pos);
        }

        public void WriteNextOffset(BinaryWriter writer, bool isLastElement)
        {
            writer.AlignBytes(4);

            var offset = writer.BaseStream.Position - StartPosition;
            using (writer.BaseStream.TemporarySeek(StartPosition + 12, SeekOrigin.Begin))
            {
                if (isLastElement)
                    writer.Write(uint.MaxValue);
                else
                    writer.Write((uint)offset);
            }
        }

        public static void WriteList(IEnumerable<SectionBase> sections, BinaryWriter writer, PtclFile header)
        {
            var list = sections.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Write(writer, header);
                list[i].WriteNextOffset(writer, i == list.Count - 1);
            }
        }

        public void WriteChildOffset(BinaryWriter writer) {
            writer.WriteOffset(StartPosition + 8, StartPosition);
        }

        public void WriteBinaryOffset(BinaryWriter writer) {
            writer.WriteOffset(StartPosition + 20, StartPosition);
        }

        public void WriteSubSectionOffset(BinaryWriter writer) {
            writer.WriteOffset(StartPosition + 16, StartPosition);
        }

        public void WriteSectionSize(BinaryWriter writer)
        {
            var size = writer.BaseStream.Position - StartPosition;
            using (writer.BaseStream.TemporarySeek(StartPosition + 4, SeekOrigin.Begin))
            {
                writer.Write((uint)size);
            }
        }

        public void WriteSectionSize(BinaryWriter writer, long size)
        {
            using (writer.BaseStream.TemporarySeek(StartPosition + 4, SeekOrigin.Begin))
            {
                writer.Write((uint)size);
            }
        }
    }

    //Default instance to use when a section is unsupported
    public class SectionDefault : SectionBase
    {
        public byte[] Data;

        public SectionDefault() { }

        public SectionDefault(string magic)
        {
            this.Header.Magic = magic;
        }

        public override void Read(BinaryReader reader, PtclFile ptclFile)
        {
            base.Read(reader, ptclFile);

            if (this.Header.BinaryOffset != uint.MaxValue)
            {
                reader.SeekBegin(this.StartPosition + this.Header.BinaryOffset);
                //read entire section
                Data = reader.ReadBytes((int)(this.Header.Size));
            }

            //goto next section
            if (Header.NextSectionOffset != uint.MaxValue)
                reader.SeekBegin(StartPosition + Header.NextSectionOffset);
        }

        public override void Write(BinaryWriter writer, PtclFile ptclFile)
        {
            base.Write(writer, ptclFile);

            if (this.Data?.Length > 0)
            {
                this.Header.Size = (uint)Data.Length;
                WriteBinaryOffset(writer);
                writer.Write(Data);
            }
        }
    }
}
