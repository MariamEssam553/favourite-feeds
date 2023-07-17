using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace HW5.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItem> outlines { get; set; } = new List<FeedItem>();
    public int PageSize { get; set; } = 12;
    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");
        XmlDocument opmlDocument = new XmlDocument();

        //XmlElement? root = opmlDocument.DocumentElement;
        //XmlNodeList nodes = root.GetElementsByTagName("outline");

        if (!Request.Cookies.ContainsKey("favFeeds"))
        {
            var serializedFavFeeds = JsonSerializer.Serialize(new List<FeedItem>());
            Response.Cookies.Append("favFeeds", serializedFavFeeds, new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(1)
            });
            //Request.Cookies["favFeeds"]
            Console.WriteLine("favFeeds cookie created");
        }

        string OPMLcontent = await response.Content.ReadAsStringAsync();
        opmlDocument.LoadXml(OPMLcontent);
        var nodes = opmlDocument.SelectNodes("opml/body/outline");

        var nodesCount = nodes.Count;
        Console.WriteLine("nodes count = " + nodesCount);
        var startIndex = (page - 1) * PageSize;
        var endIndex = startIndex + PageSize;
        var itemsForPage = nodes.Cast<XmlNode>().Skip(startIndex).Take(PageSize);

        int id = 0;

        List<FeedItem> DeseralizedFavFeeds = null;
        //var favFeedsJson = Request.Cookies["favFeeds"];

        // Check if the "FavFeeds" cookie exists and is not empty
        if (Request.Cookies.TryGetValue("favFeeds", out var favFeedsJson) && !string.IsNullOrEmpty(favFeedsJson))
        {
            // Deserialize the JSON string to a List<FeedItem>
            DeseralizedFavFeeds = JsonSerializer.Deserialize<List<FeedItem>>(favFeedsJson);
        }

        foreach (XmlNode node in itemsForPage)
        {
            string Text = node.Attributes["text"].Value ?? "";
            string link = node.Attributes["xmlUrl"].Value ?? "";

            FeedItem newItem = new FeedItem()
            {
                ID = id++,
                Text = Text,
                XmlLink = link
            };

            var favFeed = DeseralizedFavFeeds.FirstOrDefault(x => x.XmlLink == link); //could cause a problem
            if (favFeed != null)
            {
                newItem.IsFavorite = true;
            }

            outlines.Add(newItem);
        }

        for (int i = 0; i < outlines.Count; i++)
        {
            Console.WriteLine("Item ID = " + outlines[i].ID + " Item status : " + outlines[i].IsFavorite);
        }

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = (int)Math.Ceiling((double)nodesCount / PageSize);

        return Page();
    }

    public async Task<IActionResult> OnPostToggleFavorite(string link, string title, int page)
    {
        //var btnID = int.Parse(Request.Form["btnID"]);
        //Console.WriteLine("feed pressed ID = " + btnID);
        var favoriteFeedsCookie = JsonSerializer.Deserialize<List<FeedItem>>(Request.Cookies["favFeeds"]);
        var feedChosen = favoriteFeedsCookie.FirstOrDefault(x => x.XmlLink == link);
        if (feedChosen != null)
        {
            favoriteFeedsCookie.Remove(feedChosen);
            feedChosen.IsFavorite = false;
        }
        else
        {
            feedChosen = new FeedItem { XmlLink = link, Text = title, IsFavorite = true };
            favoriteFeedsCookie.Add(feedChosen);
        }

        var serializedFavFeeds = JsonSerializer.Serialize(favoriteFeedsCookie);

        Response.Cookies.Append("favFeeds", serializedFavFeeds);

        return RedirectToPage();
    }

    //    public void UpdateCookies(int itemID)
    //    {
    //        Console.WriteLine("Inside UpdateCookies");
    //        var favFeedsCookie = Request.Cookies["favFeeds"];

    //        var favs = new List<int>();

    //        if (!string.IsNullOrEmpty(favFeedsCookie))
    //        {
    //            favs = JsonSerializer.Deserialize<List<int>>(favFeedsCookie);
    //        }

    //        if (!favs.Contains(itemID))
    //        {
    //            favs.Add(itemID);
    //        }
    //        else
    //        {
    //            favs.Remove(itemID);
    //        }

    //        Response.Cookies.Append("favFeeds", JsonSerializer.Serialize(favs),
    //            new CookieOptions
    //            {
    //                Path = "/",
    //                IsEssential = true,
    //                Secure = true,
    //                Expires = DateTimeOffset.Now.AddMinutes(10)
    //            });
    //        Console.WriteLine(favFeedsCookie);
    //    }

    //    public async Task UpdateFeed(int itemID)
    //    {
    //        Console.WriteLine("Inside UpdateFeed");

    //        if (outlines != null)
    //        {
    //            List<FeedItem> tempItems = outlines;
    //            FeedItem temp = tempItems.Find(item => item.ID == itemID);
    //            if (temp != null)
    //            {
    //                temp.IsFavorite = !temp.IsFavorite;
    //                outlines = tempItems;
    //            }
    //        }

    //        Console.WriteLine("Outlines items = " + outlines.Count);
    //    }
}

public class FeedItem
{
    public int ID { get; set; }
    public string? Text { get; set; }
    public string? XmlLink { get; set; }
    public bool IsFavorite { get; set; } = false;
}


