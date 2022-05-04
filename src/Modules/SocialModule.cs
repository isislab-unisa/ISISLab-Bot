namespace ISISLab.HelpDesk.Modules
{
  using System;
  using System.Threading.Tasks;

  using Discord;
  using Discord.Commands;
  using Fergun.Interactive;
  using Fergun.Interactive.Selection;
  using ISISLab.HelpDesk.Common;
  using ISISLab.HelpDesk.Logging;

  public class SocialModule : HelpDeskModule
  {
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public SocialModule(ILogger<SocialModule> logger, IServiceProvider serviceProvider)
    {
      _logger           = logger;
      _serviceProvider  = serviceProvider;
    }

    [Command("Informazioni", RunMode = RunMode.Async)]
    public async Task Informazioni()
    {
      var options = new ButtonOption<string>[]
      {
        new ButtonOption<string>("Informazioni sulla tesi", ButtonStyle.Primary, HelpType.Thesis),
        new ButtonOption<string>("Informazioni sul tirocinio", ButtonStyle.Primary, HelpType.Intership)
      };

      var builder = new EmbedBuilder()
        .WithTitle("Info su ISISLab")
        .WithDescription("ISISLab è il laboratorio diretto da Alberto Negro e Vittorio Scarano e in cui si svolgono attività di ricerca e di didattica. Se vuoi saperne di più visita il sito o partecipa ai nostri seminari")
        .WithUrl("https://www.isislab.it")
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Blue);

      var pageBuilder = new PageBuilder()
          .WithDescription("Se hai bisogno di informazioni aggiuntive riguardanti la tesi o il tirocinio, clicca su uno dei pulsanti qui sotto, oppure chiedimelo esplicitamente..")
          .WithColor(Color.Gold);

      var buttonSelection = new ButtonSelectionBuilder<string>()
        .WithOptions(options)
        .WithStringConverter(x => x.Option)
        .WithSelectionPage(pageBuilder)
        .AddUser(Context.User)
        .Build();

      await ReplyAsync(embed: builder.Build());

      var response = await InteractiveService.SendSelectionAsync(buttonSelection, Context.Channel);
      if (response.IsSuccess)
      {
        switch (response.Value.HelpType)
        {
          case HelpType.Thesis:
            await CommandService.ExecuteAsync(Context, "Thesis", _serviceProvider);
            break;

          case HelpType.Intership:
            await CommandService.ExecuteAsync(Context, "Intership", _serviceProvider);
            break;
        }

        await response.Message.DeleteAsync();
      }
    }

    [Command("Social", RunMode = RunMode.Async)]
    public async Task Social()
    {
      var builder = new EmbedBuilder()
        .WithTitle("Canali social del laboratorio")
        .WithDescription("Di seguito potrai visualizzare una lista contentente tutti i canali social associati al laboratorio")
        .AddField(new EmbedFieldBuilder()
          .WithName("Facebook")
          .WithValue("https://www.facebook.com/ISISLabUNISA/"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Instagram")
          .WithValue("https://www.instagram.com/isislab_unisa/"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Twitter")
          .WithValue("https://twitter.com/isislab"))
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Blue);

      await ReplyAsync(embed: builder.Build());
    }


    public enum HelpType
    {
      Thesis,
      Intership
    }

    public class ButtonSelectionBuilder<T> : BaseSelectionBuilder<ButtonSelection<T>, ButtonOption<T>, ButtonSelectionBuilder<T>>
    {
      public override InputType InputType => InputType.Buttons;
      public override ButtonSelection<T> Build() => new ButtonSelection<T>(this);
    }

    public class ButtonSelection<T> : BaseSelection<ButtonOption<T>>
    {
      public ButtonSelection(ButtonSelectionBuilder<T> builder)
        : base(builder)
      { }

      public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
      {
        builder ??= new ComponentBuilder();
        foreach (var option in Options)
        {
          var emote = EmoteConverter?.Invoke(option);
          string label = StringConverter?.Invoke(option);

          if (emote is null && label is null)
          {
            throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
          }

          var button = new ButtonBuilder()
            .WithCustomId(emote?.ToString() ?? label)
            .WithStyle(option.Style)
            .WithEmote(emote)
            .WithDisabled(disableAll);

          if (!(label is null))
            button.Label = label;

          builder.WithButton(button);
        }

        return builder;
      }
    }

    public struct ButtonOption<T>
    {
      public T Option { get; set; }
      public ButtonStyle Style { get; set; }
      public HelpType HelpType { get; set; }

      public ButtonOption(T option, ButtonStyle style, HelpType helpType)
      {
        Option    = option;
        Style     = style;
        HelpType  = helpType;
      }
    }
  }
}
