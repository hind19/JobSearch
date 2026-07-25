// JobSearch.WPF/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace JobSearch.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HomeViewModel _homeViewModel;
    private readonly UserProfileViewModel _userProfileViewModel;
    private readonly JobSitesViewModel _jobSitesViewModel;
    private readonly EmailSettingsViewModel _emailSettingsViewModel;
    private readonly StatisticsViewModel _statisticsViewModel;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(
        HomeViewModel homeViewModel,
        UserProfileViewModel userProfileViewModel,
        JobSitesViewModel jobSitesViewModel,
        EmailSettingsViewModel emailSettingsViewModel,
        StatisticsViewModel statisticsViewModel)
    {
        _homeViewModel = homeViewModel;
        _userProfileViewModel = userProfileViewModel;
        _jobSitesViewModel = jobSitesViewModel;
        _emailSettingsViewModel = emailSettingsViewModel;
        _statisticsViewModel = statisticsViewModel;

        homeViewModel.NavigateToProfile = NavigateToProfile;
        homeViewModel.NavigateToJobSites = NavigateToJobSites;
        homeViewModel.NavigateToEmailSettings = NavigateToEmailSettings;
        homeViewModel.NavigateToStatistics = NavigateToStatistics;
        userProfileViewModel.NavigateHome = NavigateToHome;
        jobSitesViewModel.NavigateHome = NavigateToHome;
        emailSettingsViewModel.NavigateHome = NavigateToHome;
        statisticsViewModel.NavigateHome = NavigateToHome;

        _currentViewModel = homeViewModel;
    }

    public async Task InitializeAsync(Guid userId)
    {
        await _userProfileViewModel.LoadUserProfileAsync(userId);
    }

    private void NavigateToProfile() => CurrentViewModel = _userProfileViewModel;
    private async void NavigateToJobSites()
    {
        CurrentViewModel = _jobSitesViewModel;
        await _jobSitesViewModel.LoadAsync();
    }
    private async void NavigateToEmailSettings()
    {
        CurrentViewModel = _emailSettingsViewModel;
        await _emailSettingsViewModel.LoadAsync();
    }
    private async void NavigateToStatistics()
    {
        CurrentViewModel = _statisticsViewModel;
        await _statisticsViewModel.LoadAsync();
    }
    private void NavigateToHome() => CurrentViewModel = _homeViewModel;
}