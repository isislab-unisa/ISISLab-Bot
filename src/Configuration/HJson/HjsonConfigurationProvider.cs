using System.IO;

using Hjson;
using Microsoft.Extensions.Configuration.Json;

namespace ISISLab.HelpDesk.Configuration.HJson
{
  public class HjsonConfigurationProvider : JsonConfigurationProvider
  {
    public HjsonConfigurationProvider(JsonConfigurationSource source)
      : base(source)
    {
    }

    public override void Load(Stream stream)
    {
      var hjson = HjsonValue.Load(stream);
      using (var jsonStream = new MemoryStream())
      {
        hjson.Save(jsonStream);
        jsonStream.Position = 0;
        base.Load(jsonStream);
      }
    }
  }
}
