using System.Net;
using System.Text;

namespace Netmancer.UnitTests.Services;

/// <summary>
/// Shared test helpers and XML builders for UpnpContentDirectoryService tests.
/// </summary>
internal static class UpnpTestHelpers
{
    /// <summary>
    /// A fake HttpMessageHandler that returns pre-configured responses.
    /// </summary>
    internal sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        private readonly List<HttpRequestMessage> _sentRequests = [];

        public IReadOnlyList<HttpRequestMessage> SentRequests => _sentRequests;

        public void EnqueueResponse(HttpResponseMessage response) => _responses.Enqueue(response);

        public void EnqueueResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            EnqueueResponse(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/xml")
            });
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _sentRequests.Add(request);
            return Task.FromResult(_responses.Dequeue());
        }
    }

    public static string BuildDeviceDescriptionXml(string controlUrl) =>
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <root xmlns="urn:schemas-upnp-org:device-1-0">
          <device>
            <friendlyName>Test Media Server</friendlyName>
            <serviceList>
              <service>
                <serviceType>urn:schemas-upnp-org:service:ContentDirectory:1</serviceType>
                <controlURL>{controlUrl}</controlURL>
              </service>
            </serviceList>
          </device>
        </root>
        """;

    public static string BuildBrowseResponseXml(string didlContent, int numberReturned, int totalMatches) =>
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"
                    s:encodingStyle="http://schemas.xmlsoap.org/encoding/">
          <s:Body>
            <u:BrowseResponse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">
              <Result>{EscapeXml(didlContent)}</Result>
              <NumberReturned>{numberReturned}</NumberReturned>
              <TotalMatches>{totalMatches}</TotalMatches>
              <UpdateID>1</UpdateID>
            </u:BrowseResponse>
          </s:Body>
        </s:Envelope>
        """;

    public static string EscapeXml(string xml) =>
        xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    public static string BuildDidlWithContainers(params (string id, string parentId, string title, string upnpClass)[] containers)
    {
        var sb = new StringBuilder();
        sb.Append("""<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/">""");
        foreach (var (id, parentId, title, upnpClass) in containers)
        {
            sb.Append($"""<container id="{id}" parentID="{parentId}"><dc:title>{title}</dc:title><upnp:class>{upnpClass}</upnp:class></container>""");
        }
        sb.Append("</DIDL-Lite>");
        return sb.ToString();
    }

    public static string BuildDidlWithItems(params (string id, string parentId, string title, string upnpClass, string? resUrl)[] items)
    {
        var sb = new StringBuilder();
        sb.Append("""<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/">""");
        foreach (var (id, parentId, title, upnpClass, resUrl) in items)
        {
            sb.Append($"""<item id="{id}" parentID="{parentId}"><dc:title>{title}</dc:title><upnp:class>{upnpClass}</upnp:class>""");
            if (resUrl is not null)
                sb.Append($"""<res>{resUrl}</res>""");
            sb.Append("</item>");
        }
        sb.Append("</DIDL-Lite>");
        return sb.ToString();
    }

    public static string BuildDidlMixed(
        (string id, string parentId, string title, string upnpClass)[] containers,
        (string id, string parentId, string title, string upnpClass, string? resUrl)[] items)
    {
        var sb = new StringBuilder();
        sb.Append("""<DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/">""");
        foreach (var (id, parentId, title, upnpClass) in containers)
            sb.Append($"""<container id="{id}" parentID="{parentId}"><dc:title>{title}</dc:title><upnp:class>{upnpClass}</upnp:class></container>""");
        foreach (var (id, parentId, title, upnpClass, resUrl) in items)
        {
            sb.Append($"""<item id="{id}" parentID="{parentId}"><dc:title>{title}</dc:title><upnp:class>{upnpClass}</upnp:class>""");
            if (resUrl is not null)
                sb.Append($"""<res>{resUrl}</res>""");
            sb.Append("</item>");
        }
        sb.Append("</DIDL-Lite>");
        return sb.ToString();
    }
}

