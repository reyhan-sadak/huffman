using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Utility
    {
        //public const byte BYTE_PRIMARY_INDEX = 8;
        public const int UINT_PRIMARY_INDEX = 32;

        /*public static byte GetBitAtIndexOfByte(byte i_byte, byte i_bitIndex)
        {
            int result = (i_byte & (1 << i_bitIndex - 1)) == 0 ? 0 : 1;
            return (byte)(result);
        }*/

        public static bool GetBitAtIndexOfUint(uint i_uint, int i_bitIndex)
        {
            bool result;
            if (i_bitIndex == UINT_PRIMARY_INDEX)
            {
                uint primaryBit = i_uint >> (UINT_PRIMARY_INDEX - 1);
                result = (primaryBit != 0u);
            }
            else
            {
                uint temp = (uint)(1 << i_bitIndex - 1);
                result = (i_uint & temp) != 0u;
            }
            return result;
        }

        public static int AppendMeaningBitsOfUintToUint(ref uint o_firstUint, ref uint o_secondUint, uint i_uint, int bitsNumber, int i_fromIndex)
        {
            int toIndex = bitsNumber;
            //while ((Utility.GetBitAtIndexOfUint(i_uint, (byte)toIndex)) == 0 && toIndex > 1)
            //{
            //    toIndex--;
            //}
            //i_uint = (uint)(i_uint << (Utility.UINT_PRIMARY_INDEX - toIndex + 1));
            //i_uint = (uint)(i_uint >> (Utility.UINT_PRIMARY_INDEX - toIndex + 1));

            int remainingBits = UINT_PRIMARY_INDEX - i_fromIndex;
            if (remainingBits >= toIndex)
            {
                i_uint = (uint)(i_uint << remainingBits - toIndex);
                o_firstUint |= i_uint;
            }
            else
            {
                int notEnoughBits = toIndex - remainingBits;
                uint copyOfTheByte = i_uint;

                i_uint = (uint)(i_uint >> notEnoughBits);
                o_firstUint |= i_uint;

                copyOfTheByte = (uint)(copyOfTheByte << (UINT_PRIMARY_INDEX - notEnoughBits));
                o_secondUint |= copyOfTheByte;
            }
            return i_fromIndex + toIndex;
        }
    }
}
