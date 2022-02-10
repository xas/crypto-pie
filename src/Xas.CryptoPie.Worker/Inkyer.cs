// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using SkiaSharp;
using System.Drawing;
using Xas.CryptoPie.Drivers;

namespace Xas.CryptoPie.Worker
{
    public class Inkyer : IDisposable
    {
        private readonly Inky _inky;
        private readonly SKFont _font;
        private readonly SKPaint _paintTitle;
        private readonly SKPaint _paintValue;
        private readonly SKBitmap _bitmap;
        private readonly SKCanvas _canvas;
        private int FontSize => 24;
        private SKColor _redColor => new SKColor(128, 128, 128);
        private SKColor _blackColor => new SKColor(0, 0, 0);
        private SKColor _whiteColor => new SKColor(255, 255, 255);

        public Inkyer()
        {
            _inky = new Inky(250, 136, 0, 6, EInkColor.RED);

            _font = new SKFont(SKTypeface.Default, FontSize);
            _paintTitle = new SKPaint(_font);
            _paintValue = new SKPaint(_font);
            _paintTitle.Color = _blackColor;

            SKImageInfo settings = new SKImageInfo(_inky.Width, _inky.Height, SKColorType.Gray8);
            _bitmap = new SKBitmap(settings);
            _canvas = new SKCanvas(_bitmap);
            _canvas.Translate(0, _inky.Height);
            _canvas.Scale(1, -1, 0, 0);
        }

        public void Display(ChartData data)
        {
            if (data == null)
            {
                return;
            }
            _canvas.Clear(_whiteColor);
            int yStart = FontSize + _inky.OffsetY;
            if (data.Color == Color.Red)
            {
                _paintValue.Color = _redColor;
            }
            else
            {
                _paintValue.Color = _blackColor;
            }
            SKTextBlob textTitle = SKTextBlob.Create($"{data.Name} : ", _font);
            SKTextBlob textValue = SKTextBlob.Create($"{data.CurrencySymbol} {data.Value}", _font);
            _canvas.DrawText(textTitle, 0, yStart, _paintTitle);
            _canvas.DrawText(textValue, Math.Abs(textTitle.Bounds.Right) - Math.Abs(textTitle.Bounds.Left), yStart, _paintValue);
            yStart *= 2;

            double max = data.Values.Max();
            double min = data.Values.Min();
            double height = _inky.Height - yStart - _inky.OffsetY;
            double ratio = height / (max - min);
            int xPos = 0;

            foreach(float value in data.Values.Skip(Math.Max(0, data.Values.Count() - _inky.Width)))
            {
                _canvas.DrawPoint(xPos++, (int)(yStart + ((max - value) * ratio)), _paintValue.Color);
            }

            _canvas.Flush();
            _inky.SetBuffer(SplitSimpleArray(_bitmap.Bytes, _inky.Width, _inky.Height));
            _inky.Display();
        }

        private byte[,] SplitSimpleArray(byte[] originalArray, int width, int height)
        {
            if (originalArray.Length != width * height)
            {
                throw new ArgumentOutOfRangeException($"Size error {originalArray.Length} vs {width * height}");
            }
            int monoIndex = 0;
            byte[,] array = new byte[width, height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    array[j, i] = originalArray[monoIndex++];
                }
            }
            return array;
        }

        public void Dispose()
        {
            _inky.Dispose();
        }
    }
}
