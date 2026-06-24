using CommunityToolkit.Mvvm.ComponentModel;
using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.WPF.Models;

public partial class SkillItem : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourceLabel))]
    private bool _isFromCv;

    [ObservableProperty]
    private string _skillName = string.Empty;

    [ObservableProperty]
    private ProficiencyLevel _proficiencyLevel = ProficiencyLevel.NotSpecified;

    [ObservableProperty]
    private int? _yearsOfExperience;

    public string SourceLabel => IsFromCv ? "CV" : "Вручную";
}
