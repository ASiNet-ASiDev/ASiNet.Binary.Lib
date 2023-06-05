using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Serializer;
public enum SerializeFlags : byte
{
    None = 1 << 0,
    NullValue = 1 << 1,
}
