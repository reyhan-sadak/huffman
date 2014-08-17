using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Node
    {
        private byte m_byte;
        public byte Byte
        {
            get
            {
                return m_byte;
            }
            set
            {
                m_byte = value;
            }
        }

        private uint m_frequency;
        public uint Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                m_frequency = value;
            }
        }

        private bool m_bIsSet;
        public bool IsSet
        {
            get
            {
                return m_bIsSet;
            }
        }

        public Node()
        {
            Byte = 0;
            Frequency = 0;
            m_bIsSet = false;
        }

        public Node(byte i_byte, uint i_freq)
        {
            Byte = i_byte;
            Frequency = i_freq;
            m_bIsSet = true;
        }

        public Node(uint i_freq)
        {
            Byte = 0;
            Frequency = i_freq;
            m_bIsSet = true;
        }

        public override string ToString()
        {
            string node = "[ " + Byte + " , " + Frequency + " ]";
            return node;
        }
    }
}
