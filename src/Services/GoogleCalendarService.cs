namespace ISISLab.HelpDesk.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using System.Linq;

  using ISISLab.HelpDesk.Models;
  using ISISLab.HelpDesk.Logging;

  using Google.Apis.Calendar.v3;
  using Google.Apis.Services;
  using Microsoft.Extensions.Options;

  public class GoogleCalendarService
  {
    private readonly ILogger _logger;
    private readonly AppOptions _appOptions;
    private readonly CalendarService _calendarService;

    public virtual bool IsConfigured => _calendarService != null;

    public GoogleCalendarService(ILogger<GoogleCalendarService> logger, IOptions<AppOptions> options)
    {
      _logger     = logger;
      _appOptions = options.Value;

      var configured = !string.IsNullOrEmpty(_appOptions.Google.Calendar.APIKey) &&
                       !string.IsNullOrEmpty(_appOptions.Google.Calendar.CalendarId);

      if (!configured)
        throw new Exception("Unable to initialize Google Calendar Service. Invalid configuration.");

      _calendarService = new CalendarService(new BaseClientService.Initializer()
      {
        ApiKey = _appOptions.Google.Calendar.APIKey,
      });
    }

    public virtual async Task<CalendarEventsModel> GetEventsAsync()
    {
      try
      {
        var request = _calendarService.Events.List(_appOptions.Google.Calendar.CalendarId);
        request.TimeMin       = DateTime.Now;
        request.ShowDeleted   = false;
        request.SingleEvents  = true;
        request.MaxResults    = 5;
        request.OrderBy       = EventsResource.ListRequest.OrderByEnum.StartTime;

        var events = await request.ExecuteAsync();
        var isisLabEventList = new List<CalendarEventModel>();

        events.Items.ToList().ForEach(x => isisLabEventList.Add(new CalendarEventModel
        {
          StartDate   = x.Start.DateTime,
          Summary     = x.Summary,
          Description = x.Description,
          Location    = x.Location,
          ColorId     = x.ColorId
        }));

        return CalendarEventsModel.RetSuccess(isisLabEventList);
      }
      catch (Exception ex)
      { return CalendarEventsModel.RetError(ex.Message); }
    }
  }
}
