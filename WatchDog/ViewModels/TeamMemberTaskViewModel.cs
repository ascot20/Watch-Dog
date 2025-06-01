using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WatchDog.Models;

namespace WatchDog.ViewModels;

public partial class TeamMemberTaskViewModel:ObservableObject
{
    public User Member { get; set; }

    [ObservableProperty] private ObservableCollection<string> _taskDescriptions = new();
}