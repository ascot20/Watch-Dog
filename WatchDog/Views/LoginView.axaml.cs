using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WatchDog.ViewModels;

namespace WatchDog.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnLoginAccountTapped(object sender, TappedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
           viewModel.LoginCommand.Execute(null); 
        }
    }
}