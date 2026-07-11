using System.Windows;
using JobSearch.WPF.Localization;

namespace JobSearch.WPF.Dialogs
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowWarning(string message, string? title = null)
            => MessageBox.Show(message, title ?? LocalizationManager.Get("Dialog_Warning"),
                MessageBoxButton.OK, MessageBoxImage.Warning);

        public MessageBoxResult ShowError(string message, string? title = null)
            => MessageBox.Show(message, title ?? LocalizationManager.Get("Dialog_Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);

        public MessageBoxResult ShowInfo(string message, string? title = null)
            => MessageBox.Show(message, title ?? LocalizationManager.Get("Dialog_Info"),
                MessageBoxButton.OK, MessageBoxImage.Information);

        public bool ShowConfirmation(string message, string? title = null)
            => MessageBox.Show(message, title ?? LocalizationManager.Get("Dialog_Confirmation"),
                MessageBoxButton.YesNo, MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }
}
