namespace ISISLab.HelpDesk
{
  using ISISLab.HelpDesk.Services.Interfaces;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using System.Threading.Tasks;

  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddService<TService, TImplementation>(this IServiceCollection This)
        where TImplementation : class, IService, TService
    {
      return This.AddSingleton(typeof(IService), x => x.GetService(typeof(TService)))
          .AddSingleton(typeof(TService), typeof(TImplementation));
    }

    public static IServiceCollection AddService<TImplementation>(this IServiceCollection This)
        where TImplementation : class, IService
    {
      return This.AddSingleton<IService>(x => x.GetRequiredService<TImplementation>())
          .AddSingleton<TImplementation>();
    }

    public static IServiceCollection AddHostedServiceEx<TService, TImplementation>(this IServiceCollection This)
        where TImplementation : class, IHostedService, TService
    {
      return This.AddSingleton(typeof(IHostedService), x => x.GetService(typeof(TService)))
          .AddSingleton(typeof(TService), typeof(TImplementation));
    }

    public static IServiceCollection AddHostedServiceEx<TImplementation>(this IServiceCollection This)
        where TImplementation : class, IHostedService
    {
      return This.AddSingleton<IHostedService>(x => x.GetRequiredService<TImplementation>())
          .AddSingleton<TImplementation>();
    }
  }

  public static class TaskExtensions
  {
    public static void WaitEx(this Task task)
      => task.GetAwaiter().GetResult();

    public static TResult WaitEx<TResult>(this Task<TResult> task)
      => task.GetAwaiter().GetResult();
  }

  public static class StringExtensions
  {
    public static string RemoveNewLine(this string @this)
    {
      return @this.Replace("\n", " ");
    }
  }
}
