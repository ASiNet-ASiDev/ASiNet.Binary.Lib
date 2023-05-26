using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ASiNet.Binary.Lib.Expressions.BaseTypes;
public static class BinaryBufferBaseTypes
{
    #region write
    public static bool Write(this ref BinaryBuffer self, in byte value)
    {
        var buffer = self.GetBuffer();
        buffer[0] = value;
        return self.WriteBuffer(sizeof(byte));
    }

    public static bool Write(this ref BinaryBuffer self, in sbyte value)
    {
        var buffer = self.GetBuffer();
        buffer[0] = (byte)value;
        return self.WriteBuffer(sizeof(sbyte));
    }

    public static bool Write(this ref BinaryBuffer self, in bool value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(bool));
    }

    public static bool Write(this ref BinaryBuffer self, in short value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(short));
    }

    public static bool Write(this ref BinaryBuffer self, in ushort value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(ushort));
    }

    public static bool Write(this ref BinaryBuffer self, in int value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(int));
    }

    public static bool Write(this ref BinaryBuffer self, in uint value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(uint));
    }

    public static bool Write(this ref BinaryBuffer self, in long value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(long));
    }

    public static bool Write(this ref BinaryBuffer self, in ulong value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(ulong));
    }

    public static bool Write(this ref BinaryBuffer self, in float value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(float));
    }

    public static bool Write(this ref BinaryBuffer self, in double value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(double));
    }

    public static bool Write(this ref BinaryBuffer self, in char value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(char));
    }

    public static bool Write(this ref BinaryBuffer self, in DateTime value)
    {
        var binaryDate = value.ToBinary();
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, binaryDate);
        return self.WriteBuffer(sizeof(long));
    }

    public static bool Write(this ref BinaryBuffer self, in Guid value)
    {

        var buffer = self.GetBuffer();
        if (value.TryWriteBytes(buffer))
            return self.WriteBuffer(sizeof(decimal));
        return false;
    }

    public static bool Write(this ref BinaryBuffer self, string value, Encoding encoding)
    {
        var buffer = self.GetBuffer();
        var size = encoding.GetByteCount(value);

        if(size > buffer.Length)
            throw new Exception("String Binary Size > Buffer Size!");

        var localBuffer = new BinaryBuffer(buffer, stackalloc byte[buffer.Length]);

        localBuffer.Write(size);
        localBuffer.WriteSpan(value.AsSpan(), encoding);

        return self.WriteBuffer((ushort)(sizeof(int) + size));
    }

    public static bool Write(this ref BinaryBuffer self, in Enum value)
    {
        var etype = value.GetType().GetEnumUnderlyingType().Name;
        var result = etype switch
        {
            nameof(SByte) => Write(ref self, (sbyte)(object)value),
            nameof(Byte) => Write(ref self, (byte)(object)value),
            nameof(Int16) => Write(ref self, (short)(object)value),
            nameof(UInt16) => Write(ref self, (ushort)(object)value),
            nameof(Int32) => Write(ref self, (int)(object)value),
            nameof(UInt32) => Write(ref self, (uint)(object)value),
            nameof(Int64) => Write(ref self, (long)(object)value),
            nameof(UInt64) => Write(ref self, (ulong)(object)value),
            _ => throw new NotImplementedException($"Enum type is '{etype}' not implimented!"),
        };
        return result;
    }

    #endregion
    #region read

    public static byte ReadByte(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(byte));
        var buffer = self.GetBuffer();
        return buffer[0];
    }

    public static sbyte ReadSByte(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(byte));
        var buffer = self.GetBuffer();
        return (sbyte)buffer[0];
    }

    public static bool ReadBoolean(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(bool));
        var buffer = self.GetBuffer();
        return BitConverter.ToBoolean(buffer);
    }

    public static short ReadInt16(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(short));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt16(buffer);
    }

    public static ushort ReadUInt16(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(ushort));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt16(buffer);
    }

    public static int ReadInt32(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(int));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt32(buffer);
    }

    public static uint ReadUInt32(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(uint));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt32(buffer);
    }

    public static long ReadInt64(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(long));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt64(buffer);
    }

    public static ulong ReadUInt64(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(ulong));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt64(buffer);
    }

    public static float ReadSingle(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(float));
        var buffer = self.GetBuffer();
        return BitConverter.ToSingle(buffer);
    }

    public static double ReadDouble(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(double));
        var buffer = self.GetBuffer();
        return BitConverter.ToDouble(buffer);
    }

    public static char ReadChar(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(char));
        var buffer = self.GetBuffer();
        return BitConverter.ToChar(buffer);
    }

    public static DateTime ReadDateTime(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(long));
        var buffer = self.GetBuffer();
        var binaryDt = BitConverter.ToInt64(buffer);
        return DateTime.FromBinary(binaryDt);
    }

    public static Guid ReadGuid(this ref BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(decimal));
        var buffer = self.GetBuffer();
        return new(buffer.Slice(0, sizeof(decimal)));
    }

    public static string ReadString(this ref BinaryBuffer self, Encoding encoding)
    {
        self.ReadToBuffer(sizeof(int));
        var buffer = self.GetBuffer();
        var size = BitConverter.ToInt32(buffer);
        self.ReadToBuffer((ushort)size);
        var str = encoding.GetString(buffer.Slice(0, size));
        return str;
    }

    public static T ReadEnum<T>(this ref BinaryBuffer self) where T : struct, Enum
    {
        var etype = typeof(T).GetEnumUnderlyingType().Name;
        var result = etype switch
        {
            nameof(SByte) => Enum.Parse<T>(ReadSByte(ref self).ToString()),
            nameof(Byte) => Enum.Parse<T>(ReadByte(ref self).ToString()),
            nameof(Int16) => Enum.Parse<T>(ReadInt16(ref self).ToString()),
            nameof(UInt16) => Enum.Parse<T>(ReadUInt16(ref self).ToString()),
            nameof(Int32) => Enum.Parse<T>(ReadInt32(ref self).ToString()),
            nameof(UInt32) => Enum.Parse<T>(ReadUInt32(ref self).ToString()),
            nameof(Int64) => Enum.Parse<T>(ReadInt64(ref self).ToString()),
            nameof(UInt64) => Enum.Parse<T>(ReadUInt64(ref self).ToString()),
            _ => throw new NotImplementedException($"Enum type is '{etype}' not implimented!"),
        };
        return result;
    }

    public static Enum ReadEnum(this ref BinaryBuffer self, Type enumType)
    {
        if (!enumType.IsEnum)
            throw new Exception("Type not enum!");
        var etype = enumType.GetEnumUnderlyingType().Name;
        var result = etype switch
        {
            nameof(SByte) => Enum.Parse(enumType, ReadSByte(ref self).ToString()),
            nameof(Byte) => Enum.Parse(enumType, ReadByte(ref self).ToString()),
            nameof(Int16) => Enum.Parse(enumType, ReadInt16(ref self).ToString()),
            nameof(UInt16) => Enum.Parse(enumType, ReadUInt16(ref self).ToString()),
            nameof(Int32) => Enum.Parse(enumType, ReadInt32(ref self).ToString()),
            nameof(UInt32) => Enum.Parse(enumType, ReadUInt32(ref self).ToString()),
            nameof(Int64) => Enum.Parse(enumType, ReadInt64(ref self).ToString()),
            nameof(UInt64) => Enum.Parse(enumType, ReadUInt64(ref self).ToString()),
            _ => throw new NotImplementedException($"Enum type is '{etype}' not implimented!"),
        };
        return (Enum)result;
    }

    #endregion
}
