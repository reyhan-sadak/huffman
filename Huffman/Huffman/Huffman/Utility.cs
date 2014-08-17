using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Utility
    {
        public static bool b_use_debug = true;
        public static bool b_use_info = true;
        public const int INT_PRIMARY_INDEX = 32;

        public static byte GetBitAtIndexOfByte(uint i_byte, byte i_bitIndex)
        {
            uint result = (uint)((i_byte & (1 << i_bitIndex - 1)) == 0 ? 0 : 1);
            return (byte)(result);
        }

        public static int AppendMeaningBitsOfByteToByte(ref uint o_firstByte, ref uint o_secondByte, uint i_byte, int bitsNumber, int i_fromIndex)
        {
            int toIndex = bitsNumber;

            int remainingBits = INT_PRIMARY_INDEX - i_fromIndex;
            if (remainingBits >= toIndex)
            {
                i_byte = (uint)(i_byte << remainingBits - toIndex);
                o_firstByte |= i_byte;
            }
            else
            {
                int notEnoughBits = toIndex - remainingBits;
                uint copyOfTheByte = i_byte;

                i_byte = i_byte >> notEnoughBits;
                o_firstByte |= i_byte;

                copyOfTheByte = copyOfTheByte << (INT_PRIMARY_INDEX - notEnoughBits);
                o_secondByte |= copyOfTheByte;
            }
            return i_fromIndex + toIndex;
        }

        public static void PrintDebug(string iMsg="")
        {
            if (Utility.b_use_debug)
            {
                Console.WriteLine("Debug: " + iMsg);
            }
        }

        public static void PrintInfo(string iMsg = "")
        {
            if (Utility.b_use_info)
            {
                Console.WriteLine("Info: " + iMsg);
            }
        }
    }
}
