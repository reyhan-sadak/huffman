using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class Program
    {
        static void Main(string[] args)
        {
            System.ConsoleKeyInfo KInfo;
            string baseFolderPath = "..\\..\\..\\..\\";
            string[] files = { baseFolderPath + "file3.bin" };
            string outputFile = baseFolderPath + "output.bin";
            string outputDirectory = baseFolderPath + "output\\";

            HuffmanCompressor comressor = new HuffmanCompressor(4096, files, outputFile);
            comressor.CompressFiles();

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            KInfo = Console.ReadKey(true);

            HuffmanDecompressor decompressor = new HuffmanDecompressor(outputFile, outputDirectory);
            decompressor.DecompressFiles();
        }
    }
}
