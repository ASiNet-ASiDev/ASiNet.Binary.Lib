using ASiNet.Binary;
using ASiNet.Binary.Lib;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;

BenchmarkRunner.Run<Test1>();

Console.ReadLine();

[MemoryDiagnoser()]
public class Test1
{
    public static MemoryStream Stream = new();

    public int i = 0;

    [Benchmark]
    public void A1()
    {
        var buffer = new BinaryBuffer(stackalloc byte[ushort.MaxValue * 4]);
        while (buffer.Write("3hh35h3h3", Encoding.UTF8))
        {
            i++;
        }

        buffer.WriteToStream(Stream);
    }

    [Benchmark]
    public void B2()
    {
        var buffer = BinaryBuffer.FromStream(Stream, stackalloc byte[(int)Stream.Length]);

        while (i >= 0)
        {
            var str = buffer.ReadString(Encoding.UTF8);
            i--;
        }
    }

}