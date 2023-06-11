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
        encoding ??= Encoding.UTF8;

        return BaseSerialize(typeof(T), obj!, buffer, encoding);
    }

    public static bool Serialize(Type type, in object obj, BinaryBuffer buffer, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        return BaseSerialize(type, obj, buffer, encoding);
    }
    /// <summary>
    /// Читает обьект из <see cref="BinaryBuffer"/>, незаполняет проигнорированные поля, заменяет <see cref="null"/> на значение по умолчанию. 
    /// </summary>
    /// <param name="buffer">Буфер из которого будет читаться обьект.</param>
    /// <param name="encoding">Кодировка в которой будут кодироватся <see cref="string"/>, по умолчанию используется <see cref="Encoding.UTF8"/></param>
    /// <returns>Обьект если операция прошла успешно, иначе <see cref="null"/></returns>
    public static T? Deserialize<T>(BinaryBuffer buffer, Encoding? encoding = null) where T : new()
    {
        encoding ??= Encoding.UTF8;

        return (T?)BaseDeserialize(typeof(T), buffer, encoding);
    }

    public static object? Deserialize(Type type, BinaryBuffer buffer, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        return BaseDeserialize(type, buffer, encoding);
    }

    internal static bool BaseSerialize(Type type, in object obj, BinaryBuffer buffer, Encoding encoding)
    {
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        lambda.Serialize(obj, buffer, encoding);
        return true;
    }

    internal static object? BaseDeserialize(Type type, BinaryBuffer buffer, Encoding encoding)
    {
        var lambda = GenerateLambdaFromTypeOrGetFromBuffer(type);
        var result = lambda.Deserialize(buffer, encoding);
        return result;
    }

    public static (DeserializeObjLambda Deserialize, SerializeObjLambda Serialize) GenerateLambdaFromTypeOrGetFromBuffer(Type type)
    {
        if(_buffer.TryGetValue(type.FullName!, out var value))
            return value;

        var serialize = GenerateSerializeLambda(type);
        var deserialize = GenerateDeserializeLambda(type);

        _buffer.TryAdd(type.FullName!, (deserialize, serialize));

        return (deserialize, serialize);
    }

    private static DeserializeObjLambda GenerateDeserializeLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        NewExpression inst;
        if (type.IsGenericType)
        {
            var genericType = type.GetConstructors().FirstOrDefault()!;
            inst = Expression.New(genericType);
        }
        else
            inst = Expression.New(type);

        var binbufParameter = Expression.Parameter(bb);
        var encodingParameter = Expression.Parameter(typeof(Encoding));
        
        var binbufVar = Expression.Variable(bb, "binbuffVar");
        var instVar = Expression.Variable(type, "instanceVar");
        var encodingVar = Expression.Variable(typeof(Encoding), "encodingVar");


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
        body.Add(Expression.Convert(instVar, typeof(object)));

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
                Expression.Assign(Expression.Property(inst, property.Name), Expression.Convert(value.value, property.PropertyType)), 
                Expression.Assign(Expression.Property(inst, property.Name), property.PropertyType.IsNullable() ? Expression.Constant(null, property.PropertyType) : value.defaultValue)));
        }

        return result;
    }

    private static (Expression isNotNull, Expression value, Expression defaultValue) DeserializeToProperty(Type propType, Expression binbuf, Expression inst, Expression encoding)
    {
        var isNotNull = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf);
        if (!propType.IsArray)
        {
            if(propType.IsEnum)
            {
                var mi = typeof(BinaryBufferBaseTypes).GetMethod(nameof(BinaryBufferBaseTypes.ReadEnum))!.MakeGenericMethod(propType);

                var enumReader = Expression.Call(mi, binbuf);
                var def = Expression.Constant(Activator.CreateInstance(propType));
                return (isNotNull, enumReader, def);
            }
            else
            {
                var isNulablePrimitive = Helper.IsNullable(propType) && propType.GenericTypeArguments.First().IsPrimitive;
                var prop = isNulablePrimitive ? propType.GenericTypeArguments.First() : propType;
                var (result, defaultValue) = prop.Name switch
                {
                    nameof(SByte) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadSByte), null, binbuf), Expression.Constant(default(sbyte))),
                    nameof(Byte) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadByte), null, binbuf), Expression.Constant(default(byte))),
                    nameof(Single) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadSingle), null, binbuf), Expression.Constant(default(float))),
                    nameof(Double) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadDouble), null, binbuf), Expression.Constant(default(double))),
                    nameof(Boolean) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf), Expression.Constant(default(bool))),
                    nameof(Int16) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt16), null, binbuf), Expression.Constant(default(short))),
                    nameof(UInt16) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt16), null, binbuf), Expression.Constant(default(ushort))),
                    nameof(Int32) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt32), null, binbuf), Expression.Constant(default(int))),
                    nameof(UInt32) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt32), null, binbuf), Expression.Constant(default(uint))),
                    nameof(Int64) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt64), null, binbuf), Expression.Constant(default(long))),
                    nameof(UInt64) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadUInt64), null, binbuf), Expression.Constant(default(ulong))),
                    nameof(Char) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadChar), null, binbuf), Expression.Constant(default(char))),
                    nameof(String) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadString), null, binbuf, encoding), Expression.Constant(default(string), typeof(string))),
                    nameof(DateTime) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadDateTime), null, binbuf), Expression.Constant(default(DateTime))),
                    nameof(Guid) => (Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadGuid), null, binbuf), Expression.Constant(default(Guid))),
                    _ => (Expression.Call(typeof(BinaryBufferSerializer), nameof(BaseDeserialize), null, Expression.Constant(propType), binbuf, encoding), Expression.Constant(null, propType)),
                };
                return (isNotNull, result, defaultValue);
            }
        }
        else
        {
            var arrType = propType.GetElementType()!;
            if (arrType.IsEnum)
            {
                var mi = typeof(BinaryBufferArrays).GetMethod(nameof(BinaryBufferArrays.ReadEnumArray))!.MakeGenericMethod(arrType);

                var enumReader = Expression.Call(mi, binbuf);
                var def = Expression.Constant(Array.CreateInstance(arrType, 0));
                return (isNotNull, enumReader, def);
            }
            else
            {
                var (result, defaultValue) = arrType.Name switch
                {
                    nameof(SByte) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadSByteArray), null, binbuf), Expression.Constant(Array.Empty<sbyte>())),
                    nameof(Byte) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadByteArray), null, binbuf), Expression.Constant(Array.Empty<byte>())),
                    nameof(Single) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadSingleArray), null, binbuf), Expression.Constant(Array.Empty<float>())),
                    nameof(Double) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadDoubleArray), null, binbuf), Expression.Constant(Array.Empty<double>())),
                    nameof(Boolean) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadBooleanArray), null, binbuf), Expression.Constant(Array.Empty<bool>())),
                    nameof(Int16) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadInt16Array), null, binbuf), Expression.Constant(Array.Empty<short>())),
                    nameof(UInt16) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadUInt16Array), null, binbuf), Expression.Constant(Array.Empty<ushort>())),
                    nameof(Int32) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadInt32Array), null, binbuf), Expression.Constant(Array.Empty<int>())),
                    nameof(UInt32) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadUInt32Array), null, binbuf), Expression.Constant(Array.Empty<uint>())),
                    nameof(Int64) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadInt64Array), null, binbuf), Expression.Constant(Array.Empty<long>())),
                    nameof(UInt64) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadUInt64Array), null, binbuf), Expression.Constant(Array.Empty<ulong>())),
                    nameof(Char) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadCharArray), null, binbuf), Expression.Constant(Array.Empty<char>())),
                    nameof(String) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadStringArray), null, binbuf, encoding), Expression.Constant(Array.Empty<string>())),
                    nameof(DateTime) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadDateTimeArray), null, binbuf), Expression.Constant(Array.Empty<DateTime>())),
                    nameof(Guid) => (Expression.Call(typeof(BinaryBufferArrays), nameof(BinaryBufferArrays.ReadGuidArray), null, binbuf), Expression.Constant(Array.Empty<Guid>())),
                    _ => DeserializeObjectArray(propType, binbuf, encoding),
                };
                return (isNotNull, result, defaultValue);
            }
        }
    }

    private static SerializeObjLambda GenerateSerializeLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        var binbufParameter = Expression.Parameter(bb, "binbufParam");
        var encodingParameter = Expression.Parameter(typeof(Encoding), "encodingParam");
        var instParameter = Expression.Parameter(typeof(object), "instanceParam");


        var binbufVar = Expression.Variable(bb, "binbufVar");
        var instVar = Expression.Variable(type, "instanceVar");
        var encodingVar = Expression.Variable(typeof(Encoding), "encodingVar");
        var isNull = Expression.Variable(typeof(bool), "isNullVar");

        var variables = new[] { instVar, binbufVar, encodingVar, isNull };
        var body = new List<Expression>();

        // VARIABLES
        body.AddRange(new[]
        {
            Expression.Assign(instVar, Expression.Convert(instParameter, type)),
            Expression.Assign(binbufVar, binbufParameter),
            Expression.Assign(encodingVar, encodingParameter)
        });

        // SET_PRORERTIES
        body.AddRange(GetProperties(type, isNull, binbufVar, instVar, encodingVar));

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
            var value = SerializeToBuffer(binbuf, prop, inst, encoding);
            
            result.Add(Expression.Assign(isNull, Expression.IsTrue(value.isNotNull)));

            result.Add(Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, isNull));
            result.Add(Expression.IfThen(isNull, value.method));
        }

        return result;
    }


    private static (Expression isNotNull, Expression method) SerializeToBuffer(Expression binbuf, Expression property, Expression inst, Expression encoding)
    {
        var isNotNull = Expression.TypeIs(property, property.Type);
        if (!property.Type.IsArray)
        {
            if (property.Type.IsEnum)
            {
                var enumWriter = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, Expression.TypeAs(property, typeof(Enum)));
                return (isNotNull, enumWriter);
            }
            else
            {
                var isNulablePrimitive = Helper.IsNullable(property.Type) && property.Type.GenericTypeArguments.First().IsPrimitive;
                var prop = isNulablePrimitive ? property.Type.GenericTypeArguments.First() : property.Type;
                var getPrimitive = () => isNulablePrimitive ? Expression.Call(property, nameof(Nullable<byte>.GetValueOrDefault), null) : property;
                var result = prop.Name switch
                {
                    nameof(SByte) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Byte) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Single) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Double) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Boolean) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Int16) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(UInt16) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Int32) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(UInt32) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Int64) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(UInt64) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Char) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(String) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, property, encoding),
                    nameof(DateTime) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    nameof(Guid) => Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, getPrimitive()),
                    _ => Expression.Call(typeof(BinaryBufferSerializer), nameof(BaseSerialize), null, Expression.Constant(property.Type), property, binbuf, encoding),
                };
                return (isNotNull, result);
            }
        }
        else
        {
            var arrType = property.Type.GetElementType()!;
            if (arrType.IsEnum)
            {
                var mi = typeof(BinaryBufferArrays).GetMethod(nameof(BinaryBufferArrays.WriteEnumArray))!.MakeGenericMethod(arrType);

                var enumWriter = Expression.Call(mi, binbuf, property);
                return (isNotNull, enumWriter);
            }
            else
            {
                var mi = (property.Type == typeof(string[]) ? 
                typeof(BinaryBufferArrays).GetMethod(nameof(BinaryBufferArrays.WriteArray), new[] { binbuf.Type, encoding.Type, property.Type }) :
                typeof(BinaryBufferArrays).GetMethod(nameof(BinaryBufferArrays.WriteArray), new[] { binbuf.Type, property.Type }))!;
                var result = arrType.Name switch
                {
                    nameof(SByte) => Expression.Call(mi, binbuf, property),
                    nameof(Byte) => Expression.Call(mi, binbuf, property),
                    nameof(Single) => Expression.Call(mi, binbuf, property),
                    nameof(Double) => Expression.Call(mi, binbuf, property),
                    nameof(Boolean) => Expression.Call(mi, binbuf, property),
                    nameof(Int16) => Expression.Call(mi, binbuf, property),
                    nameof(UInt16) => Expression.Call(mi, binbuf, property),
                    nameof(Int32) => Expression.Call(mi, binbuf, property),
                    nameof(UInt32) => Expression.Call(mi, binbuf, property),
                    nameof(Int64) => Expression.Call(mi, binbuf, property),
                    nameof(UInt64) => Expression.Call(mi, binbuf, property),
                    nameof(Char) => Expression.Call(mi, binbuf, property),
                    nameof(String) => Expression.Call(mi, binbuf, encoding, property),
                    nameof(DateTime) => Expression.Call(mi, binbuf, property),
                    nameof(Guid) => Expression.Call(mi, binbuf, property),
                    _ => SerializeObjectArray(binbuf, property, encoding),
                };
                return (isNotNull, result);
            }
        }
    }

    internal static Expression SerializeObjectArray(Expression binbuf, Expression parameter, Expression encoding)
    {
        var i = Expression.Variable(typeof(int), "iVar");
        var max = Expression.Variable(typeof(int), "maxVar");

        var arr = Expression.Variable(parameter.Type, "arrVar");

        var elementType = parameter.Type.GetElementType()!;
        
        var writeSize = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, max);
        var body = Expression.Call(typeof(BinaryBufferSerializer), nameof(BaseSerialize), null, Expression.Constant(elementType), Expression.ArrayAccess(arr, i), binbuf, encoding);

        var result = Expression.Block(new[] { arr, i, max },
            Expression.Assign(arr, parameter),
            Expression.Assign(max, Expression.ArrayLength(arr)),
            writeSize,
            Expression.Assign(i, Expression.Constant(0)),
            Helper.For(body, i, max),
            arr
        );

        return result;
    }

    internal static (Expression value, Expression defaultValue) DeserializeObjectArray(Type paramType, Expression binbuf, Expression encoding)
    {
        var i = Expression.Variable(typeof(int), "iVar");
        var max = Expression.Variable(typeof(int), "maxVar");

        var arr = Expression.Variable(paramType);

        var elementType = paramType.GetElementType()!;

        var readSize = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt32), null, binbuf);
        var callToDeserializer = Expression.Call(typeof(BinaryBufferSerializer), nameof(BaseDeserialize), null, Expression.Constant(elementType), binbuf, encoding);

        var body = Expression.Assign(Expression.ArrayAccess(arr, i), Expression.Convert(callToDeserializer, elementType));

        var result = Expression.Block(new[] { i, max, arr },
            Expression.Assign(max, readSize),
            Expression.Assign(i, Expression.Constant(0)),
            Expression.Assign(arr, Expression.NewArrayBounds(elementType, max)),
            Helper.For(body, i, max),
            arr
        );

        return (result, Expression.NewArrayBounds(elementType, Expression.Constant(0)));
    }
}
