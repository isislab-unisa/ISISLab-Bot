namespace ISISLab.HelpDesk.Services
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;

  using Discord;
  using Discord.Commands;
  using Discord.WebSocket;
  using ISISLab.HelpDesk.Logging;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Options;

  public class DiscordBotService : IHostedService
  {
    private readonly ILogger _logger;
    private readonly AppOptions _appOptions;

    private readonly AIService _aiService;
    private readonly IServiceProvider _serviceProvider;
    private readonly InteractionLockService _lockService;

    private readonly DiscordSocketClient _discordClient;
    private readonly CommandService _commandService;

    public DiscordBotService(IServiceProvider serviceProvider, DiscordSocketClient discordClient, CommandService commandService, 
      ILogger logger, IOptions<AppOptions> options, AIService aiService, InteractionLockService lockService)
    {
      _logger             = logger;
      _aiService          = aiService;
      _lockService        = lockService;
      _appOptions         = options.Value;
      _serviceProvider    = serviceProvider;
      _discordClient      = discordClient;
      _commandService     = commandService;

      _discordClient.Log += handleLog;
      _commandService.Log += handleLog;

      _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
      _discordClient.MessageReceived += handleMessageRecv;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await _discordClient.LoginAsync(TokenType.Bot, _appOptions.BotToken);
      await _discordClient.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      _discordClient.Log -= handleLog;
      _commandService.Log -= handleLog;

      _discordClient.MessageReceived -= handleMessageRecv;
      await _discordClient.StopAsync();
    }

    private async Task handleMessageRecv(SocketMessage socketMessage)
    {
      if (!(socketMessage is SocketUserMessage { Source: MessageSource.User } message)) return;
      if (socketMessage.Channel.GetType() == typeof(SocketDMChannel))
      {
        var context = new SocketCommandContext(_discordClient, message);
        var prediction = await _aiService.PredictAsync(message.Content);

        if (!prediction.Success)
        {
          _logger.Error("Unable to interact with prediction service.");
          return;
        }

        if (!_lockService.IsUserInInteractionLock(message.Author))
        {
          //await socketMessage.Channel.SendMessageAsync(prediction.Intent);
          var commandExecutionResult = await _commandService.ExecuteAsync(context, prediction.Intent, _serviceProvider);
          if (!commandExecutionResult.IsSuccess)
          {
            await message.ReplyAsync("Mi spiace, non credo di aver capito cosa intendessi.");
            await _commandService.ExecuteAsync(context, "Aiuto", _serviceProvider);
          }
        }
      }
      else
      {
        //Manage guild messages (command etc)
        if (socketMessage.MentionedUsers.FirstOrDefault(x => x.Id == _discordClient.CurrentUser.Id) != null)
        {
          var directChannel = await socketMessage.Author.CreateDMChannelAsync();
          var embedBuilder = new EmbedBuilder()
            .WithTitle("ISISLab HelpDesk")
            .WithDescription("Ciao, sono l'assistente virtuale del laboratorio. Onde evitare un eccessivo spam, chiedimi in privato tutto ciò che vuoi, io ti aiuterò nel miglior modo possibile.")
            .WithTimestamp(DateTimeOffset.Now)
            .WithColor(Color.Blue);

          await directChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
      }
    }

    private Task handleLog(LogMessage message)
    {
      switch (message.Severity)
      {
        case LogSeverity.Critical:
        case LogSeverity.Error:
          {
            _logger.Error(message.Exception, "{source}: {message}", message.Source, message.Message);
            break;
          }

        /*  WARNING DISABLED FOR INCONSISTENT LOGGING INFO
        case LogSeverity.Warning:
          {
            _logger.Warning(message.Exception, "{source}: {message}", message.Source, message.Message);
            break;
          }
        */

        case LogSeverity.Info:
          {
            _logger.Information(message.Exception, "{source}: {message}", message.Source, message.Message);
            break;
          }

        case LogSeverity.Verbose:
          {
            _logger.Verbose(message.Exception, "{source}: {message}", message.Source, message.Message);
            break;
          }

        case LogSeverity.Debug:
          {
            _logger.Debug(message.Exception, "{source}: {message}", message.Source, message.Message);
            break;
          }
      }
      return Task.CompletedTask;
    }
  }
}
