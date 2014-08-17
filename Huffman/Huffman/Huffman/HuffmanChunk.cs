using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using HuffmanDictionary = System.Collections.Generic.Dictionary<byte, uint>;
using ReversedHuffmanDictionary = System.Collections.Generic.Dictionary<uint, byte>;

namespace Huffman
{
    class HuffmanChunk
    {
        public enum EChunkMode
        {
            ChunkMode_Compress,
            ChunkMode_Decompress,
            ChunkMode_Count
        };

        EChunkMode              m_mode;
        public EChunkMode Mode
        {
            get
            {
                return m_mode;
            }
            set
            {
                m_mode = value;
            }
        }
        private BinaryTree[]        m_HuffmanTree;
        HuffmanDictionary           m_HuffmanDictionary;
        ReversedHuffmanDictionary m_reversedHuffmanDictionary;
        private uint[]              m_ByteFrequencies;
        private byte[]              m_ChunkBytes;

        public HuffmanChunk(EChunkMode iMode, byte[] iBytes, int iSize)
        {
            this.Mode = iMode;
            this.SetInputBytes(iBytes, iSize);
        }

        public void SetInputBytes(byte[] iBytes, int iSize)
        {
            m_ChunkBytes = new byte[iSize];
            Buffer.BlockCopy(iBytes, 0, m_ChunkBytes, 0, iSize);
        }

        public void SerializeOutput(ref MemoryStream stream)
        {
            if (this.Mode == EChunkMode.ChunkMode_Compress)
            {
                CreateFrequencyTable();
                CreateHuffmanTree();

                BinaryWriter bWriter = new BinaryWriter(stream);
                // write the Huffman dictionary
                uint numberOfItemsInTheDictionary = (uint)m_HuffmanDictionary.Keys.ToArray().Length;
                bWriter.Write(numberOfItemsInTheDictionary);
                foreach (byte key in m_HuffmanDictionary.Keys)
                {
                    bWriter.Write(key);
                    bWriter.Write(m_HuffmanDictionary[key]);
                }

                uint numberOfUnusedBitsInTheLastByte = 0;
                ArrayList encodedBytes = HuffmanEncode(ref numberOfUnusedBitsInTheLastByte);
                uint numberOfTheNewBytes = (uint)encodedBytes.ToArray().Length;
                // write the number of the bytes
                bWriter.Write(numberOfTheNewBytes);
                // write the number of the unused bits in the last byte
                bWriter.Write(numberOfUnusedBitsInTheLastByte);
                for (uint newBytesIndex = 0; newBytesIndex < numberOfTheNewBytes; newBytesIndex++)
                {
                    uint currentByteToWrite = (uint)encodedBytes.ToArray()[newBytesIndex];
                    Utility.PrintDebug("Writing byte in the archived file :: " + currentByteToWrite);
                    bWriter.Write(currentByteToWrite);
                }
                Utility.PrintDebug();
                Utility.PrintDebug();
                Utility.PrintDebug();

                // close the writer
                //bWriter.Close();
            }
            else if (this.Mode == EChunkMode.ChunkMode_Decompress)
            {
                MemoryStream memoryStream = new MemoryStream(m_ChunkBytes);
                BinaryReader bReader = new BinaryReader(memoryStream);
                BinaryWriter bWriter = new BinaryWriter(stream);
                m_reversedHuffmanDictionary = new Dictionary<uint, byte>();
                // read the Huffman dictionary
                uint numberOfTheItemsInTheDictionary = bReader.ReadUInt32();
                Utility.PrintDebug("Reverse Huffman dictionary");
                for (uint dictionaryIndex = 0; dictionaryIndex < numberOfTheItemsInTheDictionary; dictionaryIndex++)
                {
                    byte value = bReader.ReadByte();
                    uint key = bReader.ReadUInt32();
                    m_reversedHuffmanDictionary[key] = value;
                    Utility.PrintDebug("HuffmanDict[ " + key + " ] = " + m_reversedHuffmanDictionary[key]);
                }

                uint bytesNumber = bReader.ReadUInt32();
                uint numberOfUnusedBits = bReader.ReadUInt32();
                byte byteValue = 1;
                for (uint bytesIndex = 0; bytesIndex < bytesNumber; bytesIndex++)
                {
                    uint currentByte = bReader.ReadUInt32();
                    byte keyIndex = Utility.INT_PRIMARY_INDEX;
                    byteValue *= 2;
                    byteValue += Utility.GetBitAtIndexOfByte(currentByte, keyIndex);
                    uint limitOfTheLoop = 0;
                    if (bytesIndex == bytesNumber - 1)
                    {
                        limitOfTheLoop = numberOfUnusedBits;
                    }
                    for (; keyIndex > limitOfTheLoop; keyIndex--)
                    {
                        byte valueInTheMap = byteValue;
                        if (m_reversedHuffmanDictionary.ContainsKey(valueInTheMap))
                        {
                            Utility.PrintDebug("Writing byte :: " + m_reversedHuffmanDictionary[byteValue]);
                            byte toWriteByte = m_reversedHuffmanDictionary[byteValue];
                            bWriter.Write(toWriteByte);
                            if (keyIndex > limitOfTheLoop + 1)
                            {
                                byteValue = 2;
                                byteValue += Utility.GetBitAtIndexOfByte(currentByte, (byte)(keyIndex - 1));
                            }
                            else
                            {
                                byteValue = 1;
                            }
                        }
                        else if (keyIndex > 1)
                        {
                            byteValue *= 2;
                            byteValue += Utility.GetBitAtIndexOfByte(currentByte, (byte)(keyIndex - 1));
                        }
                    }
                }

                bReader.Close();
                // Close the writer
                //bWriter.Close();
            }
            else
            {
                throw new Exception("Unhandled Huffman chunk mode.");
            }
        }

