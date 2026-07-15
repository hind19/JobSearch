// JobSearch.WPF/Models/JobSiteItem.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace JobSearch.WPF.Models;

public partial class JobSiteItem : ObservableObject
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _baseUrl = string.Empty;

    [ObservableProperty]
    private bool _isActive;

    // ScrapeConfig поля — плоско, чтобы биндить напрямую в форму
    [ObservableProperty]
    private string _searchUrlTemplate = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _containerSelector = string.Empty;

    [ObservableProperty]
    private string _linkSelector = string.Empty;

    [ObservableProperty]
    private string _companySelector = string.Empty;

    [ObservableProperty]
    private string _snippetSelector = string.Empty;

    [ObservableProperty]
    private string _dateSelector = string.Empty;

    // Режим заполнения селекторов
    [ObservableProperty]
    private SelectorInputMode _selectorInputMode = SelectorInputMode.Manual;

    // Для режима "Вставить HTML"
    [ObservableProperty]
    private string _htmlInput = string.Empty;

    // Для режима "По URL"
    [ObservableProperty]
    private string _detectUrl = string.Empty;
}

public enum SelectorInputMode
{
    Manual,
    Html,
    Url
}