using System;
using System.Collections.Generic;
using System.Linq;
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
        return (Enum)Enum.Parse(type, x.ToString()!);
    }

}
