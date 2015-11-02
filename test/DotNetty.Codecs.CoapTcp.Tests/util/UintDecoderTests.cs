namespace DotNetty.Codecs.CoapTcp.Tests.util
{
    using System;
    using DotNetty.Codecs.CoapTcp.util;
    using DotNetty.Buffers;
    using Xunit;

    public class UintDecoderTests
    {
        private static readonly IByteBufferAllocator ALLOCATOR = new UnpooledByteBufferAllocator();

        [Theory]
        [InlineData(0, new byte[4] { 0xAA, 0xAA, 0xAA, 0xAA }, 0)]
        [InlineData(12, new byte[4] { 0xAA, 0xAA, 0xAA, 0xAA }, 12)]
        [InlineData(13, new byte[4] { 0x00, 0xAA, 0xAA, 0xAA }, 13)]
        [InlineData(13, new byte[4] { 0xAA, 0xAA, 0xAA, 0xAA }, 183)]
        [InlineData(13, new byte[4] { 0xFF, 0x00, 0xAA, 0xAA }, 268)]
        [InlineData(14, new byte[4] { 0x00, 0x00, 0xAA, 0xAA }, 269)]
        [InlineData(14, new byte[4] { 0xAA, 0xAA, 0xAA, 0xAA }, 43959)]
        [InlineData(14, new byte[4] { 0xFF, 0xFF, 0xAA, 0xAA }, 65804)]
        [InlineData(15, new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 65805)]
        [InlineData(15, new byte[4] { 0xAA, 0xAA, 0xAA, 0xAA }, 2863377335)]
        public void DecodeTest(byte code, byte[] bytes, uint expectedValue)
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(4);
            buffer.WriteBytes(bytes);
            uint value = UintDecoder.Decode(buffer, code);

            Assert.Equal(expectedValue, value);
        }
    }
}
