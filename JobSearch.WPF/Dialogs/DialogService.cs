using System.Windows;

namespace JobSearch.WPF.Dialogs
{
    public class DialogService : IDialogService
    {
        //TODO: MOve text to constants
        public MessageBoxResult ShowWarning(string message, string title = "Предупреждение")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public MessageBoxResult ShowError(string message, string title = "Ошибка")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public MessageBoxResult ShowInfo(string message, string title = "Информация")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public bool ShowConfirmation(string message, string title = "Подтверждение")
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }
}
