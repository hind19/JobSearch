// JobSearch.WPF/ViewModels/HomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JobSearch.WPF.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    // устанавливается из MainViewModel после создания
    public Action? NavigateToProfile { get; set; }
    public Action? NavigateToJobSites { get; set; }
    public Action? NavigateToEmailSettings { get; set; }
    public Action? NavigateToStatistics { get; set; }

    [RelayCommand]
    private void OpenProfile() => NavigateToProfile?.Invoke();

    [RelayCommand]
    private void OpenJobSites() => NavigateToJobSites?.Invoke();

    [RelayCommand]
    private void OpenEmailSettings() => NavigateToEmailSettings?.Invoke();

    [RelayCommand]
    private void OpenStatistics() => NavigateToStatistics?.Invoke();
}