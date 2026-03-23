using Netmancer.Services;
using NSubstitute;
using static Netmancer.UnitTests.Services.UpnpTestHelpers;

namespace Netmancer.UnitTests.Services;

public class ParseBrowseResponseTests
{
    private readonly HttpClient _httpClient = Substitute.For<HttpClient>();

    [Fact]
    public void WithContainers_ReturnsCorrectItems()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var didl = BuildDidlWithContainers(
            ("1", "0", "Music", "object.container"),
            ("2", "0", "Video", "object.container"));
        var xml = BuildBrowseResponseXml(didl, 2, 2);

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Equal(2, items.Count);
        Assert.Equal(2, numberReturned);
        Assert.Equal(2, totalMatches);

        Assert.Equal("1", items[0].Id);
        Assert.Equal("0", items[0].ParentId);
        Assert.Equal("Music", items[0].Title);
        Assert.True(items[0].IsContainer);
        Assert.Equal("object.container", items[0].MediaClass);

        Assert.Equal("2", items[1].Id);
        Assert.Equal("Video", items[1].Title);
        Assert.True(items[1].IsContainer);
    }

    [Fact]
    public void WithMediaItems_ReturnsCorrectItems()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var didl = BuildDidlWithItems(
            ("10", "1", "Song.mp3", "object.item.audioItem.musicTrack", "http://server/song.mp3"),
            ("11", "1", "Movie.mp4", "object.item.videoItem", "http://server/movie.mp4"));
        var xml = BuildBrowseResponseXml(didl, 2, 2);

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Equal(2, items.Count);

        Assert.Equal("10", items[0].Id);
        Assert.Equal("1", items[0].ParentId);
        Assert.Equal("Song.mp3", items[0].Title);
        Assert.False(items[0].IsContainer);
        Assert.Equal("http://server/song.mp3", items[0].ResourceUrl);
        Assert.Equal("object.item.audioItem.musicTrack", items[0].MediaClass);

        Assert.Equal("11", items[1].Id);
        Assert.Equal("Movie.mp4", items[1].Title);
        Assert.Equal("http://server/movie.mp4", items[1].ResourceUrl);
    }

    [Fact]
    public void WithMixedContent_ReturnsContainersAndItems()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var didl = BuildDidlMixed(
            containers: [("1", "0", "Albums", "object.container.album")],
            items: [("20", "1", "Track.flac", "object.item.audioItem", "http://server/track.flac")]);
        var xml = BuildBrowseResponseXml(didl, 2, 2);

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Equal(2, items.Count);
        Assert.True(items[0].IsContainer);
        Assert.False(items[1].IsContainer);
    }

    [Fact]
    public void WithItemMissingResUrl_ResourceUrlIsNull()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var didl = BuildDidlWithItems(("10", "1", "NoResource", "object.item", null));
        var xml = BuildBrowseResponseXml(didl, 1, 1);

        var (items, _, _) = service.ParseBrowseResponse(xml);

        Assert.Single(items);
        Assert.Null(items[0].ResourceUrl);
    }

    [Fact]
    public void WithEmptyResult_ReturnsEmptyList()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var didl = """<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"></DIDL-Lite>""";
        var xml = BuildBrowseResponseXml(didl, 0, 0);

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Empty(items);
        Assert.Equal(0, numberReturned);
        Assert.Equal(0, totalMatches);
    }

    [Fact]
    public void WithMalformedXml_ReturnsEmptyList()
    {
        var service = new UpnpContentDirectoryService(_httpClient);

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse("not xml at all");

        Assert.Empty(items);
        Assert.Equal(0, numberReturned);
        Assert.Equal(0, totalMatches);
    }

    [Fact]
    public void WithNoBrowseResponseElement_ReturnsEmptyList()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
              <s:Body>
                <SomeOtherResponse/>
              </s:Body>
            </s:Envelope>
            """;

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Empty(items);
        Assert.Equal(0, numberReturned);
        Assert.Equal(0, totalMatches);
    }

    [Fact]
    public void WithMissingResultElement_ReturnsEmptyListButCorrectPagination()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        const string xml = """
                           <?xml version="1.0" encoding="utf-8"?>
                           <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"
                                       s:encodingStyle="http://schemas.xmlsoap.org/encoding/">
                             <s:Body>
                               <u:BrowseResponse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">
                                 <NumberReturned>5</NumberReturned>
                                 <TotalMatches>10</TotalMatches>
                               </u:BrowseResponse>
                             </s:Body>
                           </s:Envelope>
                           """;

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Empty(items);
        Assert.Equal(5, numberReturned);
        Assert.Equal(10, totalMatches);
    }

    [Fact]
    public void WithMissingAttributes_UsesDefaults()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        const string didl = """<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"><container></container></DIDL-Lite>""";
        var xml = BuildBrowseResponseXml(didl, 1, 1);

        var (items, _, _) = service.ParseBrowseResponse(xml);

        Assert.Single(items);
        Assert.Equal(string.Empty, items[0].Id);
        Assert.Equal(string.Empty, items[0].ParentId);
        Assert.Equal("(unknown)", items[0].Title);
        Assert.Equal(string.Empty, items[0].MediaClass);
    }

    [Fact]
    public void WithNonNumericPagination_DefaultsToZero()
    {
        var service = new UpnpContentDirectoryService(_httpClient);
        const string xml = """
                           <?xml version="1.0" encoding="utf-8"?>
                           <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"
                                       s:encodingStyle="http://schemas.xmlsoap.org/encoding/">
                             <s:Body>
                               <u:BrowseResponse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">
                                 <Result>&lt;DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"&gt;&lt;/DIDL-Lite&gt;</Result>
                                 <NumberReturned>abc</NumberReturned>
                                 <TotalMatches>xyz</TotalMatches>
                               </u:BrowseResponse>
                             </s:Body>
                           </s:Envelope>
                           """;

        var (items, numberReturned, totalMatches) = service.ParseBrowseResponse(xml);

        Assert.Empty(items);
        Assert.Equal(0, numberReturned);
        Assert.Equal(0, totalMatches);
    }
}

