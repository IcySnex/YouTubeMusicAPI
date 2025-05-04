using YouTubeMusicAPI.Utils;

namespace YouTubeMusicApi.Tests.Utils;

[TestFixture]
internal sealed class UrlTests
{
    [Test]
    [TestCase("https://api.example.com/endpoint", "key1", ExpectedResult = "https://api.example.com/endpoint")]
    [TestCase("https://api.example.com/endpoint?key1=value1", "key1", ExpectedResult = "https://api.example.com/endpoint")]
    [TestCase("https://api.example.com/endpoint?key1=value1&key2=value2", "key1", ExpectedResult = "https://api.example.com/endpoint?key2=value2")]
    [TestCase("https://api.example.com/endpoint?key1=value1&key2=value2", "key2", ExpectedResult = "https://api.example.com/endpoint?key1=value1")]
    [TestCase("https://api.example.com/endpoint?key1=value1&key2=value2&key3=value3", "key2", ExpectedResult = "https://api.example.com/endpoint?key1=value1&key3=value3")]
    public string Should_remove_parameter_from_query(
        string url,
        string key) =>
        Url.RemoveQueryParameter(url, key);

    [Test]
    [TestCase("https://api.example.com/endpoint", "key1", "value1", ExpectedResult = "https://api.example.com/endpoint?key1=value1")]
    [TestCase("https://api.example.com/endpoint?key1=value1", "key1", "newValue1", ExpectedResult = "https://api.example.com/endpoint?key1=newValue1")]
    [TestCase("https://api.example.com/endpoint?key1=value1&key2=value2", "key1", "newValue1", ExpectedResult = "https://api.example.com/endpoint?key2=value2&key1=newValue1")]
    [TestCase("https://api.example.com/endpoint?key1=value1&key2=value2", "key2", "newValue2", ExpectedResult = "https://api.example.com/endpoint?key1=value1&key2=newValue2")]
    public string Should_add_or_replace_parameter_from_query(
        string url,
        string key,
        string value) =>
        Url.SetQueryParameter(url, key, value);
}
