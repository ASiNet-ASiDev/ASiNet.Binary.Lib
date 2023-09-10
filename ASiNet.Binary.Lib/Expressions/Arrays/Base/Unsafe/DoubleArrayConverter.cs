using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class DoubleArrayConverter
{
    public static byte[] AsByteArray(this double[] input)
    {
        var union = new DoubleUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static double[] AsDoubleArray(this byte[] input)
    {
        var union = new DoubleUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<double>(ArrayConverterBase.DOUBLE_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(double[] input, out byte[] converted)
    {
        var union = new DoubleUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsDoubleArray(byte[] input, out double[] converted)
    {
        var union = new DoubleUnion { Bytes = input };
        union.Bytes.ToObjestsArray<double>(ArrayConverterBase.DOUBLE_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
