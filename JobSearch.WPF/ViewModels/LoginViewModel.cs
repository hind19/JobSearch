using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Localization;
using Microsoft.Extensions.Configuration;

namespace JobSearch.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserProfileService _userProfileService;
    private readonly string _bypassEmail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle), nameof(LoginButtonText), nameof(NewUserButtonText))]
    private bool _isNewUser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string FormTitle => IsNewUser
        ? LocalizationManager.Get("Login_FormTitle_New")
        : LocalizationManager.Get("Login_FormTitle_Existing");

    public string LoginButtonText => IsNewUser
        ? LocalizationManager.Get("Login_Button_CreateProfile")
        : LocalizationManager.Get("Login_Button_SignIn");

    public string NewUserButtonText => IsNewUser
        ? LocalizationManager.Get("Common_Back")
        : LocalizationManager.Get("Login_Button_NewUser");

    public Guid? LoggedInUserId { get; private set; }

    public event Action<bool>? RequestClose;

    public LoginViewModel(IUserProfileService userProfileService, IConfiguration configuration)
    {
        _userProfileService = userProfileService;
        _bypassEmail = configuration["LoginSettings:BypassEmail"] ?? string.Empty;
        _email = _bypassEmail;
    }

    private bool CanLogin() => !string.IsNullOrWhiteSpace(Email);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login()
    {
        ErrorMessage = string.Empty;

        bool isBypass = IsNewUser ||
            (!string.IsNullOrEmpty(_bypassEmail) &&
             Email.Trim().Equals(_bypassEmail, StringComparison.OrdinalIgnoreCase));

        if (isBypass)
        {
            // Always succeeds — try to resolve userId for profile loading, ignore failures
            try
            {
                LoggedInUserId = await _userProfileService
                    .FindUserByEmailAsync(Email.Trim(), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            RequestClose?.Invoke(true);
            return;
        }

        try
        {
            var existingId = await _userProfileService
                .FindUserByEmailAsync(Email.Trim(), CancellationToken.None);

            if (existingId.HasValue)
            {
                LoggedInUserId = existingId;
                RequestClose?.Invoke(true);
            }
            else
            {
                ErrorMessage = LocalizationManager.Get("Login_Error_NotFound");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(LocalizationManager.Get("Login_Error_Prefix"), ex.Message);
        }
    }

    [RelayCommand]
    private void ToggleNewUser()
    {
        IsNewUser = !IsNewUser;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);
}
