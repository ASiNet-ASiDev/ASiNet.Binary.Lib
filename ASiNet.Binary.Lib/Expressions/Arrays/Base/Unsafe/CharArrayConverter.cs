using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class CharArrayConverter
{
    public static byte[] AsByteArray(this char[] input)
    {
        var union = new CharUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static char[] AsCharArray(this byte[] input)
    {
        var union = new CharUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<char>(ArrayConverterBase.CHAR_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(char[] input, out byte[] converted)
    {
        var union = new CharUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsCharArray(byte[] input, out char[] converted)
    {
        var union = new CharUnion { Bytes = input };
        union.Bytes.ToObjestsArray<char>(ArrayConverterBase.CHAR_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
