namespace ISISLab.HelpDesk.Models
{
  public class PredictionResponseModel
  {
    public bool Success { get; set; }
    public string Intent { get; set; }

    public PredictionResponseModel()
    { }

    public static PredictionResponseModel RetSuccess(string intent)
    {
      return new PredictionResponseModel
      {
        Success = true,
        Intent  = intent
      };
    }

    public static PredictionResponseModel RetError()
    {
      return new PredictionResponseModel
      {
        Success = false,
        Intent  = ""
      };
    }
  }
}
