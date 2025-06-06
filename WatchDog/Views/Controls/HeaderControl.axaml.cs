using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WatchDog.Views.Controls;

public partial class HeaderControl : UserControl
{
    public static readonly StyledProperty<string> PageTitleProperty =
        AvaloniaProperty.Register<HeaderControl, string>(nameof(PageTitle), defaultValue: "WatchDog");
    
    
    public static readonly StyledProperty<ICommand> NavigateToDashboardCommandProperty =
        AvaloniaProperty.Register<HeaderControl, ICommand>(nameof(NavigateToDashboardCommand));
    
    public static readonly StyledProperty<ICommand> LogOutCommandProperty =
        AvaloniaProperty.Register<HeaderControl, ICommand>(nameof(LogOutCommand));


    public string PageTitle
    {
        get => GetValue(PageTitleProperty);
        set => SetValue(PageTitleProperty, value);
    }
    
    public ICommand NavigateToDashboardCommand
    {
        get => GetValue(NavigateToDashboardCommandProperty);
        set => SetValue(NavigateToDashboardCommandProperty, value);
    }
    
    public ICommand LogOutCommand
    {
        get => GetValue(LogOutCommandProperty);
        set => SetValue(LogOutCommandProperty, value);
    }

    public HeaderControl()
    {
        InitializeComponent();
    }
}