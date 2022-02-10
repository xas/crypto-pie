// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.Spi;

namespace Xas.CryptoPie.Drivers
{
    public class Inky : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public int OffsetX { get; }
        public int OffsetY { get; }
        public EInkColor InkColor { get; }
        public BorderColor BorderColor { get; private set; }
        private readonly GpioController _gpioController;
        private readonly SpiDevice _spiDevice;
        private readonly byte[] _lut;
        private byte[,] _buffer;

        private const int DataPin = 15;
        private const int ResetPin = 13;
        private const int BusyPin = 11;

        private const byte DRIVER_CONTROL = 0x01;
        private const byte WRITE_DUMMY = 0x3A;
        private const byte WRITE_GATELINE = 0x3B;
        private const byte DATA_MODE = 0x11;
        private const byte SET_RAMXPOS = 0x44;
        private const byte SET_RAMYPOS = 0x45;
        private const byte WRITE_VCOM = 0x2C;
        private const byte WRITE_LUT = 0x32;
        private const byte WRITE_BORDER = 0x3C;
        private const byte SET_RAMXCOUNT = 0x4E;
        private const byte SET_RAMYCOUNT = 0x4F;
        private const byte WRITE_RAM = 0x24;
        private const byte WRITE_ALTRAM = 0x26;
        private const byte MASTER_ACTIVATE = 0x20;

        public Inky(int width, int height, int offsetX, int offsetY, EInkColor color)
        {
            Width = width;
            Height = height;
            OffsetX = offsetX;
            OffsetY = offsetY;
            InkColor = color;
            _buffer = new byte[Width, Height];

            switch (color)
            {
                case EInkColor.BLACK:
                    _lut = LutEnumeration.BlackLut;
                    BorderColor = BorderColor.BLACK;
                    break;
                case EInkColor.RED:
                    _lut = LutEnumeration.RedLut;
                    BorderColor = BorderColor.RED;
                    break;
                case EInkColor.YELLOW:
                    _lut = LutEnumeration.YellowLut;
                    BorderColor = BorderColor.YELLOW;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color));
            }

            _gpioController = new GpioController(PinNumberingScheme.Board, new RaspberryPi3Driver());
            _gpioController.OpenPin(DataPin, PinMode.Output, PinValue.Low);
            _gpioController.OpenPin(ResetPin, PinMode.Output, PinValue.High);
            _gpioController.OpenPin(BusyPin, PinMode.Input);
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 0);
            settings.ClockFrequency = 488000;
            settings.DataFlow = DataFlow.MsbFirst;
            _spiDevice = SpiDevice.Create(settings);

            Initialize();
        }

        private void Initialize()
        {
            _gpioController.Write(ResetPin, PinValue.Low);
            Thread.Sleep(500);
            _gpioController.Write(ResetPin, PinValue.High);
            Thread.Sleep(500);
            SendCommand(0x12);
            BusySleeper();
        }

        private void Update(byte[,] buffer)
        {
            Initialize();
            SendCommand(DRIVER_CONTROL);
            SendData(new byte[] { (byte)(Width - 1), (byte)((Width - 1) >> 8), 0x00 });
            SendCommand(WRITE_DUMMY);
            SendData(0x1B);
            SendCommand(WRITE_GATELINE);
            SendData(0x0B);
            SendCommand(DATA_MODE);
            SendData(0x03);
            SendCommand(SET_RAMXPOS);
            SendData(new byte[] { 0x00, (byte)(Height / 8 - 1) });
            SendCommand(SET_RAMYPOS);
            SendData(new byte[] { 0x00, 0x00, (byte)((Width - 1) & 0xFF), (byte)((Width - 1) >> 8) });
            SendCommand(WRITE_VCOM);
            SendData(0x70);
            SendCommand(WRITE_LUT);
            SendData(_lut);
            SendCommand(WRITE_BORDER);
            SendData((byte)BorderColor);
            SendCommand(SET_RAMXCOUNT);
            SendData(0x00);
            SendCommand(SET_RAMYCOUNT);
            SendData(new byte[] { 0x00, 0x00 });
            byte[] bufferBlack = buffer.PackBits(x => x > 192);
            byte[] bufferRed = buffer.PackBits(x => x > 64 && x < 192);
            SendCommand(WRITE_RAM);
            SendData(bufferBlack);
            SendCommand(WRITE_ALTRAM);
            SendData(bufferRed);
            BusySleeper();
            SendCommand(MASTER_ACTIVATE);
        }

        public void Display()
        {
            Update(_buffer);
        }

        public void SetBuffer(byte[,] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            if (buffer.GetLength(0) != Width || buffer.GetLength(1) != Height)
            {
                throw new ArgumentOutOfRangeException($"Error on buffer size {buffer.GetLength(0)}x{buffer.GetLength(1)}");
            }
            _buffer = buffer;
            Update(_buffer.RotateArrayAntiClockwise());
        }

        public void SetPixel(int x, int y, byte color)
        {
            _buffer[x, y] = color;
        }

        public void SetBorder(BorderColor border)
        {
            if (border == BorderColor.RED && InkColor == EInkColor.RED)
            {
                BorderColor = BorderColor.RED;
            }
            else if (border == BorderColor.YELLOW && InkColor == EInkColor.YELLOW)
            {
                BorderColor = BorderColor.YELLOW;
            }
            else
            {
                BorderColor = border == BorderColor.BLACK ? BorderColor.BLACK : BorderColor.WHITE;
            }
        }

        public bool IsBusy() => _gpioController.Read(BusyPin) == PinValue.High;

        private void BusySleeper()
        {
            TimeSpan elapsedTime = TimeSpan.Zero;
            DateTime now = DateTime.Now;
            while (IsBusy() && elapsedTime < TimeSpan.FromSeconds(5))
            {
                Thread.Sleep(100);
                elapsedTime = DateTime.Now - now;
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
    }
}
