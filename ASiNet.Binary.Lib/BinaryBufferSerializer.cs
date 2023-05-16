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

			var data = GetPropertiesAndValues(obj, typeof(T));

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

            var result = new T();

            var data = GetProperties(typeof(T));

            data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name));
            var isDone = ReadPropertiesValueFromBuffer(data, result, ref buffer, encoding);
            return result;
        }
        catch
        {
            return default;
        }
    }

    private static List<(string name, SerializedTypes type, object? value)> GetPropertiesAndValues<T>(T obj, Type type)
    {
        var props = type.GetProperties();
        var data = new List<(string name, SerializedTypes type, object? value)>(props.Length);
        foreach (var item in props)
        {
            var propName = item.Name;
            var propVal = item.GetValue(obj);
            var propType = item.PropertyType.Name switch
            {
                nameof(SByte) => SerializedTypes.SByte,
                nameof(Byte) => SerializedTypes.Byte,
                nameof(Single) => SerializedTypes.Float,
                nameof(Double) => SerializedTypes.Double,
                nameof(Boolean) => SerializedTypes.Boolean,
                nameof(Int16) => SerializedTypes.Int16,
                nameof(UInt16) => SerializedTypes.UInt16,
                nameof(Int32) => SerializedTypes.Int32,
                nameof(UInt32) => SerializedTypes.UInt32,
                nameof(Int64) => SerializedTypes.Int64,
                nameof(UInt64) => SerializedTypes.UInt64,
                nameof(Char) => SerializedTypes.Char,
                nameof(String) => SerializedTypes.String,
                nameof(DateTime) => SerializedTypes.DateTime,
                nameof(Guid) => SerializedTypes.Guid,
                nameof(Enum) => SerializedTypes.Enum,
                _ => SerializedTypes.None,
            };
            if (propType == SerializedTypes.None)
                continue;
            data.Add((propName, propType, propVal));
        }

        return data;
    }

    private static List<(string name, SerializedTypes type, PropertyInfo pi)> GetProperties(Type type)
    {
        var props = type.GetProperties();
        var data = new List<(string name, SerializedTypes type, PropertyInfo pi)>(props.Length);
        foreach (var item in props)
        {
            var propName = item.Name;
            var propType = item.PropertyType.Name switch
            {
                nameof(SByte) => SerializedTypes.SByte,
                nameof(Byte) => SerializedTypes.Byte,
                nameof(Single) => SerializedTypes.Float,
                nameof(Double) => SerializedTypes.Double,
                nameof(Boolean) => SerializedTypes.Boolean,
                nameof(Int16) => SerializedTypes.Int16,
                nameof(UInt16) => SerializedTypes.UInt16,
                nameof(Int32) => SerializedTypes.Int32,
                nameof(UInt32) => SerializedTypes.UInt32,
                nameof(Int64) => SerializedTypes.Int64,
                nameof(UInt64) => SerializedTypes.UInt64,
                nameof(Char) => SerializedTypes.Char,
                nameof(String) => SerializedTypes.String,
                nameof(DateTime) => SerializedTypes.DateTime,
                nameof(Guid) => SerializedTypes.Guid,
                nameof(Enum) => SerializedTypes.Enum,
                _ => SerializedTypes.None,
            };
            if (propType == SerializedTypes.None)
                continue;
            data.Add((propName, propType, item));
        }

        return data;
    }

    private static bool WritePropertiesValueInBuffer(List<(string name, SerializedTypes type, object? value)> props, ref BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, value) in props)
        {
            var result = type switch
            {
                SerializedTypes.SByte => buffer.Write((sbyte)(value ?? default(sbyte))),
                SerializedTypes.Byte => buffer.Write((byte)(value ?? default(byte))),
                SerializedTypes.Boolean => buffer.Write((bool)(value ?? default(bool))),
                SerializedTypes.Float => buffer.Write((float)(value ?? default(float))),
                SerializedTypes.Double => buffer.Write((double)(value ?? default(double))),
                SerializedTypes.Int16 => buffer.Write((short)(value ?? default(short))),
                SerializedTypes.Int32 => buffer.Write((int)(value ?? default(int))),
                SerializedTypes.Int64 => buffer.Write((long)(value ?? default(long))),
                SerializedTypes.UInt16 => buffer.Write((ushort)(value ?? default(ushort))),
                SerializedTypes.UInt32 => buffer.Write((uint)(value ?? default(uint))),
                SerializedTypes.UInt64 => buffer.Write((ulong)(value ?? default(ulong))),
                SerializedTypes.Char => buffer.Write((char)(value ?? default(char))),
                SerializedTypes.String => buffer.Write((string)(value ?? string.Empty), encoding),
                SerializedTypes.DateTime => buffer.Write((DateTime)(value ?? DateTime.MinValue)),
                SerializedTypes.Enum => buffer.Write((Enum)(value ?? 0)),
                SerializedTypes.Guid => buffer.Write((Guid)(value ?? Guid.Empty)),
                _ => false,
            };
            if (!result)
                return false;
        }

        return true;
    }

    private static bool ReadPropertiesValueFromBuffer<T>(List<(string name, SerializedTypes type, PropertyInfo pi)> props, T obj, ref BinaryBuffer buffer, Encoding encoding)
    {
        foreach (var (name, type, pi) in props)
        {

            pi.SetValue(obj,
                type == SerializedTypes.Boolean ? buffer.ReadBoolean() :
                type == SerializedTypes.SByte ? buffer.ReadSByte() :
                type == SerializedTypes.Byte ? buffer.ReadByte() :
                type == SerializedTypes.Int16 ? buffer.ReadInt16() :
                type == SerializedTypes.Int32 ? buffer.ReadInt32() :
                type == SerializedTypes.Int64 ? buffer.ReadInt64() :
                type == SerializedTypes.UInt16 ? buffer.ReadUInt16() :
                type == SerializedTypes.UInt32 ? buffer.ReadUInt32() :
                type == SerializedTypes.UInt64 ? buffer.ReadUInt64() :
                type == SerializedTypes.Char ? buffer.ReadChar() :
                type == SerializedTypes.String ? buffer.ReadString(encoding) :
                type == SerializedTypes.Float ? buffer.ReadSingle() :
                type == SerializedTypes.Double ? buffer.ReadDouble() :
                type == SerializedTypes.DateTime ? buffer.ReadDateTime() :
                type == SerializedTypes.Enum ? buffer.ReadEnum(pi.PropertyType) :
                type == SerializedTypes.Guid ? buffer.ReadGuid() : null);
        }

        return true;
    }
}
