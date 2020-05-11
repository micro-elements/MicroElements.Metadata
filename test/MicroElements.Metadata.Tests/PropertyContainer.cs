using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyContainerTests
    {
        public static IProperty<string> Name = new Property<string>("Name");
        public static IProperty<int> Age = new Property<int>("Age");

        [Fact]
        public void simple_set_and_get()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex")
                .WithValue(Age, 42);

            container.GetValue(Name).Should().Be("Alex");
            container.GetValue(Age).Should().Be(42);
        }

        [Fact]
        public void get_value_untyped()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex")
                .WithValue(Age, 42);

            container.GetValueUntyped(Name).Should().Be("Alex");
            container.GetValueUntyped(Age).Should().Be(42);
        }
    }
}
