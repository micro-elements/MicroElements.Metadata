using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MicroElements.Functional;

namespace MicroElements.Metadata.Benchmarks
{
    //|            Method |      Mean |    Error |   StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|------------------ |----------:|---------:|---------:|------:|-------:|------:|------:|----------:|
    //|        WithSimple |  69.89 ns | 0.609 ns | 0.508 ns |  1.00 | 0.0086 |     - |     - |      72 B |
    //| WithMutableStruct |  74.08 ns | 0.469 ns | 0.416 ns |  1.06 | 0.0086 |     - |     - |      72 B |
    //|  WithMutableClass |  86.06 ns | 1.008 ns | 0.893 ns |  1.23 | 0.0162 |     - |     - |     136 B |
    //|    WithRecordData | 100.53 ns | 0.792 ns | 0.702 ns |  1.44 | 0.0238 |     - |     - |     200 B |
    [MemoryDiagnoser]
    [Config(typeof(CustomConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class BuilderBench2
    {
        string? parseResultErrorMessage = "error";

        [Benchmark(Baseline = true)]
        public Message MsgSimpleArr()
        {
            string originalMessage = "Property '{PropertyName}' failed to parse from string '{PropertyValue}'.";
            KeyValuePair<string, object>[] properties = new KeyValuePair<string, object>[3];

            properties[0] = new KeyValuePair<string, object>("PropertyName", "AAA");
            properties[1] = new KeyValuePair<string, object>("PropertyValue", "BBB");

            if (parseResultErrorMessage != null)
            {
                originalMessage += " Error: '{ParseResultError}'.";
                properties[2] = new KeyValuePair<string, object>("ParseResultError", parseResultErrorMessage);
            }

            Message message = new Message(originalMessage,
                MessageSeverity.Error,
                properties: properties);

            return message;
        }

        [Benchmark]
        public Message MsgSimpleDict()
        {
            var properties = new Dictionary<string, object>();

            string originalMessage = "Property '{PropertyName}' failed to parse from string '{PropertyValue}'.";
            properties["PropertyName"] = "AAA";
            properties["PropertyValue"] = "BBB";

            if (parseResultErrorMessage != null)
            {
                originalMessage += " Error: '{ParseResultError}'.";
                properties["ParseResultError"] = parseResultErrorMessage;
            }

            Message message = new Message(originalMessage,
                MessageSeverity.Error,
                properties: properties);

            return message;
        }

        [Benchmark]
        public Message WithNoIf()
        {
            var message =
                ValueMessageBuilder
                    .Error("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.", 3)
                    .AddProperty("PropertyName", "AAA")
                    .AddProperty("PropertyValue", "BBB")
                    .AppendToOriginalMessage(" Error: '{ParseResultError}'.")
                    .AddProperty("ParseResultError", parseResultErrorMessage)
                    .Build();
            return message;
        }

        [Benchmark]
        public Message WithIfEndIfNoLambda()
        {
            var message =
                ValueMessageBuilder
                    .Error("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.", 3)
                    .AddProperty("PropertyName", "AAA")
                    .AddProperty("PropertyValue", "BBB")
                    .If(parseResultErrorMessage != null)
                        .AppendToOriginalMessage(" Error: '{ParseResultError}'.")
                        .AddProperty("ParseResultError", parseResultErrorMessage)
                    .EndIf()
                    .Build();
            return message;
        }

        [Benchmark]
        public Message WithIfLambda()
        {
            var message =
                ValueMessageBuilder
                    .Error("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.")
                    .AddProperty("PropertyName", "AAA")
                    .AddProperty("PropertyValue", "BBB")
                    .If(parseResultErrorMessage != null, (ref ValueMessageBuilder parser) => parser
                        .AppendToOriginalMessage(" Error: '{ParseResultError}'.")
                        .AddProperty("ParseResultError", parseResultErrorMessage))
                    .Build();
            return message;
        }

        [Benchmark]
        public Message MsgBuilder()
        {
            Message message = new Message("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.", MessageSeverity.Error)
                .WithProperty("PropertyName", "AAA")
                .WithProperty("PropertyValue", "BBB");

            if (parseResultErrorMessage != null)
            {
                message = message
                    .WithText(message.OriginalMessage+" Error: '{ParseResultError}'.")
                    .WithProperty("ParseResultError", parseResultErrorMessage);
            }

            return message;
        }
    }
}
