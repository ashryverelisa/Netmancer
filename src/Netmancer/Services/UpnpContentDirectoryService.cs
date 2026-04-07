using System.Xml.Linq;
using Netmancer.Models;

namespace Netmancer.Services;

public class UpnpContentDirectoryService(HttpClient httpClient) : IUpnpContentDirectoryService
{
    private static readonly XNamespace _contentDirectoryNs = "urn:schemas-upnp-org:service:ContentDirectory:1";
    private static readonly XNamespace _didlNs = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
    private static readonly XNamespace _dcNs = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace _upnpNs = "urn:schemas-upnp-org:metadata-1-0/upnp/";

    private const int MaxRetries = 2;

    /// <summary>
    /// Browses the children of the given object ID on the device at <paramref name="descriptionLocation"/>.
    /// Pass objectId "0" for the root container.
    /// </summary>
    public async Task<List<ContentItem>> BrowseAsync(Uri descriptionLocation, string objectId = "0")
    {
        var controlUrl = await GetContentDirectoryControlUrlAsync(descriptionLocation);
        if (controlUrl is null)
            return [];

        var allItems = new List<ContentItem>();
        var startIndex = 0;
        const int pageSize = 100;

        while (true)
        {
            var soapBody = $"""
                <?xml version="1.0" encoding="utf-8"?>
                <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"
                            s:encodingStyle="http://schemas.xmlsoap.org/encoding/">
                  <s:Body>
                    <u:Browse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">
                      <ObjectID>{objectId}</ObjectID>
                      <BrowseFlag>BrowseDirectChildren</BrowseFlag>
                      <Filter>*</Filter>
                      <StartingIndex>{startIndex}</StartingIndex>
                      <RequestedCount>{pageSize}</RequestedCount>
                      <SortCriteria></SortCriteria>
                    </u:Browse>
                  </s:Body>
                </s:Envelope>
                """;

            var responseBody = await SendSoapRequestWithRetryAsync(controlUrl, soapBody);
            if (responseBody is null)
                break;

            var (items, numberReturned, totalMatches) = ParseBrowseResponse(responseBody);
            allItems.AddRange(items);

            // If the server didn't report totals, or we've fetched everything, stop
            if (numberReturned == 0 || totalMatches == 0)
                break;

            startIndex += numberReturned;

            if (startIndex >= totalMatches)
                break;
        }

        return allItems;
    }

    /// <summary>
    /// Sends a SOAP request to the given <paramref name="controlUrl"/> with
    /// automatic retry for transient HTTP errors (e.g. premature connection close).
    /// </summary>
    private async Task<string?> SendSoapRequestWithRetryAsync(Uri controlUrl, string soapBody)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, controlUrl)
                {
                    Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml")
                };
                request.Headers.Add("SOAPAction",
                    "\"urn:schemas-upnp-org:service:ContentDirectory:1#Browse\"");

                using var response = await httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                // Transient failure — wait briefly, then retry
                await Task.Delay(250 * (attempt + 1));
            }
            catch (TaskCanceledException) when (attempt < MaxRetries)
            {
                await Task.Delay(250 * (attempt + 1));
            }
        }

        return null;
    }

    private async Task<Uri?> GetContentDirectoryControlUrlAsync(Uri descriptionLocation)
    {
        try
        {
            var xml = await httpClient.GetStringAsync(descriptionLocation);
            var doc = XDocument.Parse(xml);

            XNamespace deviceNs = "urn:schemas-upnp-org:device-1-0";

            var service = doc.Descendants(deviceNs + "service")
                .FirstOrDefault(s =>
                    s.Element(deviceNs + "serviceType")?.Value
                        .Contains("ContentDirectory") == true);

            var controlPath = service?.Element(deviceNs + "controlURL")?.Value;
            if (controlPath is null)
                return null;

            if (Uri.TryCreate(controlPath, UriKind.Absolute, out var absoluteUri)
                && absoluteUri.Scheme is "http" or "https")
                return absoluteUri;

            var baseUri = new Uri($"{descriptionLocation.Scheme}://{descriptionLocation.Authority}");
            return new Uri(baseUri, controlPath);
        }
        catch
        {
            return null;
        }
    }

    internal (List<ContentItem> Items, int NumberReturned, int TotalMatches) ParseBrowseResponse(string responseXml)
    {
        var items = new List<ContentItem>();
        var numberReturned = 0;
        var totalMatches = 0;

        try
        {
            var doc = XDocument.Parse(responseXml);

            var browseResponse = doc.Descendants(_contentDirectoryNs + "BrowseResponse").FirstOrDefault();
            if (browseResponse is null)
                return (items, 0, 0);

            // Extract pagination metadata
            var numberReturnedEl = browseResponse.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "NumberReturned");
            var totalMatchesEl = browseResponse.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "TotalMatches");

            _ = int.TryParse(numberReturnedEl?.Value, out numberReturned);
            _ = int.TryParse(totalMatchesEl?.Value, out totalMatches);

            var resultElement = browseResponse.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Result");

            if (resultElement is null)
                return (items, numberReturned, totalMatches);

            var didl = XDocument.Parse(resultElement.Value);

            // Parse containers (folders)
            items.AddRange(didl.Descendants(_didlNs + "container")
            .Select(container => new ContentItem
            {
                Id = container.Attribute("id")?.Value ?? string.Empty,
                ParentId = container.Attribute("parentID")?.Value ?? string.Empty,
                Title = container.Element(_dcNs + "title")?.Value ?? "(unknown)",
                IsContainer = true,
                MediaClass = container.Element(_upnpNs + "class")?.Value ?? string.Empty
            }));

            // Parse items (media files)
            items.AddRange(didl.Descendants(_didlNs + "item")
            .Select(item => new ContentItem
            {
                Id = item.Attribute("id")?.Value ?? string.Empty,
                ParentId = item.Attribute("parentID")?.Value ?? string.Empty,
                Title = item.Element(_dcNs + "title")?.Value ?? "(unknown)",
                IsContainer = false,
                ResourceUrl = item.Element(_didlNs + "res")?.Value,
                Artist = item.Element(_upnpNs + "artist")?.Value
                         ?? item.Element(_dcNs + "creator")?.Value,
                AlbumArtUri = item.Element(_upnpNs + "albumArtURI")?.Value,
                MediaClass = item.Element(_upnpNs + "class")?.Value ?? string.Empty
            }));
        }
        catch
        {
            // Malformed response; return whatever we have
        }

        return (items, numberReturned, totalMatches);
    }
}

