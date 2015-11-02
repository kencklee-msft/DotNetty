namespace DotNetty.Codecs.CoapTcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotNetty.Codecs.CoapTcp;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;
    using Xunit;
    using Moq;

    public class EncoderTests
    {
        private static readonly IByteBufferAllocator ALLOCATOR = new UnpooledByteBufferAllocator();

        private const int OPTION_COUNT = 3;
        private const int OPTION_PAYLOAD_SIZE = 16;
        private const int PAYLOAD_SIZE = 128;
        private const byte DEFAULT_VERSION = 1;
        private const byte DEFAULT_TYPE = 1;

        private class TestEncoder : Encoder
        {
            public List<object> Encode(Message msg)
            {
                // mock
                var contextMock = new Mock<IChannelHandlerContext>();
                contextMock.Setup(x => x.Allocator).Returns(ALLOCATOR);
                
                List<object> outputs = new List<object>();
                base.Encode(contextMock.Object, msg, outputs);

                return outputs;
            }
        }

        [Fact]
        public void EncodeTest()
        {
            byte code = 0x42;
            IByteBuffer token = GetTestPayload(new byte[4] { 0x0E, 0xE0, 0xAB, 0xCD });
            int tokenLength = token.ReadableBytes;
            List<MessageOption> options = GetTestOptions(OPTION_COUNT, OPTION_PAYLOAD_SIZE);
            IByteBuffer payload = GetTestPayload(PAYLOAD_SIZE);
            int payloadLength = payload.ReadableBytes;

            Message msg = Response.Create(DEFAULT_VERSION, DEFAULT_TYPE, code, token, options, payload);
            List<object> outputs = new TestEncoder().Encode(msg);

            // validate msg size (which is composed of headers, options + payload
            int expectedOptionBytesSize =
                MessageOptionTestHelper.estimateOptionByteSize(0, OPTION_PAYLOAD_SIZE) *
                options.Count + 1;

            // validate the return object
            Assert.Equal(1, outputs.Count);
            Assert.IsAssignableFrom<IByteBuffer>(outputs[0]);

            IByteBuffer actualBuffer = (IByteBuffer)outputs[0];


            uint expectedMsgSize = (uint)(4 + 1 + 1 + tokenLength + expectedOptionBytesSize + PAYLOAD_SIZE);
            Assert.Equal(expectedMsgSize, (uint)actualBuffer.ReadableBytes);

            // validate length shim
            uint lengthShim = actualBuffer.ReadUnsignedInt();
            Assert.Equal(expectedMsgSize - 4, lengthShim);

            // validate ver, t and tkl
            byte expectedMeta = 0x45;
            byte actualMeta = actualBuffer.ReadByte();
            Assert.Equal(expectedMeta, actualMeta);

            // validate code and token
            byte actualCode = actualBuffer.ReadByte();
            Assert.Equal(code, actualCode);

            IByteBuffer actualToken = actualBuffer.ReadBytes(4);
            Assert.True(ByteBufferUtil.Equals(token.ResetReaderIndex(), actualToken));

            // skip validate the options (which are tested separately in other tests) but its termination
            IByteBuffer actualOptions = actualBuffer.ReadBytes((2 + OPTION_PAYLOAD_SIZE) * OPTION_COUNT);
            byte actualTermination = actualBuffer.ReadByte();
            Assert.Equal(MessageOption.END_OF_OPTIONS, actualTermination);

            // validate payload
            IByteBuffer actualPayload = actualBuffer.ReadBytes(actualBuffer.ReadableBytes);
            Assert.Equal(payloadLength, actualPayload.ReadableBytes);
            Assert.True(ByteBufferUtil.Equals(payload.ResetReaderIndex(), actualPayload));
        }

        // supporting methods below

        private List<MessageOption> GetTestOptions(uint optionCount, uint payloadSize)
        {
            List<MessageOption> options = new List<MessageOption>();
            for (uint i = 0; i < optionCount; i++)
            {
                options.Add(MessageOption.Create(i, payloadSize, GetTestPayload(payloadSize)));
            }
            return options;
        }

        private IByteBuffer GetTestPayload(uint payloadSize)
        {
            IByteBuffer buffer = ALLOCATOR.Buffer((int)payloadSize);
            for (int i = 0; i < payloadSize; i++)
            {
                buffer.WriteByte((byte)(i%255));
            }
            return buffer;
        }

        private IByteBuffer GetTestPayload(byte[] bytes)
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(bytes.Length);
            buffer.WriteBytes(bytes);
            return buffer;
        }

        private Message CreateResponse(byte version, byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            return Response.Create(version, type, code, token, options, payload);
        }
    }
}
