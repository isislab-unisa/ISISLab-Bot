namespace ISISLab.HelpDesk.Models
{
  public class SeminarReservationModel
  {
    public string Author { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }
    public string Speakers { get; set; }
    public string ImageUrl { get; set; }

    public SeminarReservationModel()
    { }
  }
}
