namespace ISISLab.HelpDesk.Jobs
{
  using System;
  using System.Collections.Concurrent;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  using Discord.WebSocket;
  using ISISLab.HelpDesk.Common;
  using ISISLab.HelpDesk.Logging;
  using ISISLab.HelpDesk.Models;
  using ISISLab.HelpDesk.Services;
  using Microsoft.Extensions.Options;
  using Quartz;

  public class SeminarNotification : IJob
  {
    private readonly ILogger _logger;
    private readonly AppOptions _appOptions;
    private readonly DiscordSocketClient _discordClient;
    private readonly GoogleCalendarService _calendarService;

    // Dictionary should be static because Jobs are executed as Transient service (state is not saved)
    // This dictionary only store events at runtime so, if application restart, all events are sent again
    private static readonly ConcurrentDictionary<DateTime, CalendarEventModel> _scheduledEvents =
      new ConcurrentDictionary<DateTime, CalendarEventModel>();

    public SeminarNotification(ILogger<SeminarNotification> logger, IOptions<AppOptions> options,
      DiscordSocketClient discordClient, GoogleCalendarService calendarService)
    {
      _logger           = logger;
      _appOptions       = options.Value;
      _discordClient    = discordClient;
      _calendarService  = calendarService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      //ToDo: Change this with correct ulong parsing. HJson parse issue #10
      var serverParsed = ulong.TryParse(_appOptions.Seminars.ServerId.Replace("-", ""), out var serverid);
      var channelParsed = ulong.TryParse(_appOptions.Seminars.ChannelId.Replace("-", ""), out var channelId);

      if (!serverParsed || !channelParsed)
      {
        _logger.Error("Unable to find ServerId or ChannelId");
        return;
      }

      var isisLabGuild = _discordClient.GetGuild(serverid);
      var seminarChannel = isisLabGuild?.GetTextChannel(channelId);

      if (isisLabGuild == null || seminarChannel == null)
      {
        _logger.Error("Unable to find ISISLab Server or Seminar Channel");
        return;
      }

      var calendarEvents = (await _calendarService.GetEventsAsync())?.Events;
      if (calendarEvents == null) return;

      var nextEvents = calendarEvents.Where(x => x.StartDate.Value - DateTime.Now <= TimeSpan.FromDays(_appOptions.Seminars.NotifyWithin));
      foreach (var @event in nextEvents)
      {
        if (_scheduledEvents.ContainsKey(@event.StartDate.Value)) continue;
        if (_scheduledEvents.TryAdd(@event.StartDate.Value, @event))
        {
          await sendEvent(@event);
        }
      }

      var eventsToRemove = calendarEvents.Where(x => x.StartDate.Value < DateTime.Now);
      foreach (var @event in eventsToRemove)
      {
        _scheduledEvents.TryRemove(@event.StartDate.Value, out var _);
      }

      async Task sendEvent(CalendarEventModel @event)
      {
        var dateTimeFormat = new CultureInfo("it-IT");

        var parsedTime = parseTime(@event.StartDate.Value);
        var parsedDate = @event.StartDate.Value.ToString("ddd, dd-MM-yyyy", dateTimeFormat);

        var parsedDescription =
          DescriptionParser.Parse(@event.Description);

        var stringBuilder = new StringBuilder()
          .AppendLine( ":isislab:   :loudspeaker:   **Avviso Seminari**   :bell:"           )
          .AppendLine(                                                                      )
          .AppendLine( ":information_source:  **INFO **"                                    )
          .AppendLine(                                                                      )
          .AppendLine( $"- **Title**: {parsedDescription.Title}"                            )
          .AppendLine( $"- **Abstract**: {parsedDescription.Summary}"                       )
          .AppendLine( $"- **Speaker**: {parsedDescription.Speakers}"                       )
          .AppendLine( $"- **Language**: {parsedDescription.Language}"                      )
          .AppendLine(                                                                      )
          .AppendLine( $"- :calendar_spiral:   **{parsedDate}**"                            )
          .AppendLine( $"- :alarm_clock:   {parsedTime}  **(CEST)**"                        )
          .AppendLine( $"- :speech_left:  Canale vocale :speaker: :loudspeaker: seminars "  )
          .AppendLine( $"- :crossed_swords:  Discussioni :bar_chart:-seminars "             )
          .AppendLine(                                                                      )
          .AppendLine( "https://discord.gg/6NgJAdry"                                        )
          .AppendLine(                                                                      )
          .AppendLine( "@everyone"                                                          );

        await seminarChannel.SendMessageAsync(stringBuilder.ToString());

        // This function need to be improved
        string parseTime(DateTime dateTime)
        {
          var stringBuilder = new StringBuilder();
          var splittedTime = dateTime.ToString("HH:mm").Split(':');

          foreach (var ch in splittedTime[0]) stringBuilder.Append($"{parseNumber(int.Parse($"{ch}"))} ");
          stringBuilder.Append(": ");
          foreach (var ch in splittedTime[1]) stringBuilder.Append($"{parseNumber(int.Parse($"{ch}"))} ");

          return stringBuilder.ToString();
        }

        string parseNumber(int number)
        {
          string[] numArray = new string[] { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:" };
          return number <= 9 ? numArray[number] : string.Empty;
        }
      }
    }
  }
}
