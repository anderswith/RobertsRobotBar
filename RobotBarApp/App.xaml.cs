using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL;
using RobotBarApp.DAL.Repositories;
using RobotBarApp.DAL.Repositories.Interfaces;
using RobotBarApp.Helper;
using RobotBarApp.Services;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;
using RobotBarApp.View;
using RobotBarApp.ViewModels;


namespace RobotBarApp;

public partial class App : Application
{
    public static IHost AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // DbContext
                services.AddDbContext<RobotBarContext>();
                
                // Repositories
                services.AddScoped<IBarSetupRepository, BarSetupRepository>();
                services.AddScoped<IDrinkRepository, DrinkRepository>();
                services.AddScoped<IEventRepository, EventRepository>();
                services.AddScoped<IIngredientRepository, IngredientRepository>();
                services.AddScoped<ILogRepository, LogRepository>();
                services.AddScoped<IMenuRepository, MenuRepository>();
                services.AddScoped<ISopRepository, SopRepository>();
                services.AddScoped<IIngredientUseCountRepository, IngredientUseCountRepository>();
                services.AddScoped<IDrinkUseCountRepository, DrinkUseCountRepository>();

                // Logic
                services.AddScoped<IBarSetupLogic, BarSetupLogic>();
                services.AddScoped<IDrinkLogic, DrinkLogic>();
                services.AddScoped<IEventLogic, EventLogic>();
                services.AddScoped<IIngredientLogic, IngredientLogic>();
                services.AddScoped<ILogLogic, LogLogic>();
                services.AddScoped<IMenuLogic, MenuLogic>();
                services.AddScoped<ISopLogic, SopLogic>();
                services.AddScoped<IRobotLogic, RobotLogic>();
                services.AddScoped<IIngredientUseCountLogic, IngredientUseCountLogic>();
                services.AddScoped<IDrinkUseCountLogic, DrinkUseCountLogic>();
                
                // ViewModels
                services.AddTransient<EventListViewModel>();
                services.AddTransient<KatalogViewModel>();
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<StatistikViewModel>();
                services.AddTransient<TilfoejDrinkViewModel>();
                services.AddTransient<TilfoejEventViewModel>();
                services.AddTransient<TilfoejIngrediensViewModel>();
                services.AddTransient<TilfoejMenuViewModel>();
                
                // Views
                services.AddTransient<EventListView>();
                services.AddTransient<KatalogItemView>();
                services.AddTransient<KatalogView>();
                services.AddTransient<MainWindow>();
                services.AddTransient<OpstartView>();
                services.AddTransient<StatistikView>();
                services.AddTransient<TilfoejDrinkView>();
                services.AddTransient<TilfoejEventView>();
                services.AddTransient<TilfoejIngrediensView>();
                services.AddTransient<TilfoejMenuView>();
                
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<RobotLogMonitor>(provider =>
                {
                    var logLogic = provider.GetRequiredService<ILogLogic>();
                    return new RobotLogMonitor("192.168.0.101", logLogic);
                });

                services.AddSingleton<IRobotScriptRunner>(provider =>
                {
                    var comms = provider.GetRequiredService<IRobotComms>();
                    var reader = provider.GetRequiredService<IRobotDashboardStreamReader>();
                    var log = provider.GetRequiredService<ILogLogic>();

                    return new RobotScriptRunner(comms, reader, log);
                });

            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var monitor = AppHost?.Services.GetRequiredService<RobotLogMonitor>();
            if (monitor != null)
                await monitor.StartAsync();
        }
        catch (Exception ex)
        {
            // Log but don't crash.
            try
            {
                var log = AppHost?.Services.GetService<ILogLogic>();
                log?.AddLog($"RobotLogMonitor failed to start: {ex.Message}", "RobotError");
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Failed to log RobotLogMonitor startup error: {logEx.Message}");
            }
        }

        var main = AppHost.Services.GetRequiredService<MainWindow>();
        main.Show();
    }
}