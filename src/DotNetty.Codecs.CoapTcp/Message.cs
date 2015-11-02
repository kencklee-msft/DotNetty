namespace DotNetty.Codecs.CoapTcp
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using System.Linq;

    public abstract class Message
    {
        private const byte DEFAULT_VERSION = 1;
        private const byte MESSGAE_TYPE_BITMASK = 0xE0;

        public byte Version { get { return version; } }
        public byte Type { get { return type; } }
        public byte Code { get { return code; } }
        public IByteBuffer Token { get { return token; } }
        public List<MessageOption> Options { get { return options; } }
        public IByteBuffer Payload { get { return payload; } }

        protected byte version;
        protected byte type;
        protected byte code;
        protected IByteBuffer token;
        protected List<MessageOption> options;
        protected IByteBuffer payload;

        protected Message(byte version, byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            this.version = version;
            this.type = type;
            this.code = code;
            this.token = token;
            this.options = options;
            this.payload = payload;
        }

        protected Message(byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload) :
            this(DEFAULT_VERSION, type, code, token, options, payload)
        { }

        public MessageType GetMessageType()
        {
            // code == 0.00 means EMPTY
            if (0 == code) 
            {
                return MessageType.EMPTY;
            }

            byte prefix = (byte)((code & MESSGAE_TYPE_BITMASK) >> 5);
            if (prefix == 0) {
                return MessageType.REQUEST;
            }
            else if (2 <= prefix && prefix <= 5) {
                return MessageType.RESPONSE;
            }
            throw new ArgumentException("undefined request/response type for code:" + code);
        }

        public override int GetHashCode()
        {
            // valid token is only 0-8 byte length
            // we left-shift individual i-th token byte i bit and XOR them
            int hashCode = 0;
            for (int i=0; i<Math.Min(Token.ReadableBytes,8); i++)
            {
                hashCode ^= Token.GetByte(i) << i;
            }
            // finally, we include code that differentitate types of requests and responses
            return hashCode + Code << 24;
        }

        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
            {
                return false;
            }

            Message message = (Message)obj;
            return (Version == message.Version &&
                Type == message.Type &&
                Code == message.Code &&
                ByteBufferUtil.Equals(Token, message.Token) &&
                Options.ToArray().SequenceEqual(message.Options.ToArray()) &&
                ByteBufferUtil.Equals(Payload, message.Payload));
        }
    }
}
