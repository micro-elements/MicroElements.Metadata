using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace MicroElements.Metadata.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StructVsClass>();
        }
    }

    class CustomConfig : ManualConfig
    {
        public CustomConfig()
        {
            Add(MemoryDiagnoser.Default);
        }
    }
}
