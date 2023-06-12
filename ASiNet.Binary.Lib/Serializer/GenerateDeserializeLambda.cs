using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
internal class GenerateDeserializeLambda
{
    public static DeserializeObjLambda GenerateLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        NewExpression inst = Expression.New(type);

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
        body.AddRange(DeserializeProperties(type, binbufVar, instVar, encodingVar));

        // RETURN
        body.Add(Expression.Convert(instVar, typeof(object)));

        var block = Expression.Block(variables, body);

        var lambdaRaw = Expression.Lambda<DeserializeObjLambda>(block, binbufParameter, encodingParameter);
        var lambda = lambdaRaw.Compile();
        return lambda;
    }

    private static List<Expression> DeserializeProperties(Type type, Expression binbuf, Expression inst, Expression encoding)
    {
        var result = new List<Expression>();

        var data = type.GetProperties().ToList();

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            if (!property.CanWrite)
                throw new Exception($"Property {property.Name} does not have setter!");
            var value = property.PropertyType.IsArray ?
                DeserializeArray(property.PropertyType, binbuf, inst, encoding) :
                DeserializeProperty(property.PropertyType, binbuf, inst, encoding);

            result.Add(Expression.IfThenElse(value.isNotNull,
                Expression.Assign(Expression.Property(inst, property.Name), Expression.Convert(value.value, property.PropertyType)),
                Expression.Assign(Expression.Property(inst, property.Name), property.PropertyType.IsNullable() ? Expression.Constant(null, property.PropertyType) : value.defaultValue)));
        }

        return result;
    }

    private static (Expression isNotNull, Expression value, Expression defaultValue) DeserializeProperty(Type propType, Expression binbuf, Expression inst, Expression encoding)
    {
        var isNotNull = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf);
        if (propType.IsEnum)
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
                _ => (Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseDeserialize), null, Expression.Constant(propType), binbuf, encoding), Expression.Constant(null, propType)),
            };
            return (isNotNull, result, defaultValue);
        }
    }

    private static (Expression isNotNull, Expression value, Expression defaultValue) DeserializeArray(Type propType, Expression binbuf, Expression inst, Expression encoding)
    {
        var isNotNull = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadBoolean), null, binbuf);
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

    internal static (Expression value, Expression defaultValue) DeserializeObjectArray(Type paramType, Expression binbuf, Expression encoding)
    {
        var i = Expression.Variable(typeof(int), "iVar");
        var max = Expression.Variable(typeof(int), "maxVar");

        var arr = Expression.Variable(paramType);

        var elementType = paramType.GetElementType()!;

        var readSize = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadInt32), null, binbuf);
        var callToDeserializer = Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseDeserialize), null, Expression.Constant(elementType), binbuf, encoding);

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
