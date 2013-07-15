using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Huffman
{
    class HuffmanDecompressor
    {
        private string m_inputFilePath;
        private Dictionary<byte, byte>[] m_HuffmanDictionary;
        private byte[][] m_filesBytes;
        private string[] m_inputFilesNames;
        private int m_numberOfFiles;
        private int[] m_numberOfUnusedBits;
        string m_outputDirectoryPath;

        public HuffmanDecompressor(string i_inputFilePath, string i_pathToOutPutDirectory = "")
        {
            m_outputDirectoryPath = i_pathToOutPutDirectory;
            m_inputFilePath = i_inputFilePath;

            FileStream fs = new FileStream(m_inputFilePath, FileMode.Open);
            BinaryReader bReader = new BinaryReader(fs);

            // Read the number of files
            m_numberOfFiles = bReader.ReadInt32();
            m_HuffmanDictionary = new Dictionary<byte, byte>[m_numberOfFiles];
            m_filesBytes = new byte[m_numberOfFiles][];
            m_inputFilesNames = new string[m_numberOfFiles];
            m_numberOfUnusedBits = new int[m_numberOfFiles];

            // Read the information about the files
            for (int filesIndex = 0; filesIndex < m_numberOfFiles; filesIndex++)
            {
                // Read the name of the file
                int lenghtOfTheName = bReader.ReadInt32();
                byte[] readBytes = new byte[lenghtOfTheName];
                for (int nameIndex = 0; nameIndex < lenghtOfTheName; nameIndex++)
                {
                    byte readCharacter = bReader.ReadByte();
                    readBytes[nameIndex] = readCharacter;
                }
                UTF8Encoding encoding = new UTF8Encoding();
                m_inputFilesNames[filesIndex] = encoding.GetString(readBytes);
                m_HuffmanDictionary[filesIndex] = new Dictionary<byte, byte>();
                // Read the Huffman dictionary
                int numberOfTheItemsInTheDictionary = bReader.ReadInt32();
                Console.WriteLine("Reverse Huffman dictionary");
                for (int dictionaryIndex = 0; dictionaryIndex < numberOfTheItemsInTheDictionary; dictionaryIndex++)
                {
                    byte value = bReader.ReadByte();
                    byte key = bReader.ReadByte();
                    m_HuffmanDictionary[filesIndex][key] = value;
                    Console.WriteLine("HuffmanDict[ " + key + " ] = " + m_HuffmanDictionary[filesIndex][key]);
                }

                int bytesNumber = bReader.ReadInt32();
                m_numberOfUnusedBits[filesIndex] = bReader.ReadInt32();
                m_filesBytes[filesIndex] = new byte[bytesNumber];
                for (int bytesIndex = 0; bytesIndex < bytesNumber; bytesIndex++)
                {
                    byte readByte = bReader.ReadByte();
                    m_filesBytes[filesIndex][bytesIndex] = readByte;
                    //Console.WriteLine("Reading byte from the archived file :: " + readByte);
                }
            }

            bReader.Close();
            fs.Close();
        }

        public void WriteOutputFiles()
        {
            string outputDir;
            if (m_outputDirectoryPath.Equals(""))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            else
            {
                outputDir = m_outputDirectoryPath;
            }
            for (int filesIndex = 0; filesIndex < m_numberOfFiles; filesIndex++)
            {
                string filePath = outputDir + "\\" + m_inputFilesNames[filesIndex];

                FileStream fs = new FileStream(filePath, FileMode.Create);
                BinaryWriter bWriter = new BinaryWriter(fs);

                int bytesNumber = m_filesBytes[filesIndex].Length;
                byte value = 0;
                for (int bytesIndex = 0; bytesIndex < bytesNumber; bytesIndex++)
                {
                    byte currentByte = m_filesBytes[filesIndex][bytesIndex];
                    byte keyIndex = Utility.BYTE_PRIMARY_INDEX;
                    value *= 2;
                    value += Utility.GetBitAtIndexOfByte(currentByte, keyIndex);
                    int limitOfTheLoop = 0;
                    if (bytesIndex == bytesNumber - 1)
                    {
                        limitOfTheLoop = m_numberOfUnusedBits[filesIndex];
                    }
                    for (; keyIndex > limitOfTheLoop; keyIndex--)
                    {
                        byte valueInTheMap = value;
                        if (m_HuffmanDictionary[filesIndex].ContainsKey(valueInTheMap))
                        {
                            //Console.WriteLine("Writing byte :: " + m_HuffmanDictionary[filesIndex][value]);
                            byte toWriteByte = m_HuffmanDictionary[filesIndex][value];
                            bWriter.Write(toWriteByte);
                            value = Utility.GetBitAtIndexOfByte(currentByte, (byte)(keyIndex - 1));
                        }
                        else if (keyIndex > 1)
                        {
                                value *= 2;
                                value += Utility.GetBitAtIndexOfByte(currentByte, (byte)(keyIndex - 1));
                        }
                    }
                }

                bWriter.Close();
                fs.Close();
            }
        }
    }
}
