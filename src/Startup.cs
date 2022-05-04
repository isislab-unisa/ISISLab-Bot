namespace ISISLab.HelpDesk
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  using ISISLab.HelpDesk.Configuration;
  using ISISLab.HelpDesk.Configuration.HJson;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json;
  using Serilog;
  using Serilog.Events;
  using Serilog.Formatting.Json;

  public static class Startup
  {
    public static IConfiguration Initialize(string baseDirectory, string configFile, Func<IConfiguration, LoggerOption> loggerOptionsFactory)
    {
      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        Converters = new List<JsonConverter>
        {
          //Add future json converters
        }
      };

      configFile = Path.Combine(baseDirectory, configFile);
      var configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddHjsonFile(configFile, false, true)
        .Build();

      var loggerOption = loggerOptionsFactory(configuration);
      initializeSerilog(baseDirectory, loggerOption);
      return configuration;
    }

    private static void initializeSerilog(string baseDirectory, LoggerOption loggerOption)
    {
      var loggerDirectory = Path.Combine(baseDirectory, loggerOption.Directory);
      loggerDirectory = loggerDirectory.Replace("$(BASE)", AppDomain.CurrentDomain.BaseDirectory);

      if (!Enum.TryParse<LogEventLevel>(loggerOption.Level, out var logLevel))
      {
        Console.Error.WriteLine($"Invalid log level {loggerOption.Level}. Valid values are {string.Join(",", Enum.GetNames(typeof(LogEventLevel)))}");
        Environment.Exit(1);
      }

      var jsonlog = Path.Combine(loggerDirectory, $"{loggerOption.Name}.log.json");
      var logfile = Path.Combine(loggerDirectory, $"{loggerOption.Name}.log");
      Log.Logger = new LoggerConfiguration()
          .WriteTo.File(
              new JsonFormatter(),
              jsonlog,
              rollingInterval: RollingInterval.Day,
              rollOnFileSizeLimit: true,
              fileSizeLimitBytes: 50 * 1024 * 1024 // 50 MB
          )
          .WriteTo.File(
              logfile,
              rollingInterval: RollingInterval.Day,
              rollOnFileSizeLimit: true,
              fileSizeLimitBytes: 50 * 1024 * 1024, // 50 MB
              outputTemplate:
              "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3} {SourceContext}] {Message:lj}{NewLine}    {Properties:l} {NewLine}{Exception}"
          )
          .WriteTo.Console(
              outputTemplate: "[{Level} {SourceContext}] {Message:lj}{NewLine}    {Properties:l} {NewLine}{Exception}"
          )
          .MinimumLevel.Is(logLevel)
          .CreateLogger().ForContext(Serilog.Core.Constants.SourceContextPropertyName, "HelpDesk");
    }
  }
}
