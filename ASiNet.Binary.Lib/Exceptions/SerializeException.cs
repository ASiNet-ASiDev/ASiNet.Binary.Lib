namespace ASiNet.Binary.Lib.Exceptions;
public class SerializeException : Exception
{
    public SerializeException(string? msg, Exception inner) : base(msg, inner)
    {

    }
}
