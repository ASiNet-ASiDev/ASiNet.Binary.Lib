using System.Linq.Expressions;
using System.Reflection;
using ASiNet.Binary.Lib.Enums;
using ASiNet.Binary.Lib.Expressions.BaseTypes;
using ASiNet.Binary.Lib.Serializer;
using Microsoft.VisualBasic;

namespace ASiNet.Binary.Lib;
public static class Helper
{

    public static TEnum ToEnum<TType, TEnum>(TType x) where TType : struct where TEnum : Enum
    {
        return (TEnum)(object)x;
    }

    public static Enum ToEnum<TType>(TType x, Type type) where TType : struct
    {
        var result = Activator.CreateInstance(type);
        result = x;
        return (Enum)result;
    }


    public static Enum[] ToEnumArray(object enumArray)
    {
        var baseArray = (enumArray as Array)!;

        var arr = Array.CreateInstance(typeof(Enum), baseArray.Length);

        for (int i = 0; i < baseArray.Length; i++)
        {
            arr.SetValue(baseArray.GetValue(i), i);
        }
        return (Enum[])arr;
    }

    public static object FromEnumArray(Enum[] arr, Type enumType)
    {
        var res = Array.CreateInstance(enumType, arr.Length);

        for (int i = 0; i < arr.Length; i++)
        {
            res.SetValue(arr[i], i);
        }
        return res;
    }

    public static bool IsNullable(Type type)
        => Nullable.GetUnderlyingType(type) != null;

    public static Expression NullableHashValue(Expression input) =>
        Expression.PropertyOrField(input, nameof(Nullable<byte>.HasValue));

    public static Expression CallEnumHashFlag(Expression instEnum, Expression flags) =>
        Expression.Call(instEnum, nameof(Enum.HasFlag), null, Expression.Convert(flags, typeof(Enum)));

