namespace ZOVserver.Basic.Titan.Streams.Tests;

public class BitStreamTests
{
    [Fact]
    public void WriteThenRead_BooleanAndIntegers_ReturnsOriginalValues()
    {
        using var writer = new BitStream(512);
        
        writer.WriteBoolean(true);
        writer.WritePositiveInt(13, 4);
        writer.WriteInt(-1, 1);
        writer.WritePositiveVInt(1000, 4);
        writer.WritePositiveIntMax255(200);
        writer.WriteIntMax65535(-30000);
        writer.WritePositiveVIntMax255OftenZero(0);
        writer.WritePositiveVIntMax255OftenZero(50);

        var data = writer.GetByteArray();
        using var reader = new BitStream(data);

        Assert.True(reader.ReadBoolean());
        Assert.Equal(13u, reader.ReadPositiveInt(4));
        Assert.Equal(-1, reader.ReadInt(1));
        Assert.Equal(1000u, reader.ReadPositiveVInt(4));
        Assert.Equal(200u, reader.ReadPositiveIntMax255());
        Assert.Equal(-30000, reader.ReadIntMax65535());
        Assert.Equal(0u, reader.ReadPositiveVIntMax255OftenZero());
        Assert.Equal(50u, reader.ReadPositiveVIntMax255OftenZero());
    }

    [Fact]
    public void WritePositiveVInt_MaxValues_ReadCorrectly()
    {
        using var writer = new BitStream(128);

        writer.WritePositiveVIntMax255(255);
        writer.WritePositiveVIntMax65535(65535);
        writer.WritePositiveVIntMax2147483647(2147483647);

        var reader = new BitStream(writer.GetByteArray());

        Assert.Equal(255u, reader.ReadPositiveVIntMax255());
        Assert.Equal(65535u, reader.ReadPositiveVIntMax65535());
        Assert.Equal(2147483647u, reader.ReadPositiveVIntMax2147483647());
    }

    [Fact]
    public void WriteBoolean_WhenSpansByteBoundary_ReadsCorrectly()
    {
        using var writer = new BitStream(64);
        
        for (var i = 0; i < 17; i++)
        {
            writer.WriteBoolean(i % 2 == 0);
        }
        
        writer.WritePositiveInt(15, 4);

        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 17; i++)
        {
            Assert.Equal(i % 2 == 0, reader.ReadBoolean());
        }
        
