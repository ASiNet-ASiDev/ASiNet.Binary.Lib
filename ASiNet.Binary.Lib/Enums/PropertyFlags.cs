namespace ASiNet.Binary.Lib.Enums;
[Flags]
public enum PropertyFlags : byte
{
    None = 0,
    NotNullValue = 1 << 0,
}
