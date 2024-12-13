// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Serialization;
using MicroElements.Reflection.TypeExtensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.Swashbuckle;

/// <summary>
/// Configures serialization for <see cref="IPropertyContainer"/> as for collections.
/// Also <see cref="IPropertyValue"/> replaces for <see cref="PropertyValueContract"/> because swagger generates schemas for many unnecessary system types.
/// </summary>
public class MetadataSerializerBehavior : ISerializerDataContractResolver
{
    private readonly ISerializerDataContractResolver _dataContractResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataSerializerBehavior"/> class.
    /// </summary>
    /// <param name="dataContractResolver">Base service.</param>
    public MetadataSerializerBehavior(ISerializerDataContractResolver dataContractResolver)
    {
        _dataContractResolver = dataContractResolver;
    }

    /// <inheritdoc />
    public DataContract GetDataContractForType(Type type)
    {
        if (type.IsAssignableTo<IPropertyContainer>())
        {
            // Array is used for schema inlining.
            // If IPropertyContainer is not collection then schema will be generated as a reference but we need to make it dependent on PropertySet.
            return DataContract.ForArray(type, typeof(PropertyValueContract));
        }

        return _dataContractResolver.GetDataContractForType(type);
    }
}