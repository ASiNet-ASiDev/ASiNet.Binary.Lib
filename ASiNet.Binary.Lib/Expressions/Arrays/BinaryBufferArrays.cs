﻿using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Expressions.Arrays;
public static class BinaryBufferArrays
{
    #region write

    public static bool WriteArray(this BinaryBuffer buffer, params bool[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params byte[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params sbyte[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params short[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params ushort[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params int[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params uint[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params long[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params ulong[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params float[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params double[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params char[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params Enum[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write((Enum)item);
        }
        return true;
    }

    public static bool WriteEnumArray<T>(this BinaryBuffer buffer, params T[] array) where T : Enum
    {
        buffer.Write(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            buffer.Write(array[i]);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params DateTime[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params Guid[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
        }
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, Encoding encoding, params string[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item, encoding);
        }
        return true;
    }

    #endregion
    #region read
    public static bool[] ReadBooleanArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new bool[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadBoolean();
        }

        return array;
    }

    public static byte[] ReadByteArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new byte[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadByte();
        }

        return array;
    }

    public static sbyte[] ReadSByteArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new sbyte[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadSByte();
        }

        return array;
    }

    public static short[] ReadInt16Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new short[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadInt16();
        }

        return array;
    }

    public static ushort[] ReadUInt16Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new ushort[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadUInt16();
        }

        return array;
    }

    public static int[] ReadInt32Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadInt32();
        }

        return array;
    }

    public static uint[] ReadUInt32Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new uint[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadUInt32();
        }

        return array;
    }

    public static long[] ReadInt64Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new long[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadInt64();
        }

        return array;
    }

    public static ulong[] ReadUInt64Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new ulong[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadUInt64();
        }

        return array;
    }

    public static float[] ReadSingleArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new float[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadSingle();
        }

        return array;
    }

    public static double[] ReadDoubleArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new double[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadDouble();
        }

        return array;
    }

    public static char[] ReadCharArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new char[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadChar();
        }

        return array;
    }

    public static T[] ReadEnumArray<T>(this BinaryBuffer buffer) where T : struct, Enum
    {
        var length = buffer.ReadInt32();
        var array = new T[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadEnum<T>();
        }

        return array;
    }

    public static Guid[] ReadGuidArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new Guid[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadGuid();
        }

        return array;
    }

    public static DateTime[] ReadDateTimeArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new DateTime[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadDateTime();
        }

        return array;
    }

    public static string[] ReadStringArray(this BinaryBuffer buffer, Encoding encoding)
    {
        var length = buffer.ReadInt32();
        var array = new string[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = buffer.ReadString(encoding);
        }

        return array;
    }
    #endregion
}
