using System;
using System.Linq;
using System.Xml;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Xml;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyTests
    {
        [Fact]
        public void empty_property_should_be_empty()
        {
            Property<string>.Empty.ShouldBeEmpty();
            Property<int>.Empty.ShouldBeEmpty();
        }

        [Fact]
        public void property_creation()
        {
            IProperty<int> property = new Property<int>("test");

            property.Name.Should().Be("test");
            property.Type.Should().Be(typeof(int));

            property.ShouldBeEmpty();
        }

        [Fact]
        public void property_with_name_should_change_name()
        {
            var test1 = new Property<int>("test1");
            test1.Name.Should().Be("test1");
            test1.ShouldBeEmpty();

            var test2 = test1.WithName("test2");
            test2.Name.Should().Be("test2");
            test2.ShouldBeEmpty();

            var test3 = test1.WithNameUntyped("test3");
            test3.Name.Should().Be("test3");
            test3.Type.Should().Be(typeof(int));
            ((IProperty<int>)test3).ShouldBeEmpty();

            var test4 = CreateFilledProperty().WithNameUntyped("test4");
            ((IProperty<int>)test4).ShouldBeFilled(name: "test4");

            var test5 = CreateFilledProperty().WithAliasUntyped("alias5");
            ((IProperty<int>)test5).ShouldBeFilled(alias: "alias5");
        }

        [Fact]
        public void property_with_chaining()
        {
            Property<int> property = CreateFilledProperty();
            property.ShouldBeFilled();
        }

        [Fact]
        public void property_factory()
        {
            PropertyFactory propertyFactory = new PropertyFactory();
            IProperty property = propertyFactory.Create<int>("Test");
            property.Type.Should().Be(typeof(int));
            property.Name.Should().Be("Test");

            property = propertyFactory.Create(typeof(int), "Test");
            property.Type.Should().Be(typeof(int));
            property.Name.Should().Be("Test");
        }

        [Fact]
        public void property_factory_cached()
        {
            IPropertyFactory notCached = new PropertyFactory();
            notCached.Create<string>("Test").Should().NotBeSameAs(notCached.Create<string>("Test"));

            IPropertyFactory cached = new PropertyFactory().Cached();
            cached.Create<string>("Test").Should().BeSameAs(cached.Create<string>("Test"));
        }

        [Fact]
        public void PropertyValueFactory_CreateUntyped()
        {
            var factory = new PropertyValueFactory();

            IPropertyValue propertyValue = factory.CreateUntyped(new Property<int>("Age"), 10, ValueSource.Defined);
            propertyValue.Should().NotBeNull();
            propertyValue.PropertyUntyped.Type.Should().Be(typeof(int));
            propertyValue.PropertyUntyped.Name.Should().Be("Age");
            propertyValue.ValueUntyped.Should().Be(10);
        }

        private static Property<int> CreateFilledProperty() => PropertyTestsExtensions.CreateFilledProperty();


        [Fact]
        public void property_default_value_should_be_correct()
        {
            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValueMetadata().Should().NotBeNull();

            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValueMetadata()!.Value.Should().Be(42);

            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValue().Should().Be(42);

            new Property<int>("int_default_42")
                .WithDefaultValue(42)
                .GetDefaultValue().Should().Be(42);

            new Property<int>("int_default_42")
                .WithDefaultValueUntyped(new DefaultValue<int>(42))
                .GetDefaultValue().Should().Be(42);

            new Property<int>("int_default_42")
                .WithDefaultValue(new DefaultValue<int>(42))
                .GetDefaultValue().Should().Be(42);

            new Property<int>("int_default_42")
                .Configure(settings => settings.DefaultValue = new DefaultValue<int>(42))
                .GetDefaultValueMetadata()!.Value.Should().Be(42);

            new Property<int>("int_no_default")
                .GetDefaultValueMetadata().Should().BeNull();

            new Property<int>("int_no_default")
                .GetDefaultValue().Should().Be(0);

            new Property<int>("int_get_default_with_fallback")
                .GetDefaultValue(defaultValue: -1).Should().Be(-1);

            object[] components = new Property<int>("int_default_42")
                .WithDefaultValue(42)
                .SetRequired()
                .GetComponentsAndMetadata()
                .ToArray();
            components[0].Should().BeOfType<DefaultValue<int>>();
            components[1].Should().BeOfType<Required>();
        }

        class XmlLineInfo : IXmlLineInfo
        {
            /// <inheritdoc />
            public bool HasLineInfo() => true;

            /// <inheritdoc />
            public int LineNumber { get; }

            /// <inheritdoc />
            public int LinePosition { get; }

            public XmlLineInfo(int lineNumber, int linePosition)
            {
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }
        }

        [Fact]
        public void message_builder1()
        { 
            string? parseResultErrorMessage = "ParseError";
            var message =
                ValueMessageBuilder
                    .Error("Property '{PropertyName}' failed to parse from string '{PropertyValue}'.", 3)
                    .AddProperty("PropertyName", "AAA")
                    .AddProperty("PropertyValue", "BBB")
                    .If(parseResultErrorMessage != null)
                        .AppendToOriginalMessage(" Error: '{ParseResultError}'.")
                        .AddProperty("ParseResultError", parseResultErrorMessage)
                    .EndIf()
                    .AppendXmlLineInfo(new XmlLineInfo(10, 5))
                    .Build();

            message.FormattedMessage.Should()
                .Be("Property 'AAA' failed to parse from string 'BBB'. Error: 'ParseError'. LineNumber: 10, LinePosition: 5.");
        }

        [Fact]
        public void message_builder2()
        {
            Message message = ValueMessageBuilder
                .Error("Hello {name}")
                .AddProperty("name", "Alex")
                .WithSeverity(MessageSeverity.Warning)
                .If(true)
                    .AppendToOriginalMessage(" Good morning!")
                    .SetProperty("name", "Alexey")
                .EndIf();

            message.FormattedMessage.Should()
                .Be("Hello Alexey Good morning!");
        }

        [Fact]
        public void property_builder()
        {
            ValueMessageBuilder messageBuilder = ValueMessageBuilder
                .Error("Hello {name}")
                .AddProperty("name", "Alex")
                .WithSeverity(MessageSeverity.Warning)
                .SetProperty("name", "Alexey");

            Message message = messageBuilder.Build();
            message.FormattedMessage.Should().Be("Hello Alexey");
            message.Severity.Should().Be(MessageSeverity.Warning);

            IMessageBuilder builder = new MessageBuilder(messageBuilder);

            void ChangeFunc2(ref ValueMessageBuilder messageBuilder, ConfigureMessageRef configureMessage)
            {
                configureMessage(ref messageBuilder);
            }

            void ChangeFunc4(IMessageBuilder messageBuilder, ConfigureMessage configureMessage)
            {
                configureMessage(messageBuilder);
            }

            ChangeFunc2(ref messageBuilder, (ref ValueMessageBuilder builder) => builder.WithOriginalMessage("Func2"));
            messageBuilder.State.OriginalMessage.Should().Be("Func2");

            ChangeFunc4(builder, builder => builder.WithOriginalMessage("Func4"));
            builder.State.OriginalMessage.Should().Be("Func4");

            new Property<string>("Name1")
                .WithRewriteFast((ref PropRefData<string> data) => data.Name = "Name2")
                .Name.Should().Be("Name2");

            new Property<string>("Name1")
                .WithRewrite(data => data.Name = "Name2")
                .Name.Should().Be("Name2");

            new Property<string>("Name1")
                .WithRewriteFast((ref PropRefData<string> data) => data.Description = "Description2")
                .Description.Should().Be("Description2");

            new Property<string>("Name1")
                .WithDescription("Description2")
                .Description.Should().Be("Description2");

            new Property<string>("Name1")
                .WithRewriteFast((ref PropRefData<string> data) => data.Description = "Description2")
                .WithRewriteFast((ref PropRefData<string> data) => data.Description = null)
                .Description.Should().Be(null, because: "Property should be rewritten twice");

            new Property<string>("Name1")
                .WithRewrite(data => data.Description = "Description2")
                .WithRewrite(data => data.Description = null)
                .Description.Should().Be(null, because: "Property should be rewritten twice");

            new Property<string>("Name1")
                .With(description: "DescriptionNoRewrite")
                .With(description: null)
                .Description.Should().Be("DescriptionNoRewrite");


        }

        [Fact]
        public void schema_builder_description()
        {
            Property<string> property = new Property<string>("Name1");
            Property<string> desc1 = property.WithDescription("desc1");
            desc1.Description.Should().Be("desc1");

            IProperty<string> propInterface = property;
            IProperty<string> desc2 = propInterface.WithDescription("desc2");
            desc2.Description.Should().Be("desc2");

            IProperty propInterface2 = property;
            IProperty desc3 = propInterface2.WithDescription("desc3");
            desc3.Description.Should().Be("desc3");

            var instance1 = new NotImplementedSchemaBuilder("Int", typeof(int), "IntValue");
            var instance2 = instance1.WithDescription("aaaa!");
            instance1.Description.Should().Be("IntValue");
            instance2.Description.Should().Be("aaaa!");
        }

        public class NotImplementedSchemaBuilder : ISchemaBuilder<ISchemaDescription>, ISchema
        {
            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <inheritdoc />
            public string? Description { get; }

            public NotImplementedSchemaBuilder(string name, Type type, string? description)
            {
                Name = name;
                Type = type;
                Description = description;
            }

            /// <inheritdoc />
            public object With(ISchemaDescription schemaPart)
            {
                return new NotImplementedSchemaBuilder(Name, Type, schemaPart.Description);
            }
        }
    }

    internal static class PropertyTestsExtensions
    {
        public static void ShouldBeEmpty<T>(this IProperty<T> property)
        {
            property.Description.Should().BeNull();
            property.Alias.Should().BeNull();
            property.Examples.Should().BeNull();
            property.DefaultValue.Should().BeNull();
            property.Calculator.Should().BeNull();
        }

        public static Property<int> CreateFilledProperty()
        {
            var property = Property
                .Empty<int>()
                .With(name: "test")
                .With(alias: "alias")
                .With(description: "description")
                .With(defaultValue: new DefaultValueLazy<int>(() => 1))
                .WithExamples(examples: new[] { 0, 1 })
                .With(calculator: new PropertyCalculator<int>(container => 2));
            return property;
        }

        public static void ShouldBeFilled(
            this IProperty<int> property, 
            string? name = "test",
            string? alias = "alias")
        {
            property.Name.Should().Be(name);
            property.Alias.Should().Be(alias);
            property.Description.Should().Be("description");
            property.DefaultValue.Value.Should().Be(1);
            property.Examples.Examples.Should().BeEquivalentTo(new[] {0, 1});
            property.Calculator.Should().NotBeNull();
        }
    }
}
