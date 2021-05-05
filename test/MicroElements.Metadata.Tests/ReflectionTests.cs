using FluentAssertions;
using MicroElements.Metadata.Serialization;
using MicroElements.Validation;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class ReflectionTests
    {
        [Fact]
        public void CreatePropertyValue()
        {
            var factory = PropertyValueFactory.CreatePropertyValue(typeof(string)).Compile();
            IPropertyValue propertyValue = factory.Invoke(new PropertyValueFactory(), new Property<string>("Name"), "Alex", ValueSource.Defined);
            propertyValue.Should().NotBeNull();
        }

        [Fact]
        public void PropertyNullabilityType()
        {
            Property<string?> propertyA = new Property<string?>("A");
            Property<string> propertyB = new Property<string>("B");

            propertyA.Type.Should().Be(typeof(string));
            propertyB.Type.Should().Be(typeof(string));

            string? nullString = null;

            new PropertyValue<string?>(propertyA, nullString);
            new PropertyValue<string>(propertyB, nullString);

            new MutablePropertyContainer().SetValue(propertyA, nullString);
            new MutablePropertyContainer().SetValue(propertyB, nullString);
        }

        public interface IMeta
        {
            string GetMeta() => "Meta";
        }

        public interface IAutoMeta : IMeta
        {
            string IMeta.GetMeta() => "AutoMeta";
        }

        public interface IManualMeta : IMeta
        {
            string Meta { get; set; }

            string IMeta.GetMeta() => Meta;
        }

        public class AutoMeta : IAutoMeta
        {
        }

        public class ManualMeta : IManualMeta
        {
            /// <inheritdoc />
            public string Meta { get; set; }
        }

        public class ManualMeta2 : IManualMeta
        {
            /// <inheritdoc />
            public string Meta { get; set; }

            /// <inheritdoc />
            public string GetMeta() => Meta;
        }

        [Fact]
        public void DefaultInterfaceImplementation()
        {
            AutoMeta autoMeta = new AutoMeta();
            (autoMeta as IMeta).GetMeta().Should().Be("AutoMeta");

            IMeta autoMeta2 = new AutoMeta();
            autoMeta2.GetMeta().Should().Be("AutoMeta");

            ManualMeta manualMeta = new ManualMeta() {Meta = "ManualMeta"};
            (manualMeta as IMeta).GetMeta().Should().Be("ManualMeta");

            IMeta manualMeta2 = new ManualMeta2() { Meta = "ManualMeta2" };
            manualMeta2.GetMeta().Should().Be("ManualMeta2");
        }

        [Fact]
        public void GetTypeFriendlyName()
        {
            string friendlyName = typeof(string[]).GetFriendlyName();

            string typeName = DefaultMapperSettings.Instance.GetTypeName(typeof(string[]));
        }
    }


}
