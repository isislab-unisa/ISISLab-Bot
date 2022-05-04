namespace ISISLab.HelpDesk.Services
{
  using System;
  using System.Net;
  using System.Threading.Tasks;

  using ISISLab.HelpDesk.Logging;
  using ISISLab.HelpDesk.Models;
  using ISISLab.HelpDesk.Models.Json;
  using Microsoft.Extensions.Options;
  using RestSharp;

  public class AIService
  {
    private readonly ILogger _logger;
    private readonly AppOptions _appOptions;

    private RestClient _client;

    public AIService(ILogger<AIService> logger, IOptions<AppOptions> options)
    {
      _logger     = logger;
      _appOptions = options.Value;

      var configured = !string.IsNullOrEmpty(_appOptions.AI.BaseUrl);

      if (!configured)
        throw new Exception("Unable to initialize AI Service. Invalid configuration.");

      _client = new RestClient(_appOptions.AI.BaseUrl);
    }

    public async Task<PredictionResponseModel> PredictAsync(string message)
    {
      var predictionRequest = new RestRequest($"api/predict", Method.Get);
      predictionRequest.AddParameter("message", message, true);

      var predictionResponse = await _client.ExecuteAsync<PredictionResult>(predictionRequest, default);
      if (predictionResponse.StatusCode != HttpStatusCode.OK) return PredictionResponseModel.RetError();

      if (predictionResponse.Data == null) return PredictionResponseModel.RetError();
      if (predictionResponse.Data.Result.Equals("failed")) return PredictionResponseModel.RetError();

      return PredictionResponseModel.RetSuccess(predictionResponse.Data.Intent);
    }
  }
}
