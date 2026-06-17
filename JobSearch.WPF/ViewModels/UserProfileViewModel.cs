using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.WPF.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

namespace JobSearch.WPF.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private string _cvFilePath;

    private byte[] _cvFileData;

    private IUserProfileService userProfileService;

    [ObservableProperty]
    private string _cvFileName = string.Empty;

    [ObservableProperty]
    private string _cvFileInfo = string.Empty;

    [ObservableProperty]
    private ImageSource? _cvIcon;

    public ObservableCollection<SkillItem> Skills { get; } = new();
    public ObservableCollection<ClarifyingQuestionItem> ClarifyingQuestions { get; } = new();

    public IEnumerable<ProficiencyLevel> ProficiencyLevels { get; } = Enum.GetValues<ProficiencyLevel>();

    public UserProfileViewModel()
    {
        LoadDesignTimeData();
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
                // TODO: save dialog.FileName or byte array to property as we send file to Claude only after user press the appropriate button
                // leave only one
                _cvFilePath = dialog.FileName;
                _cvFileData = File.ReadAllBytes(dialog.FileName);
                
                
                // update labels on UI
                CvFileName = dialog.SafeFileName;

                FileInfo fileInfo = new FileInfo(dialog.FileName);
                int fileSizeInKB = (int)(fileInfo.Length / (1024));
                CvFileInfo = $"Загружено {fileSizeInKB} kB ";

                // 3. Read the file into a byte array
                //byte[] fileBytes = File.ReadAllBytes(dialog.FileName);

                // 4. Convert the byte array to a Base64 string
                //string base64String = Convert.ToBase64String(fileBytes);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file in use, access denied)
                Console.WriteLine($"Error converting file: {ex.Message}");
            }
        }
    }

    private bool CanAnalyzeCv() => !string.IsNullOrEmpty(CvFileName);

    [RelayCommand(CanExecute = nameof(CanAnalyzeCv))]
    private void AnalyzeCv()
    {
        // stub: call IJobMatchService or IUserProfileService to parse CV,
        // then populate Skills and ClarifyingQuestions
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
        
        Skills.Add(new SkillItem { IsFromCv = true,  SkillName = "C#",          ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 });
        Skills.Add(new SkillItem { IsFromCv = true,  SkillName = "ASP.NET Core", ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 });
        Skills.Add(new SkillItem { IsFromCv = false, SkillName = string.Empty,   ProficiencyLevel = ProficiencyLevel.NotSpecified });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText   = "В вашем CV не указан уровень владения английским языком. Какой у вас уровень?",
            AnswerType     = AnswerType.SingleSelect,
            Options        = new() { "A1 — Beginner", "A2 — Elementary", "B1 — Intermediate", "B2 — Upper-Intermediate", "C1 — Advanced", "C2 — Proficient" },
            SelectedAnswer = "B2 — Upper-Intermediate"
        });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText   = "Какой формат работы вы предпочитаете — удалённо, гибрид или офис?",
            AnswerType     = AnswerType.MultipleChoice,
            Options        = new() { "Удалённо", "Гибрид", "Офис" },
            SelectedAnswer = "Удалённо"
        });

        ClarifyingQuestions.Add(new ClarifyingQuestionItem
        {
            QuestionText     = "Уточните желаемую вилку зарплаты (валюта и диапазон).",
            AnswerType       = AnswerType.NumericRange,
            SelectedCurrency = "USD"
        });
    }
}