        private ArrayList HuffmanEncode(ref uint numberOfUnusedBitsInTheLastByte)
        {
            uint currentByteToWrite = 0;
            uint nextByteToWrite = 0;
            uint currentByteToWriteIndex = 0;
            ArrayList listOfTheNewBytes = new ArrayList();
            int bytesNumber = m_ChunkBytes.Length;
            for (uint byteIndex = 0; byteIndex < bytesNumber; byteIndex++)
            {
                byte singleByte = m_ChunkBytes[byteIndex];
                uint codedValue = m_HuffmanDictionary[singleByte];

                int toIndex = Utility.INT_PRIMARY_INDEX;
                while ((Utility.GetBitAtIndexOfByte(codedValue, (byte)toIndex)) == 0 && toIndex > 1)
                {
                    toIndex--;
                }
                codedValue = codedValue << (Utility.INT_PRIMARY_INDEX - toIndex + 1);
                codedValue = codedValue >> (Utility.INT_PRIMARY_INDEX - toIndex + 1);

                currentByteToWriteIndex = (uint)Utility.AppendMeaningBitsOfByteToByte(ref currentByteToWrite, ref nextByteToWrite, codedValue, (int)(toIndex - 1), (int)currentByteToWriteIndex);
                if (currentByteToWriteIndex >= Utility.INT_PRIMARY_INDEX || byteIndex == bytesNumber - 1)
                {
                    listOfTheNewBytes.Add(currentByteToWrite);
                    currentByteToWrite = nextByteToWrite;
                    nextByteToWrite = 0;
                    currentByteToWriteIndex %= Utility.INT_PRIMARY_INDEX;
                    if (byteIndex == bytesNumber - 1)
                    {
                        numberOfUnusedBitsInTheLastByte = Utility.INT_PRIMARY_INDEX - currentByteToWriteIndex;
                    }
                }
            }
            return listOfTheNewBytes;
        }

