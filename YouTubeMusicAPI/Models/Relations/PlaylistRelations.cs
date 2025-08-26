using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Playlists;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Relations;

/// <summary>
/// Represents related content for a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PlaylistRelations"/> class.
/// </remarks>
/// <param name="suggestions">The suggested items (songs/videos) to add to the source playlist.</param>
/// <param name="related">The related playlists, similar to the source playlist.</param>
public class PlaylistRelations(
    IReadOnlyList<PlaylistItem>? suggestions,
    IReadOnlyList<RelatedPlaylist> related)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="PlaylistRelations"/>.
    /// </summary>
    /// <param name="suggestionsElement">The suggestions <see cref="JElement"/> "sectionListContinuation".</param>
    /// <param name="relatedElement">The related <see cref="JElement"/> "$".</param>
    internal static PlaylistRelations Parse(
        JElement suggestionsElement,
        JElement relatedElement)
    {
        IReadOnlyList<PlaylistItem>? suggestions = suggestionsElement
            .Get("contents")
            .GetAt(0)
            .Get("musicShelfRenderer")
            .Get("contents")
            .AsArray()
            .IsNotNull(out JArray? suggestionsContents)
                ? suggestionsContents
                    .Select(item => item
                        .Get("musicResponsiveListItemRenderer"))
                    .Select(PlaylistItem.Parse)
                    .ToList()
                : null; // grrr no fluent syntax

        IReadOnlyList<RelatedPlaylist> related = relatedElement
            .Get("continuationContents")
            .Get("sectionListContinuation")
            .Get("contents")
            .GetAt(0)
            .Get("musicCarouselShelfRenderer")
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .Select(item => item
                .Get("musicTwoRowItemRenderer"))
            .Select(RelatedPlaylist.Parse)
            .ToList();

        return new(suggestions, related);
    }


    /// <summary>
    /// The suggested items to add to the source playlist.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="PlaylistInfo.IsOwner"/> is true, else <see langword="null"/>.
    /// </remarks>
    public IReadOnlyList<PlaylistItem>? Suggestions { get; } = suggestions;

    /// <summary>
    /// The related playlists, similar to the source playlist.
    /// </summary>
    public IReadOnlyList<RelatedPlaylist> Related { get; } = related;
}