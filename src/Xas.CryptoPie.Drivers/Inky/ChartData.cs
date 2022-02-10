// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Drawing;

namespace Xas.CryptoPie.Drivers
{
    public class ChartData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string CurrencySymbol { get; set; }
        public string Symbol { get; set; }
        public Color Color { get; set; }
        public IEnumerable<double> Values { get; set; }
    }
}
