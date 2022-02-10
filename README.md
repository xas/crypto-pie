# Crypto Pie

Crypto Pie is a dotnet project aiming to use dedicated hardware to display data from crypto charts API

## Requirements

The current development was tested with this hardware/software :

* Raspberry Pi Zero 2 W
* Blinkt! leds from [Pimoroni](https://shop.pimoroni.com/products/blinkt)
* Inky pHat E-Ink display (Black, white, and red color) from [Pimoroni](https://shop.pimoroni.com/products/inky-phat?variant=12549254217811)
* Mini PiTFT display from [adafruit](https://www.adafruit.com/product/4393)
* Ubuntu server 21.10 x64 version (installation guide available [here](https://ubuntu.com/blog/raspberry-pi-zero-2-w-with-ubuntu-server-support-is-here))
* dotnet 6.0 ARM64 version
* SkiSharp package 2.80.3
* CommandLinePArser package 2.8.0

For the case I used a simple Pibow, from [Pimoroni](https://shop.pimoroni.com/products/pibow-zero-2-w)

## Setup

To install the application, you need to compile the code from your computer. Do not try to build it from your Pi Zero 2, unless you need to take a nap during the compilation time...

Once you clone the repo, just go to the folder _src\Xas.CryptoPie.Worker_ and, from the command line, run the following command : `dotnet build -r linux-arm64 --self-contained`  

Then transfer all the files from _src\Xas.CryptoPie.Worker\bin\Debug\net6.0\linux-arm64_ to your Pi Zero 2

## Running the project

The application must run on a sudo mode to gain access to the GPIO/SPI interface  

Depending on your hardware, you need to set the program through arguments on the command line :

```
Required option 'b, board' is missing.

-b, --board    Required. Define the board to use. Available values are 'Blinkt', 'TFT', or 'EInk'

--help         Display this help screen.

--version      Display version information.
```

As an example, if you want to test the _Blinkt!_ product, the correct command line would be `sudo ./Xas.CryptoPie.Worker -b Blinkt`

The name of the board is not case-sensitive.

## Development

Code is not very generic, and more dedicated to the specific owned hardware. Some rework could surely be done.
