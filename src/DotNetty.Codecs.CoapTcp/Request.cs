namespace DotNetty.Codecs.CoapTcp
{
    using System.Collections.Generic;
    using DotNetty.Buffers;

    public class Request: Message
    {
        protected Request(byte version, byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload) :
            base(version, type, code, token, options, payload)
        {}

        public static Request Create(byte version, byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            return new Request(version, type, code, token, options, payload);
        }

        public RequestType GetRequestType()
        {
            byte suffix = (byte)(Code & 0x1F);
            return (RequestType)suffix;
        }
    }
}
