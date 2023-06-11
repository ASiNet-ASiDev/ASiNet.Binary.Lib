using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static Expression For(Expression body, ParameterExpression i, ParameterExpression max)
    {
        var ret = Expression.Label(typeof(int), "ret");

        var loopBody = Expression.Block(
            Expression.IfThenElse(
                Expression.LessThan(i, max),
                body,
                Expression.Break(ret, i)),
            Expression.PostIncrementAssign(i));

        var loop = Expression.Loop(loopBody, ret);

        return loop;
    }
}
