using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
public enum SerializedType
{
    None,
    Boolean,
    SByte,
    Byte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
    Enum,
    DateTime,
    Guid,
    Char,
    String,
    Object,

    BooleanArray,
    SByteArray,
    ByteArray,
    Int16Array,
    UInt16Array,
    Int32Array,
    UInt32Array,
    Int64Array,
    UInt64Array,
    FloatArray,
    DoubleArray,
    EnumArray,
    DateTimeArray,
    GuidArray,
    CharArray,
    StringArray,
    ObjectArray,
}
