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
        private int m_chunkSize;
        public int ChunkSize
        {
            get
            {
                return m_chunkSize;
            }
            set
            {
                m_chunkSize = value;
            }
        }
        private string m_outputFilePath;
        public string OutputFilePath
        {
            get
            {
                return m_outputFilePath;
            }
            set
            {
                m_outputFilePath = value;
            }
        }
        private string[] m_inputFilesNames;
        public string[] InputFileNames
        {
            get
            {
                return m_inputFilesNames;
            }
            set
            {
                m_inputFilesNames = value;
            }
        }

        public HuffmanCompressor(int iChunkSize, string[] iFilesList, string iOutputFileName)
        {
            this.ChunkSize = iChunkSize;
            this.OutputFilePath = iOutputFileName;
            this.InputFileNames = iFilesList;
        }

        public void CompressFiles()
        {
            // open the output file
            FileStream fs = new FileStream(m_outputFilePath, FileMode.Create);
            BinaryWriter bWriter = new BinaryWriter(fs);

            // write the number of files
            int filesCount = this.InputFileNames.Count();
            bWriter.Write(filesCount); // int

            // iterate all files and compress them
            for (int fileIndex = 0; fileIndex < filesCount; ++fileIndex)
            {
                string filePath = InputFileNames[fileIndex];
                int lastIndex = filePath.LastIndexOf("\\");
                InputFileNames[fileIndex] = filePath.Substring(lastIndex + 1);

                if (File.Exists(filePath))
                {
                    // write the current file name
                    int lenghtOfTheName = InputFileNames[fileIndex].Length;
                    bWriter.Write(lenghtOfTheName); // int
                    for (int nameIndex = 0; nameIndex < lenghtOfTheName; nameIndex++)
                    {
                        char character = InputFileNames[fileIndex][nameIndex];
                        bWriter.Write((byte)character);
                    }

                    FileStream inputFileStream = new FileStream(filePath, FileMode.Open);
                    BinaryReader bReader = new BinaryReader(inputFileStream);

                    Utility.PrintInfo("File: " + InputFileNames[fileIndex] + " with size: " + inputFileStream.Length.ToString());

                    byte[] chunk = new byte[ChunkSize];
                    int chunksCount = 0;
                    // write chunks

                    string tempFileName = filePath + ".temp";
                    FileStream tempFile = new FileStream(tempFileName, FileMode.Create);
                    BinaryWriter tempBWriter = new BinaryWriter(tempFile);

                    // write chunk to temporary file
                    while(bReader.BaseStream.Position != bReader.BaseStream.Length)
                    {
                        int readBytes = bReader.Read(chunk, 0, ChunkSize);
                        HuffmanChunk huffmanChunk = new HuffmanChunk(HuffmanChunk.EChunkMode.ChunkMode_Compress, chunk, readBytes);
                        MemoryStream stream = new MemoryStream();
                        huffmanChunk.SerializeOutput(ref stream);
                        int length = (int)stream.Length;
                        chunksCount++;
                        tempBWriter.Write(length);
                        tempBWriter.Write(stream.GetBuffer(), 0, length);
                    }

                    tempBWriter.Close();

                    // write chinks count
                    bWriter.Write(chunksCount);

                    FileStream tempReadFile = new FileStream(tempFileName, FileMode.Open);
                    BinaryReader tempBReader = new BinaryReader(tempReadFile);
                    
                    // write temp file to the final file
                    byte[] tempByteArray = new byte[tempBReader.BaseStream.Length];
                    tempBReader.Read(tempByteArray, 0, (int)tempBReader.BaseStream.Length);
                    bWriter.Write(tempByteArray, 0, (int)tempBReader.BaseStream.Length);
                    tempBReader.Close();

                    // delete temporary file
                    File.Delete(tempFileName);
                }
            }
            bWriter.Close();
            fs.Close();

            FileInfo fInfo = new FileInfo(m_outputFilePath);
            Utility.PrintInfo("Output file: " + m_outputFilePath + " with size: " + fInfo.Length.ToString());
        }
    }
}
