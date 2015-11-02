namespace DotNetty.Codecs.CoapTcp.util
{
    using System;
    using DotNetty.Buffers;

    class UintDecoder
    {
        /// <summary>
        /// Decode recover a variable length byte/ushort/uint based on 
        /// a 4-bit and some number of bytes.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static uint Decode(IByteBuffer buffer, byte code)
        {
            const byte FOUR_BIT_CODE_MAX = 12;
            const byte EIGHT_BIT_CODE = 13;
            const byte SIXTEEN_BIT_CODE = 14;
            const byte THIRTYTWO_BIT_CODE = 15;

            const uint FOUR_BIT_MAX_VALUE = 12;
            const uint EIGHT_BIT_MAX_VALUE = 268;
            const uint SIXTEEN_BIT_MAX_VALUE = 65804;

            if (code <= FOUR_BIT_CODE_MAX)
            {
                return code;
            }

            switch (code)
            {
                case EIGHT_BIT_CODE:
                    return buffer.ReadByte() + FOUR_BIT_MAX_VALUE + 1;
                case SIXTEEN_BIT_CODE:
                    return buffer.ReadUnsignedShort() + EIGHT_BIT_MAX_VALUE + 1;
                case THIRTYTWO_BIT_CODE:
                    return buffer.ReadUnsignedInt() + SIXTEEN_BIT_MAX_VALUE + 1;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("invalid code: {0}", code));
            }
        }
    }
}
