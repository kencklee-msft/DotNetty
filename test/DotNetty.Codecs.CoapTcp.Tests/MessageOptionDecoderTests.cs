namespace DotNetty.Codecs.CoapTcp.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DotNetty.Buffers;
    using Xunit;

    public class MessageOptionDecoderTests
    {
        private static readonly IByteBufferAllocator ALLOCATOR = new UnpooledByteBufferAllocator();
        private static readonly byte OPTION_TERMINATION = 0xFF;

        [Fact]
        public void DecodeZeroOptionTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1);
            buffer.WriteByte(OPTION_TERMINATION);

            List<MessageOption> options = MessageOptionDecoder.Decode(buffer);

            Assert.Equal(0, options.Count);
        }

        [Fact]
        public void DecodeOneOptionTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(2);
            buffer.WriteByte(0x01);
            buffer.WriteByte(OPTION_TERMINATION);

            List<MessageOption> options = MessageOptionDecoder.Decode(buffer);

            Assert.Equal(1, options.Count);
            Assert.Equal((uint)1, options[0].OptionNumber);
            Assert.Equal((uint)0, options[0].OptionLength);
        }

        [Fact]
        public void DecodeFourOptionsTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(5);
            buffer.WriteByte(0x01);
            buffer.WriteByte(0x00);
            buffer.WriteByte(0x05);
            buffer.WriteByte(0x0A);
            buffer.WriteByte(OPTION_TERMINATION);

            List<MessageOption> options = MessageOptionDecoder.Decode(buffer);

            Assert.Equal(4, options.Count);
            Assert.Equal((uint)1, options[0].OptionNumber);
            Assert.Equal((uint)1, options[1].OptionNumber);
            Assert.Equal((uint)6, options[2].OptionNumber);
            Assert.Equal((uint)16, options[3].OptionNumber);
        }

        [Fact]
        public void DecodeTwoOptionsWithNonEmptyPayloadTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(5);
            buffer.WriteByte(0x11);
            buffer.WriteByte(0xEE);
            buffer.WriteByte(0x10);
            buffer.WriteByte(0xEE);
            buffer.WriteByte(0x15);
            buffer.WriteByte(0xEE);
            buffer.WriteByte(0x1A);
            buffer.WriteByte(0xEE);
            buffer.WriteByte(OPTION_TERMINATION);

            List<MessageOption> options = MessageOptionDecoder.Decode(buffer);

            Assert.Equal(4, options.Count);
            Assert.Equal((uint)1, options[0].OptionNumber);
            Assert.Equal((uint)1, options[1].OptionNumber);
            Assert.Equal((uint)6, options[2].OptionNumber);
            Assert.Equal((uint)16, options[3].OptionNumber);

            Assert.Equal(new byte[] { 0xEE }, options[0].Payload.ToArray());
            Assert.Equal(new byte[] { 0xEE }, options[1].Payload.ToArray());
            Assert.Equal(new byte[] { 0xEE }, options[2].Payload.ToArray());
            Assert.Equal(new byte[] { 0xEE }, options[3].Payload.ToArray());
        }
    }
}
