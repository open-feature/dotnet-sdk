using BenchmarkDotNet.Running;

namespace OpenFeature.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<OpenFeatureClientBenchmarks>();
        }
    }
}
