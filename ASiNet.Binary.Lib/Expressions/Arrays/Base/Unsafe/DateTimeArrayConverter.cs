using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class DateTimeArrayConverter
{
    public static byte[] AsByteArray(this DateTime[] input)
    {
        var union = new DateTimeUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static DateTime[] AsDateTimeArray(this byte[] input)
    {
        var union = new DateTimeUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<DateTime>(ArrayConverterBase.DATETIME_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(DateTime[] input, out byte[] converted)
    {
        var union = new DateTimeUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsDateTimeArray(byte[] input, out DateTime[] converted)
    {
        var union = new DateTimeUnion { Bytes = input };
        union.Bytes.ToObjestsArray<DateTime>(ArrayConverterBase.DATETIME_ARRAY_TYPE);
        converted = union.Objects!;
    }

}
