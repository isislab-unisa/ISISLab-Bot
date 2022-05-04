namespace ISISLab.HelpDesk.Models.Json
{
  using System.Text.Json.Serialization;

  public class PredictionResult
  {
    [JsonPropertyName("Result")]
    public string Result { get; set; }
    [JsonPropertyName("Intent")]
    public string Intent { get; set; }
  }
}
