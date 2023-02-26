using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace MicroElements.Metadata.Benchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(CustomConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ConfigureStructOrStructRef
    {
        public class MutableData
        {
            public string Text { get; set; }
            public double Number { get; set; }
        }

        public readonly struct ExcelMeta
        {
            public MutableData Data1 { get; }
            public MutableData Data2 { get; }
            public MutableData Data3 { get; }
            public MutableData Data4 { get; }
            public MutableData Data5 { get; }

            public ExcelMeta(MutableData data1, MutableData data2, MutableData data3, MutableData data4, MutableData data5)
            {
                Data1 = data1;
                Data2 = data2;
                Data3 = data3;
                Data4 = data4;
                Data5 = data5;
            }
        }

        public delegate void RefAction<T>(ref T value);

        public static Action<ExcelMeta> Configure = (meta => meta.Data1.Text = "text");
        public static RefAction<ExcelMeta> ConfigureRef = (ref ExcelMeta meta) => meta.Data1.Text = "text";

        public static MutableData Data1 = new MutableData();
        public static MutableData Data2 = new MutableData();
        public static MutableData Data3 = new MutableData();
        public static MutableData Data4 = new MutableData();
        public static MutableData Data5 = new MutableData();

        [Benchmark(Baseline = true)]
        public string ConfigureStruct()
        {
            ExcelMeta excelMeta = new ExcelMeta(Data1, Data2, Data3, Data4, Data5);
            Configure(excelMeta);
            return excelMeta.Data1.Text;
        }

        [Benchmark]
        public string ConfigureStructRef()
        {
            ExcelMeta excelMeta = new ExcelMeta(Data1, Data2, Data3, Data4, Data5);
            ConfigureRef(ref excelMeta);
            return excelMeta.Data1.Text;
        }
    }
}
