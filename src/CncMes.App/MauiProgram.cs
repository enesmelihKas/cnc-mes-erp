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

		var stlDir = Path.Combine(FileSystem.AppDataDirectory, "stl");

		builder.Services.AddApplication();
		builder.Services.AddInfrastructure(connectionString, stlDir);

		builder.Services.AddAuthorizationCore();
		// Singleton: MAUI tek-pencere bir desktop process; BlazorWebView F5 ile yeniden
		// yüklendiğinde Scoped olsaydı oturum sıfırlanırdı. Singleton sayesinde
		// in-memory auth state webview reload'undan etkilenmiyor.
		builder.Services.AddSingleton<CncMesAuthStateProvider>();
		builder.Services.AddSingleton<AuthenticationStateProvider>(
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
