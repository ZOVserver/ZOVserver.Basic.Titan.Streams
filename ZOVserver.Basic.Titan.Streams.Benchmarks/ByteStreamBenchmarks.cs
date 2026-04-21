using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Configs;

namespace ZOVserver.Basic.Titan.Streams.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 10, warmupCount: 3, invocationCount: 1000)]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[DisassemblyDiagnoser]
public class ByteStreamBenchmarks
{
    private byte[] _smallBuffer = null!;
    private byte[] _mediumBuffer = null!;
    private byte[] _largeBuffer = null!;
    private string _longString = null!;
    private Random _random = null!;
    private readonly int[] _testValues = new int[1000];

    [GlobalSetup]
    public void Setup()
    {
        _random = Random.Shared;
        _smallBuffer = new byte[32];
        _mediumBuffer = new byte[1024];
        _largeBuffer = new byte[1024 * 1024];
        
        _random.NextBytes(_smallBuffer);
        _random.NextBytes(_mediumBuffer);
        _random.NextBytes(_largeBuffer);
        
        _longString = string.Create<object?>(30000, null, (span, _) =>
        {
            span.Fill('A');
            new Span<char>(new char[10000]).Fill('B');
            new Span<char>(new char[10000]).Fill('C');
        });
        
        for (var i = 0; i < _testValues.Length; i++)
            _testValues[i] = _random.Next();
    }

