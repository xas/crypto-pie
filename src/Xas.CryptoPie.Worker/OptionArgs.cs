// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using CommandLine;

namespace Xas.CryptoPie.Worker
{
    public class OptionArgs
    {
        [Option('b', "board", HelpText = "Define the board to use. Available values are 'Blinkt', 'TFT', or 'EInk'", Required = true)]
        public string BoardType { get; set; }
    }
}
