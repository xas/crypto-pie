// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Net.Http.Json;

namespace Xas.CryptoPie.Worker
{
    public class BlinkterWorker : BackgroundService
    {
        private readonly ILogger<BlinkterWorker> _logger;
        private readonly NumberFormatInfo _cultureInfo;
        private readonly HttpClient _client;
        private readonly string _apiCall;
        private readonly int _sleepingTime;
        private readonly Blinkter _blinkter;
        private Color Dump => Color.Red;
        private Color Pump => Color.Green;

        public BlinkterWorker(ILogger<BlinkterWorker> logger, [NotNull] Blinkter blinkter, [NotNull] BlinkterConfiguration configuration)
        {
            _logger = logger;
            _blinkter = blinkter;
            _cultureInfo = new CultureInfo("en-US", false).NumberFormat;
            _cultureInfo.NumberDecimalSeparator = ".";

            _client = new HttpClient();

            _sleepingTime = configuration.Timer;
            _apiCall = string.Format(configuration.ApiUrl, configuration.Symbol, configuration.Interval);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _blinkter.Off();
            _blinkter.Dispose();
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
                object[][]? fullValues = await _client.GetFromJsonAsync<object[][]>(_apiCall);
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
                double ratio = current / previous;
                _logger.LogInformation($"Current ratio: {ratio:#0.00000}");
                Color color = Color.White;
                if (ratio < 1.0)
                {
                    ratio = 1 - ratio;
                    color = Dump;
                }
                else
                {
                    ratio = ratio - 1;
                    color = Pump;
                }
                ratio *= 100;
                ratio /= 0.5;
                byte totalLeds = (byte)Math.Min(255, ratio + 1);
                _logger.LogInformation($"Color: {color}");
                _blinkter.Display(totalLeds, color);
            }
            catch { }
        }
    }
}