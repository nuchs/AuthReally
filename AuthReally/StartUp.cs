using Microsoft.Extensions.Logging.Console;
using System.Reflection;

namespace Common;

public class StartUp
{
    private readonly string[] args;
    private readonly string appName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "The app with no name";

    public StartUp(string[] args)
    {
        this.args = args;

        Log.LogInformation(
            "Hello! It's a me, {AppName} : v{Version}",
            appName,
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "UnknownVersion");
    }

    public ILogger<StartUp> Log { get; } = CreateStartupLogger();

    public Action<WebApplicationBuilder> AddServices { get; set; } = _ => { };

    public Action<WebApplication> ConfigureRequestPipeline { get; set; } = _ => { };

    public int Run()
    {
        try
        {
            Log.LogInformation("Initialising {AppName}", appName);

            var app = Build();

            Configure(app);
            StartApp(app);

            return 0;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "An error occured; {AppName} was terminated", appName);

            return 1;
        }
    }

    private static ILogger<StartUp> CreateStartupLogger()
    {
        using var loggerFactory = LoggerFactory.Create(
            builder => builder.AddJsonConsole(ConfigureJsonLogger()));

        return loggerFactory.CreateLogger<StartUp>();
    }

    private static Action<JsonConsoleFormatterOptions> ConfigureJsonLogger()
    {
        return conf =>
        {
            conf.IncludeScopes = false;
            conf.TimestampFormat = $"yyyy-MM-dd HH:mm:ss.fff";
            conf.UseUtcTimestamp = true;
        };
    }

    private void Configure(WebApplication app)
    {
        Log.LogInformation("Configuring the request pipeline for {AppName}", appName);

        ConfigureRequestPipeline(app);
    }

    private WebApplication Build()
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.LogInformation("Building {AppName} for {Env}", appName, builder.Environment.EnvironmentName);

        {
            Log.LogInformation("Running {AppName} as application", appName);
            builder.Logging.ClearProviders();
            builder.Logging.AddJsonConsole(ConfigureJsonLogger());
        }

        Log.LogInformation("Adding services to {AppName}", appName);
        AddServices(builder);

        return builder.Build();
    }

    private void StartApp(WebApplication app)
    {
        Log.LogInformation("{AppName} is cocked, locked and ready to rock!", appName);
        app.Run();
        Log.LogInformation("Bye from {AppName}", appName);
    }
}

