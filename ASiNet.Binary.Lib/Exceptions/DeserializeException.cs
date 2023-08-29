namespace ASiNet.Binary.Lib.Exceptions;
public class DeserializeException : Exception
{
    public DeserializeException(string? msg, Exception inner) : base(msg, inner)
    {

    }

}
