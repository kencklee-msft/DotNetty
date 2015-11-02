namespace DotNetty.Codecs.CoapTcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Codecs.CoapTcp.util;

    class MessageOptionEncoder
    {

        public static void Encode(IEnumerable<MessageOption> options, ref IByteBuffer buffer)
        {
            uint currentOptionNumber = 0;
            foreach (MessageOption option in options)
            {
                Encode(option, currentOptionNumber, ref buffer);
                currentOptionNumber = option.OptionNumber;
            }
            buffer.WriteByte(MessageOption.END_OF_OPTIONS);
        }

        private static void Encode(MessageOption option, uint previousOptionNumber, ref IByteBuffer buffer)
        {
            byte optionNumberDeltaCode, optionLengthCode;
            uint optionNumberDeltaExtraValue, optionLengthExtraValue;
            uint optionNumberDeltaExtraByte, optionLengthExtraByte;
            UintEncoder.Encode(option.OptionNumber - previousOptionNumber, out optionNumberDeltaCode, out optionNumberDeltaExtraValue, out optionNumberDeltaExtraByte);
            UintEncoder.Encode(option.OptionLength, out optionLengthCode, out optionLengthExtraValue, out optionLengthExtraByte);

            byte optionHeader = (byte)(optionNumberDeltaCode + (optionLengthCode << 4));
            buffer.WriteByte(optionHeader);

            WriteVariedLengthUint(optionNumberDeltaExtraValue, optionNumberDeltaExtraByte, ref buffer);
            WriteVariedLengthUint(optionLengthExtraValue, optionLengthExtraByte, ref buffer);
            buffer.WriteBytes(option.Payload, (int)option.OptionLength);
        }

        private static void WriteVariedLengthUint(uint value, uint length, ref IByteBuffer buffer)
        {
            switch (length)
            {
                case 0:
                    return;
                case 1:
                    buffer.WriteByte((byte)value);
                    return;
                case 2:
                    buffer.WriteUnsignedShort((ushort)value);
                    return;
                case 4:
                    buffer.WriteUnsignedInt(value);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("invalid length for encoded int; length: {0}", length));
            }
        }
    }
}
