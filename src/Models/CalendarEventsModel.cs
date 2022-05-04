namespace ISISLab.HelpDesk.Models
{
  using System;
  using System.Collections.Generic;

  public class CalendarEventsModel
  {
    public bool Success { get; set; }
    public string Error { get; set; }
    public IEnumerable<CalendarEventModel> Events { get; set; }

    public static CalendarEventsModel Empty
    {
      get => new CalendarEventsModel
      {
        Success = true,
        Error   = "",

        Events = Array.Empty<CalendarEventModel>()
      };
    }

    public CalendarEventsModel()
    { }

    public static CalendarEventsModel RetSuccess(IEnumerable<CalendarEventModel> events)
    {
      return new CalendarEventsModel
      {
        Success = true,
        Error   = "",

        Events = events
      };
    }

    public static CalendarEventsModel RetError(string error)
    {
      return new CalendarEventsModel
      {
        Success = false,
        Error   = error,

        Events = Array.Empty<CalendarEventModel>()
      };
    }
  }

  public class CalendarEventModel
  {
    public DateTime? StartDate { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string ColorId { get; set; }
  }
}