    #region Write Operations

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteBoolean_1000()
    {
        using var stream = new ByteStream(128);
        for (var i = 0; i < 1000; i++)
            stream.WriteBoolean((i & 1) == 0);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteU8_1000()
    {
        using var stream = new ByteStream(1024);
        for (var i = 0; i < 1000; i++)
            stream.WriteU8((byte)i);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteI32_1000()
    {
        using var stream = new ByteStream(4096);
        ref var values = ref MemoryMarshal.GetArrayDataReference(_testValues);
        for (var i = 0; i < 1000; i++)
            stream.WriteI32(Unsafe.Add(ref values, i));
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteI64_1000()
    {
        using var stream = new ByteStream(8192);
        for (var i = 0; i < 1000; i++)
            stream.WriteI64((long)i * 1_000_000L);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteI128_100()
    {
        using var stream = new ByteStream(1600);
        for (var i = 0; i < 100; i++)
            stream.WriteI128(Int128.MaxValue - i);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteVInt32_1000_Small()
    {
        using var stream = new ByteStream(4096);
        for (var i = 0; i < 1000; i++)
            stream.WriteVInt32(i & 63);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteVInt32_1000_Large()
    {
        using var stream = new ByteStream(4096);
        for (var i = 0; i < 1000; i++)
            stream.WriteVInt32(i * 100_000);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteVInt64_1000()
    {
        using var stream = new ByteStream(8192);
        for (var i = 0; i < 1000; i++)
            stream.WriteVInt64((long)i * 1_000_000_000L);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteString_Short_1000()
    {
        using var stream = new ByteStream(32768);
        for (var i = 0; i < 1000; i++)
            stream.WriteString($"Item_{i}");
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteString_Long_100()
    {
        using var stream = new ByteStream(100_000);
        for (var i = 0; i < 100; i++)
            stream.WriteString(_longString);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteCompressedString_100()
    {
        using var stream = new ByteStream(50_000);
        for (var i = 0; i < 100; i++)
            stream.WriteCompressedString(_longString);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteBytes_Small_1000()
    {
        using var stream = new ByteStream(65_536);
        for (var i = 0; i < 1000; i++)
            stream.WriteBytes(_smallBuffer);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteBytes_Medium_100()
    {
        using var stream = new ByteStream(131_072);
        for (var i = 0; i < 100; i++)
            stream.WriteBytes(_mediumBuffer);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 10)]
    public void WriteBytes_Large_10()
    {
        using var stream = new ByteStream(11 * 1024 * 1024);
        for (var i = 0; i < 10; i++)
            stream.WriteBytes(_largeBuffer);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteBytesWithoutLength_Medium_100()
    {
        using var stream = new ByteStream(131_072);
        for (var i = 0; i < 100; i++)
            stream.WriteBytesWithoutLength(_mediumBuffer);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteMixedTypes_100()
    {
        using var stream = new ByteStream(65_536);
        for (var i = 0; i < 100; i++)
        {
            stream.WriteBoolean(true);
            stream.WriteI16((short)i);
            stream.WriteString($"Test_{i}");
            stream.WriteU32((uint)i * 1000);
            stream.WriteVInt32(i - 500);
            stream.WriteBytes([(byte)i, (byte)(i >> 8)]);
        }
    }

    #endregion

    #region Read Operations

    private ByteStream PrepareStreamForRead(Action<ByteStream> writeAction)
    {
        var stream = new ByteStream(1024);
        writeAction(stream);
        stream.SetOffset(0);
        return stream;
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadBoolean_1000()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteBoolean((i & 1) == 0);
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadBoolean();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadU8_1000()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteU8((byte)i);
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadU8();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadI32_1000()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteI32(i);
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadI32();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadVInt32_1000_Small()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteVInt32(i & 63);
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadVInt32();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadString_Short_1000()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteString($"Item_{i}");
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadString();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 100)]
    public void ReadCompressedString_100()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 100; i++)
                s.WriteCompressedString(_longString);
        });
        
        for (var i = 0; i < 100; i++)
            stream.ReadCompressedString();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadBytes_Small_1000()
    {
        using var stream = PrepareStreamForRead(s =>
        {
            for (var i = 0; i < 1000; i++)
                s.WriteBytes(_smallBuffer);
        });
        
        for (var i = 0; i < 1000; i++)
            stream.ReadBytes();
    }

    #endregion

    #region Scenarios

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 1000)]
    public void Scenario_SerializeDeserialize_1000Objects()
    {
        using var stream = new ByteStream(65_536);
        
        for (var i = 0; i < 1000; i++)
        {
            stream.WriteVInt32(i);
            stream.WriteString($"User_{i}");
            stream.WriteI64(i * 1000L);
            stream.WriteBoolean((i & 1) == 0);
        }
        
        stream.SetOffset(0);
        for (var i = 0; i < 1000; i++)
        {
            stream.ReadVInt32();
            stream.ReadString();
            stream.ReadI64();
            stream.ReadBoolean();
        }
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 1000)]
    public void Scenario_NetworkPacket_1000()
    {
        using var stream = new ByteStream(4096);
        
        for (var packetId = 0; packetId < 1000; packetId++)
        {
            stream.ResetOffset();
            stream.WriteI32(packetId);
            stream.WriteU8(0x01);
            stream.WriteVInt32(packetId * 100);
            stream.WriteString($"Packet_{packetId}_Data");
            stream.WriteBytes(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
            
            stream.SetOffset(0);
            _ = stream.ReadI32();
            _ = stream.ReadU8();
            _ = stream.ReadVInt32();
            _ = stream.ReadString();
            _ = stream.ReadBytes();
        }
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 10_000)]
    public void Scenario_ExpandBuffer_FromSmall()
    {
        using var stream = new ByteStream(8);
        
        for (var i = 0; i < 10_000; i++)
            stream.WriteU8((byte)i);
        
        stream.SetOffset(0);
        for (var i = 0; i < 10_000; i++)
            stream.ReadU8();
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 500)]
    public void Scenario_Endianness_Mixed()
    {
        using var stream = new ByteStream(1024);
        
        for (var i = 0; i < 500; i++)
        {
            stream.WriteI32(i, true);
            stream.WriteI32(i, false);
            stream.WriteI16((short)i, true);
            stream.WriteI16((short)i, false);
        }
        
        stream.SetOffset(0);
        for (var i = 0; i < 500; i++)
        {
            stream.ReadI32(true);
            stream.ReadI32(false);
            stream.ReadI16(true);
            stream.ReadI16(false);
        }
    }

    #endregion

    #region Allocation Tests

    [BenchmarkCategory("Allocation"), Benchmark(OperationsPerInvoke = 1000)]
    public void Allocation_NewStreamEachWrite()
    {
        for (var i = 0; i < 1000; i++)
        {
            using var stream = new ByteStream(256);
            stream.WriteI32(i);
            stream.WriteString($"Value_{i}");
        }
    }

    [BenchmarkCategory("Allocation"), Benchmark(OperationsPerInvoke = 1000)]
    public void Allocation_ReuseStream()
    {
        using var stream = new ByteStream(256);
        for (var i = 0; i < 1000; i++)
        {
            stream.ResetOffset();
            stream.WriteI32(i);
            stream.WriteString($"Value_{i}");
        }
    }

    #endregion
}