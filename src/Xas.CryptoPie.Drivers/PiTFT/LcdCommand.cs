// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

namespace Xas.CryptoPie.Drivers
{
    internal enum LcdCommand : byte
    {
        CASET = 0x2A,
        RASET = 0x2B,
        RAMWR = 0x2C,
        RADRD = 0x2E
    };
}
