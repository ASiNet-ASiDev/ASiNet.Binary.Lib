using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ASiNet.Binary.Lib.Enums;
using ASiNet.Binary.Lib.Serializer.Attributes;

namespace ASiNet.Binary.Lib.Serializer;
internal static class GenerateSerializeLambda
{
    public static SerializeObjLambda GenerateLambda(Type type)
    {
        try
        {
            BinarySerializer._generationQueue.Add(type);
            var bb = typeof(BinaryBuffer);

            var binbufParameter = Expression.Parameter(bb, "binbufParam");
            var encodingParameter = Expression.Parameter(typeof(Encoding), "encodingParam");
            var instParameter = Expression.Parameter(typeof(object), "instanceParam");
            var deepParameter = Expression.Parameter(typeof(ushort), "deepParameter");

            var binbufVar = Expression.Variable(bb, "binbufVar");
            var instVar = Expression.Variable(type, "instanceVar");
            var encodingVar = Expression.Variable(typeof(Encoding), "encodingVar");
            var deep = Expression.Variable(typeof(ushort), "deep");

            var variables = new[] { instVar, binbufVar, encodingVar, deep };
            var body = new List<Expression>();

            // VARIABLES
            body.AddRange(new[]
            {
            Expression.Assign(instVar, Expression.Convert(instParameter, type)),
            Expression.Assign(binbufVar, binbufParameter),
            Expression.Assign(encodingVar, encodingParameter),
            Expression.Assign(deep, deepParameter),
        });


            if (type.IsEnum)
                body.Add(SerializeEnum(type, binbufVar, instVar));
            else
                body.AddRange(SerializeObject(type, binbufVar, instVar, encodingVar, deep));

            var block = Expression.Block(variables, body);

            var lambdaRaw = Expression.Lambda<SerializeObjLambda>(block, instParameter, binbufParameter, encodingParameter, deepParameter);
            var lambda = lambdaRaw.Compile();

            return lambda;
        }
        finally
        {
            BinarySerializer._generationQueue.Remove(type);
        }
    }

    private static List<Expression> SerializeObject(Type type, Expression binbuf, Expression inst, Expression encoding, Expression deep)
    {
        var data = type.GetProperties().Where(x => x.GetCustomAttribute<IgnorePropertyAttribute>() is null).ToList();

        var result = new List<Expression>(data.Count);

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            if (!property.CanRead)
                throw new Exception($"Property '{property.Name}' does not have getter!");

            var pt = Helper.IsNullable(property.PropertyType) ? Nullable.GetUnderlyingType(property.PropertyType)! : property.PropertyType;

            if (pt == typeof(object))
                throw new NotImplementedException($"'{nameof(Object)}' Type is not supported!");

            if (pt.IsPrimitive)
                result.Add(SerializePrimitivesProperty(property, binbuf, inst, encoding, deep));
            else if (pt.IsEnum)
                result.Add(SerializeEnumProperty(property, binbuf, inst, encoding, deep));
            else if (pt.IsArray)
                result.Add(SerializeArray(property, binbuf, inst, encoding, deep));
            else if (pt.IsValueType)
                result.Add(SerializeValueType(property, binbuf, inst, encoding, deep));
            else
                result.Add(SerializeObject(property, binbuf, inst, encoding, deep));
        }

        return result;
    }

    internal static Expression SerializeEnum(
        Type type,
        Expression binbuf,
        Expression inst)
    {
        if (Helper.IsNullable(type))
        {
            var et = Nullable.GetUnderlyingType(type)!.GetEnumUnderlyingType()!;
            return Helper.WriteNullableValueTypes(
                    inst,
                    binbuf,
                    Helper.CallWriteMethod(
                        Expression.Convert(inst, et),
                        binbuf));
        }
        else
        {
            var et = type.GetEnumUnderlyingType()!;
            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                Helper.CallWriteMethod(
                    Expression.Convert(inst, et),
                    binbuf));
        }
    }

    internal static Expression SerializeEnumProperty(
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        if (Helper.IsNullable(pi.PropertyType))
        {
            var et = Nullable.GetUnderlyingType(pi.PropertyType)!.GetEnumUnderlyingType()!;

            return Helper.WriteNullableValueTypes(
                    inst,
                    pi.Name,
                    binbuf,
                    GetLambdaOrUseRuntime(
                        et,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
        else
        {
            var et = pi.PropertyType.GetEnumUnderlyingType()!;
            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                GetLambdaOrUseRuntime(
                        et,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
    }

    internal static Expression SerializePrimitivesProperty(
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        if (Helper.IsNullable(pi.PropertyType))
        {
            var pt = Nullable.GetUnderlyingType(pi.PropertyType)!;

            return Helper.WriteNullableValueTypes(
                    inst,
                    pi.Name,
                    binbuf,
                    GetLambdaOrUseRuntime(
                        pt,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
        else
        {
            var pt = pi.PropertyType;

            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                GetLambdaOrUseRuntime(
                        pt,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
    }

    internal static Expression SerializeArray(
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        return Helper.WriteArray(
            inst,
            pi.Name,
            binbuf,
            Helper.ForeachGetArray(
                Expression.PropertyOrField(inst, pi.Name),
                item => GetLambdaOrUseRuntimeArrays(
                    item,
                    pi.PropertyType.GetElementType()!,
                    pi,
                    binbuf,
                    inst,
                    encoding,
                    deep)));
    }

    internal static Expression SerializeValueType(
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        if (Helper.IsNullable(pi.PropertyType))
        {
            var pt = Nullable.GetUnderlyingType(pi.PropertyType)!;
            return Helper.WriteNullableValueTypes(
                    inst,
                    pi.Name,
                    binbuf,
                    GetLambdaOrUseRuntime(
                        pt,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
        else
        {
            var pt = pi.PropertyType;
            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                GetLambdaOrUseRuntime(
                        pt,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
        }
    }

    internal static Expression SerializeObject(
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        return Helper.WriteObject(
                    inst,
                    pi.Name,
                    binbuf,
                    GetLambdaOrUseRuntime(
                        pi.PropertyType,
                        pi,
                        binbuf,
                        inst,
                        encoding,
                        deep));
    }

    private static Expression GetLambdaOrUseRuntime(
        Type propType,
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        if (BinarySerializer._generationQueue.FirstOrDefault(x => x == propType) is null)
        {
            var lambda = BinarySerializer.GenerateLambdaFromTypeOrGetFromBuffer(propType).Serialize;
            return Helper.CallSerializeLambda(
                lambda,
                Expression.PropertyOrField(inst, pi.Name),
                binbuf,
                encoding,
                deep);
        }
        else
        {
            return Helper.CallBaseSerializeMethod(
                Expression.Constant(propType),
                Expression.PropertyOrField(inst, pi.Name),
                binbuf,
                encoding,
                deep);
        }
    }

    private static Expression GetLambdaOrUseRuntimeArrays(
        Expression item,
        Type propType,
        PropertyInfo pi,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
    {
        if (BinarySerializer._generationQueue.FirstOrDefault(x => x == propType) is null)
        {
            var lambda = BinarySerializer.GenerateLambdaFromTypeOrGetFromBuffer(propType).Serialize;
            return Helper.CallSerializeLambda(
                lambda,
                item,
                binbuf,
                encoding,
                deep);
        }
        else
        {
            return Helper.CallBaseSerializeMethod(
                Expression.Constant(propType),
                item,
                binbuf,
                encoding,
                deep);
        }
    }
}