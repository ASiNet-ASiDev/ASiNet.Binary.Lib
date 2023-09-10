using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Serializer;
using ASiNet.Binary.Lib.Serializer.Attributes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ProtoBuf;
using ProtoBuf.Serializers;
using System.Text.Json;

BenchmarkRunner.Run<BinaryBufferSerializerTest>();

Console.ReadLine();

static class Data
{
    static Data()
    {
        var buffer = new byte[ushort.MaxValue];
        var size = BinarySerializer.Serialize(new GlobalSerializedTestObj(), buffer);
        RawBSObject = buffer[..size];

        RawJSObject = JsonSerializer.SerializeToUtf8Bytes(new GlobalSerializedTestObj());

        BSResultBytes = new byte[ushort.MaxValue];
        JSResultBytes = new byte[ushort.MaxValue];
    }

    public static byte[] RawBSObject { get; private set; }
    public static byte[] RawJSObject { get; private set; }
    public static GlobalSerializedTestObj Object { get; private set; }

    public static GlobalSerializedTestObj JsResultObject { get; set; }

    public static GlobalSerializedTestObj BsResultObject { get; set; }

    public static byte[] BSResultBytes { get; set; }
    public static byte[] JSResultBytes { get; set; }
}

[MemoryDiagnoser(false)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class BinaryBufferSerializerTest
{

    [BenchmarkCategory("Serialize"), Benchmark()]
    public void BinaryBuffer_Serialize()
    {
        var size = BinarySerializer.Serialize(Data.Object, Data.BSResultBytes);
    }

    [BenchmarkCategory("Deserialize"), Benchmark()]
    public void BinaryBuffer_Deserialize()
    {
        Data.BsResultObject = BinarySerializer.Deserialize<GlobalSerializedTestObj>(Data.RawBSObject)!;
    }

    [BenchmarkCategory("Serialize"), Benchmark(Baseline = true)]
    public void JsonSerializer_Serialize()
    {
        Data.JSResultBytes = JsonSerializer.SerializeToUtf8Bytes(Data.Object);
    }

    [BenchmarkCategory("Deserialize"), Benchmark(Baseline = true)]
    public void JsonSerializer_Deserialize()
    {
        Data.JsResultObject = JsonSerializer.Deserialize<GlobalSerializedTestObj>(Data.RawJSObject)!;
    }
}

public class GlobalSerializedTestObj
{

    public int C { get; set; } = 89395;

    public short D { get; set; } = 433;

    public uint C1 { get; set; } = 342;

    public ushort D1 { get; set; } = 42422;

    public byte C2 { get; set; } = 42;

    public sbyte D2 { get; set; } = 54;

    public long C3 { get; set; } = 422222;

    public ulong D3 { get; set; } = 5622222;

    public float F1 { get; set; } = 1344.66f;

    public double F2 { get; set; } = 13325.755555f;

    public string Str { get; set; } = "32r2t33jojvvnvwnivnwщцмтщцтмщцтщшмцмттмщцтщмштуцщшмтщшцтмщшцтщшмтцшщмщшцтмщш";

    public char Ch { get; set; } = 'a';

    public DateTime Dt { get; set; } = DateTime.UtcNow;

    public Guid G { get; set; } = Guid.NewGuid();

    public SerializedType En { get; set; } = SerializedType.Byte;

    public Arrays Arr { get; set; } = new();

    public Arrays[] Arr_Arrays { get; set; } = new Arrays[] { new(), new(), new() };

    public User[] Users { get; set; } = new User[]
    {
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
        new User() { FirstName = "Джин", Id = 99, LastName = "5333", RefU = new() { FirstName = "geegee", Id = 522, LastName = "2r2222", RefU = new() { FirstName = "f33", Id = 993, } } },
    };


    public int[] A_C { get; set; } = { 1, 2, 3, 4, 5 };

    public short[] A_D { get; set; } = { 1, 2, 3, 4, 5 };

    public uint[] A_C1 { get; set; } = { 1, 2, 3, 4, 5 };

    public ushort[] A_D1 { get; set; } = { 1, 2, 3, 4, 5 };

    public byte[] A_C2 { get; set; } = { 1, 2, 3, 4, 5 };

    public sbyte[] A_D2 { get; set; } = { 1, 2, 3, 4, 5 };

    public long[] A_C3 { get; set; } = { 1, 2, 3, 4, 5 };

    public ulong[] A_D3 { get; set; } = { 1, 2, 3, 4, 5 };

    public float[] A_F1 { get; set; } = { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f };

    public double[] A_F2 { get; set; } = { 1.1d, 2.1d, 3.1d, 4.1d, 5.1d };

    public string[] A_Str { get; set; } = { "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5", };

    public char[] A_Ch { get; set; } = { 'A', 'B', 'C', 'D', 'E' };

    public DateTime[] A_Dt { get; set; } = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

    public Guid[] A_G { get; set; } = { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };

    public SerializedType[] A_En { get; set; } = { SerializedType.Boolean, SerializedType.SByte, SerializedType.Int16, SerializedType.UInt32, SerializedType.UInt64 };
}

public class User
{

    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public User? RefU { get; set; }
}

public class Arrays
{

    public int Zero { get; set; } = 89395;

    public int[] C_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public short[] D_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public uint[] C1_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public ushort[] D1_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public byte[] C2_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public sbyte[] D2_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public long[] C3_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public ulong[] D3_Array { get; set; } = { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };

    public float[] F1_Array { get; set; } = { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f };

    public double[] F2_Array { get; set; } = { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 1.1f, 2.1f, 3.1f, 4.1f, 5.1f };

    public string[] Str_Array { get; set; } = 
        { "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5",
        "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5",
        "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5",
        "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5",
        "Hello World 1", "Hello World 2", "Hello World 3", "Hello World 4", "Hello World 5",};

    public char[] Ch_Array { get; set; } = { 'A', 'B', 'C', 'D', 'E', 'A', 'B', 'C', 'D', 'E', 'A', 'B', 'C', 'D', 'E', 'A', 'B', 'C', 'D', 'E', 'A', 'B', 'C', 'D', 'E', 'A', 'B', 'C', 'D', 'E' };

    public DateTime[] Dt_Array { get; set; } = { DateTime.MaxValue, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, DateTime.MinValue };

    public Guid[] G_Array { get; set; } = { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };

    public SerializedType[] En_Array { get; set; } = { SerializedType.Boolean, SerializedType.SByte, SerializedType.Int16, SerializedType.UInt32, SerializedType.UInt64 };
}
