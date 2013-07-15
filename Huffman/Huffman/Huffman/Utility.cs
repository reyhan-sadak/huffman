using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Utility
    {
        public const byte BYTE_PRIMARY_INDEX = 8;

        public static byte GetBitAtIndexOfByte(byte i_byte, byte i_bitIndex)
        {
            int result = (i_byte & (1 << i_bitIndex - 1)) == 0 ? 0 : 1;
            return (byte)(result);
        }

        public static int AppendMeaningBitsOfByteToByte(ref byte o_firstByte, ref byte o_secondByte, byte i_byte, int i_fromIndex)
        {
            int toIndex = BYTE_PRIMARY_INDEX;
            while ((Utility.GetBitAtIndexOfByte(i_byte, (byte)toIndex)) == 0 && toIndex > 1)
            {
                toIndex--;
            }
            i_byte = (byte)(i_byte << (Utility.BYTE_PRIMARY_INDEX - toIndex + 1));
            i_byte = (byte)(i_byte >> (Utility.BYTE_PRIMARY_INDEX - toIndex + 1));

            int remainingBits = BYTE_PRIMARY_INDEX - i_fromIndex;
            if (remainingBits >= toIndex)
            {
                i_byte = (byte)(i_byte << remainingBits - toIndex);
                o_firstByte |= i_byte;
            }
            else
            {
                int notEnoughBits = toIndex - remainingBits;
                byte copyOfTheByte = i_byte;

                i_byte = (byte)(i_byte >> notEnoughBits);
                o_firstByte |= i_byte;

                copyOfTheByte = (byte)(copyOfTheByte << (BYTE_PRIMARY_INDEX - notEnoughBits));
                o_secondByte |= copyOfTheByte;
            }
            return i_fromIndex + toIndex;
        }
    }
}
