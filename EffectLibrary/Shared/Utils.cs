using System;
using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EffectLibrary
{
    internal readonly ref struct TemporarySeekHandle
    {
        private readonly Stream Stream;
        private readonly long RetPos;

        public TemporarySeekHandle(Stream stream, long retpos)
        {
            this.Stream = stream;
            this.RetPos = retpos;
        }

        public readonly void Dispose()
        {
            Stream.Seek(RetPos, SeekOrigin.Begin);
        }
    }

    internal static class Utils
    {
        public static void WriteOffset(this BinaryWriter writer, long offset, long start)
        {
            long pos = writer.BaseStream.Position - start;
            using (writer.BaseStream.TemporarySeek(offset, SeekOrigin.Begin))
            {
                writer.Write((uint)pos);
            }
        }

        public static void WriteZeroTerminatedString(this BinaryWriter writer, string text)
        {
            writer.Write(Encoding.UTF8.GetBytes(text));
            writer.Write((byte)0);
        }

        public static string ReadFixedString(this BinaryReader reader, int size)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(size)).Replace("\0", "");
        }

        public static Span<byte> AsSpan<T>(ref T val) where T : unmanaged
        {
            Span<T> valSpan = MemoryMarshal.CreateSpan(ref val, 1);
            return MemoryMarshal.Cast<T, byte>(valSpan);
        }

        public static void WriteStructs<T>(this BinaryWriter writer, IEnumerable<T> val)
        {
            var list = val.ToList();
            for (int i = 0; i < list.Count; i++)
                writer.WriteStruct(list[i]);
        }

        public static void WriteStruct<T>(this BinaryWriter writer, T val)
        {
            writer.Write(StructToBytes(val));
        }

        static byte[] StructToBytes<T>(T val)
        {
            IntPtr ptr = IntPtr.Zero;
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];

            try
            {

                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(val, ptr, true);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return buffer;
        }

        public static List<T> ReadStructs<T>(this BinaryReader reader, int num)
        {
            T[] list = new T[num];
            for (int i = 0; i < num; i++)
                list[i] = reader.ReadStruct<T>();

            return list.ToList();
        }

        public static T ReadStruct<T>(this BinaryReader reader)
        {
            int size = Marshal.SizeOf<T>();
            var byteArray = reader.ReadBytes(size);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(byteArray, 0, ptr, size);

            T result = Marshal.PtrToStructure<T>(ptr);

            Marshal.FreeHGlobal(ptr);

            return result;
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, float[] values)
        {
            for (int i = 0; i < values.Length; i++)
                writer.Write(values[i]);
        }

        public static void Write(this BinaryWriter writer, uint[] values)
        {
            for (int i = 0; i < values.Length; i++)
                writer.Write(values[i]);
        }

        public static void Write(this BinaryWriter writer, int[] values)
        {
            for (int i = 0; i < values.Length; i++)
                writer.Write(values[i]);
        }

        public static void Write(this BinaryWriter writer, bool[] values)
        {
            for (int i = 0; i < values.Length; i++)
                writer.Write(values[i]);
        }

        public static void AlignBytes(this BinaryReader reader, int align)
        {
            var startPos = reader.BaseStream.Position;
            long position = reader.BaseStream.Seek((int)(-reader.BaseStream.Position % align + align) % align, SeekOrigin.Current);

            reader.SeekBegin((int)startPos);
            while (reader.BaseStream.Position != position)
            {
                reader.ReadByte();
            }
        }

        public static void AlignBytes(this BinaryWriter writer, int align, byte pad_val = 0)
        {
            var startPos = writer.BaseStream.Position;
            long position = writer.Seek((int)(-writer.BaseStream.Position % align + align) % align, SeekOrigin.Current);

            writer.Seek((int)startPos, System.IO.SeekOrigin.Begin);
            while (writer.BaseStream.Position != position)
            {
                writer.Write((byte)pad_val);
            }
        }

        public static sbyte[] ReadSbytes(this BinaryReader reader, int count)
        {
            sbyte[] values = new sbyte[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadSByte();
            return values;
        }

        public static bool[] ReadBooleans(this BinaryReader reader, int count)
        {
            bool[] values = new bool[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadBoolean();
            return values;
        }

        public static float[] ReadSingles(this BinaryReader reader, int count)
        {
            float[] values = new float[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadSingle();
            return values;
        }

        public static ushort[] ReadUInt16s(this BinaryReader reader, int count)
        {
            ushort[] values = new ushort[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadUInt16();
            return values;
        }

        public static int[] ReadInt32s(this BinaryReader reader, int count)
        {
            int[] values = new int[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadInt32();
            return values;
        }

        public static uint[] ReadUInt32s(this BinaryReader reader, int count)
        {
            uint[] values = new uint[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadUInt32();
            return values;
        }

        public static long[] ReadInt64s(this BinaryReader reader, int count)
        {
            long[] values = new long[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadInt64();
            return values;
        }

        public static ulong[] ReadUInt64s(this BinaryReader reader, int count)
        {
            ulong[] values = new ulong[count];
            for (int i = 0; i < count; i++)
                values[i] = reader.ReadUInt64();
            return values;
        }

        public static T ReadCustom<T>(this BinaryReader reader, Func<T> value, ulong offset)
        {
            if (offset == 0) return default(T);

            long pos = reader.BaseStream.Position;

            reader.SeekBegin((long)offset);

            var result = value.Invoke();

            reader.SeekBegin((long)pos);

            return result;
        }

        public static void SeekBegin(this BinaryReader reader, long offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public static void SeekBegin(this BinaryReader reader, ulong offset)
        {
            reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);
        }

        public static ushort ReadUInt16BigEndian(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(2);
            Array.Reverse(bytes); //Reverse bytes
            return BitConverter.ToUInt16(bytes, 0);
        }

        public static bool[] ReadBooleanBits(this BinaryReader reader, int count)
        {
            bool[] booleans = new bool[count];

            int idx = 0;
            var bitFlags = reader.ReadInt64s(1 + count / 64);
            for (int i = 0; i < count; i++)
            {
                if (i != 0 && i % 64 == 0)
                    idx++;

                booleans[i] = (bitFlags[idx] & ((long)1 << i)) != 0;
            }
            return booleans;
        }

        public static List<string> ReadStringOffsets(this BinaryReader reader, int count)
        {
            string[] strings = new string[count];
            for (int i = 0; i < count; i++)
            {
                var offset = reader.ReadUInt64();
                strings[i] = reader.ReadStringOffset(offset);
            }
            return strings.ToList();
        }

        public static string ReadStringOffset(this BinaryReader reader, ulong offset)
        {
            long pos = reader.BaseStream.Position;

            reader.SeekBegin(offset);

            ushort size = reader.ReadUInt16();
            string value = reader.ReadUtf8Z();
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        public static uint ReadUInt24(this BinaryReader reader)
        {
            /* Read out 3 bytes into a sizeof(uint) buffer. */
            Span<byte> bytes = stackalloc byte[4];
            reader.BaseStream.Read(bytes[..^1]);

            bytes[3] = 0;
            /* Convert buffer into uint. */
            uint v = BitConverter.ToUInt32(bytes);

            return v;
        }
        public static void WriteUInt24(this BinaryWriter writer, uint value)
        {
            /* Build a byte array from the value. */
            Span<byte> bytes = new byte[3]
            {
                (byte)(value & 0xFF),
                (byte)(value >> 8 & 0xFF),
                (byte)(value >> 16 & 0xFF),
            };

            /* Write array. */
            writer.BaseStream.Write(bytes);
        }
        public static T[] ReadArray<T>(this Stream stream, uint count) where T : struct
        {
            /* Read data. */
            T[] data = new T[count];

            /* Read into casted span. */
            stream.Read(MemoryMarshal.Cast<T, byte>(data));

            return data;
        }

        public static void WriteArray<T>(this Stream stream, ReadOnlySpan<T> array) where T : struct
        {
            stream.Write(MemoryMarshal.Cast<T, byte>(array));
        }

        public static TemporarySeekHandle TemporarySeek(this Stream stream)
        {
            return stream.TemporarySeek(0, SeekOrigin.Begin);
        }

        public static TemporarySeekHandle TemporarySeek(this Stream stream, long offset, SeekOrigin origin)
        {
            long ret = stream.Position;
            stream.Seek(offset, origin);
            return new TemporarySeekHandle(stream, ret);
        }

        public static int BinarySearch<T, K>(IList<T> arr, K v) where T : IComparable<K>
        {
            var start = 0;
            var end = arr.Count - 1;

            while (start <= end)
            {
                var mid = (start + end) / 2;
                var entry = arr[mid];
                var cmp = entry.CompareTo(v);

                if (cmp == 0)
                    return mid;
                if (cmp > 0)
                    end = mid - 1;
                else /* if (cmp < 0) */
                    start = mid + 1;
            }

            return ~start;
        }
        public static BinaryReader AsBinaryReader(this Stream stream)
        {
            return new BinaryReader(stream);
        }
        public static BinaryWriter AsBinaryWriter(this Stream stream)
        {
            return new BinaryWriter(stream);
        }

        public static string ReadUtf8(this BinaryReader reader, int size)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(size), 0, size);
        }

        public static string ReadUtf8Z(this BinaryReader reader, int maxLength = int.MaxValue)
        {
            long start = reader.BaseStream.Position;
            int size = 0;

            // Read until we hit the end of the stream (-1) or a zero
            while (reader.BaseStream.ReadByte() - 1 > 0 && size < maxLength)
            {
                size++;
            }

            reader.BaseStream.Position = start;
            string text = reader.ReadUtf8(size);
            reader.BaseStream.Position++; // Skip the null byte
            return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignUp(uint num, uint align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignUp(int num, int align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AlignUp(ulong num, ulong align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AlignUp(long num, long align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        public static float ReadHalfFloat(this BinaryReader binaryReader)
        {
            return (float)binaryReader.ReadHalf();
        }
    }
}