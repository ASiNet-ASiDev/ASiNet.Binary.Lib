using ASiNet.Binary.Lib.Exceptions;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System.Linq.Expressions;
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
            if(obj is null)
                return 0;
            var type = typeof(T);
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            var result = SerializeBaseTypes(type, obj, bb, encoding) ? true : BaseSerialize(type, obj!, bb, encoding, 0);

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

            var result = SerializeBaseTypes(type, obj, bb, encoding) ? true : BaseSerialize(type, obj, bb, encoding, 0);
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
            var type = typeof(T);
            encoding ??= Encoding.UTF8;

            var r = 0;
            var w = 0;
            Span<byte> buf = stackalloc byte[sizeof(decimal)];
            var bb = new BinaryBuffer(buffer, buf, ref w, ref r);

            var dbt = DeserializeBaseTypes(type, bb, encoding);

            if(dbt.IsPrimitiveType)
                return (T?)dbt.Obj;
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

            var dbt = DeserializeBaseTypes(type, bb, encoding);

            if (dbt.IsPrimitiveType)
                return dbt.Obj;
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

    private static bool SerializeBaseTypes(Type type, in object obj, BinaryBuffer buffer, Encoding encoding)
    {
        var result = false;
        if(type.IsPrimitive)
        {
            result = type.Name switch
            {
                nameof(SByte) => buffer.Write((sbyte)obj),
                nameof(Byte) => buffer.Write((byte)obj),
                nameof(Single) => buffer.Write((float)obj),
                nameof(Double) => buffer.Write((double)obj),
                nameof(Boolean) => buffer.Write((bool)obj),
                nameof(Int16) => buffer.Write((short)obj),
                nameof(UInt16) => buffer.Write((ushort)obj),
                nameof(Int32) => buffer.Write((int)obj),
                nameof(UInt32) => buffer.Write((uint)obj),
                nameof(Int64) => buffer.Write((long)obj),
                nameof(UInt64) => buffer.Write((ulong)obj),
                nameof(Char) => buffer.Write((char)obj),
                _ => false,
            };
        }
        else if(obj is string str)
            result = buffer.Write(str, encoding);
        else if (obj is DateTime dt)
            result = buffer.Write(dt);
        else if (obj is Guid guid)
            result = buffer.Write(guid);

        return result;
    }

    private static (object? Obj, bool IsPrimitiveType) DeserializeBaseTypes(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        object? result = null;
        var isPt = true;
        if (type.IsPrimitive)
        {
            result = type.Name switch
            {
                nameof(SByte) => buffer.ReadSByte(),
                nameof(Byte) => buffer.ReadByte(),
                nameof(Single) => buffer.ReadSingle(),
                nameof(Double) => buffer.ReadDouble(),
                nameof(Boolean) => buffer.ReadBoolean(),
                nameof(Int16) => buffer.ReadInt16(),
                nameof(UInt16) => buffer.ReadUInt16(),
                nameof(Int32) => buffer.ReadInt32(),
                nameof(UInt32) => buffer.ReadUInt32(),
                nameof(Int64) => buffer.ReadInt64(),
                nameof(UInt64) => buffer.ReadUInt64(),
                nameof(Char) => buffer.ReadChar(),
                _ => false,
            };
        }
        else if (type == typeof(string))
            result = buffer.ReadString(encoding);
        else if (type == typeof(DateTime))
            result = buffer.ReadDateTime();
        else if (type == typeof(Guid))
            result = buffer.ReadGuid();
        else
            isPt = false;

        return (result, isPt);
    }


}
