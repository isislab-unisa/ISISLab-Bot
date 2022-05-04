namespace ISISLab.HelpDesk.Logging
{
  #region LoggerFactory Interface

  public interface ILoggerFactory
  {
    ILogger CreateLogger();

    ILogger<T> CreateLogger<T>();
  }

  #endregion

  public class LoggerFactory : ILoggerFactory
  {
    public ILogger CreateLogger()
    {
      return new Logger();
    }

    public ILogger<T> CreateLogger<T>()
    {
      return new Logger<T>();
    }
  }
}
