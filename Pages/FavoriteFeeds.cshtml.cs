using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace HW5.Pages;

public class FavoriteFeeds : PageModel
{
    //public List<FeedItem> items4page { get; set; } = new();
    public List<FeedItem> outlines { get; set; } = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public FavoriteFeeds(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

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

        var favFeedsCookie = _httpContextAccessor.HttpContext.Request.Cookies["favFeeds"];

        //if(favFeedsCookie != null)
        //{
        //    int[] favFeedsID = JsonSerializer.Deserialize < int[]>(favFeedsCookie);

        //}


        foreach (XmlNode node in nodes)
        {
            string Text = node.Attributes["text"].Value ?? "";
            string link = node.Attributes["xmlUrl"].Value ?? "";

            FeedItem newItem = new FeedItem()
            {
                ID = outlines.Count(),
                Text = Text,
                XmlLink = link
            };

            itemsList.Add(newItem);

        }

        outlines = itemsList;

        // Assuming 'allItems' is the complete list of items
        var itemsForPage = outlines.Skip(startIndex).Take(endIndex - startIndex).ToList();

        //items4page = itemsForPage;

        ViewData["PageNumber"] = pageNumber;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalItems"] = totalItems;
        ViewData["TotalPages"] = totalPages;

    }


    public async Task<IActionResult> OnPostToggleFavorite()
    {
        var btnID = int.Parse(Request.Form["btnID"]);
        Console.WriteLine(btnID);
        UpdateCookies(btnID);
        await UpdateFeed(btnID);
        return RedirectToPage("/FavoriteFeeds");
    }

    public void UpdateCookies(int itemID)
    {
        var favFeedsCookie = _httpContextAccessor.HttpContext.Request.Cookies["favFeeds"];
        var favs = new List<int>();

        if (!string.IsNullOrEmpty(favFeedsCookie))
        {
            favs = JsonSerializer.Deserialize<List<int>>(favFeedsCookie);
        }

        if (!favs.Contains(itemID))
        {
            favs.Add(itemID);
            //outlines.Find(x => x.ID == itemID).IsFavorite = true;
        }
        else
        {
            favs.Remove(itemID);
        }

        _httpContextAccessor.HttpContext.Response.Cookies.Append("favFeeds", JsonSerializer.Serialize(favs),
            new CookieOptions
            {
                Path = "/",
                IsEssential = true,
                Secure = true,

            });
    }

    public async Task UpdateFeed(int itemID)
    {

        if (outlines != null)
        {
            List<FeedItem> tempItems = outlines;
            FeedItem temp = tempItems.Find(item => item.ID == itemID);
            if (temp != null)
            {
                temp.IsFavorite = !temp.IsFavorite;
                outlines = tempItems;
            }
        }
    }
}


