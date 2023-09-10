using System.Text;
using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
using ASiNet.Binary.Lib.Expressions.BaseTypes;

namespace ASiNet.Binary.Lib.Expressions.Arrays;
public static class BinaryBufferArrays
{
    #region write

    public static bool WriteArray(this BinaryBuffer buffer, params bool[] array)
    {
        buffer.Write(array.Length);
        
        var dist = new byte[array.Length];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params byte[] array)
    {
        buffer.Write(array.Length);
        buffer.WriteSpan(array);
        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params sbyte[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params short[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(short)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params ushort[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(ushort)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params int[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(int)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params uint[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(uint)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params long[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(long)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);


        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params ulong[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(ulong)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params float[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(float)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params double[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(double)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params char[] array)
    {
        buffer.Write(array.Length);

        var dist = new byte[array.Length * sizeof(char)];
        Buffer.BlockCopy(array, 0, dist, 0, dist.Length);

        buffer.WriteSpan(dist);

        return true;
    }

    public static bool WriteArray(this BinaryBuffer buffer, params Enum[] array)
    {
        buffer.Write(array.Length);
        foreach (var item in array)
        {
            buffer.Write(item);
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

        var src = new byte[length];
        buffer.ReadToSpan(src);

        return src.AsBooleanArray();
    }

    public static byte[] ReadByteArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var array = new byte[length];
        buffer.ReadToSpan(array);
        return array;
    }

    public static sbyte[] ReadSByteArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length];
        buffer.ReadToSpan(src);

        return src.AsSByteArray();
    }

    public static short[] ReadInt16Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(short)];
        buffer.ReadToSpan(src);

        return src.AsInt16Array();
    }

    public static ushort[] ReadUInt16Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(ushort)];
        buffer.ReadToSpan(src);

        return src.AsUInt16Array();
    }

    public static int[] ReadInt32Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(int)];
        buffer.ReadToSpan(src);

        return src.AsInt32Array();
    }

    public static uint[] ReadUInt32Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(uint)];
        buffer.ReadToSpan(src);

        return src.AsUInt32Array();
    }

    public static long[] ReadInt64Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(long)];
        buffer.ReadToSpan(src);

        return src.AsInt64Array();
    }

    public static ulong[] ReadUInt64Array(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(ulong)];
        buffer.ReadToSpan(src);

        return src.AsUInt64Array();
    }

    public static float[] ReadSingleArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var bytes = new byte[length * sizeof(float)];
        buffer.ReadToSpan(bytes);

        return bytes.AsSingleArray();
    }

    public static double[] ReadDoubleArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var bytes = new byte[length * sizeof(double)];
        buffer.ReadToSpan(bytes);

        return bytes.AsDoubleArray();
    }

    public static char[] ReadCharArray(this BinaryBuffer buffer)
    {
        var length = buffer.ReadInt32();
        var src = new byte[length * sizeof(char)];
        buffer.ReadToSpan(src);

        return src.AsCharArray();
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
