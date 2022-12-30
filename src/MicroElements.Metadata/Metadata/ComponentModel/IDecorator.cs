using System;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Marker interface for decorators.
    /// </summary>
    public interface IDecorator
    {
        /// <summary>
        /// Gets type that decorated by this decorator.
        /// </summary>
        Type ComponentType { get; }
    }

    /// <summary>
    /// Marker interface for decorators.
    /// </summary>
    /// <typeparam name="TComponent">Type to decorate.</typeparam>
    public interface IDecorator<out TComponent> : IDecorator
    {
        /// <inheritdoc />
        Type IDecorator.ComponentType => typeof(TComponent);

        /// <summary>
        /// Gets decorated component.
        /// </summary>
        TComponent Component { get; }
    }
}
