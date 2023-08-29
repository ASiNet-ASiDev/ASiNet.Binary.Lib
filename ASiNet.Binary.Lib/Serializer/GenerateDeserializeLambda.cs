using ASiNet.Binary.Lib.Enums;
using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
internal class GenerateDeserializeLambda
{
    public static DeserializeObjLambda GenerateLambda(Type type)
    {
        try
        {
            BinarySerializer._generationQueue.Add(type);

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
            if (type.IsEnum)
                body.Add(DeserializeEnum(type, binbufVar, instVar, encodingVar));
            else
                body.AddRange(DeserializeProperties(type, binbufVar, instVar, encodingVar));

            // RETURN
            body.Add(Expression.Convert(instVar, typeof(object)));

            var block = Expression.Block(variables, body);

            var lambdaRaw = Expression.Lambda<DeserializeObjLambda>(block, binbufParameter, encodingParameter);
            var lambda = lambdaRaw.Compile();
            return lambda;
        }
        finally
        {
            BinarySerializer._generationQueue.Remove(type);
        }
    }

    private static List<Expression> DeserializeProperties(Type type, Expression binbuf, Expression inst, Expression encoding)
    {
        var result = new List<Expression>();

        var data = type.GetProperties().ToList();

        data.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

        foreach (var property in data)
        {
            if (!property.CanWrite)
                throw new Exception($"Property '{property.Name}' does not have setter!");
            var enumInst = Expression.Variable(typeof(PropertyFlags), $"{property.Name}_flag");

            var pt = Helper.IsNullable(property.PropertyType) ? Nullable.GetUnderlyingType(property.PropertyType)! : property.PropertyType;

            if (pt == typeof(object))
                throw new NotImplementedException($"'{nameof(Object)}' Type is not supported!");


            if (pt.IsPrimitive)
                result.Add(DeserializePrimitivesProperty(pt, property, enumInst, binbuf, inst, encoding));

            else if (pt.IsEnum)
                result.Add(DeserializeEnumProperty(pt, property, enumInst, binbuf, inst, encoding));

            else if(pt.IsArray)
                result.Add(DeserializeArrayProperty(pt, property, enumInst, binbuf, inst, encoding));
            else
                result.Add(DeserializeObjectProperty(pt, property, enumInst, binbuf, inst, encoding));
        }

        return result;
    }

    private static Expression DeserializeEnum(
        Type type, 
        ParameterExpression binbuf, 
        ParameterExpression inst, 
        ParameterExpression encoding)
    {
        var ut = type.GetEnumUnderlyingType()!;
        var enumInstanse = Expression.Variable(typeof(PropertyFlags), $"{type.Name}_flag");
        return Expression.Block(new[] { enumInstanse },
                Expression.Assign(
                    enumInstanse,
                    Helper.CallReadFlagsMethod(binbuf)),
                Helper.IfHashFlag(
                    enumInstanse,
                    Expression.Constant(PropertyFlags.NotNullValue),
                    Expression.Assign(
                        inst,
                        Expression.Convert(
                            Expression.Convert(
                                GetLambdaOrUseRuntime(
                                        ut,
                                        binbuf,
                                            encoding), ut), 
                            type)
                        )
                    )
                );
    }

    internal static Expression DeserializeEnumProperty(
        Type propType, 
        PropertyInfo pi, 
        ParameterExpression enumInstanse, 
        Expression binbuf, 
        Expression inst, 
        Expression encoding)
    {
        var ut = propType.GetEnumUnderlyingType()!;
        return Expression.Block(new[] { enumInstanse },
                Expression.Assign(
                    enumInstanse, 
                    Helper.CallReadFlagsMethod(binbuf)),
                Helper.IfHashFlag(
                    enumInstanse, 
                    Expression.Constant(PropertyFlags.NotNullValue), 
                    Expression.Assign(
                        Expression.PropertyOrField(inst, pi.Name),
                        Expression.Convert(
                            Expression.Convert(
                                GetLambdaOrUseRuntime(
                                    ut, 
                                    binbuf, 
                                    encoding), 
                                propType), 
                            pi.PropertyType)
                        )
                    )
                );
    }

    internal static Expression DeserializePrimitivesProperty(
        Type propType, 
        PropertyInfo pi, 
        ParameterExpression enumInstanse, 
        Expression binbuf, 
        Expression inst, 
        Expression encoding)
    {
        return Expression.Block(new[] { enumInstanse },
                Expression.Assign(
                    enumInstanse, 
                    Helper.CallReadFlagsMethod(binbuf)),
                Helper.IfHashFlag(
                    enumInstanse, 
                    Expression.Constant(
                        PropertyFlags.NotNullValue),
                    Expression.Assign(
                        Expression.PropertyOrField(inst, pi.Name),
                        Expression.Convert(
                            GetLambdaOrUseRuntime(
                                propType, 
                                binbuf, 
                                encoding), 
                            pi.PropertyType)
                        )
                    )
                );
    }

    internal static Expression DeserializeArrayProperty(
        Type propType,
        PropertyInfo pi,
        ParameterExpression enumInstanse,
        Expression binbuf,
        Expression inst,
        Expression encoding)
    {
        var array = Expression.Variable(propType);

        var et = propType.GetElementType()!;

        return Expression.Block(new[] { enumInstanse },

            Expression.Assign(
                enumInstanse,
                Helper.CallReadFlagsMethod(binbuf)),

            Helper.IfHashFlag(
                enumInstanse,
                Expression.Constant(
                    PropertyFlags.NotNullValue),

                Expression.Block(new[] { array },
                    Expression.Assign(
                        array,
                        Expression.NewArrayBounds(
                            et,
                            Helper.CallReadMethod(
                                binbuf,
                                nameof(BinaryBufferBaseTypes.ReadInt32)
                                )
                            )
                        ),

                    Helper.ForeachSetArray(
                        array, 
                        Expression.Convert(
                            GetLambdaOrUseRuntime(
                                et,
                                binbuf, 
                                encoding),
                            et)
                        ),

                    Expression.Assign(
                        Expression.PropertyOrField(inst, pi.Name), array)
                    )
                )
            );
    }

    internal static Expression DeserializeObjectProperty(
        Type propType,
        PropertyInfo pi,
        ParameterExpression enumInstanse,
        Expression binbuf,
        Expression inst,
        Expression encoding)
    {
        return Expression.Block(new[] { enumInstanse },
                Expression.Assign(
                    enumInstanse,
                    Helper.CallReadFlagsMethod(binbuf)),
                Helper.IfHashFlag(
                    enumInstanse,
                    Expression.Constant(
                        PropertyFlags.NotNullValue),
                    Expression.Assign(
                        Expression.PropertyOrField(inst, pi.Name),
                        Expression.Convert(
                            GetLambdaOrUseRuntime(
                                propType,
                                binbuf,
                                encoding),
                            pi.PropertyType)
                        )
                    )
                );
    }


    private static Expression GetLambdaOrUseRuntime(
        Type propType,
        Expression binbuf,
        Expression encoding)
    {
        if (BinarySerializer._generationQueue.FirstOrDefault(x => x == propType) is null)
        {
            var lambda = BinarySerializer.GenerateLambdaFromTypeOrGetFromBuffer(propType).Deserialize;
            return Helper.CallDeserializeLambda(
                lambda,
                binbuf,
                encoding);
        }
        else
        {
            return Helper.CallBaseDeserializeMethod(
                Expression.Constant(propType),
                binbuf,
                encoding);
        }
    }
}
