using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib;
public static class BinaryBufferSerializer
{
    /// <summary>
    /// Записывает обьект в <see cref="BinaryBuffer"/>, может игнорировать некоторые типы.
    /// </summary>
    /// <param name="obj">Обьект который будет записан.</param>
    /// <param name="buffer">Буффер в который будет записан обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="String"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns><see cref="true"/> если запись прошла успешна, иначе <see cref="false"/></returns>
    public static bool Serialize<T>(T obj, ref BinaryBuffer buffer, Encoding? encoding = null)
    {
		try
		{
            encoding ??= Encoding.UTF8;

			var data = GetPropertiesAndValues(ref obj, typeof(T));

            data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));

            var result = WritePropertiesValueInBuffer(data, ref buffer, encoding);
            return result;
        }
		catch
		{
			return false;
		}
    }

    public static bool Serialize(Type type, object obj, ref BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var data = GetPropertiesAndValues(ref obj, type);

            data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));

            var result = WritePropertiesValueInBuffer(data, ref buffer, encoding);
            return result;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Читает обьект из <see cref="BinaryBuffer"/>, незаполняет проигнорированные поля, заменяет <see cref="null"/> на значение по умолчанию. 
    /// </summary>
    /// <param name="buffer">Буфер из которого будет читаться обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="String"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns>Обьект если операция прошла успешно, иначе <see cref="null"/></returns>
    public static T? Deserialize<T>(ref BinaryBuffer buffer, Encoding? encoding = null) where T : new()
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var result = (object)new T();

            var data = GetProperties(typeof(T));

            data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));
            var isDone = ReadPropertiesValueFromBuffer(data, ref result, ref buffer, encoding);
            return (T)result;
        }
        catch
        {
            return default;
        }
    }

    public static object? Deserialize(Type type, ref BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var result = Activator.CreateInstance(type);

            if(result is null)
                throw new NullReferenceException();

            var data = GetProperties(type);

            data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));
            var isDone = ReadPropertiesValueFromBuffer(data, ref result, ref buffer, encoding);
            return result;
        }
        catch
        {
            return default;
        }
    }

    private static List<(string name, SerializedType type, object? value)> GetPropertiesAndValues<T>(ref T obj, Type type)
    {
        var props = type.GetProperties();
        var data = new List<(string name, SerializedType type, object? value)>(props.Length);
        foreach (var item in props)
        {
            var propName = item.Name;
            var propVal = item.GetValue(obj);

            var propType = item.PropertyType.IsArray ?
                GetTypeFromTypeName(item.PropertyType.GetElementType()!.Name, true) :
                GetTypeFromTypeName(item.PropertyType.Name, false);

            if (propType == SerializedType.None)
                continue;
            data.Add((propName, propType, propVal));
        }

        return data;
    }

    private static List<(string name, SerializedType type, PropertyInfo pi)> GetProperties(Type type)
    {
        var props = type.GetProperties();
        var data = new List<(string name, SerializedType type, PropertyInfo pi)>(props.Length);
        foreach (var item in props)
        {
            var propName = item.Name;
            var propType = item.PropertyType.IsArray ?
                GetTypeFromTypeName(item.PropertyType.GetElementType()!.Name, true) :
                GetTypeFromTypeName(item.PropertyType.Name, false);
            if (propType == SerializedType.None)
                continue;
            data.Add((propName, propType, item));
        }

        return data;
    }

    private static SerializedType GetTypeFromTypeName(string typeName, bool isArray)
    {
        var propType = SerializedType.None;
        if (isArray)
        {
            propType = typeName switch
            {
                nameof(SByte) => SerializedType.SByteArray,
                nameof(Byte) => SerializedType.ByteArray,
                nameof(Single) => SerializedType.FloatArray,
                nameof(Double) => SerializedType.DoubleArray,
                nameof(Boolean) => SerializedType.BooleanArray,
                nameof(Int16) => SerializedType.Int16Array,
                nameof(UInt16) => SerializedType.UInt16Array,
                nameof(Int32) => SerializedType.Int32Array,
                nameof(UInt32) => SerializedType.UInt32Array,
                nameof(Int64) => SerializedType.Int64Array,
                nameof(UInt64) => SerializedType.UInt64Array,
                nameof(Char) => SerializedType.CharArray,
                nameof(String) => SerializedType.StringArray,
                nameof(DateTime) => SerializedType.DateTimeArray,
                nameof(Guid) => SerializedType.GuidArray,
                nameof(Enum) => SerializedType.EnumArray,
                _ => SerializedType.None,
            };
        }
        else
        {
            propType = typeName switch
            {
                nameof(SByte) => SerializedType.SByte,
                nameof(Byte) => SerializedType.Byte,
                nameof(Single) => SerializedType.Float,
                nameof(Double) => SerializedType.Double,
                nameof(Boolean) => SerializedType.Boolean,
                nameof(Int16) => SerializedType.Int16,
                nameof(UInt16) => SerializedType.UInt16,
                nameof(Int32) => SerializedType.Int32,
                nameof(UInt32) => SerializedType.UInt32,
                nameof(Int64) => SerializedType.Int64,
                nameof(UInt64) => SerializedType.UInt64,
                nameof(Char) => SerializedType.Char,
                nameof(String) => SerializedType.String,
                nameof(DateTime) => SerializedType.DateTime,
                nameof(Guid) => SerializedType.Guid,
                nameof(Enum) => SerializedType.Enum,
                _ => SerializedType.None,
            };
        }
        return propType;
    }

    private static bool WritePropertiesValueInBuffer(List<(string name, SerializedType type, object? value)> props, ref BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, value) in props)
        {
            var result = type switch
            {
                // BASE TYPES
                SerializedType.SByte => buffer.Write((sbyte)(value ?? default(sbyte))),
                SerializedType.Byte => buffer.Write((byte)(value ?? default(byte))),
                SerializedType.Boolean => buffer.Write((bool)(value ?? default(bool))),
                SerializedType.Float => buffer.Write((float)(value ?? default(float))),
                SerializedType.Double => buffer.Write((double)(value ?? default(double))),
                SerializedType.Int16 => buffer.Write((short)(value ?? default(short))),
                SerializedType.Int32 => buffer.Write((int)(value ?? default(int))),
                SerializedType.Int64 => buffer.Write((long)(value ?? default(long))),
                SerializedType.UInt16 => buffer.Write((ushort)(value ?? default(ushort))),
                SerializedType.UInt32 => buffer.Write((uint)(value ?? default(uint))),
                SerializedType.UInt64 => buffer.Write((ulong)(value ?? default(ulong))),
                SerializedType.Char => buffer.Write((char)(value ?? default(char))),
                SerializedType.String => buffer.Write((string)(value ?? string.Empty), encoding),
                SerializedType.DateTime => buffer.Write((DateTime)(value ?? DateTime.MinValue)),
                SerializedType.Enum => buffer.Write((Enum)(value ?? 0)),
                SerializedType.Guid => buffer.Write((Guid)(value ?? Guid.Empty)),
                // ARRAY TYPES
                SerializedType.SByteArray => buffer.WriteArray((sbyte[])(value ?? Array.Empty<sbyte>())),
                SerializedType.ByteArray => buffer.WriteArray((byte[])(value ?? Array.Empty<byte>())),
                SerializedType.BooleanArray => buffer.WriteArray((bool[])(value ?? Array.Empty<bool>())),
                SerializedType.FloatArray => buffer.WriteArray((float[])(value ?? Array.Empty<float>())),
                SerializedType.DoubleArray => buffer.WriteArray((double[])(value ?? Array.Empty<double>())),
                SerializedType.Int16Array => buffer.WriteArray((short[])(value ?? Array.Empty<short>())),
                SerializedType.Int32Array => buffer.WriteArray((int[])(value ?? Array.Empty<int>())),
                SerializedType.Int64Array => buffer.WriteArray((long[])(value ?? Array.Empty<long>())),
                SerializedType.UInt16Array => buffer.WriteArray((ushort[])(value ?? Array.Empty<ushort>())),
                SerializedType.UInt32Array => buffer.WriteArray((uint[])(value ?? Array.Empty<uint>())),
                SerializedType.UInt64Array => buffer.WriteArray((ulong[])(value ?? Array.Empty<ulong>())),
                SerializedType.CharArray => buffer.WriteArray((char[])(value ?? Array.Empty<char>())),
                SerializedType.StringArray => buffer.WriteArray(encoding, (string[])(value ?? Array.Empty<string>())),
                SerializedType.DateTimeArray => buffer.WriteArray((DateTime[])(value ?? Array.Empty<DateTime>())),
                SerializedType.EnumArray => buffer.WriteArray((Enum[])(value ?? Array.Empty<Enum>())),
                SerializedType.GuidArray => buffer.WriteArray((Guid[])(value ?? Array.Empty<Guid>())),
                _ => false,
            };
            if (!result)
                return false;
        }

        return true;
    }

    private static bool ReadPropertiesValueFromBuffer(List<(string name, SerializedType type, PropertyInfo pi)> props, ref object obj, ref BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, pi) in props)
        {

            pi.SetValue(obj,
                // BASE TYPES
                type == SerializedType.Boolean ? buffer.ReadBoolean() :
                type == SerializedType.SByte ? buffer.ReadSByte() :
                type == SerializedType.Byte ? buffer.ReadByte() :
                type == SerializedType.Int16 ? buffer.ReadInt16() :
                type == SerializedType.Int32 ? buffer.ReadInt32() :
                type == SerializedType.Int64 ? buffer.ReadInt64() :
                type == SerializedType.UInt16 ? buffer.ReadUInt16() :
                type == SerializedType.UInt32 ? buffer.ReadUInt32() :
                type == SerializedType.UInt64 ? buffer.ReadUInt64() :
                type == SerializedType.Char ? buffer.ReadChar() :
                type == SerializedType.String ? buffer.ReadString(encoding) :
                type == SerializedType.Float ? buffer.ReadSingle() :
                type == SerializedType.Double ? buffer.ReadDouble() :
                type == SerializedType.DateTime ? buffer.ReadDateTime() :
                type == SerializedType.Enum ? buffer.ReadEnum(pi.PropertyType) :
                type == SerializedType.Guid ? buffer.ReadGuid() :
                // ARRAY TYPES
                type == SerializedType.BooleanArray ? buffer.ReadBooleanArray() :
                type == SerializedType.SByteArray ? buffer.ReadSByteArray() :
                type == SerializedType.ByteArray ? buffer.ReadByteArray() :
                type == SerializedType.Int16Array ? buffer.ReadInt16Array() :
                type == SerializedType.Int32Array ? buffer.ReadInt32Array() :
                type == SerializedType.Int64Array ? buffer.ReadInt64Array() :
                type == SerializedType.UInt16Array ? buffer.ReadUInt16Array() :
                type == SerializedType.UInt32Array ? buffer.ReadUInt32Array() :
                type == SerializedType.UInt64Array ? buffer.ReadUInt64Array() :
                type == SerializedType.CharArray ? buffer.ReadCharArray() :
                type == SerializedType.StringArray ? buffer.ReadStringArray(encoding) :
                type == SerializedType.FloatArray ? buffer.ReadSingleArray() :
                type == SerializedType.DoubleArray ? buffer.ReadDoubleArray() :
                type == SerializedType.DateTimeArray ? buffer.ReadDateTimeArray() :
                type == SerializedType.EnumArray ? buffer.ReadEnumArray(pi.PropertyType) :
                type == SerializedType.GuidArray ? buffer.ReadGuidArray() : 
                null);
        }

        return true;
    }
}
