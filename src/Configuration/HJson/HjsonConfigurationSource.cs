using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ISISLab.HelpDesk.Configuration.HJson
{
  public class HjsonConfigurationSource : JsonConfigurationSource
  {
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
      EnsureDefaults(builder);
      return new HjsonConfigurationProvider(this);
    }
  }
}
