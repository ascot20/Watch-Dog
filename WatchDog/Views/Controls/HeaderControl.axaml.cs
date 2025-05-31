using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WatchDog.Views.Controls;

public partial class HeaderControl : UserControl
{
    public static readonly StyledProperty<string> PageTitleProperty =
        AvaloniaProperty.Register<HeaderControl, string>(nameof(PageTitle), defaultValue: "WatchDog");
    
    public static readonly StyledProperty<ICommand> NavigateToDashboardCommandProperty =
        AvaloniaProperty.Register<HeaderControl, ICommand>(nameof(NavigateToDashboardCommand));


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

    public HeaderControl()
    {
        InitializeComponent();
    }
}