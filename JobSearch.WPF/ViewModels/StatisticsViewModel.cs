using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Dialogs;
using JobSearch.WPF.Localization;
using JobSearch.WPF.Models;

namespace JobSearch.WPF.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    // ADR-0009: single-day filter, not a range — 20 rows/page.
    private const int RejectedPageSize = 20;

    private readonly IJobStatisticsService _jobStatisticsService;
    private readonly IJobRejectionService _jobRejectionService;
    private readonly IDialogService _dialogService;

    private Guid _userId;

    // Guards against the initial LoadAsync setting RejectedFilterDate
    // from re-triggering a second, redundant reload via
    // OnRejectedFilterDateChanged.
    private bool _isInitializingRejectedFilter;

    [ObservableProperty]
    private ObservableCollection<StatisticsRowItem> _rows = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEmpty;

    // ─── Rejected jobs tab (ADR-0009) ──────────────────────────

    [ObservableProperty]
    private ObservableCollection<RejectedJobRowItem> _rejectedRows = new();

    [ObservableProperty]
    private bool _isRejectedBusy;

    [ObservableProperty]
    private bool _isRejectedEmpty;

    [ObservableProperty]
    private DateTime? _rejectedFilterDate;

    [ObservableProperty]
    private int _rejectedCurrentPage = 1;

    [ObservableProperty]
    private int _rejectedTotalPages = 1;

    public Action? NavigateHome { get; set; }

    public StatisticsViewModel(
        IJobStatisticsService jobStatisticsService,
        IJobRejectionService jobRejectionService,
        IDialogService dialogService)
    {
        _jobStatisticsService = jobStatisticsService;
        _jobRejectionService = jobRejectionService;
        _dialogService = dialogService;
    }

    public async Task LoadAsync(Guid userId, CancellationToken ct = default)
    {
        _userId = userId;

        await LoadSiteStatisticsAsync(ct);
        await LoadInitialRejectedJobsAsync(ct);
    }

    private async Task LoadSiteStatisticsAsync(CancellationToken ct)
    {
        IsBusy = true;
        try
        {
            var stats = await _jobStatisticsService.GetStatisticsAsync(ct);

            // Sort by match count descending — most productive site
            // first (job-statistics-development-plan.md open question #3).
            var sorted = stats.OrderByDescending(s => s.MatchesCount);

            Rows = new ObservableCollection<StatisticsRowItem>(sorted.Select(ToRow));
            IsEmpty = Rows.Count == 0;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("Statistics_LoadError"), ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ADR-0009: on open, default the filter to the most recent day that
    // has any rejection ("last scan"), then load that day's page 1.
    private async Task LoadInitialRejectedJobsAsync(CancellationToken ct)
    {
        try
        {
            var mostRecentDate = await _jobRejectionService
                .GetMostRecentAnalysisDateAsync(_userId, ct);

            _isInitializingRejectedFilter = true;
            RejectedFilterDate = (mostRecentDate ?? DateTime.Today).Date;
            _isInitializingRejectedFilter = false;

            RejectedCurrentPage = 1;
            await LoadRejectedJobsAsync(ct);
        }
        catch (Exception ex)
        {
            _isInitializingRejectedFilter = false;
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("Statistics_RejectedLoadError"), ex.Message));
        }
    }

    private async Task LoadRejectedJobsAsync(CancellationToken ct = default)
    {
        if (RejectedFilterDate is not { } date)
            return;

        IsRejectedBusy = true;
        try
        {
            var page = await _jobRejectionService.GetRejectedJobsAsync(
                _userId, date, RejectedCurrentPage, RejectedPageSize, ct);

            RejectedRows = new ObservableCollection<RejectedJobRowItem>(
                page.Items.Select(ToRow));
            RejectedTotalPages = page.TotalPages;
            IsRejectedEmpty = RejectedRows.Count == 0;

            RejectedPreviousPageCommand.NotifyCanExecuteChanged();
            RejectedNextPageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("Statistics_RejectedLoadError"), ex.Message));
        }
        finally
        {
            IsRejectedBusy = false;
        }
    }

    // ADR-0009: changing the date reloads immediately — no separate
    // "Apply" button — and resets pagination back to page 1.
    partial void OnRejectedFilterDateChanged(DateTime? value)
    {
        if (_isInitializingRejectedFilter || value is null)
            return;

        RejectedCurrentPage = 1;
        _ = LoadRejectedJobsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousRejectedPage))]
    private async Task RejectedPreviousPage()
    {
        RejectedCurrentPage--;
        await LoadRejectedJobsAsync();
    }

    private bool CanGoToPreviousRejectedPage() => RejectedCurrentPage > 1;

    [RelayCommand(CanExecute = nameof(CanGoToNextRejectedPage))]
    private async Task RejectedNextPage()
    {
        RejectedCurrentPage++;
        await LoadRejectedJobsAsync();
    }

    private bool CanGoToNextRejectedPage() =>
        RejectedCurrentPage < RejectedTotalPages;

    // ADR-0009: opens the job posting in the OS default browser.
    [RelayCommand]
    private void OpenJobUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(string.Format(
                LocalizationManager.Get("Statistics_OpenUrlError"), ex.Message));
        }
    }

    private static StatisticsRowItem ToRow(JobSiteStatisticsDto dto) =>
        new()
        {
            JobSiteName = dto.JobSiteName,
            JobsScrapedCount = dto.JobsScrapedCount,
            MatchesCount = dto.MatchesCount,
            // "—" for null resolves job-statistics-development-plan.md
            // open question #2 (division by zero when JobsScrapedCount is 0).
            MatchRateDisplay = dto.MatchRate is { } rate
                ? rate.ToString("P0", CultureInfo.CurrentCulture)
                : "—",
            AverageScoreDisplay = dto.AverageRelevanceScore is { } score
                ? score.ToString("F1", CultureInfo.CurrentCulture)
                : "—",
            MostRecentMatchDisplay = dto.MostRecentMatchAt is { } date
                ? date.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.CurrentCulture)
                : "—"
        };

    private static RejectedJobRowItem ToRow(RejectedJobDto dto) =>
        new()
        {
            JobUrl = dto.JobUrl,
            JobTitle = dto.JobTitle,
            RelevanceReasonDisplay = string.IsNullOrWhiteSpace(dto.RelevanceReason)
                ? "—"
                : dto.RelevanceReason,
            AnalyzedAtDisplay = dto.AnalyzedAt
                .ToLocalTime()
                .ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture)
        };

    [RelayCommand]
    private void GoHome() => NavigateHome?.Invoke();
}
