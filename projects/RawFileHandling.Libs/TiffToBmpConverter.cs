using System;
using System.Drawing;
using System.Drawing.Imaging;
using BitMiracle.LibTiff.Classic;

namespace RawFileHandling.Libs
{
    public static class TiffToBmpConverter
    {
        public static void Convert16Bit(
            string inputFile,
            string outputFile)
        {
            using (var sourceImage = Tiff.Open(inputFile, "r"))
            {
                if (sourceImage == null)
                {
                    throw new TiffFileFormatException($"Failed to open file for reading: '{inputFile}'");
                }

                var width = sourceImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var height = sourceImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                var planarConfig = (PlanarConfig)sourceImage.GetField(TiffTag.PLANARCONFIG)[0].Value;
                if (planarConfig != PlanarConfig.CONTIG)
                {
                    throw new TiffFileFormatException($"PlanarConfig {planarConfig} not supported");
                }

                var samples = sourceImage.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
                if (samples != 3)
                {
                    throw new TiffFileFormatException($"Sample count {samples} not supported");
                }

                using (var outputBitmap = new Bitmap(width, height))
                {
                    var data = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    unsafe
                    {
                        var outputBitmapPtr = (byte*)data.Scan0;
                        var lineBuffer = new byte[sourceImage.ScanlineSize()];

                        for (var y = 0; y < height; y++)
                        {
                            var bitmapYOffset = y * data.Stride;

                            sourceImage.ReadScanline(lineBuffer, y);

                            fixed (byte* lineBufferPtr = lineBuffer)
                            {
                                for (var xOffset = 0; xOffset < width * 3; xOffset += 3)
                                {
                                    var bitmapOffset = xOffset + bitmapYOffset;

                                    outputBitmapPtr[bitmapOffset] = (byte)(*((ushort*)lineBufferPtr + xOffset + 2) >> 8); // red
                                    outputBitmapPtr[bitmapOffset + 1] = (byte)(*((ushort*)lineBufferPtr + xOffset + 1) >> 8); // green
                                    outputBitmapPtr[bitmapOffset + 2] = (byte)(*((ushort*)lineBufferPtr + xOffset) >> 8); // blue
                                }
                            }
                        }
                    }

                    outputBitmap.UnlockBits(data);
                    outputBitmap.Save(outputFile);
                }
            }
        }

        private static byte Invert(byte b)
        {
            return Convert.ToByte(255 - b);
        }
    }
}
