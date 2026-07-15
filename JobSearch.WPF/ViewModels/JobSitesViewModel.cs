// JobSearch.WPF/ViewModels/JobSitesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Dialogs;
using JobSearch.WPF.Localization;
using JobSearch.WPF.Models;
using System.Collections.ObjectModel;

namespace JobSearch.WPF.ViewModels;

public partial class JobSitesViewModel : ObservableObject
{
    private readonly IJobSiteService _jobSiteService;
    private readonly ISelectorDetector _selectorDetector;
    private readonly IDialogService _dialogService;

    public Action? NavigateHome { get; set; }

    [ObservableProperty]
    private ObservableCollection<JobSiteItem> _jobSites = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    private JobSiteItem? _editingItem;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDetecting;

    [ObservableProperty]
    private ScrapeConfigDto? _detectedConfig;

    [ObservableProperty]
    private bool _isValidationPopupOpen;

    [ObservableProperty]
    private List<string> _validationLinks = [];

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool IsEditing => EditingItem is not null;

    public JobSitesViewModel(
        IJobSiteService jobSiteService,
        ISelectorDetector selectorDetector,
        IDialogService dialogService)
    {
        _jobSiteService = jobSiteService;
        _selectorDetector = selectorDetector;
        _dialogService = dialogService;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var dtos = await _jobSiteService.GetAllAsync(ct);
            JobSites = new ObservableCollection<JobSiteItem>(dtos.Select(ToItem));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Load"), ex.Message));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void AddSite()
    {
        EditingItem = new JobSiteItem { Id = Guid.NewGuid() };
    }

    [RelayCommand]
    private void GoHome() => NavigateHome?.Invoke();

    [RelayCommand]
    private void EditSite(JobSiteItem item)
    {
        EditingItem = new JobSiteItem
        {
            Id = item.Id,
            Name = item.Name,
            BaseUrl = item.BaseUrl,
            IsActive = item.IsActive,
            SearchUrlTemplate = item.SearchUrlTemplate,
            SearchQuery = item.SearchQuery,
            ContainerSelector = item.ContainerSelector,
            LinkSelector = item.LinkSelector,
            CompanySelector = item.CompanySelector,
            SnippetSelector = item.SnippetSelector,
            DateSelector = item.DateSelector
        };
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingItem = null;
        DetectedConfig = null;
    }

    [RelayCommand]
    private async Task SaveSite(CancellationToken ct)
    {
        if (EditingItem is null) return;
        IsLoading = true;
        try
        {
            var dto = ToDto(EditingItem);
            var isNew = JobSites.All(s => s.Id != EditingItem.Id);
            if (isNew)
            {
                var created = await _jobSiteService.CreateAsync(dto, ct);
                JobSites.Add(ToItem(created));
            }
            else
            {
                var updated = await _jobSiteService.UpdateAsync(dto, ct);
                var existing = JobSites.First(s => s.Id == updated.Id);
                JobSites[JobSites.IndexOf(existing)] = ToItem(updated);
            }
            EditingItem = null;
            DetectedConfig = null;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Save"), ex.Message));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task DeleteSite(JobSiteItem item)
    {
        if (!_dialogService.ShowConfirmation(
            string.Format(LocalizationManager.Get("JobSites_Delete_Confirm"), item.Name)))
            return;

        IsLoading = true;
        try
        {
            await _jobSiteService.DeleteAsync(item.Id);
            JobSites.Remove(item);
            if (EditingItem?.Id == item.Id)
            {
                EditingItem = null;
                DetectedConfig = null;
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Delete"), ex.Message));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task ToggleActive(JobSiteItem item)
    {
        try
        {
            await _jobSiteService.SetActiveAsync(item.Id, !item.IsActive);
            item.IsActive = !item.IsActive;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Status"), ex.Message));
        }
    }

    [RelayCommand]
    private async Task DetectFromHtml(CancellationToken ct)
    {
        if (EditingItem is null || string.IsNullOrWhiteSpace(EditingItem.HtmlInput)) return;
        IsDetecting = true;
        DetectedConfig = null;
        try
        {
            DetectedConfig = await _selectorDetector.DetectFromHtmlAsync(EditingItem.HtmlInput, ct);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Detect"), ex.Message));
        }
        finally { IsDetecting = false; }
    }

    [RelayCommand]
    private async Task ValidateConfig(CancellationToken ct)
    {
        if (EditingItem is null) return;

        IsLoading = true;
        try
        {
            var dto = ToDto(EditingItem);
            var (isValid, errorMessage, links) =
    await _jobSiteService.ValidateConfigAsync(dto, ct);

            // TODO: Consider parsing each link and showing full job data (title, company, date)
            ValidationLinks = links;
            ValidationMessage = isValid
                ? string.Format(
                    LocalizationManager.Get("JobSites_Popup_Found"),
                    links.Count)
                : errorMessage
                  ?? LocalizationManager.Get("JobSites_Popup_Empty");

            IsValidationPopupOpen = true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(
                    LocalizationManager.Get("JobSites_Error_Test"),
                    ex.Message));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseValidationPopup()
    {
        IsValidationPopupOpen = false;
        ValidationLinks = [];
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    private async Task DetectFromUrl(CancellationToken ct)
    {
        if (EditingItem is null || string.IsNullOrWhiteSpace(EditingItem.DetectUrl)) return;
        IsDetecting = true;
        DetectedConfig = null;
        try
        {
            DetectedConfig = await _selectorDetector.DetectFromUrlAsync(EditingItem.DetectUrl, ct);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(
                string.Format(LocalizationManager.Get("JobSites_Error_Detect"), ex.Message));
        }
        finally { IsDetecting = false; }
    }

    [RelayCommand]
    private void ApplyDetectedConfig()
    {
        if (EditingItem is null || DetectedConfig is null) return;

        if (DetectedConfig.ContainerSelector is not null)
            EditingItem.ContainerSelector = DetectedConfig.ContainerSelector;
        if (DetectedConfig.LinkSelector is not null)
            EditingItem.LinkSelector = DetectedConfig.LinkSelector;
        if (DetectedConfig.CompanySelector is not null)
            EditingItem.CompanySelector = DetectedConfig.CompanySelector;
        if (DetectedConfig.SnippetSelector is not null)
            EditingItem.SnippetSelector = DetectedConfig.SnippetSelector;
        if (DetectedConfig.DateSelector is not null)
            EditingItem.DateSelector = DetectedConfig.DateSelector;

        DetectedConfig = null;
    }

    [RelayCommand]
    private void ApplySingleSelector(string field)
    {
        if (EditingItem is null || DetectedConfig is null) return;

        switch (field)
        {
            case nameof(JobSiteItem.ContainerSelector):
                if (DetectedConfig.ContainerSelector is not null)
                    EditingItem.ContainerSelector = DetectedConfig.ContainerSelector;
                break;
            case nameof(JobSiteItem.LinkSelector):
                if (DetectedConfig.LinkSelector is not null)
                    EditingItem.LinkSelector = DetectedConfig.LinkSelector;
                break;
            case nameof(JobSiteItem.CompanySelector):
                if (DetectedConfig.CompanySelector is not null)
                    EditingItem.CompanySelector = DetectedConfig.CompanySelector;
                break;
            case nameof(JobSiteItem.SnippetSelector):
                if (DetectedConfig.SnippetSelector is not null)
                    EditingItem.SnippetSelector = DetectedConfig.SnippetSelector;
                break;
            case nameof(JobSiteItem.DateSelector):
                if (DetectedConfig.DateSelector is not null)
                    EditingItem.DateSelector = DetectedConfig.DateSelector;
                break;
        }
    }

    // ─── Mapping helpers ─────────────────────────────────────

    private static JobSiteItem ToItem(JobSiteDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            BaseUrl = dto.BaseUrl,
            IsActive = dto.IsActive,
            SearchUrlTemplate = dto.ScrapeConfig.SearchUrlTemplate,
            SearchQuery = dto.ScrapeConfig.SearchQuery,
            ContainerSelector = dto.ScrapeConfig.ContainerSelector,
            LinkSelector = dto.ScrapeConfig.LinkSelector,
            CompanySelector = dto.ScrapeConfig.CompanySelector,
            SnippetSelector = dto.ScrapeConfig.SnippetSelector,
            DateSelector = dto.ScrapeConfig.DateSelector
        };

    private static JobSiteDto ToDto(JobSiteItem item) =>
        new(
            id: item.Id,
            name: item.Name,
            baseUrl: item.BaseUrl,
            isActive: item.IsActive,
            scrapeConfig: new ScrapeConfigDto(
                searchUrlTemplate: item.SearchUrlTemplate,
                searchQuery: item.SearchQuery,
                containerSelector: item.ContainerSelector,
                linkSelector: item.LinkSelector,
                companySelector: item.CompanySelector,
                snippetSelector: item.SnippetSelector,
                dateSelector: item.DateSelector
            )
        );
}