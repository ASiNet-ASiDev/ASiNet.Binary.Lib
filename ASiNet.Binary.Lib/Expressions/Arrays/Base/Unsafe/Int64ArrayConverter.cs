using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class Int64ArrayConverter
{
    public static byte[] AsByteArray(this long[] input)
    {
        var union = new Int64Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static long[] AsInt64Array(this byte[] input)
    {
        var union = new Int64Union { Bytes = input };
        union.Bytes!.ToObjestsArray<long>(ArrayConverterBase.INT64_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(long[] input, out byte[] converted)
    {
        var union = new Int64Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsInt64Array(byte[] input, out long[] converted)
    {
        var union = new Int64Union { Bytes = input };
        union.Bytes.ToObjestsArray<long>(ArrayConverterBase.INT64_ARRAY_TYPE);
        converted = union.Objects!;
    }
}


public static class UInt64ArrayConverter
{
    public static byte[] AsByteArray(this ulong[] input)
    {
        var union = new UInt64Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static ulong[] AsUInt64Array(this byte[] input)
    {
        var union = new UInt64Union { Bytes = input };
        union.Bytes!.ToObjestsArray<ulong>(ArrayConverterBase.UINT64_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(ulong[] input, out byte[] converted)
    {
        var union = new UInt64Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsUInt64Array(byte[] input, out ulong[] converted)
    {
        var union = new UInt64Union { Bytes = input };
        union.Bytes.ToObjestsArray<ulong>(ArrayConverterBase.UINT64_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
