using System.Collections.Generic;

namespace Utils.Dictionaries
{
    public static class TagHelper
    {
        public static Dictionary<byte[], string> TagMapper = new Dictionary<byte[], string>(ByteArrayComparer.Default)
        {
            [new byte[] { 0x5F, 0x20 }] = "cardholdername",
            [new byte[] { 0x5F, 0x50 }] = "emvkernel"
        };
    }
}
