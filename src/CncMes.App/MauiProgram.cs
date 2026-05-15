using CncMes.App.Authentication;
using CncMes.Application;
using CncMes.Infrastructure;
using CncMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
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

		builder.Services.AddAuthorizationCore();
		builder.Services.AddScoped<CncMesAuthStateProvider>();
		builder.Services.AddScoped<AuthenticationStateProvider>(
			sp => sp.GetRequiredService<CncMesAuthStateProvider>());

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		DbInitializer.InitializeAsync(app.Services).GetAwaiter().GetResult();

		return app;
	}
}
