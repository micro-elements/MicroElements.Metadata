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

            string md5HashInHex = content.Md5Hash();
            md5HashInHex.Should().Be("1CB251EC0D568DE6A929B520C4AED8D1");

            string md5HashInBase58 = content.GenerateMd5HashInBase58(length: 32);
            md5HashInBase58.Should().Be("4YXanez5o6yRVNPNVxW9TN");

            string md5HashInBase58Trimmed = content.GenerateMd5HashInBase58(length: 8);
            md5HashInBase58Trimmed.Should().Be("PNVxW9TN");

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string textInBase58 = Base58.Encoding.GetString(bytes);
            textInBase58.Should().Be("3yZeVh");

            bytes = new byte[] { 0, 0 }.Concat(bytes).ToArray();
            string textInBase58WithOffset = new string(Base58.Encoding.GetChars(bytes, 2, 4));
            textInBase58WithOffset.Should().Be("3yZeVh");

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
