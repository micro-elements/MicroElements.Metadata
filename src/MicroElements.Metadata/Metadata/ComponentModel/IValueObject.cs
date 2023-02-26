using System;

namespace MicroElements.Metadata.ComponentModel
{
    public interface IValueObject
    {
        object? Value { get; }
    }

    public interface IValueObject<out TBaseValue> : IValueObject
    {
        /// <inheritdoc />
        object? IValueObject.Value => Value;

        TBaseValue? Value { get; }
    }

    /// <summary>
    /// Generic builder interface.
    /// </summary>
    public interface IValueObjectBuilder
    {
    }

    /// <summary>
    /// Generic builder that knows how to build with the provided component type.
    /// It's supposed to be used with immutable objects.
    /// For mutable objects use <see cref="ICompositeSetter{TComponent}"/>.
    /// </summary>
    /// <typeparam name="TBaseValue">Component type.</typeparam>
    public interface IValueObjectBuilder<in TBaseValue> : IValueObjectBuilder
    {
        /// <summary>
        /// Creates a copy of the source with provided <paramref name="component"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The source copy or the source itself in case of mutable object.</returns>
        object Create(TBaseValue component);
    }

    public interface IValueObjectBuilder<out TValueObject, TBaseValue> :
        IValueObjectBuilder<TBaseValue>,
        IValueObject<TBaseValue>
    {
        /// <inheritdoc />
        object IValueObjectBuilder<TBaseValue>.Create(TBaseValue component) => Create(component);

        /// <summary>
        /// Creates a copy of the source with provided <paramref name="component"/>.
        /// </summary>
        /// <param name="value">The component.</param>
        /// <returns>The source copy or the source itself in case of mutable object.</returns>
        TValueObject Create(TBaseValue value);

        TValueObject CreateAuto(TBaseValue value)
        {
            return (TValueObject)Activator.CreateInstance(typeof(TValueObject), value);
        }

        //static abstract TValueObject CreateStatic(TComponent component);
    }

    // public static class aaaa
    // {
    //     public static T Create<T>()
    //     {
    //
    //     }
    // }

    public readonly record struct Cur(string Value) : IValueObjectBuilder<Cur, string>
    {
        /// <inheritdoc />
        public Cur Create(string value) => new Cur(value);
    }

    public static class aaa
    {
        public static T As<T>(this string value)
            where T : struct, IValueObjectBuilder<T, string>
        {
            return default(T).Create(value);
        }
    }
}
