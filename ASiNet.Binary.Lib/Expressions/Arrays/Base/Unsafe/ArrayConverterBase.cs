using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe;
internal static unsafe class ArrayConverterBase
{
    static ArrayConverterBase()
    {
        fixed (void* pType = new byte[1])
            BYTE_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new sbyte[1])
            SBYTE_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new bool[1])
            BOOLEAN_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new float[1])
            FLOAT_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new double[1])
            DOUBLE_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new short[1])
            INT16_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new ushort[1])
            UINT16_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new int[1])
            INT32_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new uint[1])
            UINT32_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new long[1])
            INT64_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new ulong[1])
            UINT64_ARRAY_TYPE = GetHeader(pType)->type;

        fixed (void* pType = new char[1])
            CHAR_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new Guid[1])
            GUID_ARRAY_TYPE = GetHeader(pType)->type;
        fixed (void* pType = new DateTime[1])
            DATETIME_ARRAY_TYPE = GetHeader(pType)->type;

    }

    public static readonly UIntPtr BYTE_ARRAY_TYPE;
    public static readonly UIntPtr SBYTE_ARRAY_TYPE;

    public static readonly UIntPtr BOOLEAN_ARRAY_TYPE;

    public static readonly UIntPtr FLOAT_ARRAY_TYPE;
    public static readonly UIntPtr DOUBLE_ARRAY_TYPE;

    public static readonly UIntPtr INT16_ARRAY_TYPE;
    public static readonly UIntPtr UINT16_ARRAY_TYPE;

    public static readonly UIntPtr INT32_ARRAY_TYPE;
    public static readonly UIntPtr UINT32_ARRAY_TYPE;

    public static readonly UIntPtr INT64_ARRAY_TYPE;
    public static readonly UIntPtr UINT64_ARRAY_TYPE;

    public static readonly UIntPtr CHAR_ARRAY_TYPE;
    public static readonly UIntPtr GUID_ARRAY_TYPE;
    public static readonly UIntPtr DATETIME_ARRAY_TYPE;

    public static ArrayHeader* GetHeader(void* pBytes)
    {
        return (ArrayHeader*)pBytes - 1;
    }

    public static void ToObjestsArray<T>(this byte[] bytes, UIntPtr type) where T : unmanaged
    {
        fixed (void* pArray = bytes)
        {
            var pHeader = GetHeader(pArray);

            pHeader->type = type;
            pHeader->length = (UIntPtr)(bytes.Length / sizeof(T));
        }
    }

    public static void ToBytesArray<T>(this T[] floats) where T : unmanaged
    {
        fixed (void* pArray = floats)
        {
            var pHeader = GetHeader(pArray);

            pHeader->type = BYTE_ARRAY_TYPE;
            pHeader->length = (UIntPtr)(floats.Length * sizeof(T));
        }
    }
}
