using CncMes.Application;
using CncMes.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CncMes.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cncmes.db");
		var connectionString = $"Data Source={dbPath}";

		builder.Services.AddApplication();
		builder.Services.AddInfrastructure(connectionString);

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
