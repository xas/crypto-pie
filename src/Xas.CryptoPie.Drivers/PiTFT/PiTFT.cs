// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.Spi;

namespace Xas.CryptoPie.Drivers
{
    public class PiTFT : IDisposable
    {
        private readonly GpioController _gpioController;
        private readonly SpiDevice _spiDevice;
        private const int DataPin = 22;
        private const int BufferPixel = 128;
        private const int BacklightPin = 15;
        private const int ButtonA = 16;
        private const int ButtonB = 18;
        private bool _inversion;

        private const byte SWRESET = 0x01;
        private const byte SPLOUT = 0x11;
        private const byte COLMOD = 0x3A;
        private const byte COL_65K = 0x50;
        private const byte COL_16BIT = 0x05;
        private const byte MADCTL = 0x36;
        private const byte MADCTL_ML = 0xC0;
        private const byte INVON = 0x21;
        private const byte NORON = 0x13;
        private const byte DISPON = 0x29;

        public int Width { get; }
        public int Height { get; }
        public int OffsetX { get; }
        public int OffsetY { get; }

        public PiTFT(int width, int height, int offsetX = 0, int offsetY = 0)
        {
            Width = width;
            Height = height;
            OffsetX = offsetX;
            OffsetY = offsetY;
            _gpioController = new GpioController(PinNumberingScheme.Board, new RaspberryPi3Driver());
            SpiConnectionSettings spiSettings = new SpiConnectionSettings(0, 0);
            spiSettings.ClockFrequency = 24000000;
            _spiDevice = SpiDevice.Create(spiSettings);

            _gpioController.OpenPin(DataPin, PinMode.Output, PinValue.High);
            _gpioController.OpenPin(BacklightPin, PinMode.Output, PinValue.High);
            _gpioController.OpenPin(ButtonA, PinMode.Input);
            _gpioController.OpenPin(ButtonB, PinMode.Input);

            Initialize();
        }

        private void Initialize()
        {
            SendCommand(SWRESET);
            Thread.Sleep(150);
            SendCommand(SPLOUT);
            SendCommand(COLMOD);
            SendData(COL_65K | COL_16BIT);
            Thread.Sleep(10);
            SendCommand(MADCTL);
            SendData(MADCTL_ML);
            SendCommand(INVON);
            Thread.Sleep(10);
            SendCommand(NORON);
            Thread.Sleep(10);

            InitWindows();

            SendCommand(DISPON);
            Thread.Sleep(100);
        }

        public void InitWindows()
        {
            InitWindows(0, 0, Width, Height);
        }

        public void InitWindows(int x0, int y0, int x1, int y1)
        {
            x0 += OffsetX;
            x1 += OffsetX - 1;
            y0 += OffsetY;
            y1 += OffsetY - 1;

            byte[] xData = new byte[] { (byte)(x0 >> 8), (byte)(x0 & 0xFF), (byte)(x1 >> 8), (byte)(x1 & 0xFF) };
            byte[] yData = new byte[] { (byte)(y0 >> 8), (byte)(y0 & 0xFF), (byte)(y1 >> 8), (byte)(y1 & 0xFF) };

            SendCommand((byte)LcdCommand.CASET);
            SendData(xData);
            SendCommand((byte)LcdCommand.RASET);
            SendData(yData);
            SendCommand((byte)LcdCommand.RAMWR);
        }

        public void SendRawBuffer(byte[] array)
        {
            InitWindows();
            int chunks = Width * Height / BufferPixel;
            int rest = Width * Height % BufferPixel;
            _gpioController.Write(DataPin, PinValue.High);
            if (chunks > 0)
            {
                for (int i = 0; i < chunks; i++)
                {
                    _spiDevice.Write(array);
                }
            }
            if (rest > 0)
            {
                _spiDevice.Write(array[0..(rest * 2)]);
            }
        }

        private void SendCommand(byte command)
        {
            _gpioController.Write(DataPin, PinValue.Low);
            _spiDevice.WriteByte(command);
        }

        private void SendCommand(byte[] command)
        {
            _gpioController.Write(DataPin, PinValue.Low);
            _spiDevice.Write(command);
        }

        private void SendData(byte data)
        {
            _gpioController.Write(DataPin, PinValue.High);
            _spiDevice.WriteByte(data);
        }

        private void SendData(byte[] data)
        {
            _gpioController.Write(DataPin, PinValue.High);
            _spiDevice.Write(data);
        }

        public void Dispose()
        {
            _spiDevice?.Dispose();
            _gpioController?.Dispose();
        }

        public void Inverse()
        {
            _inversion = !_inversion;
            _gpioController.Write(BacklightPin, _inversion);
        }
    }
}
