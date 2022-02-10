// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Drawing;

namespace Xas.CryptoPie.Drivers
{
    public static class HelperExtensions
    {
        public static ushort Color12bppRgb444(this Color color) =>// 0xFF;
                   (ushort)(((color.R & 0b11110000) << 4) | (color.G & 0b11110000) | (color.B >> 4));

        public static ushort Color16bppRgb565(this Color color) =>
            (ushort)(((color.R & 0xF8) << 8) | ((color.G & 0xFC) << 3) | (color.B >> 3));

        public static byte[] PackBits(this byte[,] array, Func<byte, bool> func)
        {
            List<byte> bits = new List<byte>();
            int currentIndex = 1;
            byte currentValue = 0;
            foreach (byte pixel in array)
            {
                if (currentIndex % 8 == 0)
                {
                    currentValue |= (byte)(func(pixel) ? 1 : 0);
                    bits.Add(currentValue);
                    currentIndex = 1;
                    currentValue = 0;
                }
                else
                {
                    currentValue |= (byte)((func(pixel) ? 1 : 0) << (8 - currentIndex));
                    currentIndex++;
                }
            }
            if (currentIndex != 1)
            {
                bits.Add(currentValue);
            }
            return bits.ToArray();
        }

        public static byte[,] RotateArrayAntiClockwise(this byte[,] src)
        {
            int width;
            int height;
            byte[,] dst;

            width = src.GetUpperBound(0) + 1;
            height = src.GetUpperBound(1) + 1;
            dst = new byte[height, width];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int newRow;
                    int newCol;

                    newRow = width - (col + 1);
                    newCol = row;

                    dst[newCol, newRow] = src[col, row];
                }
            }

            return dst;
        }
    }
}
