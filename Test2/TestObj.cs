using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASiNet.Binary.Lib.Serializer;
using ASiNet.Binary.Lib.Serializer.Attributes;

namespace Test2;
public class Package
{
    public Package() { }

    public Package(Guid id, NPlusStatus status, PackageHeader[] headers, params PackageContent[] contents)
    {
        Id = id;
        Headers = headers;
        Contents = contents;
        Status = status;
    }

    public Guid Id { get; set; }

    public NPlusStatus Status { get; set; }

    public PackageHeader[] Headers { get; set; } = null!;

    public PackageContent[] Contents { get; set; } = null!;


    [IgnoreProperty]
    public bool IsFileStream => false;//TryGetHeader("", out bool isFs) ? isFs : false;

    [IgnoreProperty]
    public bool IsStreamPackage => false;//ContainsHeader("");

    [IgnoreProperty]
    public bool IsOpenStreamRequest => false;////ContainsHeader("");

    [IgnoreProperty]
    public bool IsCloseStreamRequest => false;//("", out bool isClose) ? isClose : false;


    [IgnoreProperty]
    public bool IsAllowStreamResponse => false;//("", out bool isDone) ? isDone : false;

}


public struct PackageHeader
{
    public PackageHeader() { }

    public PackageHeader(string key, byte[] value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; set; } = null!;

    public byte[] Value { get; set; } = null!;

    public int GetSize() => Value.Length + Encoding.UTF8.GetByteCount(Key);
}

public struct PackageContent
{
    public PackageContent() { }

    public PackageContent(string key, byte[] content)
    {
        Key = key;
        Content = content;
    }

    public string Key { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public int GetSize() => Content.Length + Encoding.UTF8.GetByteCount(Key);
}

public enum NPlusStatus : ushort
{
    None,
    Ok,
    Disconnected,
    NotAvalible,
    NotFound,
    Timeout,
    SendError,
    AcceptError,
    SerializeError,
    DeserializeError,
    RemoteSerizlizeError,
    RemoteDeserializeError,
    RemoteClientError,
    RemoteServerError,
    HeadersMismatch,
    Rejected,
}