using RawFileHandling.Libs;

namespace RawConverter
{
    internal class Program
    {
        public static void Main()
        {
            var inputFile = @"D:\Scratch\RawFileHandling_test_data\M42_03_09_2013\Lights\Lights1.NEF";
            //var outputFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + "_out.bmp");

            //if (File.Exists(outputFile))
            //{
            //    File.Delete(outputFile);
            //}

            //TiffToBmpConverter.Convert16Bit(inputFile, outputFile);
            DCRaw.ReadFile(inputFile);
        }
    }
}
