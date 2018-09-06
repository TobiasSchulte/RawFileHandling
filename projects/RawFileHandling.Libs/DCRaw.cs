using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RawFileHandling.Libs
{
    public static class DCRaw
    {
        public static void ReadFile(string filename)
        {
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "dcraw-9.27-ms-64-bit.exe",
                    Arguments = $"-W -4 -c \"{filename}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };

            p.Start();
            var binaryReader = new BinaryReader(p.StandardOutput.BaseStream);
            var magicNumber = ReadMagicNumber(binaryReader);
            var (width, height) = ReadWidthAndHeight(binaryReader);
            var maxValue = ReadMaxChannelValue(binaryReader);

            if (magicNumber != "P6")
            {
                throw new RawFileException($"PPM format {magicNumber} not supported");
            }

            if (maxValue != 65535)
            {
                throw new RawFileException($"Channel maximum value {maxValue} not supported");
            }

            Console.WriteLine($"{magicNumber}, width: {width}, height: {height}, maxValue: {maxValue}");

            var imageData = binaryReader.ReadBytes(width * height * 6);
            p.WaitForExit();

            using (var outputBitmap = new Bitmap(width, height))
            {
                var data = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                unsafe
                {
                    var outputBitmapPtr = (byte*)data.Scan0;

                    var imageReader = new BinaryReader(new MemoryStream(imageData));

                    for (var y = 0; y < height; y++)
                    {
                        var bitmapYOffset = y * data.Stride;
                        //Console.WriteLine($"{y}");

                        for (var x = 0; x < width; x++)
                        {
                            //Console.WriteLine($"{x} {y}");

                            var bitmapOffset = (x * 3) + bitmapYOffset;

                            var red = ReadBigEndianUInt16(imageReader);
                            var green = ReadBigEndianUInt16(imageReader);
                            var blue = ReadBigEndianUInt16(imageReader);

                            outputBitmapPtr[bitmapOffset] = (byte)(red >> 8); // red
                            outputBitmapPtr[bitmapOffset + 1] = (byte)(green >> 8); // green
                            outputBitmapPtr[bitmapOffset + 2] = (byte)(blue >> 8); // blue
                        }
                    }
                }

                outputBitmap.UnlockBits(data);
                outputBitmap.Save(@"C:\Users\705562\Desktop\blub.bmp");
            }
        }

        private static string ReadMagicNumber(BinaryReader binaryReader)
        {
            var magicNumber = new string(binaryReader.ReadChars(2));
            var delimiter = binaryReader.ReadChar();

            if (!magicNumber.StartsWith("P") || !char.IsDigit(magicNumber[1]) || delimiter != '\n')
            {
                throw new RawFileException("Magic number format not recognized");
            }

            return magicNumber;
        }

        private static (int, int) ReadWidthAndHeight(BinaryReader binaryReader)
        {
            var line = new string(binaryReader.ReadChars(3));
            var c = binaryReader.ReadChar();
            while (c != '\n')
            {
                line = line + c;
                c = binaryReader.ReadChar();
            }

            var parts = line.Split(' ');
            if (parts.Length != 2)
            {
                throw new RawFileException("Cannot read width and height");
            }

            try
            {
                var width = int.Parse(parts[0]);
                var height = int.Parse(parts[1]);

                return (width, height);
            }
            catch (FormatException e)
            {
                throw new RawFileException("dimension format error", e);
            }
        }

        private static int ReadMaxChannelValue(BinaryReader binaryReader)
        {
            var line = new string(binaryReader.ReadChars(1));
            var c = binaryReader.ReadChar();
            while (c != '\n')
            {
                line = line + c;
                c = binaryReader.ReadChar();
            }

            try
            {
                return int.Parse(line);
            }
            catch (FormatException e)
            {
                throw new RawFileException("maximum channel value format error", e);
            }
        }

        private static ushort ReadBigEndianUInt16(BinaryReader reader)
        {
            return BitConverter.ToUInt16(reader.ReadBytes(sizeof(ushort)).Reverse().ToArray(), 0);
        }
    }
}
