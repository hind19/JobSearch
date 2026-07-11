using System.Windows;

namespace JobSearch.WPF.Dialogs
{
    public interface IDialogService
    {
        MessageBoxResult ShowWarning(string message, string? title = null);
        MessageBoxResult ShowError(string message, string? title = null);
        MessageBoxResult ShowInfo(string message, string? title = null);
        bool ShowConfirmation(string message, string? title = null);
    }
}
