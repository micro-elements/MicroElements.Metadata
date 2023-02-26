using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace MicroElements.Metadata.Benchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(CustomConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class MapBench
    {
        public static Property<string> Prop1 = new Property<string>("Prop1");
        public static Property<string> Name = new Property<string>("Name");
        public static IPropertyContainer Container = new MutablePropertyContainer()
            .WithValue(Prop1, "Prop1")
            .WithValue(Name, "Alex");

        IProperty<string> mappedName1 = Name.Map(name => name);
        IProperty<string> mappedName2 = Name.MapNew(name => name);

        IProperty<string> mappedName3 = Name.Map(value => "");

        [Benchmark]
        public string NoMap()
        {
            return Container.GetValue(Name);
        }

        [Benchmark]
        public string Map()
        {
            return Container.GetValue(mappedName1);
        }

        [Benchmark(Baseline = true)]
        public string MapNew()
        {
            DefaultSearchAlgorithm.Instance.GetPropertyValue2(Container, mappedName2, null, out var result);
            return result.Value;
        }
    }
}
