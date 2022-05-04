namespace ISISLab.HelpDesk.Configuration
{
  public class GoogleOption
  {
    public CalendarOption Calendar { get; set; }
    public GMailOption Gmail { get; set; }
  }

  public class CalendarOption
  {
    public string APIKey { get; set; }
    public string CalendarId { get; set; }
  }

  public class GMailOption
  {
    public string From { get; set; }
    public string To { get; set; }
    public string Password { get; set; }
  }
}
