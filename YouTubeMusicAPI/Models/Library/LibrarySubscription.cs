namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library subscription
/// </summary>
/// <param name="name">The name of this subscription</param>
/// <param name="id">The id of this subscription</param>
/// <param name="subscribersInfo">The subscribers info of the subscription</param>
/// <param name="radio">The radio channel of this subscription</param>
/// <param name="thumbnails">The thumbnails of this subscription</param>
public class LibrarySubscription(
    string name,
    string id,
    string? subscribersInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : LibraryEntity(name, id, thumbnails)
{
    /// <summary>
    /// The subscribers info of the subscription
    /// </summary>
    public string? SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// The radio channel of this subscription
    /// </summary>
    public Radio? Radio { get; } = radio;
}