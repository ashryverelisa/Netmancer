using System.Net;
using Netmancer.Services;
using static Netmancer.UnitTests.Services.UpnpTestHelpers;

namespace Netmancer.UnitTests.Services;

public class BrowseAsyncTests
{
    [Fact]
    public async Task ReturnsItemsFromServer()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var didl = BuildDidlWithContainers(("1", "0", "Music", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(didl, 1, 1));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"), "0");

        Assert.Single(result);
        Assert.Equal("Music", result[0].Title);
        Assert.True(result[0].IsContainer);
    }

    [Fact]
    public async Task WithPagination_FetchesAllPages()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var page1 = BuildDidlWithContainers(
            ("1", "0", "Folder1", "object.container"),
            ("2", "0", "Folder2", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(page1, 2, 3));

        var page2 = BuildDidlWithContainers(
            ("3", "0", "Folder3", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(page2, 1, 3));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Equal(3, result.Count);
        Assert.Equal("Folder1", result[0].Title);
        Assert.Equal("Folder2", result[1].Title);
        Assert.Equal("Folder3", result[2].Title);
    }

    [Fact]
    public async Task WhenNoContentDirectoryService_ReturnsEmptyList()
    {
        var handler = new FakeHttpMessageHandler();

        var descriptionXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <root xmlns="urn:schemas-upnp-org:device-1-0">
              <device>
                <friendlyName>Some Device</friendlyName>
                <serviceList>
                  <service>
                    <serviceType>urn:schemas-upnp-org:service:RenderingControl:1</serviceType>
                    <controlURL>/ctrl/Rendering</controlURL>
                  </service>
                </serviceList>
              </device>
            </root>
            """;
        handler.EnqueueResponse(descriptionXml);

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Empty(result);
    }

    [Fact]
    public async Task WhenServerReturnsEmpty_ReturnsEmptyList()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var emptyDidl = """<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"></DIDL-Lite>""";
        handler.EnqueueResponse(BuildBrowseResponseXml(emptyDidl, 0, 0));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Empty(result);
    }

    [Fact]
    public async Task WithAbsoluteControlUrl_UsesItDirectly()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("http://192.168.1.100:8200/ctrl/ContentDirectory"));

        var didl = BuildDidlWithContainers(("1", "0", "Root", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(didl, 1, 1));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Single(result);
        Assert.Equal(
            new Uri("http://192.168.1.100:8200/ctrl/ContentDirectory"),
            handler.SentRequests[1].RequestUri);
    }

    [Fact]
    public async Task WithRelativeControlUrl_ResolvesAgainstBaseUri()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var didl = BuildDidlWithContainers(("1", "0", "Root", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(didl, 1, 1));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Single(result);
        Assert.Equal(
            new Uri("http://192.168.1.100:8200/ctrl/ContentDirectory"),
            handler.SentRequests[1].RequestUri);
    }

    [Fact]
    public async Task SendsCorrectSoapAction()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var didl = BuildDidlWithContainers(("1", "0", "Root", "object.container"));
        handler.EnqueueResponse(BuildBrowseResponseXml(didl, 1, 1));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        var browseRequest = handler.SentRequests[1];
        Assert.Equal(HttpMethod.Post, browseRequest.Method);
        Assert.Contains(
            "\"urn:schemas-upnp-org:service:ContentDirectory:1#Browse\"",
            browseRequest.Headers.GetValues("SOAPAction"));
    }

    [Fact]
    public async Task PassesCorrectObjectId()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(BuildDeviceDescriptionXml("/ctrl/ContentDirectory"));

        var didl = BuildDidlWithItems(("42", "7", "Track.mp3", "object.item.audioItem", "http://server/track.mp3"));
        handler.EnqueueResponse(BuildBrowseResponseXml(didl, 1, 1));

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"), "7");

        var browseRequest = handler.SentRequests[1];
        var body = await browseRequest.Content!.ReadAsStringAsync();
        Assert.Contains("<ObjectID>7</ObjectID>", body);
    }

    [Fact]
    public async Task WhenDescriptionFetchFails_ReturnsEmptyList()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Server Error")
        });

        using var httpClient = new HttpClient(handler);
        var service = new UpnpContentDirectoryService(httpClient);

        var result = await service.BrowseAsync(new Uri("http://192.168.1.100:8200/rootDesc.xml"));

        Assert.Empty(result);
    }
}

