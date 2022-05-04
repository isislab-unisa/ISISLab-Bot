namespace ISISLab.HelpDesk
{
  using ISISLab.HelpDesk.Configuration;

  public class AppOptions
  {
    public string BotToken { get; set; }
    public AIOption AI { get; set; }
    public LoggerOption Logging { get; set; }
    public SeminarsOption Seminars { get; set; }
    public GoogleOption Google { get; set; }
  }
}
