namespace DotNetty.Codecs.CoapTcp
{
    using System.Collections.Generic;
    using DotNetty.Buffers;

    class Response: Message
    {
        protected Response(byte version, byte type, byte code, 
            IByteBuffer token, List<MessageOption> options, IByteBuffer payload) :
            base(version, type, code, token, options, payload)
        { }

        public static Response Create(byte version, byte type, byte code, 
            IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            return new Response(version, type, code, token, options, payload);
        }

        public string GetResponseCode()
        {
            byte prefix = (byte)(Code >> 5);
            byte suffix = (byte)(Code & 0x1F);

            return string.Format("{0}.{1}", prefix, suffix);
        }
    }
}
