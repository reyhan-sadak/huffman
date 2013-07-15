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
        private Dictionary<byte, byte>[] m_HuffmanDictionary;
        private byte[][] m_filesBytes;
        private string m_outputFilePath;
        private string[] m_inputFilesNames;

        public HuffmanCompressor(string[] i_filesList, string i_outputFileName)
        {
            m_outputFilePath = i_outputFileName;

            int fileListLength = i_filesList.Length;
            m_ByteFrequencies = new int[fileListLength][];
            m_HuffmanTree = new BinaryTree[fileListLength][];
            m_HuffmanDictionary = new Dictionary<byte, byte>[fileListLength];
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

            byte currentByteToWrite = 0;
            byte nextByteToWrite = 0;
            int currentByteToWriteIndex = 0;
            for (int fileIndex = 0; fileIndex < filesNumber; fileIndex++)
            {
                // Write the file name
                int lenghtOfTheName = m_inputFilesNames[fileIndex].Length;
                bWriter.Write(lenghtOfTheName);
                for (int nameIndex = 0; nameIndex < lenghtOfTheName; nameIndex++)
                {
                    char character = m_inputFilesNames[fileIndex][nameIndex];
                    bWriter.Write((byte)character);
                }
                // write the Huffman dictionary
                int numberOfItemsInTheDictionary = m_HuffmanDictionary[fileIndex].Keys.ToArray().Length;
                bWriter.Write(numberOfItemsInTheDictionary);
                foreach (byte key in m_HuffmanDictionary[fileIndex].Keys)
                {
                    bWriter.Write(key);
                    bWriter.Write(m_HuffmanDictionary[fileIndex][key]);
                }

                ArrayList listOfTheNewBytes = new ArrayList();
                int bytesNumber = m_filesBytes[fileIndex].Length;
                int numberOfUnusedBitsInTheLastByte = 0;
                for (int byteIndex = 0; byteIndex < bytesNumber; byteIndex++)
                {
                    byte singleByte = m_filesBytes[fileIndex][byteIndex];
                    byte codedValue = m_HuffmanDictionary[fileIndex][singleByte];

                    currentByteToWriteIndex = Utility.AppendMeaningBitsOfByteToByte(ref currentByteToWrite, ref nextByteToWrite, codedValue, currentByteToWriteIndex);
                    if (currentByteToWriteIndex >= Utility.BYTE_PRIMARY_INDEX || byteIndex == bytesNumber - 1)
                    {
                        listOfTheNewBytes.Add(currentByteToWrite);
                        currentByteToWrite = nextByteToWrite;
                        nextByteToWrite = 0;
                        currentByteToWriteIndex %= Utility.BYTE_PRIMARY_INDEX;
                        if (byteIndex == bytesNumber - 1)
                        {
                            numberOfUnusedBitsInTheLastByte = Utility.BYTE_PRIMARY_INDEX - currentByteToWriteIndex;
                        }
                    }
                }

                int numberOfTheNewBytes = listOfTheNewBytes.ToArray().Length;
                // Write the number of the bytes
                bWriter.Write(numberOfTheNewBytes);
                // Write the number of the unused bits in the last byte
                bWriter.Write(numberOfUnusedBitsInTheLastByte);
                for (int newBytesIndex = 0; newBytesIndex < numberOfTheNewBytes; newBytesIndex++)
                {
                    currentByteToWrite = (byte)listOfTheNewBytes.ToArray()[newBytesIndex];
                    //Console.WriteLine("Writing byte in the archived file :: " + currentByteToWrite);
                    bWriter.Write(currentByteToWrite);
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
            m_HuffmanDictionary[i_fileIndex] = new Dictionary<byte, byte>();
            CreateHuffmanMap(i_fileIndex, ref finalHuffmanTree, 1);
            //Console.WriteLine(finalHuffmanTree);
            //Console.WriteLine();

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

        private void CreateHuffmanMap(int i_fileIndex, ref BinaryTree i_tree, byte i_code)
        {
            if (i_tree.Left != null)
            {
                BinaryTree tempTree = i_tree.Left;
                CreateHuffmanMap(i_fileIndex, ref tempTree, (byte)(2 * i_code + 0));
            }
            if (i_tree.Right != null)
            {
                BinaryTree tempTree = i_tree.Right;
                CreateHuffmanMap(i_fileIndex, ref tempTree, (byte)(2 * i_code + 1));
            }
            if (i_tree.Left == null && i_tree.Right == null)
            {
                m_HuffmanDictionary[i_fileIndex][i_tree.Node.Byte] = i_code;
            }
        }
    }
}
