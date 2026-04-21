namespace ZOVserver.Basic.Titan.Streams.Tests;

public class ByteStreamTests
{
    [Fact]
    public void Constructor_WithByteArray_ShouldUseProvidedBuffer()
    {
        var expected = new byte[] { 10, 20, 30 };
        using var stream = new ByteStream(expected);
        
        Assert.Equal(3, stream.Length);
        Assert.Equal(0, stream.Offset);
        Assert.Equal(expected, stream.GetBuffer().ToArray());
    }

    [Fact]
    public void Dispose_ShouldReleasePooledBuffer()
    {
        var stream = new ByteStream(1000);
        stream.WriteI32(42);
        stream.Dispose();
    
        Assert.Throws<NullReferenceException>(() => stream.GetBuffer());
    }

    [Fact]
    public void Constructor_WithLargeCapacity_ShouldUsePooledBuffer()
    {
        using var stream = new ByteStream(1000);
        stream.WriteU8(128);
        
        Assert.Equal(1, stream.Length);
    }

    [Fact]
    public void GetBuffer_ShouldReturnOnlyWrittenData()
    {
        using var stream = new ByteStream(100);
        stream.WriteU8(1);
        stream.WriteU8(2);
        stream.WriteU8(3);
        
        var buffer = stream.GetBuffer();
        
        Assert.Equal(3, buffer.Length);
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer.ToArray());
    }

    [Fact]
    public void GetMemory_ShouldReturnWrittenDataAsMemory()
    {
        using var stream = new ByteStream(100);
        stream.WriteI16(12345);
        
        var memory = stream.GetMemory();
        
        Assert.Equal(2, memory.Length);
    }

    [Fact]
    public void IsAtEnd_WhenOffsetEqualsLength_ShouldReturnTrue()
    {
        using var stream = new ByteStream(10);
        stream.WriteU8(5);
        stream.SetOffset(1);
        
        Assert.True(stream.IsAtEnd());
    }

    [Fact]
    public void IsAtEnd_WhenOffsetLessThanLength_ShouldReturnFalse()
    {
        using var stream = new ByteStream(10);
        stream.WriteU8(5);
        stream.SetOffset(0);
        
        Assert.False(stream.IsAtEnd());
    }

    [Fact]
    public void SetOffset_ShouldResetBitOffset()
    {
        using var stream = new ByteStream(10);
        stream.WriteBoolean(true);
        stream.WriteBoolean(false);
        stream.SetOffset(0);
        
        Assert.Equal(0, stream.Offset);
        Assert.True(stream.ReadBoolean());
    }

    [Fact]
    public void ResetOffset_ShouldResetBothOffsetAndLength()
    {
        using var stream = new ByteStream(10);
        stream.WriteU16(1000);
        stream.ResetOffset();
        
        Assert.Equal(0, stream.Offset);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void WriteBoolean_ThenReadBoolean_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteBoolean(true);
        stream.WriteBoolean(false);
        stream.WriteBoolean(true);
        
        stream.SetOffset(0);
        
        Assert.True(stream.ReadBoolean());
        Assert.False(stream.ReadBoolean());
        Assert.True(stream.ReadBoolean());
    }

    [Fact]
    public void WriteBoolean_ShouldAutoExpandBuffer()
    {
        using var stream = new ByteStream(1);
        
        for (var i = 0; i < 100; i++)
            stream.WriteBoolean(i % 2 == 0);
        
        stream.SetOffset(0);
        
        for (var i = 0; i < 100; i++)
            Assert.Equal(i % 2 == 0, stream.ReadBoolean());
    }

    [Fact]
    public void ReadBoolean_WhenBufferEmpty_ThrowsIndexOutOfRangeException()
    {
        var stream = new ByteStream([]);
    
        Assert.Throws<IndexOutOfRangeException>(() => stream.ReadBoolean());
    }

    [Fact]
    public void WriteI8_ThenReadI8_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteI8(-128);
        stream.WriteI8(0);
        stream.WriteI8(127);
        
        stream.SetOffset(0);
        
        Assert.Equal(-128, stream.ReadI8());
        Assert.Equal(0, stream.ReadI8());
        Assert.Equal(127, stream.ReadI8());
    }

    [Fact]
    public void WriteU8_ThenReadU8_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteU8(0);
        stream.WriteU8(128);
        stream.WriteU8(255);
        
        stream.SetOffset(0);
        
        Assert.Equal(0u, stream.ReadU8());
        Assert.Equal(128u, stream.ReadU8());
        Assert.Equal(255u, stream.ReadU8());
    }

    [Fact]
    public void WriteI16_WithBigEndian_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteI16(-32768);
        stream.WriteI16(0);
        stream.WriteI16(32767);
        
        stream.SetOffset(0);
        
        Assert.Equal(-32768, stream.ReadI16());
        Assert.Equal(0, stream.ReadI16());
        Assert.Equal(32767, stream.ReadI16());
    }

    [Fact]
    public void WriteI16_WithLittleEndian_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteI16(0x1234, false);
        
        stream.SetOffset(0);
        
        Assert.Equal(0x1234, stream.ReadI16(false));
    }

    [Fact]
    public void WriteU16_ThenReadU16_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteU16(0);
        stream.WriteU16(32768);
        stream.WriteU16(65535);
        
        stream.SetOffset(0);
        
        Assert.Equal(0u, stream.ReadU16());
        Assert.Equal(32768u, stream.ReadU16());
        Assert.Equal(65535u, stream.ReadU16());
    }

    [Fact]
    public void WriteI32_ThenReadI32_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteI32(-2147483648);
        stream.WriteI32(0);
        stream.WriteI32(2147483647);
        
        stream.SetOffset(0);
        
        Assert.Equal(-2147483648, stream.ReadI32());
        Assert.Equal(0, stream.ReadI32());
        Assert.Equal(2147483647, stream.ReadI32());
    }

    [Fact]
    public void WriteU32_ThenReadU32_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteU32(0);
        stream.WriteU32(0x80000000);
        stream.WriteU32(0xFFFFFFFF);
        
        stream.SetOffset(0);
        
        Assert.Equal(0u, stream.ReadU32());
        Assert.Equal(0x80000000u, stream.ReadU32());
        Assert.Equal(0xFFFFFFFFu, stream.ReadU32());
    }

    [Fact]
    public void WriteI64_ThenReadI64_ShouldWork()
    {
        using var stream = new ByteStream(20);
        
        stream.WriteI64(-9223372036854775808);
        stream.WriteI64(0);
        stream.WriteI64(9223372036854775807);
        
        stream.SetOffset(0);
        
        Assert.Equal(-9223372036854775808, stream.ReadI64());
        Assert.Equal(0, stream.ReadI64());
        Assert.Equal(9223372036854775807, stream.ReadI64());
    }

    [Fact]
    public void WriteU64_ThenReadU64_ShouldWork()
    {
        using var stream = new ByteStream(20);
        
        stream.WriteU64(0);
        stream.WriteU64(0x8000000000000000);
        stream.WriteU64(0xFFFFFFFFFFFFFFFF);
        
        stream.SetOffset(0);
        
        Assert.Equal(0uL, stream.ReadU64());
        Assert.Equal(0x8000000000000000uL, stream.ReadU64());
        Assert.Equal(0xFFFFFFFFFFFFFFFFuL, stream.ReadU64());
    }

    [Fact]
    public void WriteI128_ThenReadI128_ShouldWork()
    {
        using var stream = new ByteStream(20);
        
        stream.WriteI128(Int128.MaxValue - 1);
        stream.WriteI128(Int128.MinValue + 1);
        stream.WriteI128(0);
    
        stream.SetOffset(0);
    
        Assert.Equal(Int128.MaxValue - 1, stream.ReadI128());
        Assert.Equal(Int128.MinValue + 1, stream.ReadI128());
        Assert.Equal(0, stream.ReadI128());
    }

    [Fact]
    public void WriteU128_ThenReadU128_ShouldWork()
    {
        using var stream = new ByteStream(20);
    
        var value1 = new UInt128(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);
        var value2 = UInt128.Zero;
    
        stream.WriteU128(value1);
        stream.WriteU128(value2);
    
        stream.SetOffset(0);
    
        Assert.Equal(value1, stream.ReadU128());
        Assert.Equal(value2, stream.ReadU128());
    }

    [Fact]
    public void WriteVInt32_PositiveSmall_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteVInt32(0);
        stream.WriteVInt32(1);
        stream.WriteVInt32(63);
        
        stream.SetOffset(0);
        
        Assert.Equal(0, stream.ReadVInt32());
        Assert.Equal(1, stream.ReadVInt32());
        Assert.Equal(63, stream.ReadVInt32());
    }

    [Fact]
    public void WriteVInt32_PositiveLarge_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteVInt32(1000000);
        stream.WriteVInt32(2147483647);
        
        stream.SetOffset(0);
        
        Assert.Equal(1000000, stream.ReadVInt32());
        Assert.Equal(2147483647, stream.ReadVInt32());
    }

    [Fact]
    public void WriteVInt32_NegativeSmall_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteVInt32(-1);
        stream.WriteVInt32(-63);
        
        stream.SetOffset(0);
        
        Assert.Equal(-1, stream.ReadVInt32());
        Assert.Equal(-63, stream.ReadVInt32());
    }

    [Fact]
    public void WriteVInt32_NegativeLarge_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteVInt32(-1000000);
        stream.WriteVInt32(-2147483648);
        
        stream.SetOffset(0);
        
        Assert.Equal(-1000000, stream.ReadVInt32());
        Assert.Equal(-2147483648, stream.ReadVInt32());
    }

    [Fact]
    public void WriteVInt64_ThenReadVInt64_ShouldWork()
    {
        using var stream = new ByteStream(20);
        
        stream.WriteVInt64(0);
        stream.WriteVInt64(1234567890123456789);
        stream.WriteVInt64(-987654321098765432);
        
        stream.SetOffset(0);
        
        Assert.Equal(0, stream.ReadVInt64());
        Assert.Equal(1234567890123456789, stream.ReadVInt64());
        Assert.Equal(-987654321098765432, stream.ReadVInt64());
    }

    [Fact]
    public void WriteString_NonNull_ShouldWork()
    {
        using var stream = new ByteStream(100);
        
        stream.WriteString("Hello World");
        
        stream.SetOffset(0);
        
        Assert.Equal("Hello World", stream.ReadString());
    }

    [Fact]
    public void WriteString_Null_ShouldWriteNegativeLength()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteString(null);
        
        stream.SetOffset(0);
        
        Assert.Equal(string.Empty, stream.ReadString());
    }

    [Fact]
    public void WriteString_Empty_ShouldWork()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteString("");
        
        stream.SetOffset(0);
        
        Assert.Equal("", stream.ReadString());
    }

    [Fact]
    public void ReadString_WithMaxLengthExceeded_ShouldReturnEmpty()
    {
        using var stream = new ByteStream(100);
        stream.WriteString("This is a long string");
        
        stream.SetOffset(0);
        
        Assert.Equal(string.Empty, stream.ReadString(5));
    }

    [Fact]
    public void ReadString_WhenLengthInvalid_ShouldReturnEmpty()
    {
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        using var stream = new ByteStream(data);
        
        Assert.Equal(string.Empty, stream.ReadString());
    }

    [Fact]
    public void WriteCompressedString_ThenReadCompressedString_ShouldWork()
    {
        using var stream = new ByteStream(200);
        
        var original = "This is a test string that will be compressed. " +
                       "It should be longer than the compressed version.";
        
        stream.WriteCompressedString(original);
        
        stream.SetOffset(0);
        
        var result = stream.ReadCompressedString();
        
        Assert.Equal(original, result);
    }

    [Fact]
    public void WriteBytes_ThenReadBytes_ShouldWork()
    {
        using var stream = new ByteStream(50);
        
        var data = new byte[] { 1, 2, 3, 4, 5 };
        stream.WriteBytes(data);
        
        stream.SetOffset(0);
        
        var result = stream.ReadBytes();
        
        Assert.Equal(data, result.ToArray());
    }

    [Fact]
    public void WriteBytes_Null_ShouldWriteNegativeLength()
    {
        using var stream = new ByteStream(10);
        
        stream.WriteBytes(null);
        
        stream.SetOffset(0);
        
        Assert.True(stream.ReadBytes().IsEmpty);
    }

    [Fact]
    public void ReadBytes_WhenLengthInvalid_ShouldReturnEmpty()
    {
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        using var stream = new ByteStream(data);
        
        Assert.True(stream.ReadBytes().IsEmpty);
    }

    [Fact]
    public void WriteBytesWithoutLength_ThenReadBytesWithoutLength_ShouldWork()
    {
        using var stream = new ByteStream(50);
        
        var data = new byte[] { 10, 20, 30, 40 };
        stream.WriteBytesWithoutLength(data);
        
        stream.SetOffset(0);
        
        var result = stream.ReadBytesWithoutLength(data.Length);
        
        Assert.Equal(data, result.ToArray());
    }

    [Fact]
    public void WriteBytesWithoutLength_WithSpan_ShouldWork()
    {
        using var stream = new ByteStream(50);
        
        Span<byte> data = [100, 101, 102];
        stream.WriteBytesWithoutLength(data);
        
        stream.SetOffset(0);
        
        var result = stream.ReadBytesWithoutLength(data.Length);
        
        Assert.Equal(data.ToArray(), result.ToArray());
    }

    [Fact]
    public void ReadBytesWithoutLength_WithNegativeLength_ShouldReadToEnd()
    {
        using var stream = new ByteStream(20);
        
        var data = new byte[] { 1, 2, 3, 4, 5, 6 };
        stream.WriteBytesWithoutLength(data);
        
        stream.SetOffset(0);
        
        var result = stream.ReadBytesWithoutLength(-1);
        
        Assert.Equal(data, result.ToArray());
    }

    [Fact]
    public void WriteMultipleTypes_MixedSequence_ShouldReadCorrectly()
    {
        using var stream = new ByteStream(200);
        
        stream.WriteBoolean(true);
        stream.WriteI16(1234);
        stream.WriteString("Test");
        stream.WriteU32(56789);
        stream.WriteVInt32(-42);
        stream.WriteBytes([9, 8, 7]);
        stream.WriteBoolean(false);
        
        stream.SetOffset(0);
        
        Assert.True(stream.ReadBoolean());
        Assert.Equal(1234, stream.ReadI16());
        Assert.Equal("Test", stream.ReadString());
        Assert.Equal(56789u, stream.ReadU32());
        Assert.Equal(-42, stream.ReadVInt32());
        Assert.Equal(new byte[] { 9, 8, 7 }, stream.ReadBytes().ToArray());
        Assert.False(stream.ReadBoolean());
    }

    [Fact]
    public void EnsureCapacity_AutoExpand_ShouldWork()
    {
        using var stream = new ByteStream(5);
        
        for (var i = 0; i < 1000; i++)
            stream.WriteU8((byte)(i % 256));
        
        stream.SetOffset(0);
        
        for (var i = 0; i < 1000; i++)
            Assert.Equal((byte)(i % 256), stream.ReadU8());
    }

    [Fact]
    public void WriteThenRead_LargeData_ShouldNotCorrupt()
    {
        using var stream = new ByteStream(1000);
        
        var random = new Random(42);
        var strings = new List<string>();
        
        for (var i = 0; i < 100; i++)
        {
            var str = Path.GetRandomFileName();
            strings.Add(str);
            stream.WriteString(str);
            stream.WriteI32(random.Next());
            stream.WriteBoolean(random.Next(2) == 0);
        }
        
        stream.SetOffset(0);
        
        foreach (var expected in strings)
        {
            Assert.Equal(expected, stream.ReadString());
            stream.ReadI32();
            stream.ReadBoolean();
        }
    }

    [Fact]
    public void ReadVInt32_WhenBufferIncomplete_ShouldReadPartial()
    {
        var data = new byte[] { 0x80 };
        using var stream = new ByteStream(data);
        
        Assert.Throws<IndexOutOfRangeException>(() => stream.ReadVInt32());
    }

    [Fact]
    public void Endianness_MixedReads_ShouldWork()
    {
        using var stream = new ByteStream(20);
    
        stream.WriteI32(0x12345678);
        stream.WriteI32(0x12345678, false);
    
        stream.SetOffset(0);
    
        Assert.Equal(0x12345678, stream.ReadI32());
        Assert.Equal(0x12345678, stream.ReadI32(false));
    }
}