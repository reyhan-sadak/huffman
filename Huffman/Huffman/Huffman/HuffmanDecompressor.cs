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
        private string[] m_inputFilesNames;
        private uint m_numberOfFiles;
        string m_outputDirectoryPath;

        public HuffmanDecompressor(string i_inputFilePath, string i_pathToOutPutDirectory = "")
        {
            m_outputDirectoryPath = i_pathToOutPutDirectory;
            m_inputFilePath = i_inputFilePath;
        }

        public void DecompressFiles()
        {
            FileStream inputFs = new FileStream(m_inputFilePath, FileMode.Open);
            BinaryReader bReader = new BinaryReader(inputFs);

            // Read the number of files
            m_numberOfFiles = bReader.ReadUInt32();
            m_inputFilesNames = new string[m_numberOfFiles];

            string outputDir;
            if (m_outputDirectoryPath.Equals(""))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            else
            {
                outputDir = m_outputDirectoryPath;
            }
            for (uint filesIndex = 0; filesIndex < m_numberOfFiles; filesIndex++)
            {
                // read the name of the file
                uint lenghtOfTheName = bReader.ReadUInt32();
                byte[] readBytes = new byte[lenghtOfTheName];
                for (uint nameIndex = 0; nameIndex < lenghtOfTheName; nameIndex++)
                {
                    byte readCharacter = bReader.ReadByte();
                    readBytes[nameIndex] = readCharacter;
                }
                UTF8Encoding encoding = new UTF8Encoding();
                m_inputFilesNames[filesIndex] = encoding.GetString(readBytes);

                // read chunks

                // read chunks count
                int chunksCount = bReader.ReadInt32();
                int chunkIndex = 0;
                string filePath = outputDir + "\\" + m_inputFilesNames[filesIndex];
                FileStream fs = new FileStream(filePath, FileMode.Create);
                BinaryWriter bWriter = new BinaryWriter(fs);

                while (chunkIndex < chunksCount && bReader.BaseStream.Position != bReader.BaseStream.Length)
                {
                    int dataLength = bReader.ReadInt32();
                    byte[] data = new byte[dataLength];
                    int readBytesCount = bReader.Read(data, 0, dataLength);
                    chunkIndex++;
                    HuffmanChunk huffmanChunk = new HuffmanChunk(HuffmanChunk.EChunkMode.ChunkMode_Decompress, data, readBytesCount);
                    MemoryStream stream = new MemoryStream();
                    huffmanChunk.SerializeOutput(ref stream);
                    bWriter.Write(stream.GetBuffer(), 0, (int)stream.Length);
                }

                bWriter.Close();
                fs.Close();
            }

            bReader.Close();
            inputFs.Close();
        }
    }
}
