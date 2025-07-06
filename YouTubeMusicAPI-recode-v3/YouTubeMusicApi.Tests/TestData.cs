using System.Net;

namespace YouTubeMusicApi.Tests;

internal static class TestData
{
    public const string GeographicalLocation = "US";


    public static string RandomString() =>
        Guid.NewGuid().ToString();

    public static Cookie[] RandomCookies() =>
        [
            new Cookie("__Secure-3PAPISID", RandomString()) { Domain = ".youtube.com" },
            new Cookie("SAPISID", RandomString()) { Domain = ".youtube.com" },
            new Cookie("SSID", RandomString()) { Domain = ".youtube.com" }
            //...
        ];
}