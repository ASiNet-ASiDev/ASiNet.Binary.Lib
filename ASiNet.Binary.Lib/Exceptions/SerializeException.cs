using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Binary.Lib.Exceptions;
public class SerializeException : Exception
{
    public SerializeException(string? msg, Exception inner) : base(msg, inner)
    {

    }
}
