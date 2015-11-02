namespace DotNetty.Codecs.CoapTcp
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// CoAPTcpDecoder implements codec for CoAP over TCP.
    /// The message structure follows the Specification (L1 alternative with 
    /// 32-bit length field in the front). The specification is at:
    /// https://www.ietf.org/id/draft-tschofenig-core-coap-tcp-tls-04.txt
    /// 
    /// BLBT aliases the first letters of proposers' last names.
    /// * C. Bormann
    /// * S. Lemay
    /// * V. Solorzano Barboza
    /// * H. Tschofenig
    /// 
    /// Message format:
    ///  0                   1                   2                   3
    ///  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// | Message Length ...                                            |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |Ver| T |  TKL  |     Code      |   Token (if any, TKL bytes)   |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |  Options (if any) ...                                         |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |1 1 1 1 1 1 1 1|    Payload (if any) ...                       |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// 
    /// Reference:
    /// Figure 6 in 
    /// https://www.ietf.org/id/draft-tschofenig-core-coap-tcp-tls-04.txt
    /// 
    /// And, options are in format defined in RFC7252
    /// 
    ///   0   1   2   3   4   5   6   7
    /// +---------------+---------------+
    /// |               |               |
    /// |  Option Delta | Option Length |   1 byte
    /// |               |               |
    /// +---------------+---------------+
    /// \                               \
    /// /         Option Delta          /   0-2 bytes
    /// \          (extended)           \
    /// +-------------------------------+
    /// \                               \
    /// /         Option Length         /   0-2 bytes
    /// \          (extended)           \
    /// +-------------------------------+
    /// \                               \
    /// /                               /
    /// \                               \
    /// /         Option Value          /   0 or more bytes
    /// \                               \
    /// /                               /
    /// \                               \
    /// +-------------------------------+
    ///
    /// </summary>
    public class Decoder: ReplayingDecoder<Decoder.ParseState>
    {
        // 32-bit fixed length shim length
        private const int SHIM_LENGTH_SIZE = 4;
        // 0x05 = 0101 (version = 01 and type = 01 (NON))
        private const int FIXED_VERSION_AND_TYPE = 0x05;

        public enum ParseState
        {
            Ready,
            Failed
        }

        public Decoder():
            base(ParseState.Ready)
        {}

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (this.State)
                {
                    case ParseState.Ready:
                        Message message = null;
                        if (false == TryDecode(context, input, out message))
                        {
                            this.RequestReplay();
                            return;
                        }

                        output.Add(message);
                        this.Checkpoint();
                        //CoAPTcpLogger.Log.Debug("Decoded message:{0}", message);
                        break;
                    case ParseState.Failed:
                        input.SkipBytes(input.ReadableBytes);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (DecoderException exception)
            {
                input.SkipBytes(input.ReadableBytes);
                this.Checkpoint(ParseState.Failed);
                //CoapTcpLogger.Log.Error("Exception while decoding.", exception);
                this.CloseAsync(context);
            }
        }

        bool TryDecode(IChannelHandlerContext context, IByteBuffer buffer, out Message message)
        {
            message = null;
            if (!buffer.IsReadable(4))
            {
                return false;
            }

            int messageLength = buffer.ReadInt();
            if (!buffer.IsReadable(messageLength))
            {
                return false;
            }

            message = this.Parse(context, buffer, messageLength);
            return true;
        }

        Message Parse(IChannelHandlerContext context, IByteBuffer buffer, int messageLength)
        {
            byte meta = buffer.ReadByte();
            byte code = buffer.ReadByte();
            byte version = (byte)(meta & 0x03);
            byte type = (byte)((meta >> 2) & 0x03);

            int tokenLength = meta >> 4;
            IByteBuffer token = buffer.ReadBytes(tokenLength);

            List<MessageOption> options = MessageOptionDecoder.Decode(buffer);
            int endOfOptions = buffer.ReaderIndex;

            int payLength = messageLength + 4 - endOfOptions;
            IByteBuffer payload = buffer.ReadBytes(payLength);


            return Request.Create(version, type, code, token, options, payload);
            //return Message.Create(code, token.ToArray, options, payload.ToArray);
        }

        Message CreateMessage(byte version, byte type, byte code, IByteBuffer token, List<MessageOption> options, IByteBuffer payload)
        {
            if (0 == code)
            {
                return EmptyMessage.Create(version, type, code, token, options, payload);
            }

            byte prefix = (byte)(code >> 5);
            if (0 == prefix)
            {
                return Request.Create(version, type, code, token, options, payload);
            }
            else if (2<=prefix && prefix <=5)
            {
                return Response.Create(version, type, code, token, options, payload);
            }
            throw new DecoderException(string.Format("invalid message code:{0}", code));
        }
    }
}
