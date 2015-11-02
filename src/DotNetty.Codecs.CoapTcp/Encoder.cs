namespace DotNetty.Codecs.CoapTcp
{
    using System.Collections.Generic;
    using DotNetty.Transport.Channels;
    using DotNetty.Buffers;

    class Encoder : MessageToMessageEncoder<Message>
    {
        // 32-bit fixed length shim length
        private const int SHIM_LENGTH_SIZE = 4;
        // 0x05 = 0101 (version = 01 and type = 01 (NON))
        private const int FIXED_VERSION_AND_TYPE = 0x05;
        private const int INIT_MESSAGE_SIZE = 1024;

        protected override void Encode(IChannelHandlerContext context, Message message, List<object> output)
        {
            byte tokenLength = (byte)message.Token.ReadableBytes;
            byte meta = (byte)(tokenLength << 4 | FIXED_VERSION_AND_TYPE);

            IByteBuffer buffer = context.Allocator.Buffer(INIT_MESSAGE_SIZE);

            buffer.WriteInt(0); // filler for message length and will be refilled later
            buffer.WriteByte(meta);
            buffer.WriteByte(message.Code);
            buffer.WriteBytes(message.Token);

            MessageOptionEncoder.Encode(message.Options, ref buffer);

            buffer.WriteBytes(message.Payload);

            uint msgLen = (uint)buffer.ReadableBytes;
            buffer.SetUnsignedInt(0, (msgLen - sizeof(int)));

            output.Add(buffer);
        }
    }
}
