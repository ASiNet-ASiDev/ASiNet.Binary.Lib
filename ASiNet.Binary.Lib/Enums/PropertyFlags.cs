using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Enums;
[Flags]
public enum PropertyFlags : byte
{
    None = 0,
    NotNullValue = 1 << 0,
}
