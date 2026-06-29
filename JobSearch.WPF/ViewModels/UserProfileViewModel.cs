using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Dialogs;
using JobSearch.WPF.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

namespace JobSearch.WPF.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private string _cvFilePath = string.Empty;

    private readonly IUserProfileService _userProfileService;

    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _cvFileName = string.Empty;

    [ObservableProperty]
    private string _cvFileInfo = string.Empty;

    [ObservableProperty]
    private ImageSource? _cvIcon;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private ObservableCollection<SkillItem> _skills = new();
    [ObservableProperty]
    private ObservableCollection<ClarifyingQuestionItem> _clarifyingQuestions = new();

    public IEnumerable<ProficiencyLevel> ProficiencyLevels { get; } = Enum.GetValues<ProficiencyLevel>();

    public UserProfileViewModel(IUserProfileService userProfileService, IDialogService dialogService)
    {
        _userProfileService = userProfileService;
        _dialogService = dialogService;
       // LoadDesignTimeData();
    }

    [RelayCommand]
    private void ReplaceCv()
    {
        // Configure save file dialog box
        var dialog = new OpenFileDialog();
        dialog.FileName = "Document"; // Default file name
        dialog.DefaultExt = ".pdf"; // Default file extension
        dialog.Filter = "Text documents (.pdf)|*.pdf"; // Filter files by extension

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _cvFilePath = dialog.FileName;

                CvFileName = dialog.SafeFileName;

                FileInfo fileInfo = new FileInfo(dialog.FileName);
                int fileSizeInKB = (int)(fileInfo.Length / (1024));
                CvFileInfo = $"Загружено {fileSizeInKB} kB ";
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error reading file: {ex.Message}");
            }
        }
    }

    private bool CanAnalyzeCv() => !string.IsNullOrEmpty(CvFileName);

    [RelayCommand(CanExecute = nameof(CanAnalyzeCv))]
    private async Task AnalyzeCv()
    {
        IsAnalyzing = true;
        try
        {
            var fileBytes = File.ReadAllBytes(_cvFilePath);

            var result = await _userProfileService.AnalyzeCvAsync(fileBytes, CancellationToken.None);

            if (!result.IsSuccess)
            {
                _dialogService.ShowError(result.ErrorMessage ?? "CV analysis failed.");
                return;
            }

            Skills = new ObservableCollection<SkillItem>(result.Skills.Select(x => new SkillItem
            {
                IsFromCv = true,
                ProficiencyLevel = x.ProficiencyLevel,
                SkillName = x.SkillName,
                YearsOfExperience = (int)Math.Round(x.YearsOfExperience.GetValueOrDefault())
            }));

            ClarifyingQuestions = new ObservableCollection<ClarifyingQuestionItem>(result.ClarifyingQuestions.Select(x => new ClarifyingQuestionItem
            {
                AnswerType = x.AnswerType,
                SelectedCurrency = x.Currency,
                QuestionText = x.QuestionText,
                RangeFrom = x.RangeFrom,
                RangeTo = x.RangeTo,
                TextAnswer = x.TextAnswer,
                SelectedAnswer = x.SelectedAnswer,
                Options = x.Options,
            }));
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private void AddSkill()
    {
        Skills.Add(new SkillItem { IsFromCv = false });
    }

    [RelayCommand]
    private void RemoveSkill(SkillItem? item)
    {
        if (item is not null)
            Skills.Remove(item);
    }

    [RelayCommand]
    private void SaveProfile()
    {
        // stub: call IUserProfileService.SaveAsync(profile)
    }

    [RelayCommand]
    private void Cancel()
    {
        // stub: navigate to previous view or close window
    }

    partial void OnCvFileNameChanged(string value) => AnalyzeCvCommand.NotifyCanExecuteChanged();

    private void LoadDesignTimeData()
    {

        Skills.Add(new SkillItem { IsFromCv = true, SkillName = "C#", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 });
        Skills.Add(new SkillItem { IsFromCv = true, SkillName = "ASP.NET Core", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 });
        Skills.Add(new SkillItem { IsFromCv = false, SkillName = string.Empty, ProficiencyLevel = ProficiencyLevel.NotSpecified });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText = "В вашем CV не указан уровень владения английским языком. Какой у вас уровень?",
            AnswerType = AnswerType.SingleSelect,
            Options = new() { "A1 — Beginner", "A2 — Elementary", "B1 — Intermediate", "B2 — Upper-Intermediate", "C1 — Advanced", "C2 — Proficient" },
            SelectedAnswer = "B2 — Upper-Intermediate"
        });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText = "Какой формат работы вы предпочитаете — удалённо, гибрид или офис?",
            AnswerType = AnswerType.MultipleChoice,
            Options = new() { "Удалённо", "Гибрид", "Офис" },
            SelectedAnswer = "Удалённо"
        });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText = "Уточните желаемую вилку зарплаты (валюта и диапазон).",
            AnswerType = AnswerType.NumericRange,
            SelectedCurrency = "USD"
        });
    }
}
