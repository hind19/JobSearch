using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Dialogs;
using JobSearch.WPF.Localization;

namespace JobSearch.WPF.ViewModels;

public partial class EmailSettingsViewModel : ObservableObject
{
    private readonly IEmailSettingsService _emailSettingsService;
    private readonly IDialogService _dialogService;

    // null until the first successful load/save — distinguishes "form
    // reflects an existing DB row" from "nothing configured yet, Save
    // will create the singleton row" (see EmailSettingsRepository.UpsertAsync).
    private Guid? _currentId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _smtpHost = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _smtpPort = 587;

    [ObservableProperty]
    private bool _useSsl = true;

    [ObservableProperty]
    private string _smtpUsername = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _fromAddress = string.Empty;

    [ObservableProperty]
    private string _fromDisplayName = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    // Deliberately no password property here — ADR-0005 §2: the SMTP
    // password is never read or written by this form, only set via
    // dotnet user-secrets. The form shows a static note instead.

    public Action? NavigateHome { get; set; }

    public EmailSettingsViewModel(
        IEmailSettingsService emailSettingsService,
        IDialogService dialogService)
    {
        _emailSettingsService = emailSettingsService;
        _dialogService = dialogService;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        try
        {
            var settings = await _emailSettingsService.GetAsync(ct);

            if (settings is null)
            {
                // Nothing in the DB and no seed in appsettings.json —
                // form stays at its defaults, Save will create the row.
                _currentId = null;
                return;
            }

            _currentId = settings.Id;
            SmtpHost = settings.SmtpHost;
            SmtpPort = settings.SmtpPort;
            UseSsl = settings.UseSsl;
            SmtpUsername = settings.SmtpUsername;
            FromAddress = settings.FromAddress;
            FromDisplayName = settings.FromDisplayName;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("EmailSettings_LoadError"), ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(SmtpHost) &&
        SmtpPort is > 0 and <= 65535 &&
        !string.IsNullOrWhiteSpace(FromAddress);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var dto = new EmailSettingsDto(
                id: _currentId ?? Guid.NewGuid(),
                smtpHost: SmtpHost.Trim(),
                smtpPort: SmtpPort,
                useSsl: UseSsl,
                smtpUsername: SmtpUsername.Trim(),
                fromAddress: FromAddress.Trim(),
                fromDisplayName: FromDisplayName.Trim(),
                // Overwritten server-side regardless (EmailSettingsService
                // always stamps UpdatedAt itself) — value here is unused.
                updatedAt: DateTime.UtcNow);

            var saved = await _emailSettingsService.SaveAsync(dto);
            _currentId = saved.Id;

            _dialogService.ShowInfo(LocalizationManager.Get("EmailSettings_SaveSuccess"));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("EmailSettings_SaveError"), ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoHome() => NavigateHome?.Invoke();
}
