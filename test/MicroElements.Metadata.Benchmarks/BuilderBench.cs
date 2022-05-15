using System;
using System.Diagnostics.Contracts;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Benchmarks
{
    //|            Method |     Mean |    Error |   StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|------------------ |---------:|---------:|---------:|------:|-------:|------:|------:|----------:|
    //|        WithSimple | 68.66 ns | 0.287 ns | 0.268 ns |  1.00 | 0.0086 |     - |     - |      72 B |
    //| WithMutableStruct | 77.62 ns | 0.403 ns | 0.377 ns |  1.13 | 0.0086 |     - |     - |      72 B |
    //|  WithMutableClass | 83.32 ns | 0.357 ns | 0.334 ns |  1.21 | 0.0162 |     - |     - |     136 B |
    //|    WithRecordData | 95.96 ns | 0.301 ns | 0.281 ns |  1.40 | 0.0238 |     - |     - |     200 B |
    [MemoryDiagnoser]
    [Config(typeof(CustomConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class BuilderBench
    {
        public static Property<string> Property = new Property<string>("Name1");

        [Benchmark(Baseline = true)]
        public Property<string> WithSimple()
        {
            return Property.With(name: "Name2");
        }

        [Benchmark]
        public Property<string> WithMutableStruct()
        {
            return Property.WithRewriteFast((ref PropertyData<string> data) => data.Name = "Name2");
        }

        [Benchmark]
        public Property<string> WithMutableClass()
        {
            return Property.WithRewrite(data => data.Name = "Name2");
        }

        [Benchmark]
        public Property<string> WithRecordData()
        {
            return Property.WithRecord(data => data with {Name = "Name2"});
        }
    }

    public record PropertyRecordData<T>(
        string Name,
        string? Description,
        string? Alias,
        IDefaultValue<T>? DefaultValue,
        IPropertyCalculator<T>? Calculator,
        IExamples<T> Examples
    );

    public static class PropertyExtensions
    {
        [Pure]
        public static Property<T> WithRecord<T>(
            this IProperty<T> source,
            Func<PropertyRecordData<T>, PropertyRecordData<T>> configure)
        {
            source.AssertArgumentNotNull(nameof(source));

            PropertyRecordData<T> sourceData = new PropertyRecordData<T>(
                Name: source.Name,
                source.Description,
                source.Alias,
                source.DefaultValue,
                source.Calculator,
                source.Examples);

            PropertyRecordData<T> resultData = configure(sourceData);

            Property<T> property = new Property<T>(
                name: resultData.Name,
                description: resultData.Description,
                defaultValue: resultData.DefaultValue,
                examples: resultData.Examples,
                calculator: resultData.Calculator,
                alias: source.Alias);

            source.CopyMetadataTo(property);

            return property;
        }

        public static void Deconstruct<T>(
            this IProperty<T> source,
            out string name,
            out string? description,
            out string? alias,
            out IDefaultValue<T>? defaultValue,
            out IPropertyCalculator<T>? calculator,
            out IExamples<T> examples
        )
        {
            name = source.Name;
            description = source.Description;
            alias = source.Alias;
            defaultValue = source.DefaultValue;
            calculator = source.Calculator;
            examples = source.Examples;
        }


    }
}
