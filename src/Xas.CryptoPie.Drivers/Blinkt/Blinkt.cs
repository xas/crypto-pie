// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

/*
 * Library ported to C3 for Windows 10 iOt by Marc Jennings (marc-jennings.co.uk)
 * Original from https://github.com/MarcJenningsUK/PimoroniSharp
 * Refactored by https://github.com/xas
 */
using System.Device.Gpio;
using System.Drawing;

namespace Xas.CryptoPie.Drivers
{
    public class Blinkt : IDisposable
    {
        private const int DATA_PIN = 23;
        private const int CLK_PIN = 24;
        private const int NUM_PIXELS = 8;
        private const decimal BRIGHTNESS = 0.2m;

        Pixel[] pixels = new Pixel[NUM_PIXELS];

        private GpioController _gpioController;

        public Blinkt()
        {
            for (int count = 0; count < NUM_PIXELS; count++)
            {
                pixels[count] = new Pixel();
            }
            SetupGpio();
        }

        public void Dispose()
        {
            _gpioController?.Dispose();
            _gpioController = null!;
        }

        private void SetupGpio()
        {
            _gpioController = new GpioController();
            if (null == _gpioController)
            {
                throw new ArgumentNullException("No GPIO controller on this device");
            }

            /*
             If you get exceptions one the next couple of lines, particularly a message that the pin 
             is already in use, or inaccessible, then please ensure you add the line
                     <DeviceCapability Name="lowLevel" />
             to the capabilities section of package.appxmanifest in the calling app.
             */

            _gpioController.OpenPin(DATA_PIN, PinMode.Output, PinValue.Low);
            _gpioController.OpenPin(CLK_PIN, PinMode.Output, PinValue.Low);

            Show();
        }

        private void WritePixel(Pixel pixel)
        {
            var sendBright = (int)(31.0m * pixel.Brightness) & 0x1F;
            write_byte(Convert.ToByte(0xE0 | sendBright));
            write_byte(pixel.Blue);
            write_byte(pixel.Green);
            write_byte(pixel.Red);
        }

        private void write_byte(byte input)
        {
            //int value;
            byte modded = input;
            PinValuePair[] toWrite = new PinValuePair[8 * 3];
            for (int count = 0; count < toWrite.Length; count += 3)
            {
                toWrite[count] = new PinValuePair(DATA_PIN, (modded & 0x80) > 0 ? PinValue.High : PinValue.Low);
                toWrite[count + 1] = new PinValuePair(CLK_PIN, PinValue.High);
                toWrite[count + 2] = new PinValuePair(CLK_PIN, PinValue.Low);
                modded = Convert.ToByte((modded << 1) % 256);
                //value = modded & 0x80;
                //_gpioController.Write(DATA_PIN, value == 0x80 ? PinValue.High : PinValue.Low);
                //_gpioController.Write(CLK_PIN, PinValue.High);
                //modded = Convert.ToByte((modded << 1) % 256);
                //_gpioController.Write(CLK_PIN, PinValue.Low);
            }
            _gpioController.Write(toWrite.AsSpan());
        }

        private void LockClock()
        {
            _gpioController.Write(DATA_PIN, PinValue.Low);
            for (int count = 0; count < 36; count++)
            {
                _gpioController.Write(CLK_PIN, PinValue.High);
                _gpioController.Write(CLK_PIN, PinValue.Low);
            }
        }

        private void ReleaseClock()
        {
            _gpioController.Write(DATA_PIN, PinValue.Low);
            for (int count = 0; count < 32; count++)
            {
                _gpioController.Write(CLK_PIN, PinValue.High);
                _gpioController.Write(CLK_PIN, PinValue.Low);
            }
        }

        /// <summary>
        /// Outputs the state of the pixels to the Blinkt hardware.
        /// </summary>
        public void Show()
        {
            LockClock();

            foreach (var pixel in pixels)
            {
                WritePixel(pixel);
            }

            ReleaseClock();
        }

        /// <summary>
        /// Sets the overall brightness of the pixels.
        /// </summary>
        /// <param name="bright"></param>
        public void SetBrightness(decimal bright)
        {
            if (bright < 0 || bright > 1)
            {
                throw new ArgumentOutOfRangeException("Brightness must be a value between 0 and 1");
            }
            for (int count = 0; count < NUM_PIXELS; count++)
            {
                pixels[count].Brightness = bright;
            }
        }

        /// <summary>
        /// Set values for all pixels in one call.
        /// </summary>
        /// <param name="red">Pixel red value</param>
        /// <param name="green">Pixel green value</param>
        /// <param name="blue">Pixel blue value</param>
        /// <param name="bright">Pixel brightness</param>
        public void SetAllPixels(byte red, byte green, byte blue, decimal? bright = null)
        {
            for (int count = 1; count < NUM_PIXELS + 1; count++)
            {
                SetPixel(count, red, green, blue, bright);
            }
        }

        /// <summary>
        /// Set an individual pixel value
        /// </summary>
        /// <param name="pixelNum">The index of the pixel to set (between 1 and the pixel count - NOT zero based!)</param>
        /// <param name="red">Pixel red value</param>
        /// <param name="green">Pixel green value</param>
        /// <param name="blue">Pixel blue value</param>
        /// <param name="bright">pixel brightness</param>
        public void SetPixel(int pixelNum, byte red, byte green, byte blue, decimal? bright = null)
        {
            if (pixelNum < 1 || pixelNum > NUM_PIXELS)
            {
                throw new IndexOutOfRangeException("Invalid pixel number specified");
            }
            var pix = pixels[pixelNum - 1];
            pix.Red = red;
            pix.Green = green;
            pix.Blue = blue;
            pix.Brightness = bright.HasValue ? bright.Value : BRIGHTNESS;
        }

        /// <summary>
        /// Set values for all pixels in one call.
        /// </summary>
        /// <param name="color">Color struct value</param>
        /// <param name="bright">Pixel brightness</param>
        public void SetAllPixels(Color color, decimal? bright = null)
        {
            for (int count = 1; count < NUM_PIXELS + 1; count++)
            {
                SetPixel(count, color, bright);
            }
        }

        /// <summary>
        /// Set an individual pixel value
        /// </summary>
        /// <param name="pixelNum">The index of the pixel to set (between 1 and the pixel count - NOT zero based!)</param>
        /// <param name="color">Color struct value</param>
        /// <param name="bright">pixel brightness</param>
        public void SetPixel(int pixelNum, Color color, decimal? bright = null)
        {
            if (pixelNum < 0 || pixelNum > NUM_PIXELS)
            {
                throw new IndexOutOfRangeException("Invalid pixel number specified");
            }
            var pix = pixels[pixelNum];
            pix.Red = color.R;
            pix.Green = color.G;
            pix.Blue = color.B;
            pix.Brightness = bright.HasValue ? bright.Value : BRIGHTNESS;
        }

        /// <summary>
        /// Clear all pixels.
        /// </summary>
        public void Clear()
        {
            SetAllPixels(0, 0, 0, null);
        }
    }
}