using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests.examples
{
    /// <summary>
    /// Example of using metadata for any object that does not implements IMetadataProvider.
    /// </summary>
    public static class AttachedNameUntypedExample
    {
        /// <summary>
        /// Sets AttachedName to an object.
        /// </summary>
        public static void SetAttachedName(this object target, string name)
        {
            target.AsMetadataProvider().SetMetadata("AttachedName", name);
        }

        /// <summary>
        /// Sets AttachedName and returns the same object.
        /// </summary>
        public static T WithAttachedName<T>(this T target, string name) where T : class
        {
            target.AsMetadataProvider().SetMetadata("AttachedName", name);
            return target;
        }

        /// <summary>
        /// Gets AttachedName.
        /// </summary>
        public static string GetAttachedName(this object target)
        {
            return target.AsMetadataProvider().GetMetadata<string>("AttachedName");
        }
    }

    public class AttachedNameUsage
    {
        [DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
        public class MyEntity
        {
        }

        [Fact]
        public void attached_name_should_be_get_and_set()
        {
            // Sets AttachedName to an object.
            MyEntity instance1 = new MyEntity();
            instance1.SetAttachedName("Instance1");

            // Sets AttachedName to an object (other way).
            MyEntity instance2 = new MyEntity().WithAttachedName("Instance2");

            // Get AttachedName
            instance1.GetAttachedName().Should().Be("Instance1");
            instance2.GetAttachedName().Should().Be("Instance2");
        }
    }
}
