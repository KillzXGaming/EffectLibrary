using EffectLibrary.EFT2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary.EFT2
{
    public class EmitterAnimation : EmitterSubSection
    {
        public bool Enable; // Toggles usage
        public bool Loop;
        public bool RandomizeStartFrame; // Randomizes starting frame
        public byte Reserved;

        public uint LoopCount; // Amount of times to keep looping.

        public List<KeyFrame> KeyFrames = new List<KeyFrame>();

        public EmitterAnimation() { }

        public EmitterAnimation(string magic) { this.Header.Magic = magic; }

        public override bool ReadBinary(BinaryReader reader, PtclFile ptclFile)
        {
            Enable = reader.ReadBoolean();
            Loop = reader.ReadBoolean();
            RandomizeStartFrame = reader.ReadBoolean();
            Reserved = reader.ReadByte();
            uint numKeys = reader.ReadUInt32();
            LoopCount = reader.ReadUInt32();
            for (int i = 0; i < numKeys; i++)
                KeyFrames.Add(new KeyFrame()
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle(),
                    Time = reader.ReadSingle(),
                });
            return true;
        }

        public override bool WriteBinary(BinaryWriter writer, PtclFile ptclFile)
        {
            // Check if raw data is loaded 
            // This is so .bin emitters from older builds can write back
            if (this.Data?.Length > 0)
                return false;

            writer.Write(Enable);
            writer.Write(Loop);
            writer.Write(RandomizeStartFrame);
            writer.Write(Reserved);
            writer.Write(KeyFrames.Count);
            writer.Write(LoopCount);
            for (int i = 0; i < KeyFrames.Count; i++)
            {
                writer.Write(KeyFrames[i].X);
                writer.Write(KeyFrames[i].Y);
                writer.Write(KeyFrames[i].Z);
                writer.Write(KeyFrames[i].Time);
            }
            return true;
        }

        public override void Import(string filePath)
        {
            if (filePath.EndsWith(".bin"))
                base.Import(filePath);
            else if ((filePath.EndsWith(".json")))
            {
                var anim = JsonConvert.DeserializeObject<EmitterAnimation>(File.ReadAllText(filePath));
                this.Loop = anim.Loop;
                this.Enable = anim.Enable;
                this.RandomizeStartFrame = anim.RandomizeStartFrame;
                this.Reserved = anim.Reserved;
                this.LoopCount = anim.LoopCount;
                this.KeyFrames = anim.KeyFrames;            }
            else
                throw new Exception($"Unknown file format given! {Path.GetExtension(filePath)}");
        }

        public override void Export(string filePath)
        {
            filePath = filePath.Replace(".bin", ".json");

            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public class KeyFrame
        {
            public float X;
            public float Y;
            public float Z;
            public float Time;
        }
    }
}
