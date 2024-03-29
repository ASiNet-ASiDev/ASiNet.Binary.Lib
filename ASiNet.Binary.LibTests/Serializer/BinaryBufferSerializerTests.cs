﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ASiNet.Binary.Lib.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace ASiNet.Binary.Lib.Serializer.Tests
{
    [TestClass()]
    public class BinaryBufferSerializerTests
    {
        [TestMethod()]
        public void SerializePrimitivesObjects()
        {
            var primitives = new object[]
            {
                (byte)10,
                (sbyte)11,
                false,
                Guid.NewGuid(),
                "test",
                (int)244,
                (uint)245,
                (float)1.1f,
                (double)1.2f,
                (short)31,
                (ushort)32,
                'A',
                DateTime.Now,
                (long)44444,
                (ulong)55555,
            };
            foreach (var item in primitives)
            {
                Span<byte> buffer = new byte[128];

                if (BinarySerializer.Serialize(item.GetType(), item, buffer) == -1)
                    Assert.Fail();

                var dresult = BinarySerializer.Deserialize(item.GetType(), buffer);

                Assert.IsNotNull(dresult);
                Assert.AreEqual(JsonSerializer.Serialize(item), JsonSerializer.Serialize(dresult));
            }
        }

        [TestMethod()]
        public void SerializePrimitivesTest()
        {
            var raw = new A()
            {
                F1 = 13.5f, F2 = 33.6555D,
                C = 55, C1 = 556, C2 = 13, C3 = 556, 
                D = 99, D1 = 5563, D2 = 13, D3 = 522, 
                C_Null = null, D_Null = 1,
                Str = "Hello", Ch = 'A',
                Dt = DateTime.Now,
                G = Guid.NewGuid(),
                En = SerializedType.Int16,
            };
            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1)
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<A>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }

        [TestMethod()]
        public void SerializeGenericsTest()
        {
            var raw = new Gn<Gn<Gn<int>>>()
            {
                Trr = 10,
                Value = new() 
                {
                    Trr = 433,
                    Value = new()
                    {
                        Trr = 53333,
                        Value = 10
                    }
                }
            };

            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1)
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<Gn<Gn<Gn<int>>>>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }

        [TestMethod()]
        public void SerializeArraysTest()
        {
            var raw = new B()
            {
                F1 = new[] { 13.5f },
                F2 = new[] { 33.6555D },
                C = new[] { 55 },
                C1 = new[] { 556u },
                C2 = new byte[] { 13 },
                C3 = new long[] { 556 },
                D = new short[] { 99 },
                D1 = new ushort[] { 5563 },
                D2 = new sbyte[] { 13 },
                D3 = new ulong[] { 522 },
                Str = new[] { "Hello" },
                Ch = new[] { 'A' },
                Dt = new[] { DateTime.Now },
                G = new[] { Guid.NewGuid() },
                En = new[] { SerializedType.Int16 },
            };
            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1) 
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<B>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }

        [TestMethod()]
        public void SerializeRecursionTest()
        {
            var raw = new R()
            {
                Id = 1,
                FirstName = "Игорь", 
                LastName = "укрпурп",
                RefU = new R()
                {
                    Id = 2,
                    FirstName = "Укроп",
                    LastName = "36663",
                    RefU = new R()
                    {
                        Id = 3,
                        FirstName = "Хмурый",
                        LastName = "ва888",
                        RefU = new R()
                        {
                            Id = 4,
                            FirstName = "Весельчак",
                            LastName = "B16",
                            RefU = new R()
                            {
                                Id = 4,
                                FirstName = "Весельчак",
                                LastName = "B16",
                                RefU = new R()
                                {
                                    Id = 4,
                                    FirstName = "Весельчак",
                                    LastName = "B16",
                                    RefU = new R()
                                    {
                                        Id = 4,
                                        FirstName = "Весельчак",
                                        LastName = "B16",
                                        RefU = null,
                                    }
                                }
                            }
                        }
                    }
                }
            };
            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1)
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<R>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }

        [TestMethod()]
        public void SerializeObjectArraysTest()
        {
            var raw = new O()
            {

                Arr = new R[]
                {
                    new()
                    {
                        Id = 4,
                        FirstName = "Иван",
                        LastName = "B16",
                        RefU = null,
                    },
                    new()
                    {
                        Id = 4,
                        FirstName = "Весельчак",
                        LastName = "B16",
                        RefU = null,
                    },
                    new()
                    {
                        Id = 4,
                        FirstName = "Пёс",
                        LastName = "B16",
                        RefU = null,
                    }
                }
            };

            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1)
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<O>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }

        [TestMethod()]
        public void SerializeRecursiveObjectArraysTest()
        {
            var raw = new O()
            {
                Arr = new R[]
                {
                    new R()
                    {
                        Id = 1,
                        FirstName = "Игорь",
                        LastName = "укрпурп",
                        RefU = new R()
                        {
                            Id = 2,
                            FirstName = "Укроп",
                            LastName = "36663",
                            RefU = new R()
                            {
                                Id = 3,
                                FirstName = "Хмурый",
                                LastName = "ва888",
                                RefU = new R()
                                {
                                    Id = 4,
                                    FirstName = "Весельчак",
                                    LastName = "B16",
                                    RefU = null,
                                }
                            }
                        }
                    },
                    new R()
                    {
                        Id = 1,
                        FirstName = "Игорь",
                        LastName = "укрпурп",
                        RefU = new R()
                        {
                            Id = 2,
                            FirstName = "Укроп",
                            LastName = "36663",
                            RefU = new R()
                            {
                                Id = 3,
                                FirstName = "Хмурый",
                                LastName = "ва888",
                                RefU = new R()
                                {
                                    Id = 4,
                                    FirstName = "Весельчак",
                                    LastName = "B16",
                                    RefU = null,
                                }
                            }
                        }
                    },
                    new R()
                    {
                        Id = 1,
                        FirstName = "Игорь",
                        LastName = "укрпурп",
                        RefU = new R()
                        {
                            Id = 2,
                            FirstName = "Укроп",
                            LastName = "36663",
                            RefU = new R()
                            {
                                Id = 3,
                                FirstName = "Хмурый",
                                LastName = "ва888",
                                RefU = new R()
                                {
                                    Id = 4,
                                    FirstName = "Весельчак",
                                    LastName = "B16",
                                    RefU = null,
                                }
                            }
                        }
                    }
                }
            };
            Span<byte> buffer = new byte[ushort.MaxValue];

            if (BinarySerializer.Serialize(raw, buffer) == -1)
                Assert.Fail();

            var dresult = BinarySerializer.Deserialize<O>(buffer);

            Assert.IsNotNull(dresult);
            Assert.AreEqual(JsonSerializer.Serialize(raw), JsonSerializer.Serialize(dresult));
        }
    }
}

