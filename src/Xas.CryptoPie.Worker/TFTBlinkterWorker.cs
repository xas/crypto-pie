// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Net.Http.Json;
using Xas.CryptoPie.Drivers;

namespace Xas.CryptoPie.Worker
{
    public class TFTWorker : BackgroundService
    {
        private readonly ILogger<TFTWorker> _logger;
        private readonly NumberFormatInfo _cultureInfo;
        private readonly HttpClient _client;
        private readonly string _apiCall;
        private readonly string _interval;
        private readonly int _sleepingTime;
        private readonly Screener _TFT;
        private IEnumerable<CurrencyConfiguration> _currencies;
        private Color Dump => Color.Red;
        private Color Pump => Color.Green;

        public TFTWorker(ILogger<TFTWorker> logger, [NotNull] Screener TFT, [NotNull] CurrenciesConfiguration configuration)
        {
            _logger = logger;
            _TFT = TFT;
            _cultureInfo = new CultureInfo("en-US", false).NumberFormat;
            _cultureInfo.NumberDecimalSeparator = ".";

            _client = new HttpClient();

            _sleepingTime = configuration.Timer;
            _currencies = configuration.Currencies;
            _apiCall = configuration.ApiUrl;
            _interval = configuration.Interval;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _TFT.Dispose();
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                UpdateData();
                await Task.Delay(_sleepingTime, stoppingToken);
            }
        }

        public async void UpdateData()
        {
            try
            {
                List<TftData> data = new List<TftData>();
                foreach (CurrencyConfiguration currency in _currencies)
                {
                    object[][]? fullValues = await _client.GetFromJsonAsync<object[][]>(string.Format(_apiCall, currency.Symbol, _interval));
                    if (fullValues == null)
                    {
                        return;
                    }
                    if (fullValues.GetLength(0) < 2)
                    {
                        return;
                    }
                    // close = index 4
                    if (!double.TryParse(fullValues[0][4].ToString(), NumberStyles.Float, _cultureInfo, out double previous) ||
                        !double.TryParse(fullValues[1][4].ToString(), NumberStyles.Float, _cultureInfo, out double current))
                    {
                        return;
                    }
                    Color color = Color.White;
                    if (current < previous)
                    {
                        color = Dump;
                    }
                    else
                    {
                        color = Pump;
                    }
                    _logger.LogInformation($"Color: {color}");
                    data.Add(new TftData
                    {
                        Name = currency.Name,
                        Value = current.ToString(),
                        CurrencySymbol = currency.CurrencySymbol,
                        Color = color
                    });
                }
                _TFT.Display(data);
            }
            catch { }
        }
    }
}