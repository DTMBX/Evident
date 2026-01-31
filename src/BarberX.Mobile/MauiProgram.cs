using Microsoft.Extensions.Logging;
using BarberX.Mobile.Services;
using BarberX.Mobile.ViewModels;
using BarberX.Mobile.Views;
using BarberX.Shared.Services;

namespace BarberX.Mobile;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register HttpClient with base address
		builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
		{
#if DEBUG
			client.BaseAddress = new Uri("http://localhost:5000");
#else
			client.BaseAddress = new Uri("https://api.barberx.info");
#endif
			client.Timeout = TimeSpan.FromMinutes(5);
		});

		// Register services
		builder.Services.AddSingleton<AuthService>();
		
		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<AnalysisListViewModel>();
		builder.Services.AddTransient<UploadViewModel>();
		builder.Services.AddTransient<AnalysisDetailViewModel>();
		
		// Register Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<AnalysisListPage>();
		builder.Services.AddTransient<UploadPage>();
		builder.Services.AddTransient<AnalysisDetailPage>();

		return builder.Build();
	}
}
