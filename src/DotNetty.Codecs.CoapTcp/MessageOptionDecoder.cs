using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Codecs.CoapTcp
{
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;
    using DotNetty.Codecs.CoapTcp.util;

    class MessageOptionDecoder
    {
        /// <summary>
        /// ReadOptions reads through a sequence of bytes (through BytesReader)
        /// and constructs a list of options.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<MessageOption> Decode(IByteBuffer buffer)
        {
            // read options
            List<MessageOption> options = new List<MessageOption>();
            uint currentOptionNumber = 0;
            while (true)
            {
                byte optionHeaderCode = buffer.ReadByte();
                if (optionHeaderCode == MessageOption.END_OF_OPTIONS)
                {
                    break;
                }

                // retrieve option number
                byte deltaCode = (byte)(optionHeaderCode & 0xF);
                uint delta = UintDecoder.Decode(buffer, deltaCode);
                uint number = delta + currentOptionNumber;

                // retrieve option payload length
                byte lengthCode = (byte)(optionHeaderCode >> 4);
                uint length = UintDecoder.Decode(buffer, lengthCode);
                IByteBuffer payload = buffer.ReadBytes((int)length);

                options.Add(MessageOption.Create(number, length, payload));
                currentOptionNumber = number;
            }
            return options;
        }
    }
}
