using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Expressions.Arrays;
using ASiNet.Binary.Lib.Serializer;
using Test2;

Span<byte> area = stackalloc byte[4096];
Span<byte> buff = stackalloc byte[sizeof(decimal)];
var rp = 0;
var wp = 0;
var bb = new BinaryBuffer(area, buff, ref rp, ref wp);

var dt = Guid.NewGuid();

bb.WriteArray(new Guid[] { dt });

var newArr = bb.ReadGuidArray();

/*
//Span<byte> buffer = stackalloc byte[200];

//var ab = (Span<byte>)(stackalloc byte[50]);
//ab.Fill(255);
//var arr = ab.ToArray();

//var inObj = new Package(Guid.NewGuid(), NPlusStatus.Ok, 
//    new[] 
//    { 
//        new PackageHeader("#h1", new byte[] { 10, 10, 10, 10 }),
//        new PackageHeader("#h2", new byte[] { 20, 20, 20, 20 }),
//        new PackageHeader("#h3", new byte[] { 30, 30, 30, 30 }),
//        new PackageHeader("#h4", new byte[] { 40, 40, 40, 40 }),
//    },
//    new PackageContent("content", arr)
//    );

//var size = BinarySerializer.Serialize(inObj, buffer);

//var outObj = BinarySerializer.Deserialize<Package>(buffer[..size]);
*/
Console.ReadKey();