        // iterate all the bytes and count the frequency
        private void CreateFrequencyTable()
        {
            if (this.Mode == EChunkMode.ChunkMode_Compress) // if we are compressing
            {
                m_ByteFrequencies = new uint[byte.MaxValue + 1];
                foreach (byte fileByte in m_ChunkBytes)
                {
                    m_ByteFrequencies[fileByte]++;
                }
            }
            else
            {
                throw new Exception("Method not allowed for the current mode.");
            }
        }

        // create the Huffman tree based on the counted frequency
        private void CreateHuffmanTree()
        {
            uint positiveCount = GetNumberOfPositiveValuesInFrequencyTable();
            m_HuffmanTree = new BinaryTree[2 * positiveCount - 1];
            for (uint i = 0; i < positiveCount; i++)
            {
                byte existingByte = GetExistingByteAtIndex(i);
                m_HuffmanTree[i] = new BinaryTree(new Node(existingByte, GetFrequencyOfByte(existingByte)));
            }
            for (uint i = positiveCount; i < 2 * positiveCount - 1; i++)
            {
                SortHuffmanTree();
                m_HuffmanTree[i - 1].IsInTheCurrentLevel = false;
                m_HuffmanTree[i - 2].IsInTheCurrentLevel = false;
                m_HuffmanTree[i] = new BinaryTree(new Node(m_HuffmanTree[i - 1].Node.Frequency + m_HuffmanTree[i - 2].Node.Frequency), ref m_HuffmanTree[i - 1], ref m_HuffmanTree[i - 2]);
            }
            SortHuffmanTree();
            BinaryTree finalHuffmanTree = m_HuffmanTree[2 * positiveCount - 2];
            Utility.PrintDebug(finalHuffmanTree.ToString());
            Utility.PrintDebug(finalHuffmanTree.GetHeight().ToString());
            Utility.PrintDebug();
            m_HuffmanDictionary = new Dictionary<byte, uint>();
            CreateHuffmanMap(ref finalHuffmanTree, 1);

            foreach (byte key in m_HuffmanDictionary.Keys)
            {
                Utility.PrintDebug("HuffmanDict[ " + key + " ] = " + m_HuffmanDictionary[key]);
            }
            Utility.PrintDebug();
        }

        private uint GetNumberOfPositiveValuesInFrequencyTable()
        {
            uint number = 0;
            for (uint i = 0; i <= byte.MaxValue; i++)
            {
                if (m_ByteFrequencies[i] > 0)
                {
                    number++;
                }
            }
            return number;
        }

        private byte GetExistingByteAtIndex(uint i_index)
        {
            byte index = 255;
            for (byte i = 0; i <= byte.MaxValue; i++)
            {
                if (m_ByteFrequencies[i] > 0)
                    index++;
                if (index == i_index)
                    return i;
            }
            return 0;
        }

        private uint GetFrequencyOfByte(byte i_byte)
        {
            return m_ByteFrequencies[i_byte];
        }

        private void SortHuffmanTree()
        {
            Array.Sort(m_HuffmanTree, delegate(BinaryTree first, BinaryTree second) { return (second == null) ? (first == null) ? 0 : -1 : (first == null) ? 1 : first.CompareTo(second); });
        }

        private void CreateHuffmanMap(ref BinaryTree i_tree, uint i_code)
        {
            if (i_tree.Left != null)
            {
                BinaryTree tempTree = i_tree.Left;
                CreateHuffmanMap(ref tempTree, 2 * i_code + 0);
            }
            if (i_tree.Right != null)
            {
                BinaryTree tempTree = i_tree.Right;
                CreateHuffmanMap(ref tempTree, 2 * i_code + 1);
            }
            if (i_tree.Left == null && i_tree.Right == null)
            {
                if (m_HuffmanDictionary.ContainsValue(i_code))
                {
                    throw new System.ArgumentException("Code alrady in the Huffman map");
                }
                m_HuffmanDictionary[i_tree.Node.Byte] = i_code;
            }
        }
    }
}
