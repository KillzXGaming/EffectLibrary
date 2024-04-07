using ShaderLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public class NamcoEffectFile
    {
        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        public class Header 
        {
            public Magic Magic = "EFFN"; 

            public uint Version = 131072;
            public ushort Num_Effects;
            public ushort Num_External_Models;
            public ushort Multi_Part_Effects;
            public ushort Header_Chunk_Align = 1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        public class EffectHeader 
        {
            public ushort Kind;
            public ushort Unknown;
            public uint EmitterSet_ID;
            public uint External_Model_Idx;
            public ushort Variant_Start_Idx;
            public ushort Variant_Count;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        public class EffectVariant 
        {
            public ushort Unknown;
            public ushort EffectID;
        }

        [JsonIgnore]
        public Header FileHeader = new Header();

        public List<EffectHeader> Entries = new List<EffectHeader>();
        public List<EffectVariant> EffectVariants = new List<EffectVariant>();
        public List<byte> EffectModels = new List<byte>();
        public List<string> EntryNames = new List<string>();
        public List<string> ExternalModelNames = new List<string>();
        public List<string> ExternalBoneNames = new List<string>();

        [JsonIgnore]
        public PtclFile PtclFile;

        public NamcoEffectFile() { }

        public NamcoEffectFile(PtclFile ptcl) { PtclFile = ptcl; }

        public NamcoEffectFile(string filePath)
        {
            Read(File.OpenRead(filePath));
        }

        public void Save(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Write(fs);
            }
        }

        private void Read(Stream stream)
        {
            var reader = new BinaryReader(stream);

            this.FileHeader = reader.ReadStruct<Header>();
            this.Entries = reader.ReadStructs<EffectHeader>(FileHeader.Num_Effects);
            this.EffectVariants = reader.ReadStructs<EffectVariant>(FileHeader.Multi_Part_Effects);
            this.EffectModels = reader.ReadBytes((int)FileHeader.Num_External_Models).ToList();


            for (int i = 0; i < FileHeader.Num_Effects; i++)
                this.EntryNames.Add(reader.ReadUtf8Z());

            for (int i = 0; i < FileHeader.Num_External_Models; i++)
                this.ExternalModelNames.Add(reader.ReadUtf8Z());

            for (int i = 0; i < FileHeader.Multi_Part_Effects; i++)
                this.ExternalBoneNames.Add(reader.ReadUtf8Z());

            var align = GetRequiredChunkAlign();

            reader.AlignBytes(align);

            var subStream = new SubStream(reader.BaseStream, reader.BaseStream.Position);
            PtclFile = new PtclFile(subStream);
        }

        private void Write(Stream stream)
        {
            var writer = new BinaryWriter(stream);

            var align = GetRequiredChunkAlign();

            FileHeader.Header_Chunk_Align = 1;
            if (align == 4096) FileHeader.Header_Chunk_Align = 1;
            if (align == 8192) FileHeader.Header_Chunk_Align = 2;

            FileHeader.Num_Effects = (ushort)this.Entries.Count;
            FileHeader.Multi_Part_Effects = (ushort)this.EffectVariants.Count;
            FileHeader.Num_External_Models = (ushort)this.ExternalModelNames.Count;

            writer.WriteStruct(FileHeader);
            writer.WriteStructs(Entries);
            writer.WriteStructs(EffectVariants);
            writer.Write(EffectModels.ToArray());

            for (int i = 0; i < this.EntryNames.Count; i++)
                writer.WriteZeroTerminatedString(EntryNames[i]);

            for (int i = 0; i < this.ExternalModelNames.Count; i++)
                writer.WriteZeroTerminatedString(ExternalModelNames[i]);

            for (int i = 0; i < this.ExternalBoneNames.Count; i++)
                writer.WriteZeroTerminatedString(ExternalBoneNames[i]);


            writer.AlignBytes(align);

            var mem = new MemoryStream();
            PtclFile.Save(mem);
            writer.Write(mem.ToArray());
        }


        private int GetRequiredChunkAlign()
        {
            int size = 0x10; //header size
            size += this.Entries.Count * 0x10;
            size += this.EffectVariants.Count * 0x4;
            size += this.EffectModels.Count;
            size += this.EntryNames.Sum(x => x.Length + 1);
            size += this.ExternalModelNames.Sum(x => x.Length + 1);
            size += this.ExternalBoneNames.Sum(x => x.Length + 1);
            return (size + 0x1000) & ~0xFFF;
        }

        #region Json Conversion

        public void Export(string filePath) {

            var list = new List<JsonExportEntry>();
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                JsonExportEntry json_entry = new JsonExportEntry()
                {
                    EmitterSet_ID = entry.EmitterSet_ID,
                    Kind = entry.Kind,
                    Name = this.EntryNames[i],
                    Unknown = entry.Unknown,
            };
                list.Add(json_entry);

                int model_idx = (int)entry.External_Model_Idx - 1;

                if (model_idx != -1 && this.EffectModels.Count > 0)
                {
                    var model_flag = this.EffectModels[(int)model_idx];
                    json_entry.ExternalModelFlag = (byte)model_flag;
                    json_entry.ExternalModelString = this.ExternalModelNames[(int)model_idx];
                }

                int start_idx = (int)entry.Variant_Start_Idx - 1;

                for (int j = 0; j < entry.Variant_Count;  j++)
                {
                    var variant = this.EffectVariants[start_idx + j];
                    json_entry.Variants.Add(new JsonExportVariant()
                    {
                        BoneName = this.ExternalBoneNames[start_idx + j],
                        EffectID = variant.EffectID,
                        Unknown = variant.Unknown,
                    });
                }
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public void Import(string filePath)
        {
            var imported = JsonConvert.DeserializeObject<List<JsonExportEntry>>(File.ReadAllText(filePath));

            int variant_Start_Idx = 0;

            EntryNames.Clear();
            EffectModels.Clear();
            ExternalModelNames.Clear();
            ExternalBoneNames.Clear();

            foreach (var entry in imported)
            {
                var effect_entry = new EffectHeader()
                {
                    EmitterSet_ID = entry.EmitterSet_ID,
                    External_Model_Idx = 0,
                    Kind = entry.Kind,
                    Unknown = entry.Unknown,
                    Variant_Count = 0,
                    Variant_Start_Idx = 0,
                };
                this.Entries.Add(effect_entry);
                EntryNames.Add(entry.Name);

                if (!string.IsNullOrEmpty(entry.ExternalModelString))
                {
                    effect_entry.External_Model_Idx = (uint)EffectModels.Count + 1; //0 based index

                    EffectModels.Add(entry.ExternalModelFlag);
                    ExternalModelNames.Add(entry.ExternalModelString);
                }

                if (entry.Variants.Count > 0) //0 based index
                    effect_entry.Variant_Start_Idx = (ushort)(this.EffectVariants.Count + 1);

                foreach (var variant in entry.Variants)
                {
                    this.EffectVariants.Add(new EffectVariant()
                    {
                        EffectID = variant.EffectID,
                        Unknown = variant.Unknown,
                    });
                    ExternalBoneNames.Add(variant.BoneName);
                }

                variant_Start_Idx += entry.Variants.Count;
            }
        }

        class JsonExportEntry //more readable exported option
        {
            public string Name;

            public ushort Kind;
            public ushort Unknown;
            public uint EmitterSet_ID;

            public byte ExternalModelFlag;
            public byte ExternalModelID;
            public string ExternalModelString = "";

            public List<JsonExportVariant> Variants = new List<JsonExportVariant>();
        }

        class JsonExportVariant
        {
            public string BoneName;

            public ushort Unknown;
            public ushort EffectID;
        }

        #endregion
    }
}
