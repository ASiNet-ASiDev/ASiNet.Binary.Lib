using System.Text;
using ASiNet.Binary.Lib.Exceptions;
using ASiNet.Binary.Lib.Expressions.BaseTypes;

namespace ASiNet.Binary.Lib.Serializer;

public delegate object DeserializeObjLambda(BinaryBuffer buffer, Encoding encoding);
public delegate void SerializeObjLambda(object obj, BinaryBuffer buffer, Encoding encoding, ushort deep);
public static class BinarySerializer
{

    static BinarySerializer()
    {
        foreach (var item in GenerateBaseTypesSD())
        {
            _buffer.Add(item.TypeName, (item.Deserialize, item.Serialize));
        }
    }

    private static Dictionary<string, (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize)> _buffer = new();


    internal static List<Type> _generationQueue = new();

    public static ushort MaxSerializeDepth { get; set; } = 16;

    /// <summary>
    /// Serializes the object to the specified buffer.
    /// </summary>
    /// <typeparam name="T">Serializable object</typeparam>
    /// <param name="obj">Serializable object</param>
    /// <param name="buffer">The buffer into which the objects will be serialized</param>
    /// <param name="encoding">Character encoding used, by default <see cref="Encoding.UTF8"/></param>
    /// <returns>The number of bytes written, or -1 if failed</returns>
    /// <exception cref="SerializeException"></exception>
    public static int Serialize<T>(in T obj, Span<byte> buffer, Encoding? encoding = null)
    {
        try
        {
            if (obj is null)
                return 0;
            var type = typeof(T);
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            var result = BaseSerialize(type, obj!, bb, encoding, 0);

            if (result)
            {
                return bb.WritePosition;
            }
            else
            {
                buffer.Clear();
                return -1;
            }
        }
        catch (Exception ex)
        {
            buffer.Clear();
            throw new SerializeException("Serialize Exception, check 'InnerException' to get more info.", ex);
        }
    }
    /// <summary>
    /// Serializes the object to the specified buffer.
    /// </summary>
    /// <param name="type">Serializable object type</param>
    /// <param name="obj">Serializable object</param>
    /// <param name="buffer">The buffer into which the objects will be serialized</param>
    /// <param name="encoding">Character encoding used, by default <see cref="Encoding.UTF8"/></param>
    /// <returns>The number of bytes written, or -1 if failed</returns>
    /// <exception cref="SerializeException"></exception>
    public static int Serialize(Type type, in object obj, Span<byte> buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            var result = BaseSerialize(type, obj, bb, encoding, 0);
            if (result)
            {
                return bb.WritePosition;
            }
            else
            {
                buffer.Clear();
                return -1;
            }
        }
        catch (Exception ex)
        {
            buffer.Clear();
            throw new SerializeException("Serialize Error, check 'InnerException' to get more info.", ex);
        }
    }
    /// <summary>
    /// Deserializes an object from the specified buffer.
    /// </summary>
    /// <typeparam name="T">Expected object.</typeparam>
    /// <param name="buffer">The buffer from which the object will be deserialized.</param>
    /// <param name="encoding">Character encoding used, by default <see cref="Encoding.UTF8"/>.</param>
    /// <returns>Deserialized object, or nothing if deserialized object failed.</returns>
    /// <exception cref="DeserializeException"></exception>
    public static T? Deserialize<T>(Span<byte> buffer, Encoding? encoding = null)
    {
        try
        {
            var type = typeof(T);
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            return (T?)BaseDeserialize(type, bb, encoding);
        }
        catch (Exception ex)
        {

            throw new DeserializeException("Deserialize Error, check 'InnerException' to get more info.", ex);
        }
    }
    /// <summary>
    /// Deserializes an object from the specified buffer.
    /// </summary>
    /// <param name="type">Type of deserializable object.</param>
    /// <param name="buffer">The buffer from which the object will be deserialized.</param>
    /// <param name="encoding">Character encoding used, by default <see cref="Encoding.UTF8"/>.</param>
    /// <returns>Deserialized object, or nothing if deserialized object failed.</returns>
    /// <exception cref="DeserializeException"></exception>
    public static object? Deserialize(Type type, Span<byte> buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            return BaseDeserialize(type, bb, encoding);
        }
        catch (Exception ex)
        {
            throw new DeserializeException("Deserialize Error, check 'InnerException' to get more info.", ex);
        }
    }

    internal static bool BaseSerialize(Type type, in object obj, BinaryBuffer buffer, Encoding encoding, ushort deep)
    {
        deep++;
        if (deep > MaxSerializeDepth)
            throw new Exception("Превышена максимальная глубина сериализации!");
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        lambda.Serialize(obj, buffer, encoding, deep);
        return true;
    }

    internal static object? BaseDeserialize(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        var result = lambda.Deserialize(buffer, encoding);
        return result;
    }

    internal static (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize) GenerateLambdaFromTypeOrGetFromBuffer(Type type)
    {
        if (_buffer.TryGetValue(type.FullName!, out var value))
            return value;

        var serialize = GenerateSerializeLambda.GenerateLambda(type);
        var deserialize = GenerateDeserializeLambda.GenerateLambda(type);

        _buffer.TryAdd(type.FullName!, (deserialize, serialize));

        return (deserialize, serialize);
    }

    private static IEnumerable<(string TypeName, DeserializeObjLambda Deserialize, SerializeObjLambda Serialize)> GenerateBaseTypesSD()
    {
        yield return (typeof(sbyte).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadSByte(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((sbyte)obj));

        yield return (typeof(byte).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadByte(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((byte)obj));

        yield return (typeof(float).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadSingle(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((float)obj));

        yield return (typeof(double).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadDouble(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((double)obj));

        yield return (typeof(bool).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadBoolean(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((bool)obj));

        yield return (typeof(short).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadInt16(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((short)obj));

        yield return (typeof(ushort).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadUInt16(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((ushort)obj));

        yield return (typeof(int).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadInt32(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((int)obj));

        yield return (typeof(uint).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadUInt32(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((uint)obj));

        yield return (typeof(long).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadInt64(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((long)obj));

        yield return (typeof(ulong).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadUInt64(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((ulong)obj));

        yield return (typeof(char).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadChar(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((char)obj));

        yield return (typeof(string).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadString(encoding),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((string)obj, encoding));

        yield return (typeof(DateTime).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadDateTime(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((DateTime)obj));

        yield return (typeof(Guid).FullName!,
            (BinaryBuffer buffer, Encoding encoding) => buffer.ReadGuid(),
            (object obj, BinaryBuffer buffer, Encoding encoding, ushort deep) => buffer.Write((Guid)obj));


    }
}
