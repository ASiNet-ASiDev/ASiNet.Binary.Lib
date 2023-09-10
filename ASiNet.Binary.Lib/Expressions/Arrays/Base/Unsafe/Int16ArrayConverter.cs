using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class Int16ArrayConverter
{
    public static byte[] AsByteArray(this short[] input)
    {
        var union = new Int16Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static short[] AsInt16Array(this byte[] input)
    {
        var union = new Int16Union { Bytes = input };
        union.Bytes!.ToObjestsArray<short>(ArrayConverterBase.INT16_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(short[] input, out byte[] converted)
    {
        var union = new Int16Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsInt16Array(byte[] input, out short[] converted)
    {
        var union = new Int16Union { Bytes = input };
        union.Bytes.ToObjestsArray<short>(ArrayConverterBase.INT16_ARRAY_TYPE);
        converted = union.Objects!;
    }
}

public static class UInt16ArrayConverter
{
    public static byte[] AsByteArray(this ushort[] input)
    {
        var union = new UInt16Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static ushort[] AsUInt16Array(this byte[] input)
    {
        var union = new UInt16Union { Bytes = input };
        union.Bytes!.ToObjestsArray<ushort>(ArrayConverterBase.UINT16_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(ushort[] input, out byte[] converted)
    {
        var union = new UInt16Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsUInt16Array(byte[] input, out ushort[] converted)
    {
        var union = new UInt16Union { Bytes = input };
        union.Bytes.ToObjestsArray<ushort>(ArrayConverterBase.UINT16_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
