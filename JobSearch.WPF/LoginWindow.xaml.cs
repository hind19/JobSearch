using System.Windows;
using JobSearch.WPF.ViewModels;

namespace JobSearch.WPF;

public partial class LoginWindow : Window
{
    public LoginViewModel ViewModel { get; }

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => DialogResult = result;
    }
}
