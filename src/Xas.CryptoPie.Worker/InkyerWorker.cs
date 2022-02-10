// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Net.Http.Json;
using Xas.CryptoPie.Drivers;

namespace Xas.CryptoPie.Worker
{
    public class InkyerWorker : BackgroundService
    {
        private readonly ILogger<InkyerWorker> _logger;
        private readonly NumberFormatInfo _cultureInfo;
        private readonly HttpClient _client;
        private readonly string _apiCall;
        private readonly string _interval;
        private readonly string _symbol;
        private readonly string _currencySymbol;
        private readonly string _name;
        private readonly int _sleepingTime;
        private readonly Inkyer _inkyer;
        private Color Dump => Color.Red;
        private Color Pump => Color.Green;

        public InkyerWorker(ILogger<InkyerWorker> logger, [NotNull] Inkyer inkyer, [NotNull] InkyConfiguration configuration)
        {
            _logger = logger;
            _inkyer = inkyer;
            _cultureInfo = new CultureInfo("en-US", false).NumberFormat;
            _cultureInfo.NumberDecimalSeparator = ".";

            _client = new HttpClient();

            _sleepingTime = configuration.Timer;
            _apiCall = configuration.ApiUrl;
            _interval = configuration.Interval;
            _symbol = configuration.Symbol;
            _currencySymbol = configuration.CurrencySymbol;
            _name = configuration.Name;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _inkyer.Dispose();
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
                ChartData data = new ChartData();
                List<double> values = new List<double>();
                object[][]? fullValues = await _client.GetFromJsonAsync<object[][]>(string.Format(_apiCall, _symbol, _interval));
                if (fullValues == null)
                {
                    return;
                }
                if (fullValues.Length < 2)
                {
                    return;
                }
                // close = index 4
                double previous = 0;
                Color color = Pump;
                for (int i = 0; i < fullValues.Length; i++)
                {
                    if (double.TryParse(fullValues[i][4].ToString(), out double value))
                    {
                        values.Add(value);
                        if (value < previous)
                        {
                            color = Dump;
                        }
                        else
                        {
                            color = Pump;
                        }
                        previous = value;
                    }
                }
                data = new ChartData
                {
                    Name = _name,
                    Values = values,
                    Value = values.Last(),
                    Symbol = _symbol,
                    CurrencySymbol = _currencySymbol,
                    Color = color
                };
                _logger.LogDebug($"New Inky data for {data.Symbol}. Last value is {data.Value} with {values.Count} available values");
                _inkyer.Display(data);
            }
            catch { }
        }
    }
}