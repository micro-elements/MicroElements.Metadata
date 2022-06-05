using FluentAssertions;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Formatters;
using Xunit;

namespace MicroElements.Metadata.Tests;

public class CloneableTests
{
    [Fact]
    public void CloneTest()
    {
        var settings = new CollectionFormatterSettings();
        var clone = settings.Clone();
        clone.Should().BeEquivalentTo(settings);
    }

    [Fact]
    public void CloneTest2()
    {
        var settings = new CollectionFormatterSettings();
        var clone1 = Cloneable.GetMemberwiseClone(typeof(CollectionFormatterSettings))(settings);
        var clone2 = Cloneable.GetMemberwiseClone<CollectionFormatterSettings>()(settings);
        clone1.Should().BeEquivalentTo(clone2);
    }
}
