using System.Collections.ObjectModel;
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
    private readonly IJobStatisticsService _jobStatisticsService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<StatisticsRowItem> _rows = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEmpty;

    public Action? NavigateHome { get; set; }

    public StatisticsViewModel(
        IJobStatisticsService jobStatisticsService,
        IDialogService dialogService)
    {
        _jobStatisticsService = jobStatisticsService;
        _dialogService = dialogService;
    }

    public async Task LoadAsync(CancellationToken ct = default)
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

    [RelayCommand]
    private void GoHome() => NavigateHome?.Invoke();
}
