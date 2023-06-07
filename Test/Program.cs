using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Serializer;
using ASiNet.Binary.Lib.Serializer.Attributes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ProtoBuf;
using System;
using System.Reflection.Metadata;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Xml.Serialization;

BenchmarkRunner.Run<BinaryBufferSerializerTest>();

//var bb = new BinaryBufferSerializerTest();
//bb.BinaryBufferSerializer_SerializerTest();

Console.ReadLine();

[MemoryDiagnoser]
public class BinaryBufferSerializerTest
{
    private A _obj = new();

    [Benchmark]
    public void BinaryBufferSerializer_SerializerTest()
    {
        var r = 0;
        var w = 0;
        var bb = new BinaryBuffer(stackalloc byte[ushort.MaxValue], stackalloc byte[sizeof(decimal)], ref r, ref w);


        var result = BinaryBufferSerializer.Serialize(_obj, bb);

        _obj = BinaryBufferSerializer.Deserialize<A>(bb)!;

        //using (var fs = new FileStream(@"C:\Users\Alexa\OneDrive\Рабочий стол\binbuf.txt", FileMode.Create)) 
        //{
        //    fs.Write(bb.ToSpan());
        //}
    }

    [Benchmark]
    public void JsonSerializer_SerializerTest()
    {
        var result = JsonSerializer.SerializeToUtf8Bytes(_obj);

        _obj = JsonSerializer.Deserialize<A>(result)!;

        //using (var fs = new FileStream(@"C:\Users\Alexa\OneDrive\Рабочий стол\json.txt", FileMode.Create))
        //{
        //    fs.Write(result);
        //}
    }

    //[Benchmark]
    //public void XmlSerializer_SerializerTest()
    //{
    //    using (var fs = new FileStream(@"C:\Users\Alexa\OneDrive\Рабочий стол\xml.txt", FileMode.Create))
    //    {
    //        var serializer = new XmlSerializer(typeof(A));
    //        serializer.Serialize(fs, _obj);
    //        //fs.Position = 0;
    //        //_obj = (A)serializer.Deserialize(fs)!;
    //    }
    //}

    //[Benchmark]
    //public void Protobuf_SerializerTest()
    //{
    //    using (var fs = new FileStream(@"C:\Users\Alexa\OneDrive\Рабочий стол\protobuf.txt", FileMode.Create))
    //    {
    //        Serializer.Serialize(fs, _obj);
    //        //fs.Position = 0;
    //        //_obj = Serializer.Deserialize<A>(fs);
    //    } 
    //}
}

[ProtoContract]
public class A
{
    [ProtoMember(1)]
    public int C { get; set; } = 89395;
    [ProtoMember(2)]
    public short D { get; set; } = 433;
    [ProtoMember(3)]
    public uint C1 { get; set; } = 342;
    [ProtoMember(4)]
    public ushort D1 { get; set; } = 42422;

    [ProtoMember(5)]
    public byte C2 { get; set; } = 42;
    [ProtoMember(6)]
    public sbyte D2 { get; set; } = 54;
    [ProtoMember(7)]
    public long C3 { get; set; } = 422222;
    [ProtoMember(8)]
    public ulong D3 { get; set; } = 5622222;
    [ProtoMember(9)]
    public float F1 { get; set; } = 1344.66f;
    [ProtoMember(10)]
    public double F2 { get; set; } = 13325.755555f;
    [ProtoMember(11)]
    public string Str { get; set; } = "32r2t33jojvvnvwnivnwщцмтщцтмщцтщшмцмттмщцтщмштуцщшмтщшцтмщшцтщшмтцшщмщшцтмщш";
    [ProtoMember(12)]
    public char Ch { get; set; } = 'a';
    [ProtoMember(13)]
    public DateTime Dt { get; set; } = DateTime.UtcNow;
    [ProtoMember(14)]
    public Guid G { get; set; } = Guid.NewGuid();
    //[ProtoMember(15)]
    //public SerializedType En { get; set; } = SerializedType.Byte;
    //[ProtoMember(16)]
    //public Arrays Arr { get; set; } = new();
    //[ProtoMember(17)]
    //public User[] Users { get; set; } = new User[] 
    //{ 
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //    new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    //};
}
[ProtoContract]
public class User
{
    [ProtoMember(1)]
    public int Id { get; set; }
    [ProtoMember(2)]
    public string? FirstName { get; set; }
    [ProtoMember(3)]
    public string? LastName { get; set; }
    [ProtoMember(4)]
    public User? RefU { get; set; }
}

[ProtoContract]
public class Arrays
{
    [ProtoMember(1)]
    public int[] C { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(2)]
    public short[] D { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(3)]
    public uint[] C1 { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(4)]
    public ushort[] D1 { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(5)]
    public byte[] C2 { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(6)]
    public sbyte[] D2 { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(7)]
    public long[] C3 { get; set; } = { 1, 2, 3, 4, 5 };

    [ProtoMember(8)]
    public ulong[] D3 { get; set; } = { 1, 2, 3, 4, 5 };
    [ProtoMember(9)]
    public float[] F1 { get; set; } = { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f };
    [ProtoMember(10)]
    public double[] F2 { get; set; } = { 1.1d, 2.1d, 3.1d, 4.1d, 5.1d };
    [ProtoMember(11)]
    public string[] Str { get; set; } = { "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5", };
    [ProtoMember(12)]
    public char[] Ch { get; set; } = { 'A', 'B', 'C', 'D', 'E' };
    [ProtoMember(13)]
    public DateTime[] Dt { get; set; } = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
    [ProtoMember(14)]
    public Guid[] G { get; set; } = { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
    [ProtoMember(15)]
    public SerializedType[] En { get; set; } = { SerializedType.Boolean, SerializedType.SByte, SerializedType.Int16, SerializedType.UInt32, SerializedType.UInt64 };
}
