namespace ASiNet.Binary.Lib.Serializer;
public enum SerializeFlags : byte
{
    None = 1 << 0,
    NullValue = 1 << 1,
}
