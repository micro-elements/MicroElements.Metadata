using System.Collections.Generic;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class AllowedValuesTests
    {
        public enum SexType
        {
            Male,
            Female
        }

        [Fact]
        public void check_allowed_values()
        {
            IProperty<string> Sex1 = new Property<string>("Sex1")
                .SetAllowedValues("Male", "Female");

            IProperty<string> Sex2 = new Property<string>("Sex2")
                .SetAllowedValuesFromEnum(typeof(SexType));

            IProperty<SexType> Sex3 = new Property<SexType>("Sex3")
                .SetAllowedValuesFromEnum();

            IProperty<int> Sex4 = new Property<int>("Sex4")
                .SetAllowedValuesFromEnum(typeof(SexType));

            IProperty<int> Sex5 = new Property<int>("Sex5")
                .SetAllowedValuesFromEnum<SexType>();

            ISchema<string> sexSchema = new SimpleTypeSchema<string>("Sex")
                .SetAllowedValues("Male", "Female");

            IProperty<string> Sex6 = new Property<string>("Sex6")
                .SetSchema(sexSchema);

            Sex1.GetAllowedValues().Values.Should().BeEquivalentTo("Male", "Female");
            Sex1.GetAllowedValuesUntyped().ValuesUntyped.Should().BeEquivalentTo(new [] { "Male", "Female" });
            Sex2.GetAllowedValues().Values.Should().BeEquivalentTo("Male", "Female");
            Sex3.GetAllowedValues().Values.Should().BeEquivalentTo([SexType.Male, SexType.Female]);
            Sex4.GetAllowedValues().Values.Should().BeEquivalentTo([0, 1]);
            Sex5.GetAllowedValues().Values.Should().BeEquivalentTo([0, 1]);
            Sex6.GetAllowedValues().Values.Should().BeEquivalentTo("Male", "Female");
        }

    }
}