    public static Expression CallBaseSerializeMethod(Expression type, Expression obj, Expression buffer, Expression encoding, Expression deep) =>
        Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseSerialize), null, type, Expression.Convert(obj, typeof(object)), buffer, encoding, deep);

    public static Expression CallBaseDeserializeMethod(Expression type, Expression buffer, Expression encoding) =>
        Expression.Call(typeof(BinarySerializer), nameof(BinarySerializer.BaseDeserialize), null, type, buffer, encoding);

    public static Expression CallSerializeLambda(SerializeObjLambda lambda, Expression obj, Expression buffer, Expression encoding, Expression deep) =>
        Expression.Invoke(Expression.Constant(lambda), Expression.Convert(obj, typeof(object)), buffer, encoding, deep);

    public static Expression CallDeserializeLambda(DeserializeObjLambda lambda, Expression buffer, Expression encoding) =>
        Expression.Invoke(Expression.Constant(lambda), buffer, encoding);

    public static Expression CallWriteFlagsMethod(Expression flags, Expression binbuf) =>
        Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, Expression.Convert(flags, typeof(byte)));

    public static Expression IfHashFlag(Expression enumInst, Expression flag, Expression ifTrue, Expression? ifFalse = null) => ifFalse is null ?
        Expression.IfThen(CallEnumHashFlag(enumInst, flag), ifTrue) :
        Expression.IfThenElse(CallEnumHashFlag(enumInst, flag), ifTrue, ifFalse);

    public static Expression CallReadFlagsMethod(Expression binbuf) =>
        Expression.Convert(Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.ReadByte), null, binbuf), typeof(PropertyFlags));

    public static Expression CallReadMethod(Expression binbuf, string methodName) =>
        Expression.Call(typeof(BinaryBufferBaseTypes), methodName, null, binbuf);

    public static Expression CallWriteMethod(Expression value, Expression binbuf) =>
        Expression.Call(typeof(BinaryBufferBaseTypes), nameof(BinaryBufferBaseTypes.Write), null, binbuf, value);

    public static Expression GetArrayLength(Expression array) =>
        Expression.PropertyOrField(array, nameof(Array.Length));

    public static Expression GetCollectionLength(Expression array) =>
        Expression.PropertyOrField(array, nameof(Collection.Count));

    public static Expression GetArrayLength(PropertyInfo pi, Expression inst)
        => GetArrayLength(Expression.PropertyOrField(inst, pi.Name));

    public static Expression GetCollectionLength(PropertyInfo pi, Expression inst)
        => GetCollectionLength(Expression.PropertyOrField(inst, pi.Name));

    public static Expression WriteNullableValueTypes(
        Expression inst,
        string propName,
        Expression binbuf,
        Expression isNotNullWriteMethod,
        PropertyFlags isNotNullFlag = PropertyFlags.NotNullValue,
        PropertyFlags isNullFlag = PropertyFlags.None) =>
        Expression.IfThenElse(
            NullableHashValue(Expression.PropertyOrField(inst, propName)),
            Expression.Block(
                CallWriteFlagsMethod(Expression.Constant(isNotNullFlag), binbuf),
                isNotNullWriteMethod),
            CallWriteFlagsMethod(Expression.Constant(isNullFlag), binbuf));

    public static Expression WriteNullableValueTypes(
        Expression inst,
        Expression binbuf,
        Expression isNotNullWriteMethod,
        PropertyFlags isNotNullFlag = PropertyFlags.NotNullValue,
        PropertyFlags isNullFlag = PropertyFlags.None) =>
        Expression.IfThenElse(
            NullableHashValue(inst),
            Expression.Block(
                CallWriteFlagsMethod(Expression.Constant(isNotNullFlag), binbuf),
                isNotNullWriteMethod),
            CallWriteFlagsMethod(Expression.Constant(isNullFlag), binbuf));

    public static Expression WriteNotNullableObject(
        Expression binbuf,
        PropertyFlags flag,
        Expression writeMethod) =>
        Expression.Block(
            CallWriteFlagsMethod(Expression.Constant(flag), binbuf),
            writeMethod);

    public static Expression WriteArray(
        Expression inst,
        string propName,
        Expression binbuf,
        Expression isNotNullWriteMethod,
        PropertyFlags isNotNullFlag = PropertyFlags.NotNullValue,
        PropertyFlags isNullFlag = PropertyFlags.None) =>
        Expression.IfThenElse(
            Expression.NotEqual(Expression.PropertyOrField(inst, propName), Expression.Constant(null)),
            Expression.Block(
                CallWriteFlagsMethod(Expression.Constant(isNotNullFlag), binbuf),
                Expression.Call(
                    typeof(BinaryBufferBaseTypes),
                    nameof(BinaryBufferBaseTypes.Write),
                    null,
                    binbuf,
                    GetArrayLength(Expression.PropertyOrField(inst, propName))),
                isNotNullWriteMethod),
            CallWriteFlagsMethod(Expression.Constant(isNullFlag), binbuf));

    public static Expression WriteObject(
        Expression inst,
        string propName,
        Expression binbuf,
        Expression isNotNullWriteMethod,
        PropertyFlags isNotNullFlag = PropertyFlags.NotNullValue,
        PropertyFlags isNullFlag = PropertyFlags.None) =>
        Expression.IfThenElse(
            Expression.NotEqual(Expression.PropertyOrField(inst, propName), Expression.Constant(null)),
            Expression.Block(
                CallWriteFlagsMethod(Expression.Constant(isNotNullFlag), binbuf),
                isNotNullWriteMethod),
            CallWriteFlagsMethod(Expression.Constant(isNullFlag), binbuf));

    public static Expression For(Expression body, ParameterExpression max, ParameterExpression? i = null)
    {
        var ret = Expression.Label(typeof(int), "ret");

        if (i is not null)
        {
            var loopBody = Expression.Block(
            Expression.IfThenElse(
                Expression.LessThan(i, max),
                body,
                Expression.Break(ret, i)),
            Expression.PostIncrementAssign(i));

            var loop = Expression.Loop(loopBody, ret);

            return loop;
        }
        else
        {
            var p = Expression.Variable(typeof(int));

            var loopBody = Expression.Block(
            Expression.IfThenElse(
                Expression.LessThan(p, max),
                body,
                Expression.Break(ret, p)),
            Expression.PostIncrementAssign(p));

            var loop = Expression.Block(new ParameterExpression[] { p },
                Expression.Assign(p, Expression.Constant(0)),
                Expression.Loop(loopBody, ret));

            return loop;
        }
    }

    public static Expression ForeachGetArray(Expression array, Func<Expression, Expression> body)
    {
        var ret = Expression.Label(typeof(int), "ret");

        var i = Expression.Variable(typeof(int));
        var max = Expression.Variable(typeof(int));
        var item = Expression.Variable(array.Type.GetElementType()!);

        var loopBody = Expression.Block(
        Expression.IfThenElse(
            Expression.LessThan(i, max),
            Expression.Block(
                Expression.Assign(item, Expression.ArrayAccess(array, i)),
                body(item)),
            Expression.Break(ret, i)),
        Expression.PostIncrementAssign(i));

        var loop = Expression.Block(new[] { i, max, item },
            Expression.Assign(i, Expression.Constant(0)),
            Expression.Assign(max, GetArrayLength(array)),
            Expression.Loop(loopBody, ret));

        return loop;
    }

    public static Expression ForeachSetArray(Expression array, Expression body)
    {
        var ret = Expression.Label(typeof(int), "ret");

        var i = Expression.Variable(typeof(int));
        var max = Expression.Variable(typeof(int));
        var item = Expression.Variable(array.Type.GetElementType()!);

        var loopBody = Expression.Block(
        Expression.IfThenElse(
            Expression.LessThan(i, max),
            Expression.Assign(Expression.ArrayAccess(array, i), body),
            Expression.Break(ret, i)),
        Expression.PostIncrementAssign(i));

        var loop = Expression.Block(new ParameterExpression[] { i, max, item },
            Expression.Assign(i, Expression.Constant(0)),
            Expression.Assign(max, GetArrayLength(array)),
            Expression.Loop(loopBody, ret));

        return loop;
    }
}
