namespace ISISLab.HelpDesk.Common
{
  using System.Collections.Generic;
  using System.Linq;

  using Discord;
  using Discord.Commands;
  using Discord.WebSocket;
  using Fergun.Interactive;
  using Fergun.Interactive.Selection;
  using ISISLab.HelpDesk.Services;

  public abstract class HelpDeskModule : ModuleBase
  {
    public CommandService CommandService { get; set; }
    public InteractiveService InteractiveService { get; set; }
    public InteractionLockService LockService { get; set; }

    protected override void BeforeExecute(CommandInfo command)
    {
      if (Context.User is SocketUser user)
        LockService.AddUserToInteractionLock(user, command.Name);
    }

    protected override void AfterExecute(CommandInfo command)
    {
      if (Context.User is SocketUser user)
      {
        LockService.RemoveUserFromInteractionLock(user);
        if (InteractiveService.NextReactionAsync(x => x.MessageId == Context.Message.Id).WaitEx().IsSuccess)
        {
          var availableCommands = new List<string>();
          foreach (var module in CommandService.Modules)
          {
            var commands = module.Commands.Select(x => x.Name).ToArray();
            availableCommands.AddRange(commands);
          }

          var pageBuilder = new PageBuilder()
            .WithTitle("Comandi disponibili")
            .WithDescription("Aiutami a capire meglio quale comando intendessi eseguire selezionandolo dalla lista qui sotto")
            .WithColor(Color.Blue);

          var selection = new SelectionBuilder<string>()
            .AddUser(Context.User)
            .WithOptions(availableCommands)
            .WithInputType(InputType.SelectMenus)
            .WithDeletion(DeletionOptions.None)
            .WithSelectionPage(pageBuilder)
            .Build();

          var selectionResult = InteractiveService.SendSelectionAsync(selection, Context.Channel).WaitEx();
          if (selectionResult.IsSuccess)
          { } //Save all inside a json
        }
      }
    }
  }
}
