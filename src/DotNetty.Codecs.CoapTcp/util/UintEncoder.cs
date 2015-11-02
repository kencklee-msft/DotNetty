namespace DotNetty.Codecs.CoapTcp.util
{
    class UintEncoder
    {
        /// <summary>
        /// Encode generates a 4-bit code, an x-byte uint (x=0,1,2,4) and the size of the uint.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="code"></param>
        /// <param name="extraValue"></param>
        /// <param name="extraValueLength"></param>
        public static void Encode(uint value, out byte code, out uint extraValue, out uint extraValueLength)
        {
            const byte EIGHT_BIT_CODE = 13;
            const byte SIXTEEN_BIT_CODE = 14;
            const byte THIRTYTWO_BIT_CODE = 15;

            const uint FOUR_BIT_MAX_VALUE = 12;
            const uint EIGHT_BIT_MAX_VALUE = 268;
            const uint SIXTEEN_BIT_MAX_VALUE = 65804;

            if (value <= FOUR_BIT_MAX_VALUE)
            {
                code = (byte)value;
                extraValue = 0;
                extraValueLength = 0;
                return;
            }

            if (value <= EIGHT_BIT_MAX_VALUE)
            {
                code = (byte)EIGHT_BIT_CODE;
                extraValue = value - FOUR_BIT_MAX_VALUE - 1;
                extraValueLength = 1;
                return;
            }

            if (value <= SIXTEEN_BIT_MAX_VALUE)
            {
                code = (byte)SIXTEEN_BIT_CODE;
                extraValue = value - EIGHT_BIT_MAX_VALUE - 1;
                extraValueLength = 2;
                return;
            }

            code = (byte)THIRTYTWO_BIT_CODE;
            extraValue = value - SIXTEEN_BIT_MAX_VALUE - 1;
            extraValueLength = 4;
        }
    }
}
