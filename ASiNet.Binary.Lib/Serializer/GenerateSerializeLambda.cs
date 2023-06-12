using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
internal static class GenerateSerializeLambda
{
    public static SerializeObjLambda GenerateLambda(Type type)
    {
        var bb = typeof(BinaryBuffer);

        var binbufParameter = Expression.Parameter(bb, "binbufParam");
        var encodingParameter = Expression.Parameter(typeof(Encoding), "encodingParam");
        var instParameter = Expression.Parameter(typeof(object), "instanceParam");
        var deepParameter = Expression.Parameter(typeof(ushort), "deepParameter");

        var binbufVar = Expression.Variable(bb, "binbufVar");
        var instVar = Expression.Variable(type, "instanceVar");
        var encodingVar = Expression.Variable(typeof(Encoding), "encodingVar");
        var isNull = Expression.Variable(typeof(bool), "isNullVar");
        var deep = Expression.Variable(typeof(ushort), "deep");

        var variables = new[] { instVar, binbufVar, encodingVar, isNull, deep };
        var body = new List<Expression>();

        // VARIABLES
        body.AddRange(new[]
        {
            Expression.Assign(instVar, Expression.Convert(instParameter, type)),
            Expression.Assign(binbufVar, binbufParameter),
            Expression.Assign(encodingVar, encodingParameter),
            Expression.Assign(deep, deepParameter),
        });

        // SET_PRORERTIES
        body.AddRange(SerializeProperties(type, isNull, binbufVar, instVar, encodingVar, deep));

        var block = Expression.Block(variables, body);

        var lambdaRaw = Expression.Lambda<SerializeObjLambda>(block, instParameter, binbufParameter, encodingParameter, deepParameter);
        var lambda = lambdaRaw.Compile();
        return lambda;
    }

    private static List<Expression> SerializeProperties(Type type, Expression isNull, Expression binbuf, Expression inst, Expression encoding, Expression deep)
    {
        var result = new List<Expression>();

        var data = type.GetProperties().ToList();

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            if (!property.CanRead)
                throw new Exception($"Property {property.Name} does not have getter!");

            var prop = Expression.Property(inst, property.Name);
            var value = property.PropertyType.IsArray ?
                SerializeArray(binbuf, prop, inst, encoding, deep) :
                SerializeProperty(binbuf, prop, inst, encoding, deep);

            result.Add(Expression.Assign(isNull, Expression.IsTrue(value.isNotNull)));

            result.Add(Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, isNull));
            result.Add(Expression.IfThen(isNull, value.method));
        }

        return result;
    }


    private static (Expression isNotNull, Expression method) SerializeProperty(Expression binbuf, Expression property, Expression inst, Expression encoding, Expression deep)
    {
        var isNotNull = Expression.TypeIs(property, property.Type);
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
                _ => Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseSerialize), null, Expression.Constant(property.Type), property, binbuf, encoding, deep),
            };
            return (isNotNull, result);
        }
    }

    private static (Expression isNotNull, Expression method) SerializeArray(Expression binbuf, Expression property, Expression inst, Expression encoding, Expression deep)
    {
        var isNotNull = Expression.TypeIs(property, property.Type);
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
                _ => SerializeObjectArray(binbuf, property, encoding, deep),
            };
            return (isNotNull, result);
        }
    }

    internal static Expression SerializeObjectArray(Expression binbuf, Expression parameter, Expression encoding, Expression deep)
    {
        var i = Expression.Variable(typeof(int), "iVar");
        var max = Expression.Variable(typeof(int), "maxVar");

        var arr = Expression.Variable(parameter.Type, "arrVar");

        var elementType = parameter.Type.GetElementType()!;

        var writeSize = Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, max);
        var body = Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseSerialize), null, Expression.Constant(elementType), Expression.ArrayAccess(arr, i), binbuf, encoding, deep);

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

}