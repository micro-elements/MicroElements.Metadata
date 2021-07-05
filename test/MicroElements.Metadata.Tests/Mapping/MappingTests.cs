using System;
using System.Collections.Generic;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.Mapping;
using MicroElements.Metadata.Schema;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests.Mapping
{
    public class MappingTests
    {
        public enum Sex { Unknown, Male, Female }

        public class Model
        {
            public string TextValue { get; set; }

            public int IntValue { get; set; }

            public Sex Sex { get; set; }
        }

        public class ModelScheme : IStaticSchema, IStaticPropertySet
        {
            public static IProperty<string> TextValue = new Property<string>("TextValue");
            public static IProperty<int> IntValue = new Property<int>("IntValue");

            // Enum as string
            public static IProperty<string> Sex = new Property<string>("Sex")
                .SetAllowedValuesFromEnum<Sex>();
        }

        [Fact]
        public void MapSimple()
        {
            Model model1 = new Model() {TextValue = "text", IntValue = 42, Sex = Sex.Female};
            PropertyContainer<ModelScheme> container = model1.MapToContainer<ModelScheme>();
            Model model2 = container.MapToObject<Model>();
            model2.Should().BeEquivalentTo(model1);
        }

        [Fact]
        public void Extract()
        {
            Model model = new Model() { TextValue = "text", IntValue = 42, Sex = Sex.Male};
            PropertyContainer<ModelScheme> container = model.MapToContainer<ModelScheme>();

            container
                .Extractor()
                .Required(ModelScheme.TextValue, out string textValue)
                .Required(ModelScheme.IntValue, out int intValue)
                .Required(ModelScheme.Sex, Enum.Parse<Sex>, out Sex sex)
                .ExtractErrors(out var messages);

            messages.Should().BeNullOrEmpty();
            textValue.Should().Be("text");
            intValue.Should().Be(42);
            sex.Should().Be(Sex.Male);
        }

        [Fact]
        public void extract_dsl()
        {
            var container = new MutablePropertyContainer()
                .WithValue(ModelScheme.TextValue, "text")
                .WithValue(ModelScheme.Sex, Sex.Male.ToString());

            Model model = new Model();

            container
                .Extractor()
                .Extract(ModelScheme.TextValue).Required().Output(out string textValue)
                .Extract(ModelScheme.TextValue).WithValidation(property => property.NotNull()).Output(out string textValue2)
                .Extract(ModelScheme.IntValue).Optional().Output(out int? intValue)
                .Extract(ModelScheme.Sex).Map(Enum.Parse<Sex>).WithValidation(property => property.NotDefault()).Output(out Sex sex)
                .Extract(ModelScheme.Sex).MapToEnum<Sex>().Output(out Sex sex2)
                .Extract(ModelScheme.Sex).MapToEnum<Sex>().Output(model)
                .ExtractErrors(out IReadOnlyCollection<Message>? messages);

            messages.Should().BeNullOrEmpty();
            textValue.Should().Be("text");
            textValue2.Should().Be("text");
            intValue.Should().Be(null);
            sex.Should().Be(Sex.Male);
            sex2.Should().Be(Sex.Male);
            model.Sex.Should().Be(Sex.Male);
        }

        [Fact]
        public void extract_map_error()
        {
            {
                var container = new MutablePropertyContainer()
                    .WithValue(ModelScheme.Sex, "other");

                container
                    .Extractor()
                    .Extract(ModelScheme.Sex).MapToEnum<Sex>().Output(out Sex sex)
                    .ExtractErrors(out IReadOnlyCollection<Message>? messages);

                messages.Should().HaveCount(1);
            }

            {
                var container = new MutablePropertyContainer()
                    .WithValue(ModelScheme.Sex, Sex.Unknown.ToString());

                container
                    .Extractor()
                    .Extract(ModelScheme.Sex).Map(Enum.Parse<Sex>).WithValidation(property => property.NotDefault()).Output(out Sex sex)
                    .ExtractErrors(out IReadOnlyCollection<Message>? messages);

                messages.Should().HaveCount(1);
            }
        }

        [Fact]
        public void extract_map_error2()
        {
            var container = new MutablePropertyContainer()
                .WithValue(ModelScheme.TextValue, "text")
                .WithValue(ModelScheme.Sex, "male");

            container
                .Extractor()
                .Extract(ModelScheme.Sex).MapToEnum<Sex>().Map(enumValue => (int)enumValue).Output(out int sex)
                .ExtractErrors(out IReadOnlyCollection<Message>? messages);

            IProperty<Sex> allowedValues = ModelScheme.Sex.MapToEnum<Sex>().SetAllowedValues(Sex.Male);

            messages.Should().BeNullOrEmpty();
            sex.Should().Be(1);
        }

        [Fact]
        public void ExtractToTuple()
        {
            Model model = new Model() { TextValue = "text", IntValue = 42, Sex = Sex.Female};
            PropertyContainer<ModelScheme> container = model.MapToContainer<ModelScheme>();

            {
                (var textValue, int intValue) = container.ToTuple(ModelScheme.TextValue, ModelScheme.IntValue);
                textValue.Should().Be("text");
                intValue.Should().Be(42);
            }

            {
                (var textValue, int intValue, Sex sex) = container.ToTuple(ModelScheme.TextValue, ModelScheme.IntValue, ModelScheme.Sex.MapToEnum<Sex>());
                textValue.Should().Be("text");
                intValue.Should().Be(42);
                sex.Should().Be(Sex.Female);
            }
            
        }

        [Fact]
        public void extract_required_absent_value_should_add_error()
        {
            var container = new MutablePropertyContainer()
                .WithValue(ModelScheme.TextValue, "text");
            
            container
                .Extractor()
                .Required(ModelScheme.TextValue, out string textValue)
                .Required(ModelScheme.IntValue, out int intValue)
                .ExtractErrors(out var messages);

            messages.Should().HaveCount(1);
        }

        [Fact]
        public void extract_optional_absent_value_should_add_error()
        {
            var container = new MutablePropertyContainer()
                .WithValue(ModelScheme.TextValue, "text");

            container
                .Extractor()
                .Required(ModelScheme.TextValue, out string textValue)
                .Optional(ModelScheme.IntValue, out int? intValue)
                .ExtractErrors(out var messages);

            messages.Should().BeNullOrEmpty();
            textValue.Should().Be("text");
            intValue.Should().Be(null);
        }
    }
}
