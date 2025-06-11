using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WatchDog.Data.Factories;
using WatchDog.Data.Repositories;
using WatchDog.Services;
using WatchDog.ViewModels;
using WatchDog.Views;

namespace WatchDog;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var services = new ServiceCollection();
            var configuration = BuildConfiguration();
            ConfigureServices(services, configuration);
            ServiceProvider = services.BuildServiceProvider();
            
            desktop.MainWindow = new MainWindow();
            
           Navigator.Navigate<LoginViewModel>(); 
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true, reloadOnChange:true)
            .AddEnvironmentVariables();
        
        return builder.Build();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfiguration>(configuration);
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddSingleton<IDbConnectionFactory>(provider => 
            new PostDbConnectionFactory(connectionString));

        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IUserProjectRepository, UserProjectRepository>();
        services.AddSingleton<ITaskRepository, TaskRepository>();
        services.AddSingleton<ISubTaskRepository, SubTaskRepository>();
        services.AddSingleton<IProgressionMessageRepository, ProgressionMessageRepository>();
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<ITimeLineMessageRepository, TimeLineMessageRepository>();

        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ITaskService, TaskService>();
        services.AddSingleton<ISubtaskService, SubTaskService>();
        services.AddSingleton<IProgressionMessageService, ProgressionMessageService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ITimeLineMessageService, TimeLineMessageService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<NewProjectViewModel>();
        services.AddTransient<ProjectViewModel>();
        services.AddTransient<TaskViewModel>();
        services.AddTransient<RegisterUserViewModel>();
    }
}