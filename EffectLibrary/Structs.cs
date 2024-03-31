using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct BinaryHeader //A header shared between bntx and other formats
    {
        public ulong Magic; //VFXB + padding

        public ushort GraphicsAPIVersion;
        public ushort VFXVersion;

        public ushort ByteOrder;
        public byte Alignment;
        public byte TargetAddressSize;
        public uint NameOffset; //
        public ushort Flag;
        public ushort BlockOffset;
        public uint RelocationTableOffset; //0
        public uint FileSize;
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct SectionHeader //A header shared between bntx and other formats
    {
        public Magic Magic; //VFXB + padding
        public uint Size;
        public uint ChildrenOffset;
        public uint NextSectionOffset; 
        public uint AttrOffset; //Offsets to another section in emitter data
        public uint BinaryOffset;
        public uint Padding;
        public ushort ChildrenCount;
        public ushort Unknown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic
    {
        int value;
        public static implicit operator string(Magic magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
        public static implicit operator Magic(string s) => new Magic { value = BitConverter.ToInt32(Encoding.ASCII.GetBytes(s), 0) };

        public override string ToString()
        {
            return Encoding.ASCII.GetString(BitConverter.GetBytes(value));
        }
    }
}
