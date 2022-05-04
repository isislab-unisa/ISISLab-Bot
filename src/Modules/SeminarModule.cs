namespace ISISLab.HelpDesk.Modules
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  using Discord;
  using Discord.Commands;
  using Fergun.Interactive;
  using Fergun.Interactive.Selection;
  using ISISLab.HelpDesk.Common;
  using ISISLab.HelpDesk.Logging;
  using ISISLab.HelpDesk.Models;
  using ISISLab.HelpDesk.Services;

  public class SeminarModule : HelpDeskModule
  {
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailSenderService _emailSenderService;

    public SeminarModule(ILogger<SocialModule> logger, IServiceProvider serviceProvider, EmailSenderService emailSenderService)
    {
      _logger             = logger;
      _serviceProvider    = serviceProvider;
      _emailSenderService = emailSenderService;
    }

    [Command("Seminari", RunMode = RunMode.Async)]
    public async Task Seminari()
      => await CommandService.ExecuteAsync(context: Context, "Calendario", _serviceProvider);

    [Command("Prenotazione", RunMode = RunMode.Async)]
    public async Task Prenotazione()
    {
      var embedInfo = new EmbedBuilder()
        .WithTitle("Info sulla prenotazione seminari")
        .WithDescription("L'interfaccia seguente verrà popolata con i tuoi dati durante la prenotazione guidata del tuo seminario. Alla fine potrai scegliere se inviare il tutto oppure prenotare nuovamente il tuo seminario.")
        .WithFields(new EmbedFieldBuilder()
          .WithName("Campi richiesti")
          .WithValue(" - Nome e Cognome\n - Titolo del seminario\n - Descrizione del seminario\n - Immagine descrittiva\n - Tag partecipanti\n - Lingua\n"))
        .WithTimestamp(DateTime.Now)
        .WithColor(Color.Gold);

      var seminarMessageBuilder = new EmbedBuilder()
        .WithTitle("Prenotazione Seminario")
        .WithFields(new EmbedFieldBuilder()
          .WithName("Nome e Cognome")
          .WithValue("-"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Titolo del seminario")
          .WithValue("-"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Descrizione del seminario")
          .WithValue("-"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Immagine descrittiva")
          .WithValue("-"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Tag partecipanti")
          .WithValue("-"))
        .WithFields(new EmbedFieldBuilder()
          .WithName("Lingua")
          .WithValue("-"))
        .WithTimestamp(DateTimeOffset.Now)
        .WithColor(Color.Blue);
      
      var seminarReservation = new SeminarReservationModel();

      var infoMessage = await ReplyAsync(embed: embedInfo.Build());
      var seminarMessage = await ReplyAsync(embed: seminarMessageBuilder.Build());

      await askForNameAndSurname();
      await askForTitle();
      await askForSummary();
      await askForPeoples();
      await askForLanguage();
      await askForAnImage();

      var options = new string[] { "Si", "No" };

      var selection = new SelectionBuilder<string>()
        .AddUser(Context.User)
        .WithOptions(options)
        .WithSelectionPage(new PageBuilder()
          .WithDescription("La procedura guidata di prenotazione del tuo seminario è stata completata. Confermi la prenotazione del tuo seminario?"))
        .WithInputType(InputType.SelectMenus)
        .WithDeletion(DeletionOptions.None)
        .Build();

      var acceptSeminar = await InteractiveService.SendSelectionAsync(selection, Context.Channel);
      if (!acceptSeminar.IsSuccess || acceptSeminar.IsTimeout)
      {
        await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
        return;
      }

      if (acceptSeminar.Value == "Si")
      {
        sendEmail();
        await ReplyAsync("Grazie per aver inviato la tua richiesta.");
      }
      else
      {
        await ReplyAsync("Ci dispiace che qualcosa sia andato storto. Puoi riprovare se vuoi.");
      }

      await acceptSeminar.Message.DeleteAsync();

      async Task askForNameAndSurname()
      {
        var nsAnswer = await ReplyAsync("Inserisci il tuo nome e il tuo cognome");
        var nameAndSurname = await InteractiveService.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id);

        if (!nameAndSurname.IsSuccess || nameAndSurname.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Nome e Cognome")
          .WithValue(nameAndSurname.Value.Content);

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
        await nsAnswer.DeleteAsync();

        seminarReservation.Author = nameAndSurname.Value.Content;
      }

      async Task askForTitle()
      {
        var titleAnswer = await ReplyAsync("Inserisci il titolo del tuo seminario");
        var seminarTitle = await InteractiveService.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id);

        if (!seminarTitle.IsSuccess || seminarTitle.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Titolo del seminario")
          .WithValue(seminarTitle.Value.Content);

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
        await titleAnswer.DeleteAsync();

        seminarReservation.Title = seminarTitle.Value.Content;
      }

      async Task askForSummary()
      {
        var summaryAnswer = await ReplyAsync("Inserisci la descrizione del tuo seminario");
        var seminarSummary = await InteractiveService.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id);

        if (!seminarSummary.IsSuccess || seminarSummary.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        var fieldSummary = seminarSummary.Value.Content.Length > 1023
          ? $"{seminarSummary.Value.Content.Substring(0, 512)}..."
          : $"{seminarSummary.Value.Content}";

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Descrizione del seminario")
          .WithValue(fieldSummary);

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
        await summaryAnswer.DeleteAsync();

        seminarReservation.Description = seminarSummary.Value.Content;
      }

      async Task askForPeoples()
      {
        var peopleTagAnswer = await ReplyAsync("Inserisci i tag dei partecipanti divisi da una virgola. es [Nome Cognome, Nome Cognome]");
        var seminarPeopleTags = await InteractiveService.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id);

        if (!seminarPeopleTags.IsSuccess || seminarPeopleTags.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Tag partecipanti")
          .WithValue(seminarPeopleTags.Value.Content);

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
        await peopleTagAnswer.DeleteAsync();

        seminarReservation.Speakers = seminarPeopleTags.Value.Content;
      }

      async Task askForLanguage()
      {
        var options = new FlagItem[]
        {
          new FlagItem("Italiano", new Emoji("🇮🇹")),
          new FlagItem("English", new Emoji("🇬🇧"))
        };

        var pageBuilder = new PageBuilder()
          .WithDescription("Seleziona la tua lingua con la quale si terrà il tuo seminario");

        var selection = new SelectionBuilder<FlagItem>()
          .AddUser(Context.User)
          .WithOptions(options)
          .WithSelectionPage(pageBuilder)
          .WithInputType(InputType.SelectMenus)
          .WithDeletion(DeletionOptions.None)
          .WithStringConverter(x => x.Name)
          .WithEmoteConverter(x => x.Emote)
          .Build();

        var seminarLang = await InteractiveService.SendSelectionAsync(selection, Context.Channel);

        if (!seminarLang.IsSuccess || seminarLang.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Lingua")
          .WithValue($"{seminarLang.Value.Emote.Name} {seminarLang.Value.Name}");

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
        await seminarLang.Message.DeleteAsync();

        seminarReservation.Language = $"{seminarLang.Value.Emote.Name} {seminarLang.Value.Name}";
      }

      async Task askForAnImage()
      {
        var options = new string[] { "Si", "No" };

        var selection = new SelectionBuilder<string>()
          .AddUser(Context.User)
          .WithOptions(options)
          .WithSelectionPage(new PageBuilder()
            .WithDescription("Hai già un'immagine da usare per il tuo seminario?"))
          .WithInputType(InputType.SelectMenus)
          .WithDeletion(DeletionOptions.None)
          .Build();

        var seminarHaveImage = await InteractiveService.SendSelectionAsync(selection, Context.Channel);

        if (!seminarHaveImage.IsSuccess || seminarHaveImage.IsTimeout)
        {
          await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
          return;
        }

        await seminarHaveImage.Message.DeleteAsync();
        switch (seminarHaveImage.Value)
        {
          case "Si":
            {
              var uploadImageAsk = await ReplyAsync("Perfetto, carica l'immagine in questa chat per utilizzarla nel tuo seminario");
              var uploadImageResult = await InteractiveService.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id);

              if (!uploadImageResult.IsSuccess || uploadImageResult.IsTimeout)
              {
                await ReplyAsync("Non sono riuscito a prenotare il tuo seminario, riprova..");
                return;
              }

              var uploadedImage = uploadImageResult.Value.Attachments.FirstOrDefault();
              await uploadImageAsk.DeleteAsync();

              seminarReservation.ImageUrl = uploadedImage.Url;
              break;
            }

          case "No":
            {
              seminarReservation.ImageUrl = string.Empty;
              break;
            }
        }

        seminarMessageBuilder.Fields
          .FirstOrDefault(x => x.Name == "Immagine descrittiva")
          .WithValue("[Selezione dell'immagine completata]");

        await seminarMessage.ModifyAsync(x => x.Embed = seminarMessageBuilder.Build());
      }

      void sendEmail()
      {
        var description = new DescriptionParser()
          .WithName(seminarReservation.Author)
          .WithTitle(seminarReservation.Title)
          .WithSummary(seminarReservation.Description)
          .WithSpeakers(seminarReservation.Speakers)
          .WithLanguage(seminarReservation.Language)
          .WithImageUrl(seminarReservation.ImageUrl)
          .ToString();

        _emailSenderService.SendEmail($"Seminar Reservation: {seminarReservation.Title}", description);
      }
    }

    private struct FlagItem
    {
      public string Name { get; private set; }
      public IEmote Emote { get; private set; }

      public FlagItem(string name, IEmote emote)
      {
        Name = name;
        Emote = emote;
      }
    }
  }
}
