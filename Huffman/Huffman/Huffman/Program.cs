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
            //string[] files = { "D:\\C#\\file1.bin", "D:\\C#\\picture.jpg" };//, "D:\\C#\\file2.bin" };
            string[] files = { /*"D:\\C#\\picture.jpg", */ "D:\\C#\\file2.bin", "D:\\C#\\file1.bin", };
            string outputFile = "D:\\C#\\output.bin";
            if (false)//args.Length < 2)
            {
                Console.WriteLine("Please enter the path to the output file:");
                outputFile = Console.ReadLine();
                Console.WriteLine("Please enter the number of files you want to add to the archive:");
                string numberOfFilesStr = Console.ReadLine();
                int numberOfFiles = Convert.ToInt32(numberOfFilesStr);
                files = new string[numberOfFiles];
                Console.WriteLine("Please enter {0} file paths:", numberOfFiles);
                for (int i = 0; i < numberOfFiles; i++)
                {
                    string filePath = Console.ReadLine();
                    files[i] = filePath;
                }
                Console.WriteLine("Press any key to continue...");
                KInfo = Console.ReadKey(true);
            }

            HuffmanCompressor comressor = new HuffmanCompressor(files, outputFile);
            comressor.WriteOutputFile();

            Console.WriteLine("Blabala");
            Console.WriteLine("Press any key to continue...");
            KInfo = Console.ReadKey(true);

            HuffmanDecompressor decompressor = new HuffmanDecompressor(outputFile);
            decompressor.WriteOutputFiles();
        }
    }
}
