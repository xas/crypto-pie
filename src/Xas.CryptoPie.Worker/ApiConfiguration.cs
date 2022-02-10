// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

namespace Xas.CryptoPie.Worker
{
    public class ApiConfiguration
    {
        public BlinkterConfiguration BlinkterConfiguration { get; set; }
        public InkyConfiguration InkyConfiguration { get; set; }
        public CurrenciesConfiguration TftConfiguration { get; set; }
    }

    public abstract class AConfiguration
    {
        public int Timer { get; set; }
        public string Interval { get; set; }
        public string ApiUrl { get; set; }
    }

    public class BlinkterConfiguration : AConfiguration
    {
        public string Symbol { get; set; }
    }

    public class InkyConfiguration : AConfiguration
    {
        public string Symbol { get; set; }
        public string CurrencySymbol { get; set; }
        public string Name { get; set; }
    }

    public class CurrenciesConfiguration : AConfiguration
    {
        public List<CurrencyConfiguration> Currencies { get; set; }
    }

    public class CurrencyConfiguration
    {
        public string Name { get; set; }
        public string CurrencySymbol { get; set; }
        public string Symbol { get; set; }
    }
}
