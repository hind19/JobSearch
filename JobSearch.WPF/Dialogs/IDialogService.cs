using System.Windows;

namespace JobSearch.WPF.Dialogs
{
    public interface IDialogService
    {
        //TODO: Move text to constants
        MessageBoxResult ShowWarning(string message, string title = "Предупреждение");
        MessageBoxResult ShowError(string message, string title = "Ошибка");
        MessageBoxResult ShowInfo(string message, string title = "Информация");
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}
