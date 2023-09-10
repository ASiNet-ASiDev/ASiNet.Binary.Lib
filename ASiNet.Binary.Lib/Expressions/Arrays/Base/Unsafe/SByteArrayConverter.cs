using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class SByteArrayConverter
{
    public static byte[] AsByteArray(this sbyte[] input)
    {
        var union = new SByteUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static sbyte[] AsSByteArray(this byte[] input)
    {
        var union = new SByteUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<sbyte>(ArrayConverterBase.SBYTE_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(sbyte[] input, out byte[] converted)
    {
        var union = new SByteUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsSByteArray(byte[] input, out sbyte[] converted)
    {
        var union = new SByteUnion { Bytes = input };
        union.Bytes.ToObjestsArray<sbyte>(ArrayConverterBase.SBYTE_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
