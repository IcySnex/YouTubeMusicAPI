using System.Windows;
using YouTubeMusicAPI.Sample.WPF.ViewModels;

namespace YouTubeMusicAPI.Sample.WPF.Views;

public partial class SessionDetailsWindow : Window
{
    public SessionDetailsWindow(
        SessionDetailsViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}