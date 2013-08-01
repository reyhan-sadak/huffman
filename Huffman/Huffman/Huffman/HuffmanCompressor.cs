using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Huffman
{
    class HuffmanCompressor
    {
        private BinaryTree[][] m_HuffmanTree;
        private int[][] m_ByteFrequencies;
        private Dictionary<byte, uint>[] m_HuffmanDictionary;
        private byte[][] m_filesBytes;
        private string m_outputFilePath;
        private string[] m_inputFilesNames;

        public HuffmanCompressor(string[] i_filesList, string i_outputFileName)
        {
            m_outputFilePath = i_outputFileName;

            int fileListLength = i_filesList.Length;
            m_ByteFrequencies = new int[fileListLength][];
            m_HuffmanTree = new BinaryTree[fileListLength][];
            m_HuffmanDictionary = new Dictionary<byte, uint>[fileListLength];
            m_filesBytes = new byte[fileListLength][];
            m_inputFilesNames = new string[fileListLength];

            for(int i = 0; i < fileListLength; i++)
            {
                string filePath = i_filesList[i];
                int lastIndex = filePath.LastIndexOf("\\");
                m_inputFilesNames[i] = filePath.Substring(lastIndex + 1);
                if (File.Exists(filePath))
                {
                    m_filesBytes[i] = File.ReadAllBytes(filePath);
                    CreateFrequencyTable(i);
                    CreateHuffmanTree(i);
                }
            }
        }

        public void WriteOutputFile()
        {
            FileStream fs = new FileStream(m_outputFilePath, FileMode.Create);
            BinaryWriter bWriter = new BinaryWriter(fs);

            int filesNumber = m_filesBytes.Length;

            // Write the number of files
            bWriter.Write(filesNumber);
            Console.WriteLine("Writing files number = " + filesNumber);

            uint currentUIntToWrite = 0;
            uint nextByteToWrite = 0;
            int currentByteToWriteIndex = 0;
            for (int fileIndex = 0; fileIndex < filesNumber; fileIndex++)
            {
                // Write the file name
                int lenghtOfTheName = m_inputFilesNames[fileIndex].Length;
                bWriter.Write(lenghtOfTheName);
                Console.WriteLine("Writing lenght of the name = " + lenghtOfTheName);
                for (int nameIndex = 0; nameIndex < lenghtOfTheName; nameIndex++)
                {
                    char character = m_inputFilesNames[fileIndex][nameIndex];
                    bWriter.Write((byte)character);
                    Console.WriteLine("Writing character = " + character);
                }
                // write the Huffman dictionary
                int numberOfItemsInTheDictionary = m_HuffmanDictionary[fileIndex].Keys.ToArray().Length;
                bWriter.Write(numberOfItemsInTheDictionary);
                Console.WriteLine("Writing number of items in the dict = " + numberOfItemsInTheDictionary);
                foreach (byte key in m_HuffmanDictionary[fileIndex].Keys)
                {
                    bWriter.Write(key);
                    uint value = m_HuffmanDictionary[fileIndex][key];
                    bWriter.Write(value);
                    Console.WriteLine("Writing key = " + key);
                    Console.WriteLine("Writing value = " + value);
                }

                ArrayList listOfTheNewBytes = new ArrayList();
                int bytesNumber = m_filesBytes[fileIndex].Length;
                int numberOfUnusedBitsInTheLastUInt = 0;
                for (int byteIndex = 0; byteIndex < bytesNumber; byteIndex++)
                {
                    byte singleByte = m_filesBytes[fileIndex][byteIndex];
                    uint codedValue = m_HuffmanDictionary[fileIndex][singleByte];

                    int toIndex = Utility.UINT_PRIMARY_INDEX;
                    while ((Utility.GetBitAtIndexOfUint(codedValue, toIndex)) == false && toIndex > 0)
                    {
                        toIndex--;
                    }
                    codedValue = codedValue << (Utility.UINT_PRIMARY_INDEX - toIndex + 1);
                    codedValue = codedValue >> (Utility.UINT_PRIMARY_INDEX - toIndex + 1);

                    currentByteToWriteIndex = Utility.AppendMeaningBitsOfUintToUint(ref currentUIntToWrite, ref nextByteToWrite, codedValue, toIndex - 1, currentByteToWriteIndex);
                    if (currentByteToWriteIndex >= Utility.UINT_PRIMARY_INDEX || byteIndex == bytesNumber - 1)
                    {
                        listOfTheNewBytes.Add(currentUIntToWrite);
                        currentUIntToWrite = nextByteToWrite;
                        nextByteToWrite = 0;
                        currentByteToWriteIndex %= Utility.UINT_PRIMARY_INDEX;
                        if (byteIndex == bytesNumber - 1)
                        {
                            numberOfUnusedBitsInTheLastUInt = Utility.UINT_PRIMARY_INDEX - currentByteToWriteIndex;
                        }
                    }
                }

                int numberOfTheNewUints = listOfTheNewBytes.ToArray().Length;
                // Write the number of the uints
                bWriter.Write(numberOfTheNewUints);
                Console.WriteLine("Writing number of the uints = " + numberOfTheNewUints);
                // Write the number of the unused bits in the last byte
                bWriter.Write(numberOfUnusedBitsInTheLastUInt);
                Console.WriteLine("Writing number of the unused bits in the last byte = " + numberOfUnusedBitsInTheLastUInt);
                for (int newUIntsIndex = 0; newUIntsIndex < numberOfTheNewUints; newUIntsIndex++)
                {
                    currentUIntToWrite = (uint)listOfTheNewBytes.ToArray()[newUIntsIndex];
                    bWriter.Write(currentUIntToWrite);
                    Console.WriteLine("Writing UInt in the archived file = " + currentUIntToWrite);
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
            }

            bWriter.Close();
            fs.Close();
        }

        private void CreateFrequencyTable(int i_fileIndex)
        {
            m_ByteFrequencies[i_fileIndex] = new int[byte.MaxValue + 1];
            foreach (byte fileByte in m_filesBytes[i_fileIndex])
            {
                m_ByteFrequencies[i_fileIndex][fileByte]++;
            }
        }

        private void CreateHuffmanTree(int i_fileIndex)
        {
            int positiveCount = GetNumberOfPositiveValuesInFrequencyTable(i_fileIndex);
            m_HuffmanTree[i_fileIndex] = new BinaryTree[2 * positiveCount - 1];
            for (int i = 0; i < positiveCount; i++)
            {
                byte existingByte = GetExistingByteAtIndex(i_fileIndex, i);
                m_HuffmanTree[i_fileIndex][i] = new BinaryTree(new Node(existingByte, GetFrequencyOfByte(i_fileIndex, existingByte)));
            }
            for (int i = positiveCount; i < 2 * positiveCount - 1; i++)
            {
                SortHuffmanTreesToIndex(i_fileIndex);
                m_HuffmanTree[i_fileIndex][i - 1].IsInTheCurrentLevel = false;
                m_HuffmanTree[i_fileIndex][i - 2].IsInTheCurrentLevel = false;
                m_HuffmanTree[i_fileIndex][i] = new BinaryTree(new Node(m_HuffmanTree[i_fileIndex][i - 1].Node.Frequency + m_HuffmanTree[i_fileIndex][i - 2].Node.Frequency), ref m_HuffmanTree[i_fileIndex][i - 1], ref m_HuffmanTree[i_fileIndex][i - 2]);
            }
            SortHuffmanTreesToIndex(i_fileIndex);
            BinaryTree finalHuffmanTree = m_HuffmanTree[i_fileIndex][2 * positiveCount - 2];
            //Console.WriteLine(finalHuffmanTree);
            Console.WriteLine("Huffman tree height = " + finalHuffmanTree.GetHeight());
            m_HuffmanDictionary[i_fileIndex] = new Dictionary<byte, uint>();
            CreateHuffmanMap(i_fileIndex, ref finalHuffmanTree, 1u);
            Console.WriteLine();

            foreach (byte key in m_HuffmanDictionary[i_fileIndex].Keys)
            {
                Console.WriteLine("HuffmanDict[ " + key + " ] = " + m_HuffmanDictionary[i_fileIndex][key]);
            }
            Console.WriteLine();
        }

        private int GetNumberOfPositiveValuesInFrequencyTable(int i_fileIndex)
        {
            int number = 0;
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                if (m_ByteFrequencies[i_fileIndex][i] > 0)
                {
                    number++;
                }
            }
            return number;
        }

        private byte GetExistingByteAtIndex(int i_fileIndex, int i_index)
        {
            byte index = 255;
            for (byte i = 0; i <= byte.MaxValue; i++)
            {
                if (m_ByteFrequencies[i_fileIndex][i] > 0)
                    index++;
                if (index == i_index)
                    return i;
            }
            return 0;
        }

        private int GetFrequencyOfByte(int i_fileIndex, byte i_byte)
        {
            return m_ByteFrequencies[i_fileIndex][i_byte];
        }

        private void SortHuffmanTreesToIndex(int i_fileIndex)
        {
            Array.Sort(m_HuffmanTree[i_fileIndex], delegate(BinaryTree first, BinaryTree second){ return(second == null) ? (first == null)? 0 : -1 : (first == null)? 1 : first.CompareTo(second);});
        }

        private void CreateHuffmanMap(int i_fileIndex, ref BinaryTree i_tree, uint i_code)
        {
            if (i_tree.Left != null)
            {
                BinaryTree tempTree = i_tree.Left;
                CreateHuffmanMap(i_fileIndex, ref tempTree, (2 * i_code + 0));
            }
            if (i_tree.Right != null)
            {
                BinaryTree tempTree = i_tree.Right;
                CreateHuffmanMap(i_fileIndex, ref tempTree, (2 * i_code + 1));
            }
            if (i_tree.Left == null && i_tree.Right == null)
            {
                if (m_HuffmanDictionary[i_fileIndex].ContainsValue(i_code))
                {
                    throw new System.ArgumentException("Code alrady in the Huffman map");
                }
                m_HuffmanDictionary[i_fileIndex][i_tree.Node.Byte] = i_code;
            }
        }
    }
}
