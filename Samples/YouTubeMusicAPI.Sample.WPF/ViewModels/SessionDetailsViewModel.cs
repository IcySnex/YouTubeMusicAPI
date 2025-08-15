using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace YouTubeMusicAPI.Sample.WPF.ViewModels;

public partial class SessionDetailsViewModel(
    string cookies,
    string visitorData,
    string poToken) : ObservableObject
{
    public string Cookies { get; } = cookies;

    public string VisitorData { get; } = visitorData;

    public string PoToken { get; } = poToken;


    [RelayCommand]
    void Copy(
        string text)
    {
        Clipboard.SetText(text);

        MessageBox.Show("Copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}