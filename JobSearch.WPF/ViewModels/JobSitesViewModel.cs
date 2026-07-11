// JobSearch.WPF/ViewModels/JobSitesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JobSearch.WPF.ViewModels;

public partial class JobSitesViewModel : ObservableObject
{
    public Action? NavigateHome { get; set; }

    [RelayCommand]
    private void GoHome() => NavigateHome?.Invoke();
}