namespace DotNetty.Codecs.CoapTcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotNetty.Codecs.CoapTcp;
    using DotNetty.Buffers;
    using Xunit;

    public class DecoderTests
    {
        private static readonly IByteBufferAllocator Allocator = new UnpooledByteBufferAllocator();

        [Fact]
        public void DecodeSmallestMessageTest()
        {
            byte[] smallestValidMessage = { 0x00, 0x00, 0x00, 0x03, 0x05, 0x00, 0xFF };

            List<object> output = new TestDecoder().Decode(smallestValidMessage);

            Assert.Equal(1, output.Count);
            Assert.True(typeof(Message).IsAssignableFrom(output.First().GetType()));

            Message message = (Message)output.First();

            Assert.Equal(0, message.Code);
            Assert.Equal(1, message.Version);
            Assert.Equal(1, message.Type);
            Assert.Equal(0, message.Token.ReadableBytes);
            Assert.Equal(0, message.Payload.ReadableBytes);
        }

        [Fact]
        public void DecodeMediumMessageTest()
        {
            byte meta = 0x15;
            byte code = 0x01;
            byte token = 0xAA;
            byte[] smallestValidMessage = { 0x00, 0x00, 0x00, 0x08, meta, code, token, 0x10, 0xEE, 0xFF, 0xAB, 0xCD };

            List<object> output = new TestDecoder().Decode(smallestValidMessage);

            Assert.Equal(1, output.Count);
            Assert.IsAssignableFrom<Request>(output.First());

            Message message = (Message)output.First();

            Assert.Equal(1, message.Version);
            Assert.Equal(1, message.Type);
            Assert.Equal(code, message.Code);
            Assert.Equal(new byte[] { token }, message.Token.ToArray());
            Assert.Equal(new byte[] { 0xAB, 0xCD }, message.Payload.ToArray());
        }

        private class TestDecoder: Decoder
        {
            public List<object> Decode(byte[] bytes)
            {
                IByteBuffer buffer = Allocator.Buffer(bytes.Length);
                buffer.WriteBytes(bytes);
                List<object> output = new List<object>();

                // test method: Decode
                base.Decode(null, buffer, output);

                return output;
            }
        }
    }
}
