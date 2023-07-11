using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace HW5.Pages;

public class XMLpageModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public XMLpageModel(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    public List<Item> Items { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        string feed = Request.Query["message"];
        await renderXml(feed);
        return Page();
    }

    public async Task renderXml(string url)
    {
        //string url = "http://scripting.com/rss.xml";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        string XMLcontent = await response.Content.ReadAsStringAsync();

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(XMLcontent);
        XmlElement? root = xmlDocument.DocumentElement;
        XmlNodeList nodes = root.GetElementsByTagName("item");
        List<Item> itemsList = new List<Item>();

        foreach (XmlNode node in nodes)
        {
            XmlNode? TitleTag = node.SelectSingleNode("title");
            string Title = (TitleTag != null) ? TitleTag.InnerText : "";

            XmlNode? DescriptionTag = node.SelectSingleNode("description");
            string htmlCode = (DescriptionTag != null) ? DescriptionTag.InnerText : "";
            HtmlString Description = new HtmlString(htmlCode);

            XmlNode? PubDateTag = node.SelectSingleNode("pubDate");
            string PubDate = (PubDateTag != null) ? PubDateTag.InnerText : "";
            var date = DateTime.Parse(PubDate);

            XmlNode? LinkTag = node.SelectSingleNode("link");
            string Link = (LinkTag != null) ? LinkTag.InnerText : "";

            Item newItem = new Item()
            {
                Title = Title,
                Description = Description,
                PubDate = date,
                Link = Link
            };

            itemsList.Add(newItem);
        }

        Items = itemsList;


    }
}
public class Item
{
    public string? Title { get; set; }
    public HtmlString? Description { get; set; }
    public DateTime PubDate { get; set; }
    public string? Link { get; set; }
}
