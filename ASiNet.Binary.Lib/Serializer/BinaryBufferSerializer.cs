using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using ASiNet.Binary.Lib.Serializer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
public static class BinaryBufferSerializer
{
    /// <summary>
    /// Записывает обьект в <see cref="BinaryBuffer"/>, может игнорировать некоторые типы.
    /// </summary>
    /// <param name="obj">Обьект который будет записан.</param>
    /// <param name="buffer">Буффер в который будет записан обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="string"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns><see cref="true"/> если запись прошла успешна, иначе <see cref="false"/></returns>
    public static bool Serialize<T>(in T obj, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseSerialize(typeof(T), obj!, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    public static bool Serialize(Type type, in object obj, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseSerialize(type, obj, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }
    /// <summary>
    /// Читает обьект из <see cref="BinaryBuffer"/>, незаполняет проигнорированные поля, заменяет <see cref="null"/> на значение по умолчанию. 
    /// </summary>
    /// <param name="buffer">Буфер из которого будет читаться обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="string"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns>Обьект если операция прошла успешно, иначе <see cref="null"/></returns>
    public static T? Deserialize<T>(BinaryBuffer buffer, Encoding? encoding = null) where T : new()
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return (T?)BaseDeserialize(typeof(T), buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    public static object? Deserialize(Type type, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseDeserialize(type, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    private static bool BaseSerialize(Type type, in object obj, BinaryBuffer buffer, Encoding encoding)
    {
        var data = GetPropertiesAndValues(obj, type);

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));

        var result = WritePropertiesValueInBuffer(data, buffer, encoding);
        return result;
    }

    private static object? BaseDeserialize(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        var result = Activator.CreateInstance(type);

        if (result is null)
            throw new NullReferenceException();

        var data = GetProperties(type);

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));
        var isDone = ReadPropertiesValueFromBuffer(data, ref result, buffer, encoding);
        return result;
    }

    private static List<(string name, SerializedType type, object? value)> GetPropertiesAndValues<T>(in T obj, Type type)
    {
        var props = type.GetProperties();
        var data = new List<(string name, SerializedType type, object? value)>(props.Length);
        foreach (var item in props)
        {
            if(item.GetCustomAttribute<IgnorePropertyAttribute>() is not null)
                continue;
            var propName = item.Name;
            var propVal = item.GetValue(obj);

            var propType = item.PropertyType.IsArray ?
                GetTypeFromTypeName(item.PropertyType.GetElementType()!.Name, true, item.PropertyType.GetElementType()!.IsEnum) :
                GetTypeFromTypeName(item.PropertyType.Name, false, item.PropertyType.IsEnum);

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
            if (item.GetCustomAttribute<IgnorePropertyAttribute>() is not null)
                continue;
            var propName = item.Name;
            var propType = item.PropertyType.IsArray ?
                GetTypeFromTypeName(item.PropertyType.GetElementType()!.Name, true, item.PropertyType.GetElementType()!.IsEnum) :
                GetTypeFromTypeName(item.PropertyType.Name, false, item.PropertyType.IsEnum);
            if (propType == SerializedType.None)
                continue;
            data.Add((propName, propType, item));
        }

        return data;
    }

    private static SerializedType GetTypeFromTypeName(string typeName, bool isArray, bool isEnum)
    {
        var propType = SerializedType.None;
        if (isArray)
        {
            if (isEnum)
                propType = SerializedType.EnumArray;
            else
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
                    _ => SerializedType.ObjectArray,
                };
            }
        }
        else
        {
            if (isEnum)
                propType = SerializedType.Enum;
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
                    _ => SerializedType.Object,
                };
            }
        }
        return propType;
    }

    private static bool WritePropertiesValueInBuffer(List<(string name, SerializedType type, object? value)> props, BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, value) in props)
        {
            if (value is null)
            {
                buffer.Write(SerializeFlags.NullValue);
                continue;
            }
            buffer.Write(true);
            var result = type switch
            {
                // BASE TYPES
                SerializedType.SByte => buffer.Write((sbyte)value!),
                SerializedType.Byte => buffer.Write((byte)value!),
                SerializedType.Boolean => buffer.Write((bool)value!),
                SerializedType.Float => buffer.Write((float)value!),
                SerializedType.Double => buffer.Write((double)value!),
                SerializedType.Int16 => buffer.Write((short)value!),
                SerializedType.Int32 => buffer.Write((int)value!),
                SerializedType.Int64 => buffer.Write((long)value!),
                SerializedType.UInt16 => buffer.Write((ushort)value!),
                SerializedType.UInt32 => buffer.Write((uint)value!),
                SerializedType.UInt64 => buffer.Write((ulong)value!),
                SerializedType.Char => buffer.Write((char)value!),
                SerializedType.String => buffer.Write((string)value!, encoding),
                SerializedType.DateTime => buffer.Write((DateTime)value!),
                SerializedType.Enum => buffer.Write((Enum)value!),
                SerializedType.Guid => buffer.Write((Guid)value!),
                SerializedType.Object => BaseSerialize(value!.GetType(), value, buffer, encoding),
                // ARRAY TYPES
                SerializedType.SByteArray => buffer.WriteArray((sbyte[])value!),
                SerializedType.ByteArray => buffer.WriteArray((byte[])value!),
                SerializedType.BooleanArray => buffer.WriteArray((bool[])value!),
                SerializedType.FloatArray => buffer.WriteArray((float[])value!),
                SerializedType.DoubleArray => buffer.WriteArray((double[])value!),
                SerializedType.Int16Array => buffer.WriteArray((short[])value!),
                SerializedType.Int32Array => buffer.WriteArray((int[])value!),
                SerializedType.Int64Array => buffer.WriteArray((long[])value!),
                SerializedType.UInt16Array => buffer.WriteArray((ushort[])value!),
                SerializedType.UInt32Array => buffer.WriteArray((uint[])value!),
                SerializedType.UInt64Array => buffer.WriteArray((ulong[])value!),
                SerializedType.CharArray => buffer.WriteArray((char[])value!),
                SerializedType.StringArray => buffer.WriteArray(encoding, (string[])value!),
                SerializedType.DateTimeArray => buffer.WriteArray((DateTime[])value!),
                SerializedType.EnumArray => buffer.WriteArray(Helper.ToEnumArray(value!)),
                SerializedType.GuidArray => buffer.WriteArray((Guid[])value!),
                SerializedType.ObjectArray => WriteObjectArray(value!, buffer, encoding),
                _ => false,
            };
            if (!result)
                return false;
        }

        return true;
    }

    private static bool ReadPropertiesValueFromBuffer(List<(string name, SerializedType type, PropertyInfo pi)> props, ref object obj, BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, pi) in props)
        {
            var flag = buffer.ReadEnum<SerializeFlags>();
            if (flag.HasFlag(SerializeFlags.NullValue))
            {
                pi.SetValue(obj, null);
                continue;
            }

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
                type == SerializedType.Object ? BaseDeserialize(pi.PropertyType, buffer, encoding) :
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
                type == SerializedType.EnumArray ? Helper.FromEnumArray(buffer.ReadEnumArray(pi.PropertyType.GetElementType()!), pi!.PropertyType.GetElementType()!) :
                type == SerializedType.GuidArray ? buffer.ReadGuidArray() :
                type == SerializedType.ObjectArray ? ReadObjectArray(pi.PropertyType, buffer, encoding) :
                null);
        }

        return true;
    }

    private static bool WriteObjectArray(object value, BinaryBuffer buffer, Encoding encoding)
    {
        var type = value.GetType();
        if(type.GetElementType() == typeof(object))
            return false;
        var arr = (value as Array)!;
        buffer.Write(arr.Length);

        foreach (var item in arr)
        {
            if(!BaseSerialize(item.GetType(), item, buffer, encoding))
            {
                throw new Exception();
            }
        }
        return true;
    }

    private static object? ReadObjectArray(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        var size = buffer.ReadUInt32();

        if(size == 0)
        {
            return Array.Empty<object>();
        }

        var et = type.GetElementType()!;

        Array arr = Array.CreateInstance(et, size);

        for (int i = 0; i < size; i++)
        {
            var item = BaseDeserialize(et, buffer, encoding);
            arr.SetValue(item, i);    
        }
        return arr;
    }
}
