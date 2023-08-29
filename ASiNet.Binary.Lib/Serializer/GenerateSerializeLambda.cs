using ASiNet.Binary.Lib.Enums;
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
        

        if(type.IsEnum)
            body.Add(SerializeEnum(type, binbufVar, instVar, encodingVar, deep));
        else
            body.AddRange(SerializeObject(type, binbufVar, instVar, encodingVar, deep));

        var block = Expression.Block(variables, body);

        var lambdaRaw = Expression.Lambda<SerializeObjLambda>(block, instParameter, binbufParameter, encodingParameter, deepParameter);
        var lambda = lambdaRaw.Compile();
        return lambda;
    }

    private static List<Expression> SerializeObject(Type type, Expression binbuf, Expression inst, Expression encoding, Expression deep)
    {
        var data = type.GetProperties().ToList();

        var result = new List<Expression>(data.Count);

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            if(property.GetCustomAttribute<IgnorePropertyAttribute>() is not null)
                continue;
            if (!property.CanRead)
                throw new Exception($"Property '{property.Name}' does not have getter!");

            var pt = Helper.IsNullable(property.PropertyType) ? Nullable.GetUnderlyingType(property.PropertyType)! : property.PropertyType;

            if(pt == typeof(object))
                throw new NotImplementedException($"'{nameof(Object)}' Type is not supported!");
       
            if (pt.IsPrimitive)
                result.Add(SerializePrimitivesProperty(property, binbuf, inst, encoding, deep));
            else if (pt.IsEnum)
                result.Add(SerializeEnumProperty(property, binbuf, inst, encoding, deep));
            else if(pt.IsArray)
                result.Add(SerializeArray(property, binbuf, inst, encoding, deep));
            else if(pt.IsValueType)
                result.Add(SerializeValueType(property, binbuf, inst, encoding, deep));
            else
                result.Add(SerializeObject(property, binbuf, inst, encoding, deep));
        }

        return result;
    }

    internal static Expression SerializeEnum(
        Type type,
        Expression binbuf,
        Expression inst,
        Expression encoding,
        Expression deep)
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
                    Helper.CallBaseSerializeMethod(Expression.Constant(et),
                        Expression.PropertyOrField(inst, pi.Name),
                        binbuf,
                        encoding,
                        deep));   
        }
        else
        {
            var et = pi.PropertyType.GetEnumUnderlyingType()!;
            return Helper.WriteNotNullableObject(
                binbuf, 
                PropertyFlags.NotNullValue,
                Helper.CallBaseSerializeMethod(Expression.Constant(et),
                    Expression.PropertyOrField(inst, pi.Name),
                    binbuf,
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
            var pt = Nullable.GetUnderlyingType(pi.PropertyType);
            return Helper.WriteNullableValueTypes(
                    inst,
                    pi.Name,
                    binbuf,
                    Helper.CallBaseSerializeMethod(Expression.Constant(pt),
                        Expression.PropertyOrField(inst, pi.Name),
                        binbuf,
                        encoding,
                        deep));
        }
        else
        {
            var pt = pi.PropertyType;
            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                Helper.CallBaseSerializeMethod(Expression.Constant(pt),
                    Expression.PropertyOrField(inst, pi.Name),
                    binbuf,
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
                item => Helper.CallBaseSerializeMethod(
                    Expression.Constant(pi.PropertyType.GetElementType()!),
                    item,
                    binbuf,
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
            var pt = Nullable.GetUnderlyingType(pi.PropertyType);
            return Helper.WriteNullableValueTypes(
                    inst,
                    pi.Name,
                    binbuf,
                    Helper.CallBaseSerializeMethod(Expression.Constant(pt),
                        Expression.PropertyOrField(inst, pi.Name),
                        binbuf,
                        encoding,
                        deep));
        }
        else
        {
            var pt = pi.PropertyType;
            return Helper.WriteNotNullableObject(
                binbuf,
                PropertyFlags.NotNullValue,
                Helper.CallBaseSerializeMethod(Expression.Constant(pt),
                    Expression.PropertyOrField(inst, pi.Name),
                    binbuf,
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
                    Helper.CallBaseSerializeMethod(Expression.Constant(pi.PropertyType),
                        Expression.PropertyOrField(inst, pi.Name),
                        binbuf,
                        encoding,
                        deep));
    }
}