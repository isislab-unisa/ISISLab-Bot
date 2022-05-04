namespace ISISLab.HelpDesk.Modules
{
  using System;
  using System.Threading.Tasks;

  using Discord;
  using Discord.Commands;
  using ISISLab.HelpDesk.Common;
  using ISISLab.HelpDesk.Logging;

  public class HelpModule : HelpDeskModule
  {
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public HelpModule(ILogger<HelpModule> logger, IServiceProvider serviceProvider)
    {
      _logger           = logger;
      _serviceProvider  = serviceProvider;
    }

    [Command("Utilities.Escalate", RunMode = RunMode.Async)]
    public async Task Escalate()
      => await CommandService.ExecuteAsync(Context, "Aiuto", _serviceProvider);

    [Command("Utilities.Help", RunMode = RunMode.Async)]
    public async Task Help()
      => await CommandService.ExecuteAsync(Context, "Aiuto", _serviceProvider);

    [Command("Aiuto", RunMode = RunMode.Async)]
    public async Task Aiuto()
    {
      var embedBuilder = new EmbedBuilder()
        .WithTitle("ISISLab HelpDesk")
        .WithDescription("Qui ti sarà possibile visionare tutto ciò che puoi fare interagendo con me. Se in futuro avrai comunque bisogno di controllare in cosa posso esserti utile, chiedimelo esplicitamente. In futuro, potrebbero essere aggiunte nuove funzionalità, resta aggiornato.")
        .WithFields(new EmbedFieldBuilder()
          .WithName("Info sul laboratorio")
          .WithValue(" - Informazioni generiche sul laboratorio\n - Canali social del laboratorio\n - Informazioni sul tirocinio e sulla tesi"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Info sui seminari")
          .WithValue(" - Prenotazione di un seminario\n - Calendario dei prossimi seminari"))
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.DarkRed);

      await ReplyAsync(embed: embedBuilder.Build());
    }

    [Command("Thesis", RunMode = RunMode.Async)]
    public async Task Thesis()
    {
      var embedBuilder = new EmbedBuilder()
        .WithTitle("Informazioni sulla tesi")
        .WithDescription("Cliccando sul link di seguito, potrai navigare sulla pagina ufficiale del laboratorio e avere maggiori informazioni sulla tesi.")
        .WithFields(new EmbedFieldBuilder()
          .WithName("ISISLab - Tesi")
          .WithValue("https://www.isislab.it/informazioni-utili/"))
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Blue);

      await ReplyAsync(embed: embedBuilder.Build());
    }

    [Command("Intership", RunMode = RunMode.Async)]
    public async Task Intership()
    {
      var embedBuilder = new EmbedBuilder()
        .WithTitle("Informazioni sul tirocinio")
        .WithDescription("Cliccando sul link di seguito, potrai navigare sulla pagina ufficiale del laboratorio e avere maggiori informazioni sul tirocinio.")
        .WithFields(new EmbedFieldBuilder()
          .WithName("ISISLab - Tirocinio")
          .WithValue("https://www.isislab.it/procedure-di-tirocinio/"))
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Blue);

      await ReplyAsync(embed: embedBuilder.Build());
    }
  }
}
