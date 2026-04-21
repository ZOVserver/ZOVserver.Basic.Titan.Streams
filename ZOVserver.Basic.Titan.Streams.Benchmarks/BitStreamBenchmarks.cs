using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Configs;

namespace ZOVserver.Basic.Titan.Streams.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 10, warmupCount: 3, invocationCount: 1000)]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BitStreamBenchmarks
{
    private Random _random = null!;
    private uint[] _testValues = null!;

    [GlobalSetup]
    public void Setup()
    {
        _random = Random.Shared;
        
        _testValues = new uint[1000];
        for (var i = 0; i < _testValues.Length; i++)
            _testValues[i] = (uint)_random.Next(1, 100_000);
    }

    #region Write Operations

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteBoolean_1000()
    {
        using var writer = new BitStream(128);
        for (var i = 0; i < 1000; i++)
            writer.WriteBoolean((i & 1) == 0);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveInt_4bits_1000()
    {
        using var writer = new BitStream(512);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveInt((uint)(i & 15), 4);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveInt_8bits_1000()
    {
        using var writer = new BitStream(1024);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveInt((uint)(i & 255), 8);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveInt_16bits_1000()
    {
        using var writer = new BitStream(2048);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveInt((uint)(i & 65535), 16);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveInt_27bits_1000()
    {
        using var writer = new BitStream(4096);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveInt((uint)i & 0x7FFFFFF, 27);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteInt_1bit_1000()
    {
        using var writer = new BitStream(128);
        for (var i = 0; i < 1000; i++)
            writer.WriteInt((i & 1) == 0 ? 0 : -1, 1);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteInt_15bits_1000()
    {
        using var writer = new BitStream(2048);
        for (var i = 0; i < 1000; i++)
            writer.WriteInt((i & 32767) - 16384, 15);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveVInt_1000_Small()
    {
        using var writer = new BitStream(4096);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveVInt((uint)(i & 255), 4);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveVInt_1000_Large()
    {
        using var writer = new BitStream(8192);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveVInt(_testValues[i], 4);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveVIntMax255_1000()
    {
        using var writer = new BitStream(2048);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveVIntMax255((uint)(i & 255));
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveVIntMax65535_1000()
    {
        using var writer = new BitStream(8192);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveVIntMax65535((uint)(i & 65535));
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WritePositiveVIntMax255OftenZero_1000()
    {
        using var writer = new BitStream(2048);
        for (var i = 0; i < 1000; i++)
            writer.WritePositiveVIntMax255OftenZero((i & 1) == 0 ? 0u : (uint)(i & 255));
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 1000)]
    public void WriteIntMax65535_1000()
    {
        using var writer = new BitStream(8192);
        for (var i = 0; i < 1000; i++)
            writer.WriteIntMax65535((i & 1) == 0 ? i : -i);
    }

    [BenchmarkCategory("Write"), Benchmark(OperationsPerInvoke = 100)]
    public void WriteMixedTypes_100()
    {
        using var writer = new BitStream(4096);
        for (var i = 0; i < 100; i++)
        {
            writer.WriteBoolean((i & 1) == 0);
            writer.WritePositiveInt((uint)i, 10);
            writer.WriteInt(i - 500, 10);
            writer.WritePositiveVInt((uint)i, 4);
            writer.WritePositiveVIntMax255OftenZero((i & 3) == 0 ? 0u : (uint)i);
        }
    }

    #endregion

    #region Read Operations

    private BitStream PrepareStreamForRead(Action<BitStream> writeAction)
    {
        var stream = new BitStream(4096);
        writeAction(stream);
        return new BitStream(stream.GetByteArray());
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadBoolean_1000()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WriteBoolean((i & 1) == 0);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadBoolean();
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveInt_4bits_1000()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveInt((uint)(i & 15), 4);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveInt(4);
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveInt_8bits_1000()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveInt((uint)(i & 255), 8);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveInt(8);
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveInt_16bits_1000()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveInt((uint)(i & 65535), 16);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveInt(16);
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveVInt_1000_Small()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveVInt((uint)(i & 255), 4);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveVInt(4);
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveVInt_1000_Large()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveVInt(_testValues[i], 4);
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveVInt(4);
    }

    [BenchmarkCategory("Read"), Benchmark(OperationsPerInvoke = 1000)]
    public void ReadPositiveVIntMax255OftenZero_1000()
    {
        var reader = PrepareStreamForRead(w =>
        {
            for (var i = 0; i < 1000; i++)
                w.WritePositiveVIntMax255OftenZero((i & 1) == 0 ? 0u : (uint)(i & 255));
        });
        
        for (var i = 0; i < 1000; i++)
            reader.ReadPositiveVIntMax255OftenZero();
    }

    #endregion

    #region Scenarios

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 1000)]
    public void Scenario_CompressedBitStream_1000()
    {
        using var writer = new BitStream(4096);
        
        for (var i = 0; i < 1000; i++)
        {
            writer.WritePositiveVIntMax255OftenZero(i % 10 == 0 ? (uint)i : 0u);
            writer.WritePositiveVIntMax65535((uint)i);
            writer.WriteBoolean((i & 1) == 0);
        }
        
        var data = writer.GetByteArray();
        var reader = new BitStream(data);
        
        for (var i = 0; i < 1000; i++)
        {
            reader.ReadPositiveVIntMax255OftenZero();
            reader.ReadPositiveVIntMax65535();
            reader.ReadBoolean();
        }
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 1000)]
    public void Scenario_AlternatingBitSizes_1000()
    {
        using var writer = new BitStream(8192);
        
        for (var i = 0; i < 1000; i++)
        {
            writer.WritePositiveInt((uint)i, 3);
            writer.WritePositiveInt((uint)i, 5);
            writer.WritePositiveInt((uint)i, 7);
            writer.WritePositiveInt((uint)i, 9);
            writer.WritePositiveInt((uint)i, 11);
            writer.WritePositiveInt((uint)i, 13);
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 1000; i++)
        {
            reader.ReadPositiveInt(3);
            reader.ReadPositiveInt(5);
            reader.ReadPositiveInt(7);
            reader.ReadPositiveInt(9);
            reader.ReadPositiveInt(11);
            reader.ReadPositiveInt(13);
        }
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 10_000)]
    public void Scenario_ByteBoundaryStress_10000()
    {
        using var writer = new BitStream(64);
        
        for (var i = 0; i < 10_000; i++)
        {
            writer.WriteBoolean(true);
            writer.WritePositiveInt((uint)i, 7);
            writer.WriteBoolean(false);
            writer.WritePositiveInt((uint)i, 9);
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 10_000; i++)
        {
            reader.ReadBoolean();
            reader.ReadPositiveInt(7);
            reader.ReadBoolean();
            reader.ReadPositiveInt(9);
        }
    }

    [BenchmarkCategory("Scenario"), Benchmark(OperationsPerInvoke = 1000)]
    public void Scenario_VIntOptimization_1000()
    {
        using var writer = new BitStream(4096);
        
        for (var i = 0; i < 1000; i++)
        {
            writer.WritePositiveVIntMax255OftenZero((uint)(i % 50));
            writer.WritePositiveVIntMax255OftenZero(0u);
            writer.WritePositiveVIntMax255OftenZero((uint)(i % 3));
        }
        
        var reader = new BitStream(writer.GetByteArray());
        
        for (var i = 0; i < 1000; i++)
        {
            reader.ReadPositiveVIntMax255OftenZero();
            reader.ReadPositiveVIntMax255OftenZero();
            reader.ReadPositiveVIntMax255OftenZero();
        }
    }

    #endregion
}