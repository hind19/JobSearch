// JobSearch.WPF/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace JobSearch.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HomeViewModel _homeViewModel;
    private readonly UserProfileViewModel _userProfileViewModel;
    private readonly JobSitesViewModel _jobSitesViewModel;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public MainViewModel(
        HomeViewModel homeViewModel,
        UserProfileViewModel userProfileViewModel,
        JobSitesViewModel jobSitesViewModel)
    {
        _homeViewModel = homeViewModel;
        _userProfileViewModel = userProfileViewModel;
        _jobSitesViewModel = jobSitesViewModel;

        homeViewModel.NavigateToProfile = NavigateToProfile;
        homeViewModel.NavigateToJobSites = NavigateToJobSites;
        userProfileViewModel.NavigateHome = NavigateToHome;
        jobSitesViewModel.NavigateHome = NavigateToHome;

        _currentViewModel = homeViewModel;
    }

    public async Task InitializeAsync(Guid userId)
    {
        await _userProfileViewModel.LoadUserProfileAsync(userId);
    }

    private void NavigateToProfile() => CurrentViewModel = _userProfileViewModel;
    private void NavigateToJobSites() => CurrentViewModel = _jobSitesViewModel;
    private void NavigateToHome() => CurrentViewModel = _homeViewModel;
}