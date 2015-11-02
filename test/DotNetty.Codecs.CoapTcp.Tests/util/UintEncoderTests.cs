namespace DotNetty.Codecs.CoapTcp.Tests.util
{
    using DotNetty.Codecs.CoapTcp;
    using DotNetty.Codecs.CoapTcp.util;
    using Xunit;

    public class UintEncoderTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(12, 12, 0, 0)]
        [InlineData(13, 13, 0, 1)]
        [InlineData(14, 13, 1, 1)]
        [InlineData(100, 13, 87, 1)]
        [InlineData(268, 13, 255, 1)]
        [InlineData(269, 14, 0, 2)]
        [InlineData(270, 14, 1, 2)]
        [InlineData(1000, 14, 731, 2)]
        [InlineData(65803, 14, 65534, 2)]
        [InlineData(65804, 14, 65535, 2)]
        [InlineData(65805, 15, 0, 4)]
        [InlineData(65806, 15, 1, 4)]
        [InlineData(100000, 15, 34195, 4)]
        public void EncodeTest(uint intValue, byte code, uint value, uint numberOfExtraBytes)
        {
            byte actualCode;
            uint actualValue, actualNumberOfExtraBytes;
            UintEncoder.Encode(intValue, out actualCode, out actualValue, out actualNumberOfExtraBytes);

            Assert.Equal(code, actualCode);
            Assert.Equal(value, actualValue);
            Assert.Equal(numberOfExtraBytes, actualNumberOfExtraBytes);
        }
    }
}
