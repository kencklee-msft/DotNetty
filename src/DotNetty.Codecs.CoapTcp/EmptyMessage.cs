namespace DotNetty.Codecs.CoapTcp
{
    using System.Collections.Generic;
    using DotNetty.Buffers;

    class EmptyMessage : Message
    {
        protected EmptyMessage(byte version, byte type, byte code, 
            IByteBuffer token, List<MessageOption> options, IByteBuffer payload) :
            base(version, type, code, token, options, payload)
        { }

        public static EmptyMessage Create(byte version, byte type, byte code, 
            IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            return new EmptyMessage(version, type, code, token, options, payload);
        }
    }
}
