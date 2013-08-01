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
        private Dictionary<uint, byte>[] m_HuffmanDictionary;
        private uint[][] m_filesUints;
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
            m_HuffmanDictionary = new Dictionary<uint, byte>[m_numberOfFiles];
            m_filesUints = new uint[m_numberOfFiles][];
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
                m_HuffmanDictionary[filesIndex] = new Dictionary<uint, byte>();
                // Read the Huffman dictionary
                int numberOfTheItemsInTheDictionary = bReader.ReadInt32();
                Console.WriteLine("Reverse Huffman dictionary");
                for (int dictionaryIndex = 0; dictionaryIndex < numberOfTheItemsInTheDictionary; dictionaryIndex++)
                {
                    byte value = bReader.ReadByte();
                    uint key = bReader.ReadUInt32();
                    m_HuffmanDictionary[filesIndex][key] = value;
                    Console.WriteLine("HuffmanDict[ " + key + " ] = " + m_HuffmanDictionary[filesIndex][key]);
                }

                int UIntsNumber = bReader.ReadInt32();
                m_numberOfUnusedBits[filesIndex] = bReader.ReadInt32();
                m_filesUints[filesIndex] = new uint[UIntsNumber];
                for (int bytesIndex = 0; bytesIndex < UIntsNumber; bytesIndex++)
                {
                    uint readUInt = bReader.ReadUInt32();
                    m_filesUints[filesIndex][bytesIndex] = readUInt;
                    Console.WriteLine("Reading byte from the archived file :: " + readUInt);
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

                int UIntsNumber = m_filesUints[filesIndex].Length;
                uint value = 1;
                for (int uintIndex = 0; uintIndex < UIntsNumber; uintIndex++)
                {
                    uint currentUInt = m_filesUints[filesIndex][uintIndex];
                    int keyIndex = Utility.UINT_PRIMARY_INDEX;
                    value *= 2;
                    value += Utility.GetBitAtIndexOfUint(currentUInt, keyIndex) ? 1u : 0u;
                    int limitOfTheLoop = 0;
                    if (uintIndex == UIntsNumber - 1)
                    {
                        limitOfTheLoop = m_numberOfUnusedBits[filesIndex];
                    }
                    for (; keyIndex > limitOfTheLoop; keyIndex--)
                    {
                        uint valueInTheMap = value;
                        if (m_HuffmanDictionary[filesIndex].ContainsKey(valueInTheMap))
                        {
                            Console.WriteLine("Writing byte :: " + m_HuffmanDictionary[filesIndex][value]);
                            byte toWriteByte = m_HuffmanDictionary[filesIndex][value];
                            bWriter.Write(toWriteByte);
                            value = 1;
                            if (keyIndex > limitOfTheLoop + 1)
                            {
                                value *= 2;
                                value += Utility.GetBitAtIndexOfUint(currentUInt, (keyIndex - 1)) ? 1u : 0u;
                            }
                        }
                        else if (keyIndex > 1)
                        {
                            value *= 2;
                            value += Utility.GetBitAtIndexOfUint(currentUInt, (keyIndex - 1)) ? 1u : 0u;
                        }
                    }
                }

                bWriter.Close();
                fs.Close();
            }
        }
    }
}
