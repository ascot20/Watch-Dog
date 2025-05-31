using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WatchDog.Services;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [RelayCommand]
    protected void NavigateToDashboard()
    {
       Navigator.Navigate<DashboardViewModel>(); 
    }
   
    [RelayCommand]
    protected void NavigateToNewProject()
    {
       Navigator.Navigate<NewProjectViewModel>(); 
    }
    
    [RelayCommand]
    protected void NavigateToLogin()
    {
        Navigator.Navigate<LoginViewModel>();
    }

    [RelayCommand]
    protected void NavigateToProject(Project project)
    {
        if (project != null)
        {
            Navigator.NavigateWithParameter<ProjectViewModel>(project.Id);
        }
    }

    
}
