using System.Runtime.InteropServices;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;

[StructLayout(LayoutKind.Explicit)]
internal struct SByteUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public sbyte[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct BooleanUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public bool[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct DoubleUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public double[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct SingleUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public float[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct Int16Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public short[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct UInt16Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public ushort[] Objects;
}


[StructLayout(LayoutKind.Explicit)]
internal struct Int32Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public int[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct UInt32Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public uint[] Objects;
}


[StructLayout(LayoutKind.Explicit)]
internal struct Int64Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public long[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct UInt64Union
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public ulong[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct CharUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public char[] Objects;
}

[StructLayout(LayoutKind.Explicit)]
internal struct DateTimeUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public DateTime[] Objects;
}


[StructLayout(LayoutKind.Explicit)]
internal struct GuidUnion
{
    [FieldOffset(0)] public byte[] Bytes;
    [FieldOffset(0)] public Guid[] Objects;
}