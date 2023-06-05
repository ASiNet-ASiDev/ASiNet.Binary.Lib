using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Serializer;
using ASiNet.Binary.Lib.Serializer.Attributes;

var w = 0;
var r = 0;
var buffer = new BinaryBuffer(stackalloc byte[4096], stackalloc byte[sizeof(decimal)], ref w, ref r);

var rawData = new Test()
{ 
    EnumTest = TestBEnum.Val3,
    EnumTest1 = TestSBEnum.Val3,
    EnumTest2 = TestUSHEnum.Val3,
    EnumTest3 = TestSHEnum.Val3,
    EnumTest4 = TestINTEnum.Val3,
    EnumTest5 = TestUINTEnum.Val3,
    EnumTest6 = TestLONEnum.Val3,
    EnumTest7 = TestULONEnum.Val3,
    StringTest = "wgbwbwbwbwывпц", 
    TestInt32Array = new[] { 4, 5, 4, 5, 4, 5, 4, 5 }, 
    BooleanTest = true, 
    ByteTest = 127, 
    CharTest = 'C', 
    DoubleTest = 1.1f, 
    DtTest = DateTime.MaxValue, 
    FloatTest = 1.2f, GuidTest = Guid.NewGuid(), 
    IntTest = -3388, 
    LongTest = -44455,
    SByteTest = -33, 
    ShortTest = -55,
    UIntTest = 5554,
    ULongTest = 25552,
    UshortTest = 62433,
    ObjectTest = new() { FirstName = "Bob", LastName = "Titer", Id = 455, Ref = new() { FirstName = "Goblin", Id = 8888,  Ref = null, } },
    TestObjectArray = new User[] { new(533, "Жора", string.Empty), new(534, "Инакендий", ""), new(535, "Елена", null) }
};

Console.WriteLine(BinaryBufferSerializer.Serialize(rawData, buffer));

Console.WriteLine(string.Join(' ', buffer.ToArray()));

var obj = BinaryBufferSerializer.Deserialize<Test>(buffer);

Console.ReadLine();

class User
{
    public User()
    {
        
    }

    public User(int id, string f, string l)
    {
        FirstName = f;
        LastName = l;
        Id = id;
    }

    public int Id { get; set; }
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public User? Ref { get; set; }
}
struct Test
{
    public User[] TestObjectArray { get; set; }
    public User ObjectTest { get; set; }
    public bool BooleanTest { get; set; }
    public bool? NullTest { get; set; }
    public sbyte SByteTest { get; set; }
    public byte ByteTest { get; set; }
    public int IntTest { get; set; }
    public uint UIntTest { get; set; }
    public short ShortTest { get; set; }
    public ushort UshortTest { get; set; }
    public long LongTest { get; set; }
    public ulong ULongTest { get; set; }
    public float FloatTest { get; set; } 
    public double DoubleTest { get; set; }
    public char CharTest { get; set; }
    public string StringTest { get; set; }
    public DateTime DtTest { get; set; }
    public Guid GuidTest { get; set; }
    public TestBEnum EnumTest { get; set; }
    public TestSBEnum EnumTest1 { get; set; }
    public TestUSHEnum EnumTest2 { get; set; }
    public TestSHEnum EnumTest3 { get; set; }
    public TestINTEnum EnumTest4 { get; set; }
    public TestUINTEnum EnumTest5 { get; set; }
    public TestLONEnum EnumTest6 { get; set; }
    public TestULONEnum EnumTest7 { get; set; }

    public int[] TestInt32Array { get; set; }
}
[Flags]
 enum TestBEnum : byte
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestSBEnum : sbyte
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestSHEnum : short
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestUSHEnum : ushort
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestINTEnum : int
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestUINTEnum : uint
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestLONEnum : long
{
    None = 0,
    Val1,
    Val2,
    Val3,
}
enum TestULONEnum : ulong
{
    None = 0,
    Val1,
    Val2,
    Val3,
}