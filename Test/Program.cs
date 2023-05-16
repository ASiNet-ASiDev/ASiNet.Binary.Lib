using ASiNet.Binary.Lib;
using System.Text;

var buffer = new BinaryBuffer(stackalloc byte[4096]);

Console.WriteLine(BinaryBufferSerializer.Serialize(new Test() { StringTest = "wgbwbwbwbwывпц" }, ref buffer));

Console.WriteLine(string.Join(' ', buffer.ToArray()));

var obj = BinaryBufferSerializer.Deserialize<Test>(ref buffer);

Console.ReadLine();



class Test
{
    public bool BooleanTest { get; set; } = true;
    public sbyte SByteTest { get; set; } = -32;
    public byte ByteTest { get; set; } = 255;
    public int IntTest { get; set; } = 6666666;
    public uint UIntTest { get; set; } = 9986999;
    public short ShortTest { get; set; } = 4433;
    public ushort UshortTest { get; set; } = 3344;
    public long LongTest { get; set; } = -455222;
    public ulong ULongTest { get; set; } = 455222;
    public float FloatTest { get; set; } = 1.25f;
    public double DoubleTest { get; set; } = 1.14444444444d;
    public char CharTest { get; set; } = 'a';
    public string StringTest { get; set; } = "Hello World!";
    public DateTime DtTest { get; set; } = DateTime.UtcNow;
    public Guid GuidTest { get; set; } = Guid.NewGuid();
    public TestEnum EnumTest { get; set; } = TestEnum.Val2;
}

enum TestEnum
{
    None = 0,
    Val1,
    Val2,
    Val3,
}