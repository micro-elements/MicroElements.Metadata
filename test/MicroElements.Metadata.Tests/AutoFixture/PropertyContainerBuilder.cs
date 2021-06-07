using System;
using System.Linq;
using AutoFixture.Kernel;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.Tests.AutoFixture
{
    // TODO: move to MicroElements.Metadata.AutoFixture package
    public class PropertyContainerBuilder : ISpecimenBuilder
    {
        private readonly Random _random = new Random(0);

        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (request is Type type && type.IsAssignableTo<IPropertyContainer>())
            {
                IPropertySet knownPropertySet = type.GetSchemaByKnownPropertySet();

                if (knownPropertySet != null)
                {
                    var propertyContainer = new MutablePropertyContainer();
                    foreach (IProperty property in knownPropertySet.GetProperties())
                    {
                        object value = null;

                        if (property.GetAllowedValuesUntyped() is { ValuesUntyped: { } allowedValues })
                        {
                            int index = _random.Next(0, allowedValues.Count - 1);
                            value = allowedValues.ToArray()[index];
                        }
                        else
                        {
                            value = context.Resolve(property.Type);
                        }

                        if (property.GetOrEvaluateNullability().IsNullAllowed)
                        {

                        }

                        if (property.GetSchema() is { } propertySchema)
                        {
                            var schemaMetadata = propertySchema.GetSchemaMetadata().ToArray();
                        }

                        propertyContainer.WithValueUntyped(property, value);
                    }

                    IPropertyContainer result = propertyContainer.ToPropertyContainerOfType(type);
                    return result;
                }
            }

            return new NoSpecimen();
        }
    }
}
