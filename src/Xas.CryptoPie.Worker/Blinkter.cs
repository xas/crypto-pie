// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Drawing;
using Xas.CryptoPie.Drivers;

namespace Xas.CryptoPie.Worker
{
    public class Blinkter : IDisposable
    {
        private readonly Blinkt _blinkt;

        public Blinkter()
        {
            _blinkt = new Blinkt();
        }

        public void Display(byte totalLed, Color color)
        {
            ShowRefresh();
            _blinkt.Clear();
            for (int i = 0; i < Math.Min(totalLed, (byte)8); i++)
            {
                _blinkt.SetPixel(i, color);
            }
            _blinkt.Show();
        }

        public void ShowRefresh()
        {
            for (int i = 0; i < 8; i++)
            {
                _blinkt.Clear();
                _blinkt.SetPixel(i, Color.White);
                _blinkt.Show();
                Thread.Sleep(50);
            }
            for (int i = 8; i > 0; i--)
            {
                _blinkt.Clear();
                _blinkt.SetPixel(i - 1, Color.White);
                _blinkt.Show();
                Thread.Sleep(50);
            }
        }

        public void Off()
        {
            _blinkt.Clear();
            _blinkt.Show();
        }

        public void Dispose()
        {
            _blinkt.Dispose();
        }
    }
}
