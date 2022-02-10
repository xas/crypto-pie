// SPDX-License-Identifier: MIT
// Original source @ https://github.com/xas/crypto-pie

using CommandLine;
using Microsoft.Extensions.Options;
using Xas.CryptoPie.Worker;

BoardType board = BoardType.None;

Parser.Default.ParseArguments<OptionArgs>(args)
  .WithParsed(options =>
  {
      switch (options.BoardType.ToLower())
      {
          case "blinkt":
              board = BoardType.Blinkt;
              break;
          case "tft":
              board = BoardType.PiTFT;
              break;
          case "eink":
              board = BoardType.Inky;
              break;
      }
  });

if (board != BoardType.None)
{
    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            IConfiguration Configuration = builder.Build();
            var apiConfiguration = Configuration.GetSection("apiConfiguration");
            services.Configure<ApiConfiguration>(apiConfiguration);
            switch (board)
            {
                case BoardType.Inky:
                    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiConfiguration>>().Value.InkyConfiguration);
                    services.AddSingleton<Inkyer>();
                    services.AddHostedService<InkyerWorker>();
                    break;
                case BoardType.PiTFT:
                    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiConfiguration>>().Value.TftConfiguration);
                    services.AddSingleton<Screener>();
                    services.AddHostedService<TFTWorker>();
                    break;
                case BoardType.Blinkt:
                    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiConfiguration>>().Value.BlinkterConfiguration);
                    services.AddSingleton<Blinkter>();
                    services.AddHostedService<BlinkterWorker>();
                    break;
            }
        })
        .Build();

    await host.RunAsync();
}

public enum BoardType
{
    None,
    Blinkt,
    PiTFT,
    Inky
}