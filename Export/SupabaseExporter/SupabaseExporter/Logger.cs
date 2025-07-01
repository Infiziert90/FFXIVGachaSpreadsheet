using Microsoft.Extensions.Logging;

namespace SupabaseExporter;

public static class Logger
{
    private static readonly ILogger Log;

    static Logger()
    {
        using var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options => { options.SingleLine = true;}));
        Log = factory.CreateLogger("Exporter");
    }

    public static void Information(string message)
    {
        Log.LogInformation(message);
    }

    public static void Warning(string message)
    {
        Log.LogWarning(message);
    }

    public static void Error(string message)
    {
        Log.LogError(message);
    }
}