class Gn<T>
{
    public T Value { get; set; }

    public int Trr { get; set; }
}

struct A
{
    public int C { get; set; }
    public short D { get; set; }

    public int? C_Null { get; set; }
    public short? D_Null { get; set; }

    public uint C1 { get; set; }
    public ushort D1 { get; set; }

    public byte C2 { get; set; }
    public sbyte D2 { get; set; }

    public long C3 { get; set; }
    public ulong D3 { get; set; }

    public float F1 { get; set; }
    public double F2 { get; set; }

    public string Str { get; set; }
    public char Ch { get; set; }

    public DateTime Dt { get; set; }

    public Guid G { get; set; }

    public SerializedType En { get; set; }
}

class B
{
    public int[] C { get; set; }
    public short[] D { get; set; }

    public uint[] C1 { get; set; }
    public ushort[] D1 { get; set; }

    public byte[] C2 { get; set; }
    public sbyte[] D2 { get; set; }

    public long[] C3 { get; set; }
    public ulong[] D3 { get; set; }

    public float[] F1 { get; set; }
    public double[] F2 { get; set; }

    public string[] Str { get; set; }
    public char[] Ch { get; set; }

    public DateTime[] Dt { get; set; }

    public Guid[] G { get; set; }

    public SerializedType[] En { get; set; }
}

public class R
{

    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public R? RefU { get; set; }
}

public class O
{
    public R[] Arr { get; set; }
}

public class RO
{
    public R[] Arr { get; set; }
}
