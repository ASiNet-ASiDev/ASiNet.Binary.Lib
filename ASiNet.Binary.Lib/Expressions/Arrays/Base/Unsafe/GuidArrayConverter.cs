using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
public static class GuidArrayConverter
{
    public static byte[] AsByteArray(this Guid[] input)
    {
        var union = new GuidUnion { Objects = input };
        union.Objects.ToBytesArray();
        return union.Bytes!;
    }

    public static Guid[] AsGuidArray(this byte[] input)
    {
        var union = new GuidUnion { Bytes = input };
        union.Bytes!.ToObjestsArray<Guid>(ArrayConverterBase.GUID_ARRAY_TYPE);
        return union.Objects!;
    }

    public static void AsByteArray(Guid[] input, out byte[] converted)
    {
        var union = new GuidUnion { Objects = input };
        union.Objects.ToBytesArray();
        converted = union.Bytes!;
    }

    public static void AsGuidArray(byte[] input, out Guid[] converted)
    {
        var union = new GuidUnion { Bytes = input };
        union.Bytes.ToObjestsArray<Guid>(ArrayConverterBase.GUID_ARRAY_TYPE);
        converted = union.Objects!;
    }
}
