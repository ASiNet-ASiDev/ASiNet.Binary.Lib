using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using ASiNet.Binary.Lib.Serializer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;

public delegate object DeserializeObjLambda(BinaryBuffer buffer, Encoding encoding);
public delegate void SerializeObjLambda(object obj, BinaryBuffer buffer, Encoding encoding);
public static class BinaryBufferSerializer
{
    private static Dictionary<string, (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize)> _buffer = new();

    /// <summary>
    /// Записывает обьект в <see cref="BinaryBuffer"/>, может игнорировать некоторые типы.
    /// </summary>
    /// <param name="obj">Обьект который будет записан.</param>
    /// <param name="buffer">Буффер в который будет записан обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="string"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns><see cref="true"/> если запись прошла успешна, иначе <see cref="false"/></returns>
    public static bool Serialize<T>(in T obj, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseSerialize(typeof(T), obj!, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    public static bool Serialize(Type type, in object obj, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseSerialize(type, obj, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }
    /// <summary>
    /// Читает обьект из <see cref="BinaryBuffer"/>, незаполняет проигнорированные поля, заменяет <see cref="null"/> на значение по умолчанию. 
    /// </summary>
    /// <param name="buffer">Буфер из которого будет читаться обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="string"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns>Обьект если операция прошла успешно, иначе <see cref="null"/></returns>
    public static T? Deserialize<T>(BinaryBuffer buffer, Encoding? encoding = null) where T : new()
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return (T?)BaseDeserialize(typeof(T), buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    public static object? Deserialize(Type type, BinaryBuffer buffer, Encoding? encoding = null)
    {
        try
        {
            encoding ??= Encoding.UTF8;

            return BaseDeserialize(type, buffer, encoding);
        }
        catch
        {
            throw;
        }
    }

    private static bool BaseSerialize(Type type, in object obj, BinaryBuffer buffer, Encoding encoding)
    {
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        lambda.Serialize(obj, buffer, encoding);
        return true;
    }

    private static object? BaseDeserialize(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        var result = lambda.Deserialize(buffer, encoding);
        return result;
    }

    public static (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize) GenerateLambdaFromTypeOrGetFromBuffer(Type type)
    {
        if(_buffer.TryGetValue(type.Name, out var value))
            return value;

        var deserialize = GenerateDeserializeLambda(type);
        var serialize = GenerateSerializeLambda(type);

        _buffer.TryAdd(type.Name, (deserialize, serialize));

        return (deserialize, serialize);
    }

    private static DeserializeObjLambda GenerateDeserializeLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        var inst = Expression.New(type);

        var binbufParameter = Expression.Parameter(bb);
        var encodingParameter = Expression.Parameter(typeof(Encoding));
        
        var binbufVar = Expression.Variable(bb);
        var instVar = Expression.Variable(type);
        var encodingVar = Expression.Variable(typeof(Encoding));


        var variables = new[] { instVar, binbufVar, encodingVar };
        var body = new List<Expression>();

        // VARIABLES
        body.AddRange(new[] 
        { 
            Expression.Assign(instVar, inst),
            Expression.Assign(binbufVar, binbufParameter),
            Expression.Assign(encodingVar, encodingParameter)
        });

        // SET_PRORERTIES
        body.AddRange(SetProperties(type, binbufVar, instVar, encodingVar));

        // RETURN
        body.Add(instVar);

        var block = Expression.Block(variables, body);

        var lambdaRaw = Expression.Lambda<DeserializeObjLambda>(block, binbufParameter, encodingParameter);
        var lambda = lambdaRaw.Compile();
        return lambda;
    }

    private static List<Expression> SetProperties(Type type, Expression binbuf, Expression inst, Expression encoding)
    {
        var result = new List<Expression>();

        var data = type.GetProperties().ToList();

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            var value = DeserializeToProperty(property.PropertyType, binbuf, inst, encoding);

            result.Add(Expression.IfThenElse(value.isNotNull,
                Expression.Assign(Expression.Property(inst, property.Name), value.value), 
                Expression.Assign(Expression.Property(inst, property.Name), value.defaultValue)));
        }

        return result;
    }

    private static (Expression isNotNull, Expression value, Expression defaultValue) DeserializeToProperty(Type propType, Expression binbuf, Expression inst, Expression encoding)
    {
        if (!propType.IsArray)
        {
            var isNotNull = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf);

            var (result, defaultValue) = propType.Name switch
            {
                nameof (SByte) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadSByte), null, binbuf), Expression.Constant(default(sbyte))),
                nameof (Byte) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadByte), null, binbuf), Expression.Constant(default(byte))),
                nameof (Single) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadSingle), null, binbuf), Expression.Constant(default(float))),
                nameof (Double) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadDouble), null, binbuf), Expression.Constant(default(double))),
                nameof (Boolean) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf), Expression.Constant(default(bool))),
                nameof (Int16) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt16), null, binbuf), Expression.Constant(default(short))),
                nameof (UInt16) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt16), null, binbuf), Expression.Constant(default(ushort))),
                nameof (Int32) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt32), null, binbuf), Expression.Constant(default(int))),
                nameof (UInt32) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt32), null, binbuf), Expression.Constant(default(uint))),
                nameof (Int64) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt64), null, binbuf), Expression.Constant(default(long))),
                nameof (UInt64) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt64), null, binbuf), Expression.Constant(default(ulong))),
                nameof (Char) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadChar), null, binbuf), Expression.Constant(default(char))),
                nameof (String) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadString), null, binbuf, encoding), Expression.Constant(default(string), typeof(string))),
                nameof (DateTime) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadDateTime), null, binbuf), Expression.Constant(default(DateTime))),
                nameof (Guid) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadGuid), null, binbuf), Expression.Constant(default(Guid))),
                _ => throw new NotImplementedException(),  
            };
            return (isNotNull, result, defaultValue);
        }
        else
        {
            var arrType = propType.GetElementType();
            throw new NotImplementedException();
        }
    }

    private static SerializeObjLambda GenerateSerializeLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        var binbufParameter = Expression.Parameter(bb);
        var encodingParameter = Expression.Parameter(typeof(Encoding));
        var instParameter = Expression.Parameter(typeof(object));


        var binbufVar = Expression.Variable(bb);
        var instVar = Expression.Variable(type);
        var encodingVar = Expression.Variable(typeof(Encoding));
        var isNull = Expression.Variable(typeof(bool));

        var variables = new[] { instVar, binbufVar, encodingVar, isNull };
        var body = new List<Expression>();

        // VARIABLES
        body.AddRange(new[]
        {
            Expression.Assign(instVar, Expression.TypeAs(instParameter, type)),
            Expression.Assign(binbufVar, binbufParameter),
            Expression.Assign(encodingVar, encodingParameter)
        });

        // SET_PRORERTIES
        body.AddRange(GetProperties(type, isNull, binbufVar, instVar, encodingVar));

        // RETURN
        body.Add(instVar);

        var block = Expression.Block(variables, body);

        var lambdaRaw = Expression.Lambda<SerializeObjLambda>(block, instParameter, binbufParameter, encodingParameter);
        var lambda = lambdaRaw.Compile();
        return lambda;
    }

    private static List<Expression> GetProperties(Type type, Expression isNull, Expression binbuf, Expression inst, Expression encoding)
    {
        var result = new List<Expression>();

        var data = type.GetProperties().ToList();

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            var prop = Expression.Property(inst, property.Name);
            var value = SerializeToBuffer(property.PropertyType, binbuf, prop, inst, encoding);
            
            result.Add(Expression.Assign(isNull, Expression.IsTrue(value.isNotNull)));

            result.Add(Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, isNull));
            result.Add(Expression.IfThen(isNull, value.method));
        }

        return result;
    }


    private static (Expression isNotNull, Expression method) SerializeToBuffer(Type propType, Expression binbuf, Expression property, Expression inst, Expression encoding)
    {
        if (!propType.IsArray)
        {
            var isNotNull = Expression.TypeIs(property, property.Type);

            var result = propType.Name switch
            {
                nameof(SByte) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Byte) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Single) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Double) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Boolean) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Int16) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(UInt16) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Int32) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(UInt32) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Int64) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(UInt64) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Char) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(String) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property, encoding),
                nameof(DateTime) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                nameof(Guid) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property),
                _ => throw new NotImplementedException(),
            };
            return (isNotNull, result);
        }
        else
        {
            var arrType = propType.GetElementType();
            throw new NotImplementedException();
        }
    }
}
