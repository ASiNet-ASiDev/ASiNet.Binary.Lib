using System.Runtime.InteropServices;

namespace ASiNet.Binary.Lib.Expressions.Arrays.Base.Unsafe.Domain;
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ArrayHeader
{
    public UIntPtr type;
    public UIntPtr length;
}