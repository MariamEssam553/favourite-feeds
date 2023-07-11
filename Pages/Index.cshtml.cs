using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace HW5.Pages;

public class IndexModel : PageModel
{
    public List<FeedItem> items4page { get; set; } = new();
    public List<FeedItem> outlines { get; set; } = new();

    private readonly IHttpClientFactory _httpClientFactory;
    public IndexModel(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;
    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 12)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

        string OPMLcontent = await response.Content.ReadAsStringAsync();

        XmlDocument opmlDocument = new XmlDocument();
        opmlDocument.LoadXml(OPMLcontent);
        XmlElement? root = opmlDocument.DocumentElement;
        XmlNodeList nodes = root.GetElementsByTagName("outline");
        List<FeedItem> itemsList = new List<FeedItem>();

        int totalItems = nodes?.Count ?? 0;
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = Math.Min(startIndex + pageSize, nodes.Count);


        foreach (XmlNode node in nodes)
        {
            string Text = node.Attributes["text"].Value ?? "";
            string link = node.Attributes["xmlUrl"].Value ?? "";

            FeedItem newItem = new FeedItem()
            {
                Text = Text,
                XmlLink = link
            };

            itemsList.Add(newItem);
        }

        outlines = itemsList;

        // Assuming 'allItems' is the complete list of items
        var itemsForPage = outlines.Skip(startIndex).Take(endIndex - startIndex).ToList();

        items4page = itemsForPage;

        ViewData["PageNumber"] = pageNumber;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalItems"] = totalItems;
        ViewData["TotalPages"] = totalPages;

    }

}

public class FeedItem
{
    public string? Text { get; set; }
    public string? XmlLink { get; set; }
    public string IsFavorite { get; set; } = "False";
}


