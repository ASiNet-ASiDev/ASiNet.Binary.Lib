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
    public static bool Write(this BinaryBuffer self, in byte value)
    {
        var buffer = self.GetBuffer();
        buffer[0] = value;
        return self.WriteBuffer(sizeof(byte));
    }

    public static bool Write(this BinaryBuffer self, in sbyte value)
    {
        var buffer = self.GetBuffer();
        buffer[0] = (byte)value;
        return self.WriteBuffer(sizeof(sbyte));
    }

    public static bool Write(this BinaryBuffer self, in bool value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(bool));
    }

    public static bool Write(this BinaryBuffer self, in short value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(short));
    }

    public static bool Write(this BinaryBuffer self, in ushort value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(ushort));
    }

    public static bool Write(this BinaryBuffer self, in int value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(int));
    }

    public static bool Write(this BinaryBuffer self, in uint value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(uint));
    }

    public static bool Write(this BinaryBuffer self, in long value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(long));
    }

    public static bool Write(this BinaryBuffer self, in ulong value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(ulong));
    }

    public static bool Write(this BinaryBuffer self, in float value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(float));
    }

    public static bool Write(this BinaryBuffer self, in double value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(double));
    }

    public static bool Write(this BinaryBuffer self, in char value)
    {
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, value);
        return self.WriteBuffer(sizeof(char));
    }

    public static bool Write(this BinaryBuffer self, in DateTime value)
    {
        var binaryDate = value.ToBinary();
        var buffer = self.GetBuffer();
        BitConverter.TryWriteBytes(buffer, binaryDate);
        return self.WriteBuffer(sizeof(long));
    }

    public static bool Write(this BinaryBuffer self, in Guid value)
    {

        var buffer = self.GetBuffer();
        if (value.TryWriteBytes(buffer))
            return self.WriteBuffer(sizeof(decimal));
        return false;
    }

    public static bool Write(this BinaryBuffer self, string value, Encoding encoding)
    {
        var size = encoding.GetByteCount(value);

        if(size + sizeof(int) > self.FreeSpace)
            return false;

        var sizeT = self.Write(size);
        var spanT = self.WriteSpan(value.AsSpan(), encoding);

        return sizeT && spanT;
    }

    public static bool Write(this BinaryBuffer self, in Enum value)
    {
        var etype = value.GetType().GetEnumUnderlyingType();

        if (etype == typeof(int))
            return Write(self, (int)(object)value);

        if (etype == typeof(byte))
            return Write(self, (byte)(object)value);

        if (etype == typeof(short))
            return Write(self, (short)(object)value);

        if (etype == typeof(long))
            return Write(self, (long)(object)value);

        if (etype == typeof(ushort))
            return Write(self, (ushort)(object)value);
        
        if (etype == typeof(uint))
            return Write(self, (uint)(object)value);
        
        if (etype == typeof(ulong))
            return Write(self, (ulong)(object)value);

        if (etype == typeof(sbyte))
            return Write(self, (sbyte)(object)value);

        throw new NotImplementedException();
    }

    #endregion
    #region read

    public static byte ReadByte(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(byte));
        var buffer = self.GetBuffer();
        return buffer[0];
    }

    public static sbyte ReadSByte(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(byte));
        var buffer = self.GetBuffer();
        return (sbyte)buffer[0];
    }

    public static bool ReadBoolean(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(bool));
        var buffer = self.GetBuffer();
        return BitConverter.ToBoolean(buffer);
    }

    public static short ReadInt16(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(short));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt16(buffer);
    }

    public static ushort ReadUInt16(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(ushort));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt16(buffer);
    }

    public static int ReadInt32(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(int));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt32(buffer);
    }

    public static uint ReadUInt32(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(uint));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt32(buffer);
    }

    public static long ReadInt64(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(long));
        var buffer = self.GetBuffer();
        return BitConverter.ToInt64(buffer);
    }

    public static ulong ReadUInt64(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(ulong));
        var buffer = self.GetBuffer();
        return BitConverter.ToUInt64(buffer);
    }

    public static float ReadSingle(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(float));
        var buffer = self.GetBuffer();
        return BitConverter.ToSingle(buffer);
    }

    public static double ReadDouble(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(double));
        var buffer = self.GetBuffer();
        return BitConverter.ToDouble(buffer);
    }

    public static char ReadChar(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(char));
        var buffer = self.GetBuffer();
        return BitConverter.ToChar(buffer);
    }

    public static DateTime ReadDateTime(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(long));
        var buffer = self.GetBuffer();
        var binaryDt = BitConverter.ToInt64(buffer);
        return DateTime.FromBinary(binaryDt);
    }

    public static Guid ReadGuid(this BinaryBuffer self)
    {
        self.ReadToBuffer(sizeof(decimal));
        var buffer = self.GetBuffer();
        return new(buffer.Slice(0, sizeof(decimal)));
    }

    public static string ReadString(this BinaryBuffer self, Encoding encoding)
    {
        self.ReadToBuffer(sizeof(int));
        var buffer = self.GetBuffer();
        var size = BitConverter.ToInt32(buffer);
        self.ReadToBuffer((ushort)size);
        var str = encoding.GetString(buffer.Slice(0, size));
        return str;
    }

    public static TEnum ReadEnum<TEnum>(this BinaryBuffer self) where TEnum : struct, Enum
    {
        var etype = typeof(TEnum).GetEnumUnderlyingType();

        if (etype == typeof(int))
            return Helper.ToEnum<int, TEnum>(ReadInt32(self));

        if (etype == typeof(byte))
            return Helper.ToEnum<byte, TEnum>(ReadByte(self));

        if (etype == typeof(short))
            return Helper.ToEnum<short, TEnum>(ReadInt16(self));

        if (etype == typeof(long))
            return Helper.ToEnum<long, TEnum>(ReadInt64(self));        
        
        if (etype == typeof(ushort))
            return Helper.ToEnum<ushort, TEnum>(ReadUInt16(self));
       
        if (etype == typeof(uint))
            return Helper.ToEnum<uint, TEnum>(ReadUInt32(self));
        
        if (etype == typeof(ulong))
            return Helper.ToEnum<ulong, TEnum>(ReadUInt64(self));

        if (etype == typeof(sbyte))
            return Helper.ToEnum<sbyte, TEnum>(ReadSByte(self));

        throw new NotSupportedException();
    }

    public static Enum ReadEnum(this ref BinaryBuffer self, Type enumType)
    {
        if (!enumType.IsEnum)
            throw new Exception("Type not enum!");
        var etype = enumType.GetEnumUnderlyingType();

        if (etype == typeof(int))
            return Helper.ToEnum(ReadInt32(self), enumType);

        if (etype == typeof(byte))
            return Helper.ToEnum(ReadByte(self), enumType);

        if (etype == typeof(short))
            return Helper.ToEnum(ReadInt16(self), enumType);

        if (etype == typeof(long))
            return Helper.ToEnum(ReadInt64(self), enumType);        
        
        if (etype == typeof(ushort))
            return Helper.ToEnum(ReadUInt16(self), enumType);
        
        if (etype == typeof(uint))
            return Helper.ToEnum(ReadUInt32(self), enumType);
        
        if (etype == typeof(ulong))
            return Helper.ToEnum(ReadUInt64(self), enumType);

        if (etype == typeof(sbyte))
            return Helper.ToEnum(ReadSByte(self), enumType);

        throw new NotSupportedException();
    }

    #endregion
}
