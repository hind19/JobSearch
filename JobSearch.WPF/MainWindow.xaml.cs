using System.Windows;
using JobSearch.WPF.ViewModels;

namespace JobSearch.WPF;

public partial class MainWindow : Window
{
    public MainWindow(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
