using ASiNet.Binary.Lib.Serializer;
using Test2;

Span<byte> buffer = stackalloc byte[200];

var ab = (Span<byte>)(stackalloc byte[50]);
ab.Fill(255);
var arr = ab.ToArray();

var inObj = new Package(Guid.NewGuid(), NPlusStatus.Ok, 
    new[] 
    { 
        new PackageHeader("#h1", new byte[] { 10, 10, 10, 10 }),
        new PackageHeader("#h2", new byte[] { 20, 20, 20, 20 }),
        new PackageHeader("#h3", new byte[] { 30, 30, 30, 30 }),
        new PackageHeader("#h4", new byte[] { 40, 40, 40, 40 }),
    },
    new PackageContent("content", arr)
    );

var size = BinarySerializer.Serialize(inObj, buffer);

var outObj = BinarySerializer.Deserialize<Package>(buffer[..size]);

Console.ReadKey();