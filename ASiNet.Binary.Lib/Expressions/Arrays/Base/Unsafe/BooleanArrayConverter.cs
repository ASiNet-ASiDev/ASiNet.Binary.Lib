using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class BooleanArrayConverter
{
    public static byte[] AsByteArray(this bool[] input)
    {
        var union = new BooleanUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static bool[] AsBooleanArray(this byte[] input)
    {
        var union = new BooleanUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<bool>(ArrayConverterBase.BOOLEAN_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(bool[] input, out byte[] converted)
    {
        var union = new BooleanUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsBooleanArray(byte[] input, out bool[] converted)
    {
        var union = new BooleanUnion { Bytes = input };
        union.Bytes.ToObjestsArray<bool>(ArrayConverterBase.BOOLEAN_ARRAY_TYPE);
        converted = union.Objects!;
    }
}