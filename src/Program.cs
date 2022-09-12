namespace ISISLab.HelpDesk
{
  using System;

  using Discord;
  using Discord.Commands;
  using Discord.WebSocket;
  using Fergun.Interactive;
  using ISISLab.HelpDesk.Configuration;
  using ISISLab.HelpDesk.Jobs;
  using ISISLab.HelpDesk.Logging;
  using ISISLab.HelpDesk.Services;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Quartz;
  using Serilog;

  using Console = Colorful.Console;

  internal static class Program
  {
    private static void Main()
    {
      int r = 200;
      int g = 72;
      int b = 149;

      Console.WriteLine();
      Console.WriteLine(@" -------------------------------------------------------------------------- ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@"  /$$   /$$           /$$           /$$$$$$$                      /$$       ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$  | $$          | $$          | $$__  $$                    | $$       ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$  | $$  /$$$$$$ | $$  /$$$$$$ | $$  \ $$  /$$$$$$   /$$$$$$$| $$   /$$ ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$$$$$$$ /$$__  $$| $$ /$$__  $$| $$  | $$ /$$__  $$ /$$_____/| $$  /$$/ ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$__  $$| $$$$$$$$| $$| $$  \ $$| $$  | $$| $$$$$$$$|  $$$$$$ | $$$$$$/  ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$  | $$| $$_____/| $$| $$  | $$| $$  | $$| $$_____/ \____  $$| $$_  $$  ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" | $$  | $$|  $$$$$$$| $$| $$$$$$$/| $$$$$$$/|  $$$$$$$ /$$$$$$$/| $$ \  $$ ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" |__/  |__/ \_______/|__/| $$____/ |_______/  \_______/|_______/ |__/  \__/ ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@"                         | $$                                               ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@" ------ [ISISLab] ------ | $$ ---- (Developed by Daniele De Falco) -------- ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine(@"                         |__/                                               ", System.Drawing.Color.FromArgb(r, g, b)); r -= 18; b -= 9;
      Console.WriteLine();

      var baseDirectory = Environment.GetEnvironmentVariable("HELPDESK_BASEDIR");
      if (string.IsNullOrEmpty(baseDirectory))
        baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

      var configuration = Startup.Initialize(baseDirectory, "config.hjson",
        x => x.GetSection(nameof(AppOptions.Logging)).Get<LoggerOption>());

      var appOptions = configuration.Get<AppOptions>();
      var hostBuilder = new HostBuilder();

      Quartz.Logging.LogProvider.IsDisabled = true;
      var discordClient = new DiscordSocketClient(new DiscordSocketConfig
      {
        LogLevel = LogSeverity.Info,

        MessageCacheSize    = 150,
        AlwaysDownloadUsers = false
      });

      var commandService = new CommandService(new CommandServiceConfig
      {
        LogLevel = LogSeverity.Info,

        IgnoreExtraArgs       = true,
        CaseSensitiveCommands = false
      });

      var interactiveConfig = new InteractiveConfig()
      {
        LogLevel        = LogSeverity.Info,
        DefaultTimeout  = TimeSpan.FromMinutes(10)
      };

      //Register all commands affected to lock service
      var interactionLock = new InteractionLockService()
        .Register("Prenotazione");

      hostBuilder
        .ConfigureServices((context, services) =>
        {
          services
            .Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true)
            .Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(1))
            .Configure<AppOptions>(context.Configuration)

            //Logging services
            .AddSingleton<ILoggerFactory, LoggerFactory>()
            .AddTransient<Logging.ILogger, Logger>()
            .AddTransient(typeof(ILogger<>), typeof(Logger<>))

            //Discord Services
            .AddSingleton(discordClient)
            .AddSingleton(commandService)
            .AddSingleton(interactiveConfig)
            .AddSingleton<InteractiveService>()

            //Services
            .AddSingleton<AIService>()
            .AddSingleton<EmailSenderService>()
            .AddSingleton<GoogleCalendarService>()
            .AddSingleton(interactionLock)

            //Hosted services
            .AddHostedServiceEx<DiscordBotService>()

            //Chrono services
            .AddQuartz(quartz =>
            {
              quartz.InterruptJobsOnShutdown = true;
              quartz.InterruptJobsOnShutdownWithWait = true;

              quartz.UseMicrosoftDependencyInjectionJobFactory();

              quartz.UseSimpleTypeLoader();
              quartz.UseInMemoryStore();
              quartz.UseDefaultThreadPool(pool =>
              {
                pool.MaxConcurrency = 10;
              });

              //Add jobs to schedule
              quartz.ScheduleJob<SeminarNotification>(conf => conf
                .WithIdentity("seminar_notification")
                .StartNow()
                .WithSimpleSchedule(x => x
                  .WithIntervalInHours(4)
                  .RepeatForever()
                )
              );
            })
            .AddQuartzHostedService(options =>
            {
              options.WaitForJobsToComplete = true;
              options.StartDelay = TimeSpan.FromSeconds(10);
            });
        })
        .ConfigureHostConfiguration(builder => builder.AddConfiguration(configuration))
        .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration))
        .UseConsoleLifetime();

      var host = hostBuilder.Build();
      host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
      {
        Log.Information("Press Ctrl + C to shutdown");
      });

      host.Run();
      host.Dispose();
    }
  }
}