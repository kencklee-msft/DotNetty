namespace DotNetty.Codecs.CoapTcp.Tests
{
    using DotNetty.Codecs.CoapTcp;
    using DotNetty.Buffers;
    using Xunit;

    public class MessageOptionEncoderTests
    {
        private static readonly IByteBufferAllocator ALLOCATOR = new UnpooledByteBufferAllocator();
        private static readonly byte OPTION_TERMINATION = 0xFF;

        [Fact]
        public void ZeroOptionTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1024);

            MessageOption[] options = new MessageOption[0] { };

            MessageOptionEncoder.Encode(options, ref buffer);

            Assert.Equal(new byte[1] { OPTION_TERMINATION }, buffer.ToArray());
        }

        [Fact]
        public void OneOptionTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1024);

            IByteBuffer emptyBuffer = ALLOCATOR.Buffer(0);
            MessageOption option = MessageOption.Create(0, 0, emptyBuffer);
            MessageOption[] options = new MessageOption[1] { option };

            MessageOptionEncoder.Encode(options, ref buffer);

            Assert.Equal(new byte[2] { 0, OPTION_TERMINATION }, buffer.ToArray());
        }

        [Fact]
        public void SmallDeltasZeroPayloadTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1024);
            MessageOption option00 = MessageOption.Create(0, 0, CreatePayload(0));
            MessageOption option12a = MessageOption.Create(7, 0, CreatePayload(0));
            MessageOption option12b = MessageOption.Create(12, 0, CreatePayload(0));
            MessageOption option13 = MessageOption.Create(13, 0, CreatePayload(0));
            MessageOption option15 = MessageOption.Create(15, 0, CreatePayload(0));
            MessageOption[] options = new MessageOption[5] { option00, option12a, option12b, option13, option15 };

            MessageOptionEncoder.Encode(options, ref buffer);

            Assert.Equal(new byte[] { 0, 0x7, 0x5, 0x1, 0x2, OPTION_TERMINATION }, buffer.ToArray());
        }

        [Fact]
        public void BigDeltasZeroPayloadTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1024);
            MessageOption option00 = MessageOption.Create(0, 0, CreatePayload(0));
            MessageOption option13 = MessageOption.Create(13, 0, CreatePayload(0));
            MessageOption option269 = MessageOption.Create(282, 0, CreatePayload(0));
            MessageOption[] options = new MessageOption[] { option00, option13, option269 };

            MessageOptionEncoder.Encode(options, ref buffer);

            Assert.Equal(new byte[] { 0, 0x0d, 0x0, 0x0e, 0x0, 0x0, OPTION_TERMINATION }, buffer.ToArray());
        }

        [Fact]
        public void PayloadTest()
        {
            IByteBuffer buffer = ALLOCATOR.Buffer(1024);
            MessageOption option00 = MessageOption.Create(0, 0, CreatePayload(0));
            MessageOption option13 = MessageOption.Create(13, 13, CreatePayload(13));
            MessageOption option269a = MessageOption.Create(282, 269, CreatePayload(269));
            MessageOption option269b = MessageOption.Create(282, 269, CreatePayload(269));
            MessageOption[] options = new MessageOption[] { option00, option13, option269a, option269b };

            MessageOptionEncoder.Encode(options, ref buffer);

            byte[] bytes = buffer.ToArray();

            // first option (0,0)
            Assert.Equal(0x00, bytes[0]);

            // second option (d,d)
            Assert.Equal(0xdd, bytes[1]);
            Assert.Equal(0x00, bytes[2]);
            Assert.Equal(0x00, bytes[3]);

            // third option (e,e)
            Assert.Equal(0xee, bytes[17]);
            Assert.Equal(0x00, bytes[18]);
            Assert.Equal(0x00, bytes[19]);
            Assert.Equal(0x00, bytes[20]);
            Assert.Equal(0x00, bytes[21]);

            // fourth option
            Assert.Equal(0xe0, bytes[291]);
            Assert.Equal(0x00, bytes[292]);
            Assert.Equal(0x00, bytes[293]);

            // end
            Assert.Equal(OPTION_TERMINATION, bytes[563]);
        }

        private IByteBuffer CreatePayload(int len)
        {
            byte[] bytes = new byte[len];
            for (int i=0; i< len; i++)
            {
                bytes[i] = 0xFF;
            }
            return ALLOCATOR.Buffer(len).WriteBytes(bytes);
        }
    }
}
