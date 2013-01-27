using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntervalTreeClocks
{
    public class ByteInt
    {

        public static byte[] IntToByteArray(long value)
        {
            return new byte[]
                       {
                           (byte) ((uint) value >> 24),
                           (byte) ((uint) value >> 16),
                           (byte) ((uint) value >> 8),
                           (byte) value
                       };
        }

        public static long ByteArrayToInt(byte[] b)
        {
            return (b[0] << 24)
                   + ((b[1] & 0xFF) << 16)
                   + ((b[2] & 0xFF) << 8)
                   + (b[3] & 0xFF);
        }
    }
}
