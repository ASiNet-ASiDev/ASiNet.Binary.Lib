using ASiNet.Binary.Lib.Exceptions;
using System.Text;

namespace ASiNet.Binary.Lib.Serializer;

public delegate object DeserializeObjLambda(BinaryBuffer buffer, Encoding encoding);
public delegate void SerializeObjLambda(object obj, BinaryBuffer buffer, Encoding encoding, ushort deep);
public static class BinarySerializer
{
    private static Dictionary<string, (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize)> _buffer = new();

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
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            var result = BaseSerialize(typeof(T), obj!, bb, encoding, 0);

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
    public static T? Deserialize<T>(Span<byte> buffer, Encoding? encoding = null) where T : new()
    {
        try
        {
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            return (T?)BaseDeserialize(typeof(T), bb, encoding);
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

    private static (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize) GenerateLambdaFromTypeOrGetFromBuffer(Type type)
    {
        if (_buffer.TryGetValue(type.FullName!, out var value))
            return value;

        var serialize = GenerateSerializeLambda.GenerateLambda(type);
        var deserialize = GenerateDeserializeLambda.GenerateLambda(type);

        _buffer.TryAdd(type.FullName!, (deserialize, serialize));

        return (deserialize, serialize);
    }
}
