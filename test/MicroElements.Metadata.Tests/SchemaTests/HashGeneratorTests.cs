using System.Linq;
using System.Text;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests.SchemaTests
{
    public class HashGeneratorTests
    {
        [Fact]
        public void HashGenerator()
        {
            string content = "text";
            string md5Hash = content.Md5Hash();
            string hash4 = content.GenerateMd5HashInBase58(length: 32);

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string hash5 = Base58.Encoding.GetString(bytes);

            bytes = new byte[] { 0, 0 }.Concat(bytes).ToArray();

            string base58_1 = bytes.EncodeBase58();
            string base58_2 = bytes.EncodeBase58_2();


            base58_1.Should().Be("113yZeVh");
            base58_2.Should().Be("113yZeVh");
        }

        [Fact]
        public void HashGenerator2()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            bytes = new byte[] { 0, 0 }.Concat(bytes).ToArray();

            string base58_1 = bytes.EncodeBase58();
            string base58_2 = bytes.EncodeBase58_2();


            base58_2.Should().Be(base58_1);
        }

        int repeats = 10;
        private string content = "12345678901234";
        private int contentRepeat = 1;

        [Fact]
        public void EncodeBase58_1()
        {
            byte[] bytes = Enumerable.Repeat(Encoding.UTF8.GetBytes(content), contentRepeat).SelectMany(bytes1 => bytes1).ToArray();

            for (int i = 0; i < repeats; i++)
            {
                string base58_1 = bytes.EncodeBase58();
            }
        }

        [Fact]
        public void EncodeBase58_2()
        {
            byte[] bytes = Enumerable.Repeat(Encoding.UTF8.GetBytes(content), contentRepeat).SelectMany(bytes1 => bytes1).ToArray();

            for (int i = 0; i < repeats; i++)
            {
                string base58_2 = bytes.EncodeBase58_2();
            }
        }


        //[Fact]
        //public void EncodeBase58_3()
        //{
        //    byte[] bytes = Enumerable.Repeat(Encoding.UTF8.GetBytes(content), contentRepeat).SelectMany(bytes1 => bytes1).ToArray();

        //    for (int i = 0; i < repeats; i++)
        //    {
        //        string base58_1 = bytes.EncodeBase58_3();
        //    }
        //}

    }
}
