using System;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Experimental;
using MicroElements.Metadata.OpenXml.Excel.Styling;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    public delegate void RefAction<T>(ref T value);

    public sealed record ConfigureAction<T>
    {
        public RefAction<T>? RefAction { get; }

        public string? Description { get; }

        public ConfigureAction(RefAction<T> configure, string? description = null)
        {
            RefAction = configure.AssertArgumentNotNull(nameof(configure));
            Description = description;
        }
    }

    public static class ConfigureExtensions
    {
        public static ImmutableChain<T> CombineWith<T>(this ImmutableChain<T>? chain, T value, CombineMode combineMode = CombineMode.AppendToEnd)
        {
            switch (combineMode)
            {
                case CombineMode.AppendToEnd:
                    return chain.Append(value);
                case CombineMode.AppendToStart:
                    return chain.Prepend(value);
                case CombineMode.Set:
                    return ImmutableChain.Create(value);
            }

            throw new InvalidOperationException();
        }

        public static void ConfigureChain<T>(this ImmutableChain<ConfigureAction<T>>? configureChain, ref T value)
        {
            if (configureChain?.Values is { } configureActions)
            {
                foreach (var configure in configureActions)
                {
                    configure.RefAction?.Invoke(ref value);
                }
            }
        }
    }
}
