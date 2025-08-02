using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Streaming;
using YouTubeMusicAPI.Sample.WPF.Logging;
using YouTubeSessionGenerator;
using YouTubeSessionGenerator.Js.Environments;

namespace YouTubeMusicAPI.Sample.WPF.ViewModels;

partial class MainViewModel : ObservableObject
{
    readonly ILogger logger;
    YouTubeMusicClient client;

    public MainViewModel()
    {
        logger = new CollectionLogger(LogMessages);
        client = new(logger);
    }


    public ObservableCollection<string> LogMessages { get; } = [];


    [ObservableProperty]
    bool isSessionAuthenticated;

    [RelayCommand]
    async Task ToggleAuthenticationAsync()
    {
        if (IsSessionAuthenticated)
        {
            client = new(logger);
            IsSessionAuthenticated = false;

            return;
        }

        // Get Cookies
        AuthenticateViewModel authenticateViewModel = new(logger);

        IReadOnlyList<Cookie> cookies = await authenticateViewModel.PromptAsync();
        if (cookies.Count < 1)
        {
            client = new(logger);
            IsSessionAuthenticated = false;

            return;
        }

        CookieContainer cookieContainer = new();
        foreach (Cookie cookie in cookies)
            cookieContainer.Add(cookie);

        string headerCookies = cookieContainer.GetCookieHeader(new("https://music.youtube.com"));

        // Get visitorData & poToken
        using HttpClient httpClient = new(new HttpClientHandler() { CookieContainer = cookieContainer });
        using NodeEnvironment jsEnvironment = new();

        YouTubeSessionCreator sessionCreator = new(new()
        {
            Logger = logger,
            HttpClient = httpClient,
            JsEnvironment = jsEnvironment
        });

        string visitorData = await sessionCreator.VisitorDataAsync();
        string poToken = await sessionCreator.ProofOfOriginTokenAsync(visitorData);

        client = new(logger, "US", visitorData, poToken, cookies);
        IsSessionAuthenticated = true;
    }


    [ObservableProperty]
    SongVideoInfo? songVideo = null;

    [RelayCommand]
    async Task SearchSongVideoAsync(
        string id)
    {
        try
        {
            SongVideo = await client.GetSongVideoInfoAsync(id);
        }
        catch (Exception ex)
        {
            SongVideo = null;
            MessageBox.Show(ex.Message, "Failed to search for song/video!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    [ObservableProperty]
    StreamingData? streamingData = null;

    AudioStreamInfo? highestAudioStreamInfo = null;

    [RelayCommand]
    async Task GetStreamingDataAsync()
    {
        if (SongVideo is null)
        {
            MessageBox.Show("Please search for a song/video first.", "Failed to get streaming data!", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StreamingData = await client.GetStreamingDataAsync(SongVideo.Id);

            highestAudioStreamInfo = StreamingData.StreamInfo
              .OfType<AudioStreamInfo>()
              .OrderByDescending(info => info.Bitrate)
              .First();
        }
        catch (Exception ex)
        {
            StreamingData = null;
            highestAudioStreamInfo = null;
            MessageBox.Show(ex.Message, "Failed to get streaming data!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    async Task DownloadAsync()
    {
        if (highestAudioStreamInfo is null)
        {
            MessageBox.Show("Please get streaming data first.", "Failed to download stream!", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string format = highestAudioStreamInfo.Container.Format;

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Download Stream",
                Filter = $"{format.ToUpper()} files (*.{format})|*.{format}|All files (*.*)|*.*",
                FileName = $"stream.{format}",
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() != true)
                return;

            Stream stream = await highestAudioStreamInfo.GetStreamAsync();

            logger.LogInformation("[MainViewModel-DownloadAsync] Saving stream to '{fileName}'", saveFileDialog.FileName);
            using FileStream fileStream = File.Create(saveFileDialog.FileName);
            await stream.CopyToAsync(fileStream);

            MessageBox.Show("Download completed successfully.", "Stream download finished!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Failed to download stream!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}