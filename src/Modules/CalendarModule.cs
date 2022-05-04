namespace ISISLab.HelpDesk.Modules
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  using Discord;
  using Discord.Commands;
  using Fergun.Interactive;
  using Fergun.Interactive.Pagination;
  using ISISLab.HelpDesk.Common;
  using ISISLab.HelpDesk.Logging;
  using ISISLab.HelpDesk.Services;

  public class CalendarModule : HelpDeskModule
  {
    private readonly ILogger _logger;
    private readonly GoogleCalendarService _calendarService;

    public CalendarModule(ILogger<CalendarModule> logger, GoogleCalendarService calendarService)
    {
      _logger             = logger;
      _calendarService    = calendarService;
    }

    [Command("Calendario", RunMode = RunMode.Async)]
    public async Task ShowCalendar()
    {
      var calendarEvents = await _calendarService.GetEventsAsync();
      if (!calendarEvents.Success)
      {
        await ReplyAsync("Non è stato possibile recuperare le informazioni sui seminari. Contattare l'amministratore");
        return;
      }

      var events = calendarEvents.Events.ToList();
      if (events.Count <= 0)
      {
        await ReplyAsync("Non sembrano esserci seminari nel prossimo futuro");
        return;
      }

      var paginator = new LazyPaginatorBuilder()
        .AddUser(Context.User)
        .WithPageFactory(generatePage)
        .WithMaxPageIndex(events.Count - 1)
        .AddOption(new Emoji("⏪"), PaginatorAction.SkipToStart)
        .AddOption(new Emoji("◀"), PaginatorAction.Backward)
        .AddOption(new Emoji("▶"), PaginatorAction.Forward)
        .AddOption(new Emoji("⏩"), PaginatorAction.SkipToEnd)
        .AddOption(new Emoji("🛑"), PaginatorAction.Exit)
        .WithCacheLoadedPages(false)
        .WithActionOnCancellation(ActionOnStop.DeleteMessage)
        .WithActionOnTimeout(ActionOnStop.DeleteInput)
        .WithFooter(PaginatorFooter.None)
        .Build();

      var embedInfo = new EmbedBuilder()
        .WithTitle("Info sul calendario")
        .WithDescription("Il calendario dei seminari può essere utilizzato per visionare tutti i prossimi eventi del laboratorio. Puoi utilizzare i comandi predisposti per navigare attraverso le pagine del calendario e, se vuoi, cancellare il calendario tramite l'apposito pulsante rosso.")
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Gold);

      await ReplyAsync(embed: embedInfo.Build());
      await InteractiveService.SendPaginatorAsync(paginator: paginator, channel: Context.Channel);

      PageBuilder generatePage(int index)
      {
        var parsedDescription =
          DescriptionParser.Parse(events[index].Description);

        var fieldSummary = parsedDescription.Summary.Length > 1023
          ? $"{parsedDescription.Summary.Substring(0, 512)}..."
          : $"{parsedDescription.Summary}";

        return new PageBuilder()
              .WithTitle(events[index].Summary)
              .WithFields(new EmbedFieldBuilder()
                .WithName("Inizio")
                .WithIsInline(true)
                .WithValue(events[index].StartDate?.ToString(" dd MMMM yyyy - HH:mm") ?? "Tutto il giorno"))
              .WithFields(new EmbedFieldBuilder()
                .WithName("Luogo")
                .WithValue(events[index].Location))
              .WithFields(new EmbedFieldBuilder()
                .WithName("Descrizione")
                .WithValue(fieldSummary))
              .WithTimestamp(DateTimeOffset.Now)
              .WithFooter($"Pagina {index + 1} di {events.Count}")
              .WithColor(Color.Blue);
      }
    }
  }
}
