using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using WatchDog.ViewModels;
using WatchDog.Views;

namespace WatchDog;

public static class Navigator
{
    public static void Navigate<T>() where T : ViewModelBase
    {
        var viewModel = CreateViewModel<T>();
        NavigateToViewModel(viewModel);
    }


    private static void NavigateToViewModel(ViewModelBase viewModel)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Content = viewModel;
        }

    }
    
    public static void NavigateWithParameter<T>(object parameter) where T : ViewModelBase
    {
        var viewModel = CreateViewModel<T>();
            
        // Initialize viewModel with parameter if it supports it
        if (viewModel is IInitializable initializable && parameter != null)
        {
            _ = initializable.InitializeAsync(parameter);
        }
            
        NavigateToViewModel(viewModel);
    }

    private static T CreateViewModel<T>() where T : ViewModelBase
    {
        if (App.ServiceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider is null");
        } 
        
        return App.ServiceProvider.GetRequiredService<T>();
    }
}

public interface IInitializable
{
    Task InitializeAsync(object parameter);
}