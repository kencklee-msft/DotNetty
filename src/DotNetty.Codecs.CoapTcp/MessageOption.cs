namespace DotNetty.Codecs.CoapTcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotNetty.Buffers;

    public class MessageOption
    {
        public enum Name
        {
            Reserved = 0,
            If_Match = 1,
            Uri_Host = 3,
            ETag = 4,
            If_None_Match = 5,
            Observe = 6,
            Uri_Port = 7,
            Location_Path = 8,
            Uri_Path = 11,
            Content_Format = 12,
            Max_Age = 14,
            Uri_Query = 15,
            Accept = 17,
            Location_Query = 20,
            Proxy_Uri = 35,
            Proxy_Scheme = 39,
            Size1= 60
        };

        public enum DataType
        {
            EMPTY,
            OPAQUE,
            UINT,
            STRING
        };

        public const byte END_OF_OPTIONS = 0xFF;
        private static Dictionary<uint, DataType> OPTION_NUMBER_TO_OPTION_DATA_TYPE =
            new Dictionary<uint, DataType>() {
                {0 /* reserved */, DataType.EMPTY},
                {1 /* if-match */, DataType.OPAQUE},
                {3 /* uri-host */, DataType.STRING},
                {4 /* etag */, DataType.OPAQUE},
                {5 /* if-none-match */, DataType.EMPTY},
                {6 /* observe */, DataType.UINT},
                {7 /* uri-port */, DataType.UINT},
                {8 /* location-path */, DataType.STRING},
                {11 /* uri-path */, DataType.STRING},
                {12 /* content-format */, DataType.UINT},
                {14 /* max-age */, DataType.UINT},
                {15 /* uri-query */, DataType.STRING},
                {17 /* accept */, DataType.UINT},
                {20 /* location-query */, DataType.STRING},
                {35 /* proxy-uri */, DataType.STRING},
                {39 /* proxy-scheme */, DataType.STRING},
                {60 /* size1 */, DataType.UINT},
                {128 /* reserved */, DataType.EMPTY},
                {132 /* reserved */, DataType.EMPTY},
                {136 /* reserved */, DataType.EMPTY},
                {140 /* reserved */, DataType.EMPTY}
            };

        public uint OptionNumber { get { return optionNumber; } }
        public uint OptionLength { get { return optionLength; } }
        public IByteBuffer Payload { get { return payload; } }
        public Name OptionName { get { return GetName(optionNumber); } }
        public DataType OptionDataType { get { return GetOptionDataType(optionNumber); } }

        private uint optionNumber;
        private uint optionLength;
        private IByteBuffer payload;

        private MessageOption(uint optionNumber, uint optionLength, IByteBuffer payload)
        {
            this.optionNumber = optionNumber;
            this.optionLength = optionLength;
            this.payload = payload;
        }

        public override bool Equals(Object obj)
        {
            if (null == obj || GetType() != obj.GetType())
            {
                return false;
            }

            MessageOption messageOption = (MessageOption)obj;
            return
                OptionNumber == messageOption.optionNumber &&
                OptionLength == messageOption.OptionLength &&
                ByteBufferUtil.Equals(Payload, messageOption.Payload);
        }

        public static MessageOption Create(uint optionNumber, uint optionLength, IByteBuffer payload)
        {
            return new MessageOption(optionNumber, optionLength, payload);
        }

        public static Name GetName(uint optionNumber)
        {
            if (!Enum.IsDefined(typeof(Name), (int)optionNumber))
            {
                return Name.Reserved;
            }
            return (Name)optionNumber;
        }

        public static DataType GetOptionDataType(uint optionNumber)
        {
            DataType type = DataType.OPAQUE;
            if (!OPTION_NUMBER_TO_OPTION_DATA_TYPE.TryGetValue(optionNumber, out type))
            {
                return DataType.OPAQUE;
            }
            return type;
        }
    }
}
