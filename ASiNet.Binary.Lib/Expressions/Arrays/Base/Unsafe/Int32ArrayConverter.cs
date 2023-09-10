using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class Int32ArrayConverter
{
    public static byte[] AsByteArray(this int[] input)
    {
        var union = new Int32Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static int[] AsInt32Array(this byte[] input)
    {
        var union = new Int32Union { Bytes = input };
        union.Bytes!.ToObjestsArray<int>(ArrayConverterBase.INT32_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(int[] input, out byte[] converted)
    {
        var union = new Int32Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsInt32Array(byte[] input, out int[] converted)
    {
        var union = new Int32Union { Bytes = input };
        union.Bytes.ToObjestsArray<int>(ArrayConverterBase.INT32_ARRAY_TYPE);
        converted = union.Objects!;
    }
}

public static class UInt32ArrayConverter
{
    public static byte[] AsByteArray(this uint[] input)
    {
        var union = new UInt32Union { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static uint[] AsUInt32Array(this byte[] input)
    {
        var union = new UInt32Union { Bytes = input };
        union.Bytes!.ToObjestsArray<uint>(ArrayConverterBase.UINT32_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(uint[] input, out byte[] converted)
    {
        var union = new UInt32Union { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsUInt32Array(byte[] input, out uint[] converted)
    {
        var union = new UInt32Union { Bytes = input };
        union.Bytes.ToObjestsArray<uint>(ArrayConverterBase.UINT32_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
