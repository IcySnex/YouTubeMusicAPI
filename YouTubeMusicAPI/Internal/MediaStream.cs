// ----------------------------------------------
// This class was heavily inspired from 'YouTubeExplode' project by Tyrrrz.
// https://github.com/Tyrrrz/YoutubeExplode/blob/30ba3f21c0449967dbc41c5d43fe478851ef60d5/YoutubeExplode/Videos/Streams/MediaStream.cs
//
// Originally licensed under the MIT License: https://github.com/Tyrrrz/YoutubeExplode/blob/30ba3f21c0449967dbc41c5d43fe478851ef60d5/License.txt
//
// Modifications contain adapting to custom 'MediaStreamInfo' model, simplifying helper methods and adding XML documentation.
// ----------------------------------------------

using System.Collections.Specialized;
using System.Web;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Provides a generic view of a sequence of bytes for a YouTube Music Media song or video
/// </summary>
/// <param name="streamInfo">The info about this media stream</param>
internal sealed class MediaStream(
    MediaStreamInfo streamInfo) : Stream
{
    static readonly HttpClient Client = new();


    readonly long segmentLength = !string.Equals(
        HttpUtility.ParseQueryString(streamInfo.Url).Get("ratebypass"),
        "yes",
        StringComparison.InvariantCultureIgnoreCase) ? 9898989 : streamInfo.ContentLenght;


    Stream? segmentStream = null;
    long actualPosition = 0;


    /// <summary>
    /// true if the stream supports reading; otherwise, false
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// true if the stream supports seeking; otherwise, false
    /// </summary>
    public override bool CanSeek => true;

    /// <summary>
    /// true if the stream supports writing; otherwise, false
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// A long value representing the length of the stream in bytes
    /// </summary>
    public override long Length => streamInfo.ContentLenght;

    /// <summary>
    /// The current position within the stream
    /// </summary>
    public override long Position { get; set; }


    void ResetSegment()
    {
        segmentStream?.Dispose();
        segmentStream = null;
    }

    string GetSegmentUrl(
        string streamUrl,
        long from,
        long to)
    {
        Uri uri = new(streamUrl);

        NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
        query["range"] = $"{from}-{to}";

        return new UriBuilder(uri)
        {
            Query = query.ToString()
        }.ToString();
    }


    async ValueTask<Stream> ResolveSegmentAsync(
        CancellationToken cancellationToken = default)
    {
        if (segmentStream is not null)
            return segmentStream;

        string url = GetSegmentUrl(streamInfo.Url, Position, Position + segmentLength - 1);
        HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();
        Stream stream = await response.Content.ReadAsStreamAsync();

        return segmentStream = stream;
    }

    async ValueTask<int> ReadSegmentAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken = default)
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            try
            {
                Stream stream = await ResolveSegmentAsync(cancellationToken);
                return await stream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException && retriesRemaining > 0)
            {
                ResetSegment();
            }
        }
    }


    /// <summary>
    /// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read
    /// </summary>
    /// <param name="buffer">The buffer to write the data into</param>
    /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream</param>
    /// <param name="count">The maximum number of bytes to read</param>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>The total number of bytes read into the buffer</returns>
    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            long requestedPosition = Position;

            if (actualPosition != requestedPosition)
                ResetSegment();

            if (requestedPosition >= Length)
                return 0;

            int bytesRead = await ReadSegmentAsync(buffer, offset, count, cancellationToken);
            Position = actualPosition = requestedPosition + bytesRead;

            if (bytesRead > 0)
                return bytesRead;

            ResetSegment();
        }
    }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read
    /// </summary>
    /// <param name="buffer">The buffer to write the data into</param>
    /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream</param>
    /// <param name="count">The maximum number of bytes to read</param>
    /// <returns>The total number of bytes read into the buffer</returns>
    public override int Read(
        byte[] buffer,
        int offset,
        int count) =>
        ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    /// <summary>
    /// writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written
    /// </summary>
    /// <param name="buffer">An array of bytes</param>
    /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream</param>
    /// <param name="count">The number of bytes to be written to the current stream</param>
    public override void Write(
        byte[] buffer,
        int offset,
        int count) =>
        throw new NotSupportedException();

    /// <summary>
    /// Sets the length of the current stream
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes</param>
    public override void SetLength(
        long value) =>
        throw new NotSupportedException();

    /// <summary>
    /// When overridden in a derived class, sets the position within the current stream
    /// </summary>
    /// <param name="offset">A byte offset relative to the origin parameter</param>
    /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position</param>
    /// <returns>The new position within the current stream</returns>
    public override long Seek(
        long offset,
        SeekOrigin origin) =>
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };

    /// <summary>
    /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device
    /// </summary>
    public override void Flush() =>
        throw new NotSupportedException();

    /// <summary>
    /// Releases all resources used by the stream
    /// </summary>
    /// <param name="disposing">Weither is disposing or not</param>
    protected override void Dispose(
        bool disposing)
    {
        if (disposing)
            ResetSegment();

        base.Dispose(disposing);
    }


    /// <summary>
    /// Initilizes the media stream and resolves the first segment
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this action</param>
    public async Task InitializeAsync(
        CancellationToken cancellationToken = default) =>
        await ResolveSegmentAsync(cancellationToken);
}