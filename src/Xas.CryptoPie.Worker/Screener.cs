// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using SkiaSharp;
using System.Drawing;
using Xas.CryptoPie.Drivers;

namespace Xas.CryptoPie.Worker
{
    public class Screener : IDisposable
    {
        private readonly PiTFT _piTft;
        private readonly SKFont _font;
        private readonly SKPaint _paintTitle;
        private readonly SKPaint _paintValue;
        private readonly SKBitmap _bitmap;
        private readonly SKCanvas _canvas;
        private int FontSize => 24;
        private Color Dump => Color.Red;
        private Color Pump => Color.Green;

        public Screener()
        {
            _piTft = new PiTFT(135, 240, 53, 40);
            _font = new SKFont(SKTypeface.Default, FontSize);
            _paintTitle = new SKPaint(_font);
            _paintValue = new SKPaint(_font);
            _paintTitle.SetColor(new SKColor(255, 255, 255), SKColorSpace.CreateSrgb());

            SKImageInfo settings = new SKImageInfo(_piTft.Width, _piTft.Height, SKColorType.Rgb565);
            _bitmap = new SKBitmap(settings);
            _canvas = new SKCanvas(_bitmap);
            // dirty hack
            _canvas.Translate(24, 235);
            _canvas.RotateDegrees(270);
        }

        public void Display(IEnumerable<TftData> datas)
        {
            if (datas == null || !datas.Any())
            {
                return;
            }
            _canvas.Clear();
            int yStart = 0;
            foreach (TftData data in datas)
            {
                if (data.Color == Dump)
                {
                    _paintValue.SetColor(new SKColor(0, 255, 0), SKColorSpace.CreateSrgb());
                }
                else
                {
                    _paintValue.SetColor(new SKColor(0, 0, 255), SKColorSpace.CreateSrgb());
                }
                SKTextBlob textTitle = SKTextBlob.Create($"{data.Name} : ", _font);
                SKTextBlob textValue = SKTextBlob.Create($"{data.CurrencySymbol} {data.Value}", _font);
                _canvas.DrawText(textTitle, 0, yStart, _paintTitle);
                _canvas.DrawText(textValue, Math.Abs(textTitle.Bounds.Right) - Math.Abs(textTitle.Bounds.Left), yStart, _paintValue);
                yStart += FontSize;
            }
            _canvas.Flush();
            _piTft.SendRawBuffer(_bitmap.Bytes);
        }

        public void Dispose()
        {
            _canvas?.Dispose();
            _bitmap?.Dispose();
            _piTft?.Dispose();
        }
    }
}
