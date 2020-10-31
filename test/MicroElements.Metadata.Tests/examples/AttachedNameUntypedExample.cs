using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests.examples
{
    /// <summary>
    /// Example of using metadata for any object that does not implements IMetadataProvider.
    /// SetAttachedName adds AttachedName to an object.
    /// GetAttachedName gets AttachedName from an object.
    /// WithAttachedName sets AttachedName and returns the same object.
    /// </summary>
    public static class AttachedNameUntypedExample
    {
        public static void SetAttachedName(this object target, string name)
        {
            target.GetInstanceMetadata().SetMetadata("AttachedName", name);
        }

        public static T WithAttachedName<T>(this T target, string name) where T : class
        {
            target.GetInstanceMetadata().SetMetadata("AttachedName", name);
            return target;
        }

        public static string GetAttachedName(this object target)
        {
            return target.GetInstanceMetadata().GetMetadata<string>("AttachedName");
        }
    }

    public class AttachedNameUsage
    {
        public class MyEntity
        {
        }

        [Fact]
        public void attached_name_should_be_get_and_set()
        {
            MyEntity instance1 = new MyEntity();
            instance1.SetAttachedName("Instance1");

            MyEntity instance2 = new MyEntity().WithAttachedName("Instance2");

            instance1.GetAttachedName().Should().Be("Instance1");
            instance2.GetAttachedName().Should().Be("Instance2");
        }
    }
}
