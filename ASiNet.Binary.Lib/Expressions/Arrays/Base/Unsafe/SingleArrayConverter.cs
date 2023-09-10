using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class SingleArrayConverter
{
    public static byte[] AsByteArray(this float[] input)
    {
        var union = new SingleUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static float[] AsSingleArray(this byte[] input)
    {
        var union = new SingleUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<float>(ArrayConverterBase.FLOAT_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(float[] input, out byte[] converted)
    {
        var union = new SingleUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsSingleArray(byte[] input, out float[] converted)
    {
        var union = new SingleUnion { Bytes = input };
        union.Bytes.ToObjestsArray<float>(ArrayConverterBase.FLOAT_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
