using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary
{
    public class PtclSerialize
    {
        public static T Serialize<T>(BinaryReader reader, T obj, int version,  long start_pos)
        {
            foreach (var field in obj.GetType().GetFields())
            {
                var versionCheck = field.GetCustomAttribute<VersionCheck>();
                if (!IsUsed(versionCheck, version) || field.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                    continue;

                if (field.FieldType == typeof(string)) //fixed string
                {
                    var attribute = field.GetCustomAttribute<MarshalAsAttribute>();
                    string value = Encoding.UTF8.GetString(reader.ReadBytes(attribute.SizeConst)).Replace("\0", "");
                    field.SetValue(obj, value);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                    LoadEnumerable(reader, field, obj, version, start_pos);
                else if (IsNonPrimitiveClass(field.FieldType))
                {
                    var instance = Activator.CreateInstance(field.FieldType);
                    PtclSerialize.Serialize(reader, instance, version, start_pos);
                    field.SetValue(obj, instance);
                }
                else
                    field.SetValue(obj, ReadPrimitive(reader, field.FieldType, obj));
            }
            return obj;
        }

        static bool IsUsed(VersionCheck versionCheck, int currentVersion)
        {
            if (versionCheck == null) return true;

            return versionCheck.IsValid(currentVersion);
        }

        public static void Deserialize<T>(BinaryWriter writer, T obj, int version)
        {
            foreach (var field in obj.GetType().GetFields())
            {
                var versionCheck = field.GetCustomAttribute<VersionCheck>();
                if (!IsUsed(versionCheck, version) || field.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                    continue;

                var value = field.GetValue(obj);
                if (field.FieldType == typeof(string)) //fixed string
                {
                    var attribute = field.GetCustomAttribute<MarshalAsAttribute>();
                    writer.Write(Encoding.UTF8.GetBytes((string)value));
                    writer.Write(new byte[attribute.SizeConst - ((string)value).Length]);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                    SaveEnumerable(writer, field, obj, version);
                else if (field.FieldType.IsClass)
                    PtclSerialize.Deserialize(writer, value, version);
                else
                    WritePrimitive(writer, field.FieldType, value);
            }
        }

        static void WritePrimitive(BinaryWriter writer, Type type, object obj)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type== typeof(uint))         writer.Write((uint)obj);
            else if (type== typeof(int))     writer.Write((int)obj);
            else if (type== typeof(float))   writer.Write((float)obj);
            else if (type== typeof(ulong))   writer.Write((ulong)obj);
            else if (type == typeof(sbyte))  writer.Write((sbyte)obj);
            else if (type == typeof(byte))   writer.Write((byte)obj);
            else if (type == typeof(bool))   writer.Write((bool)obj);
            else if (type == typeof(ushort)) writer.Write((ushort)obj);
            else if (type == typeof(short))  writer.Write((short)obj);
            else if (type== typeof(long))    writer.Write((long)obj);
            else if (type== typeof(decimal)) writer.Write((decimal)obj);
            else if (type== typeof(double)) writer.Write((double)obj);
            else if (type== typeof(char[])) writer.Write((char[])obj);
            else if (type== typeof(uint[])) writer.Write((uint[])obj);
            else if (type== typeof(int[]))  writer.Write((int[])obj);
            else
                throw new Exception($"Unsupported type {type}");
        }

        static object ReadPrimitive(BinaryReader reader, Type type, object obj)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if      (type == typeof(uint)) return reader.ReadUInt32();
            else if (type== typeof(int))   return reader.ReadInt32();
            else if (type== typeof(float)) return reader.ReadSingle();
            else if (type== typeof(byte))  return reader.ReadByte();
            else if (type== typeof(sbyte)) return reader.ReadSByte();
            else if (type == typeof(ushort)) return reader.ReadUInt16();
            else if (type == typeof(short)) return reader.ReadInt16();
            else if (type== typeof(bool)) return reader.ReadBoolean();
            else if (type== typeof(ulong)) return reader.ReadUInt64();
            else if (type== typeof(long)) return reader.ReadInt64();
            else if (type== typeof(decimal)) return reader.ReadDecimal();
            else if (type== typeof(double)) return reader.ReadDouble();
            else
                throw new Exception($"Unsupported type {type}");
        }

        static void LoadEnumerable(BinaryReader reader, FieldInfo field, object obj, int version, long start_pos)
        {
            Type elementType = GetEnumerableElementType(field.FieldType);
            if (elementType == null)
                throw new Exception($"Field {field.Name} is not an enumerable type.");

            var value = field.GetValue(obj);
            var attribute = field.GetCustomAttribute<MarshalAsAttribute>();
            if (attribute != null && attribute.Value != UnmanagedType.ByValArray)
                throw new Exception($"Field {field.Name} must have a MarshalAs attribute of type ByValArray.");

            var size = attribute.SizeConst;

            Array array = Array.CreateInstance(elementType, size);
            for (int i = 0; i < size; i++)
            {
                object instance;
                if (IsNonPrimitiveClass(elementType))
                {
                    instance = Activator.CreateInstance(elementType);
                    PtclSerialize.Serialize(reader, instance, version, start_pos);
                }
                else
                {
                    // Read primitive value
                    instance = ReadPrimitive(reader, elementType, obj);
                }
                // Set the value in the array
                array.SetValue(instance, i);
            }
            field.SetValue(obj, array);
        }

        static void SaveEnumerable(BinaryWriter writer, FieldInfo field, object obj, int version)
        {
            Type elementType = GetEnumerableElementType(field.FieldType);
            if (elementType == null)
                throw new Exception($"Field {field.Name} is not an enumerable type.");

            var value = (Array)field.GetValue(obj);
            var attribute = field.GetCustomAttribute<MarshalAsAttribute>();
            if (attribute != null && attribute.Value != UnmanagedType.ByValArray)
                throw new Exception($"Field {field.Name} must have a MarshalAs attribute of type ByValArray.");

            var size = attribute.SizeConst;
            for (int i = 0; i < size; i++)
            {
                if (IsNonPrimitiveClass(elementType))
                    PtclSerialize.Deserialize(writer, value.GetValue(i), version);
                else // Read primitive value
                    WritePrimitive(writer, elementType, value.GetValue(i));
            }
        }

        static Type GetEnumerableElementType(Type enumerableType)
        {
            // Check if the type implements IEnumerable<T>
            if (enumerableType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(enumerableType.GetGenericTypeDefinition()))
            {
                // Return the generic type argument
                return enumerableType.GetGenericArguments()[0];
            }

            // Check if the type implements IEnumerable and try to find the generic IEnumerable interface
            var iEnumerableType = enumerableType.GetInterface(typeof(IEnumerable<>).FullName);
            if (iEnumerableType != null)
            {
                return iEnumerableType.GetGenericArguments()[0];
            }

            // If it is not generic, return null (or handle it accordingly)
            return null;
        }

        static bool IsNonPrimitiveClass(Type type)
        {
            // Check if the type is a class but not a primitive type, value type, or string
            return type.IsClass && !type.IsPrimitive && !type.IsValueType
                && type != typeof(string);
        }
    }
}
