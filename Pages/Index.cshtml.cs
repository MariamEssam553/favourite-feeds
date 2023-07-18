using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace HW5.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItem> Outlines { get; set; } = new List<FeedItem>();
    public List<FeedItem> ItemsForPage { get; set; } = new List<FeedItem>();
    public int PageSize { get; set; } = 12;
    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private async Task<List<FeedItem>> GetOutlinesAsync(int page)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");
        XmlDocument opmlDocument = new XmlDocument();

        if (!Request.Cookies.ContainsKey("favFeeds"))
        {
            var serializedFavFeeds = JsonSerializer.Serialize(new List<FeedItem>());
            Response.Cookies.Append("favFeeds", serializedFavFeeds, new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.Now.AddMinutes(10)
            });
        }

        string OPMLcontent = await response.Content.ReadAsStringAsync();
        opmlDocument.LoadXml(OPMLcontent);
        var nodes = opmlDocument.SelectNodes("opml/body/outline");

        //var nodesCount = nodes.Count;
        //var startIndex = (page - 1) * PageSize;
        //var endIndex = startIndex + PageSize;
        //var itemsForPage = nodes.Cast<XmlNode>().Skip(startIndex).Take(PageSize);

        int id = 0;

        List<FeedItem> DeseralizedFavFeeds = null;
        var favFeedsJson = Request.Cookies["favFeeds"];

        if (favFeedsJson != "[]")
        {
            DeseralizedFavFeeds = JsonSerializer.Deserialize<List<FeedItem>>(favFeedsJson);
        }

        foreach (XmlNode node in nodes)
        {
            string Text = node.Attributes["text"].Value ?? "";
            string link = node.Attributes["xmlUrl"].Value ?? "";

            FeedItem newItem = new FeedItem()
            {
                ID = id++,
                Text = Text,
                XmlLink = link
            };

            if (favFeedsJson != "[]")
            {
                string? favFeeds = DeseralizedFavFeeds.SingleOrDefault(x => x.XmlLink == link)?.ToString(); //could cause a problem
                if (favFeeds != null)
                {
                    newItem.IsFavorite = true;
                }
            }
            Outlines.Add(newItem);
        }
        return Outlines;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        Outlines = await GetOutlinesAsync(page);

        int totalPages = (int)Math.Ceiling((double)Outlines.Count / PageSize);
        page = Math.Max(1, Math.Min(page, totalPages));

        // Get the items for the current page
        int startIndex = (page - 1) * PageSize;
        int endIndex = startIndex + PageSize;
        ItemsForPage = Outlines.Skip(startIndex).Take(endIndex - startIndex).ToList();


        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return Page();
    }

    public async Task<IActionResult> OnPostToggleFavorite(int page)
    {
        Outlines = await GetOutlinesAsync(page);
        var btnID = int.Parse(Request.Form["btnID"]);
        //Console.WriteLine("feed pressed ID = " + btnID);
        var favoriteFeedsCookie = JsonSerializer.Deserialize<List<FeedItem>>(Request.Cookies["favFeeds"]);
        var feedChosen = favoriteFeedsCookie.FirstOrDefault(x => x.ID == btnID);
        if (feedChosen != null)
        {
            favoriteFeedsCookie.Remove(feedChosen);
            feedChosen.IsFavorite = false;
        }
        else
        {
            feedChosen = Outlines.FirstOrDefault(x => x.ID == btnID);
            //feedChosen = new FeedItem { XmlLink = link, Text = title, IsFavorite = true };
            feedChosen.IsFavorite = true;
            favoriteFeedsCookie.Add(feedChosen);
        }

        var serializedFavFeeds = JsonSerializer.Serialize(favoriteFeedsCookie);

        Response.Cookies.Append("favFeeds", serializedFavFeeds);

        return RedirectToPage();
    }
}

public class FeedItem
{
    public int ID { get; set; }
    public string? Text { get; set; }
    public string? XmlLink { get; set; }
    public bool IsFavorite { get; set; } = false;
}


