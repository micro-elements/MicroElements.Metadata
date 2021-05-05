using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace MicroElements.Metadata.Benchmarks
{
    public interface IResult
    {
        object Value { get; }

        bool IsSuccess { get; }

        string Message { get; }
    }

    public readonly struct ResultStruct : IResult
    {
        public object Value { get; }

        public bool IsSuccess { get; }

        public string Message { get; }

        public ResultStruct(object value, bool isSuccess, string message)
        {
            Value = value;
            IsSuccess = isSuccess;
            Message = message;
        }
    }


    public class ResultClass : IResult
    {
        public object Value { get; }

        public bool IsSuccess { get; }

        public string Message { get; }

        public ResultClass(object value, bool isSuccess, string message)
        {
            Value = value;
            IsSuccess = isSuccess;
            Message = message;
        }
    }


    [MemoryDiagnoser]
    [Config(typeof(CustomConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class StructVsClass
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        ResultStruct ParseAsStruct(int i)
        {
            return new ResultStruct("value", true, i.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        ResultClass ParseAsClass(int i)
        {
            return new ResultClass("value", true, i.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        string UseResult(IResult result)
        {
            return result.Value.ToString();
        }

        [Params(100_000, 1_000_000)]
        public int N;


        [Benchmark]
        public void ParseAsStructBench()
        {
            for (int i = 0; i < N; i++)
            {
                ResultStruct result = ParseAsStruct(i);

                // BOXING!!!
                UseResult(result);
            }
        }

        [Benchmark(Baseline = true)]
        public void ParseAsClassBench()
        {
            for (int i = 0; i < N; i++)
            {
                ResultClass result = ParseAsClass(i);
                UseResult(result);
            }
        }
    }
}
