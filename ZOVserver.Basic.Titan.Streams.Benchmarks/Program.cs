using BenchmarkDotNet.Running;

namespace ZOVserver.Basic.Titan.Streams.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ByteStreamBenchmarks>();
        BenchmarkRunner.Run<BitStreamBenchmarks>();
    }
}