        Assert.Equal(15u, reader.ReadPositiveInt(4));
    }

    [Fact]
    public void WritePositiveInt_WhenValueExceedsBitWidth_TruncatesToMaxValue()
    {
        using var writer = new BitStream(64);
        
        var truncated = writer.WritePositiveInt(255, 3);
        writer.WriteBoolean(true);

        Assert.Equal(7u, truncated);
        
        var reader = new BitStream(writer.GetByteArray());
        Assert.Equal(7u, reader.ReadPositiveInt(3));
        Assert.True(reader.ReadBoolean());
    }

    [Fact]
    public void WriteInt_NegativeAndPositiveBoundaries_ReadsCorrectly()
    {
        using var writer = new BitStream(64);
        
        writer.WriteInt(-1, 1);
        writer.WriteInt(-32767, 15);
        writer.WriteInt(32767, 15);
        
        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(-1, reader.ReadInt(1));
        Assert.Equal(-32767, reader.ReadInt(15));
        Assert.Equal(32767, reader.ReadInt(15));
    }

    [Fact]
    public void WritePositiveInt_MaxBitWidths_StoresFullRange()
    {
        using var writer = new BitStream(128);
        
        writer.WritePositiveInt(0xFF, 8);
        writer.WritePositiveInt(0xFFFF, 16);
        writer.WritePositiveInt(0x7FFFFFF, 27);

        var reader = new BitStream(writer.GetByteArray());

        Assert.Equal(0xFFu, reader.ReadPositiveInt(8));
        Assert.Equal(0xFFFFu, reader.ReadPositiveInt(16));
        Assert.Equal(0x7FFFFFFu, reader.ReadPositiveInt(27));
    }

    [Fact]
    public void WritePositiveVIntOftenZero_WithZerosAndNonZeros_ReadsCorrectly()
    {
        using var writer = new BitStream(128);
        
        writer.WritePositiveVIntMax65535OftenZero(0);
        writer.WritePositiveVIntMax65535OftenZero(65535);
        writer.WritePositiveVIntMax65535OftenZero(0);

        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(0u, reader.ReadPositiveVIntMax65535OftenZero());
        Assert.Equal(65535u, reader.ReadPositiveVIntMax65535OftenZero());
        Assert.Equal(0u, reader.ReadPositiveVIntMax65535OftenZero());
    }

    [Fact]
    public void Dispose_ThenGetByteArray_ReturnsDataWithoutErrors()
    {
        byte[] data;
        
        using (var writer = new BitStream(16))
        {
            writer.WritePositiveInt(100, 7);
            data = writer.GetByteArray();
        }
        
        using var reader = new BitStream(data);
        Assert.Equal(100u, reader.ReadPositiveInt(7));
    }

    [Fact]
    public void PredefinedWriteMethods_AllVariants_WorkCorrectly()
    {
        using var writer = new BitStream(128);
        
        writer.WritePositiveIntMax1(1);
        writer.WritePositiveIntMax15(15);
        writer.WriteIntMax255(-127);
        writer.WritePositiveIntMax65535(65535);

        var reader = new BitStream(writer.GetByteArray());

        Assert.Equal(1u, reader.ReadPositiveIntMax1());
        Assert.Equal(15u, reader.ReadPositiveIntMax15());
        Assert.Equal(-127, reader.ReadIntMax255());
        Assert.Equal(65535u, reader.ReadPositiveIntMax65535());
    }

    [Fact]
    public void WritePositiveInt_WithAllBitLengths_ReadsCorrectly()
    {
        using var writer = new BitStream(1024);
        var expectedValues = new List<uint>();
        
        for (byte bits = 1; bits <= 20; bits++)
        {
            var maxForBits = (uint)((1 << bits) - 1);
            writer.WritePositiveInt(maxForBits, bits);
            expectedValues.Add(maxForBits);
        }

        var reader = new BitStream(writer.GetByteArray());
        
        for (byte bits = 1; bits <= 20; bits++)
        {
            Assert.Equal(expectedValues[bits - 1], reader.ReadPositiveInt(bits));
        }
    }

    [Fact]
    public void WriteBooleanAndPositiveInt_MixedSequence_ReadsInOrder()
    {
        using var writer = new BitStream(256);
        
        for (var i = 0; i < 32; i++)
        {
            writer.WriteBoolean(i % 2 == 0);
            writer.WritePositiveInt(0x7FFFFFF, 27);
        }

        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 32; i++)
        {
            Assert.Equal(i % 2 == 0, reader.ReadBoolean());
            Assert.Equal(0x7FFFFFFu, reader.ReadPositiveInt(27));
        }
    }

    [Fact]
    public void WritePositiveVInt_WithVariousValues_ReadsCorrectly()
    {
        using var writer = new BitStream(1024);
        
        uint[] values = [0, 1, 127, 128, 255, 256, 32767, 32768, 65535, 16777215, 2147483647];

        foreach (var value in values)
        {
            writer.WritePositiveVIntMax2147483647(value);
        }
        
        foreach (var value in values)
        {
            writer.WritePositiveVIntMax255OftenZero(value > 255 ? 0 : value);
        }

        var reader = new BitStream(writer.GetByteArray());
        
        foreach (var expected in values)
        {
            Assert.Equal(expected, reader.ReadPositiveVIntMax2147483647());
        }
        
        foreach (var expected in values)
        {
            var expectedOftenZero = expected > 255 ? 0 : expected;
            Assert.Equal(expectedOftenZero, reader.ReadPositiveVIntMax255OftenZero());
        }
    }

    [Fact]
    public void WritePositiveInt_WhenCrossingByteBoundary_MaintainsAlignment()
    {
        using var writer = new BitStream(64);
        
        writer.WritePositiveInt(0x7F, 7);
        uint largeValue = 0x7FFFFFF;
        writer.WritePositiveInt(largeValue, 27);
        writer.WritePositiveInt(0x7F, 7);

        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(0x7Fu, reader.ReadPositiveInt(7));
        Assert.Equal(largeValue, reader.ReadPositiveInt(27));
        Assert.Equal(0x7Fu, reader.ReadPositiveInt(7));
    }

    [Fact]
    public void WriteInt_WithVariousBitSizes_ReadsCorrectly()
    {
        using var writer = new BitStream(128);
        
        writer.WriteInt(0, 1);
        writer.WriteInt(-1, 1);
        writer.WriteInt(32767, 15);
        writer.WriteInt(-32767, 15);
        writer.WriteInt(8388607, 23);
        writer.WriteInt(-8388607, 23);

        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(0, reader.ReadInt(1));
        Assert.Equal(-1, reader.ReadInt(1));
        Assert.Equal(32767, reader.ReadInt(15));
        Assert.Equal(-32767, reader.ReadInt(15));
        Assert.Equal(8388607, reader.ReadInt(23));
        Assert.Equal(-8388607, reader.ReadInt(23));
    }

    [Fact]
    public void WriteMixedTypes_RandomSequence_ReadsConsistently()
    {
        using var writer = new BitStream(4096);
        var random = new Random(1337);
        var operations = new List<(string type, uint value, bool boolValue)>();
        
        for (var i = 0; i < 500; i++)
        {
            var type = random.Next(3);

            switch (type)
            {
                case 0:
                {
                    var boolVal = random.Next(2) == 0;
                    writer.WriteBoolean(boolVal);
                    operations.Add(("bool", 0, boolVal));
                    break;
                }
                case 1:
                {
                    var val = (uint)random.Next(1, 10000);
                    writer.WritePositiveInt(val % 1024, 10);
                    operations.Add(("positive", val % 1024, false));
                    break;
                }
                default:
                {
                    var val = (uint)random.Next(1, 10000);
                    writer.WritePositiveVInt(val, 4);
                    operations.Add(("vint", val, false));
                    break;
                }
            }
        }

        var reader = new BitStream(writer.GetByteArray());
        
        foreach (var op in operations)
        {
            switch (op.type)
            {
                case "bool":
                    Assert.Equal(op.boolValue, reader.ReadBoolean());
                    break;
                case "positive":
                    Assert.Equal(op.value, reader.ReadPositiveInt(10));
                    break;
                default:
                    Assert.Equal(op.value, reader.ReadPositiveVInt(4));
                    break;
            }
        }
    }

    [Fact]
    public void WritePositiveInt_WhenValueExceedsBits_TruncatesCorrectly()
    {
        using var writer = new BitStream(64);
        
        var written = writer.WritePositiveInt(uint.MaxValue, 5);
        writer.WriteBoolean(true);

        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(31u, written);
        Assert.Equal(31u, reader.ReadPositiveInt(5));
        Assert.True(reader.ReadBoolean());
    }

    [Fact]
    public void BitStream_WhenWriteBufferExceedsCapacity_AutoExpands()
    {
        using var writer = new BitStream(1);
        
        for (uint i = 0; i < 100; i++)
        {
            writer.WritePositiveInt(i, 7);
        }

        var reader = new BitStream(writer.GetByteArray());
        
        for (uint i = 0; i < 100; i++)
        {
            Assert.Equal(i, reader.ReadPositiveInt(7));
        }
    }

    [Fact]
    public void WritePositiveVIntOftenZero_WithZerosAndNonZeros_ReadsOptimized()
    {
        using var writer = new BitStream(256);
        
        uint[] values = [0, 15, 0, 255, 0, 0, 1];

        foreach (var value in values)
        {
            writer.WritePositiveVIntMax255OftenZero(value);
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        foreach (var expected in values)
        {
            Assert.Equal(expected, reader.ReadPositiveVIntMax255OftenZero());
        }
    }

    [Fact]
    public void GetByteArray_AfterDispose_ReturnsValidData()
    {
        byte[] data;
        
        using (var writer = new BitStream(16))
        {
            writer.WritePositiveInt(0x7FFFF, 19);
            data = writer.GetByteArray();
        }
        
        using var reader = new BitStream(data);
        Assert.Equal(0x7FFFFu, reader.ReadPositiveInt(19));
    }

    [Fact]
    public void GetLength_AfterWrites_ReturnsCorrectByteCount()
    {
        using var writer = new BitStream(64);
        
        Assert.Equal(0, writer.GetLength());
        
        writer.WritePositiveInt(0xFF, 8);
        Assert.Equal(1, writer.GetLength());
        
        writer.WritePositiveInt(0xFF, 8);
        Assert.Equal(2, writer.GetLength());
        
        writer.WriteBoolean(true);
        Assert.Equal(3, writer.GetLength());
    }

    [Fact]
    public void ReadBoolean_WhenBufferIsEmpty_ReturnsFalse()
    {
        var emptyBuffer = Array.Empty<byte>();
        var reader = new BitStream(emptyBuffer);
        
        var result = reader.ReadBoolean();
        
        Assert.False(result);
    }

    [Fact]
    public void ReadPositiveInt_WhenBufferIsEmpty_ReturnsZero()
    {
        var emptyBuffer = Array.Empty<byte>();
        var reader = new BitStream(emptyBuffer);
        
        var result = reader.ReadPositiveInt(8);
        
        Assert.Equal(0u, result);
    }

    [Fact]
    public void WritePositiveVInt_WithZeroValue_WritesCorrectly()
    {
        using var writer = new BitStream(64);
        
        writer.WritePositiveVInt(0, 4);
        writer.WriteBoolean(true);
        
        var reader = new BitStream(writer.GetByteArray());
        
        Assert.Equal(0u, reader.ReadPositiveVInt(4));
        Assert.True(reader.ReadBoolean());
    }

    [Fact]
    public void WriteAndRead_LargeNumberOfBits_NoDataCorruption()
    {
        using var writer = new BitStream(512);
        
        for (var i = 0; i < 100; i++)
        {
            writer.WritePositiveInt((uint)i, 12);
            writer.WriteInt(-i, 12);
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 100; i++)
        {
            Assert.Equal((uint)i, reader.ReadPositiveInt(12));
            Assert.Equal(-i, reader.ReadInt(12));
        }
    }

    [Fact]
    public void WritePositiveVIntOftenZero_WhenAllZeros_ReadsCorrectly()
    {
        using var writer = new BitStream(64);
        
        for (var i = 0; i < 10; i++)
        {
            writer.WritePositiveVIntMax255OftenZero(0);
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(0u, reader.ReadPositiveVIntMax255OftenZero());
        }
    }
}