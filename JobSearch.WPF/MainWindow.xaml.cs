using System.Windows;
using JobSearch.WPF.ViewModels;

namespace JobSearch.WPF;

public partial class MainWindow : Window
{
    public UserProfileViewModel ViewModel { get; }

    public MainWindow(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
