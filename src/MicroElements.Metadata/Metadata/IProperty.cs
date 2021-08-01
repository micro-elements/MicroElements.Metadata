// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents property description.
    /// </summary>
    public interface IProperty :
        ISchema,
        ISchemaDescription,
        IHasAlias
    {
    }

    /// <summary>
    /// Strong typed property description.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IProperty<out T> :
        ISchema<T>,
        IProperty,
        IHasDefaultValue<T>,
        IHasExamples<T>
    {
    }

    /// <summary>
    /// Strong typed property with calculator.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyWithCalculator<T> :
        IProperty<T>,
        IHasCalculator<T>
    {
    }
}
