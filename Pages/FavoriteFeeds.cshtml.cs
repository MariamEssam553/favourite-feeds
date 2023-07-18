using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HW5.Pages;

public class FavoriteFeeds : PageModel
{
    public List<FeedItem> Favorites { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        Favorites = JsonSerializer.Deserialize<List<FeedItem>>(Request.Cookies["favFeeds"]);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleFavorite()
    {
        var btnID = int.Parse(Request.Form["btnID"]);
        var favoriteFeedsCookie = JsonSerializer.Deserialize<List<FeedItem>>(Request.Cookies["favFeeds"]);
        var feedChosen = favoriteFeedsCookie.FirstOrDefault(x => x.ID == btnID);
        if (feedChosen != null)
        {
            favoriteFeedsCookie.Remove(feedChosen);
            feedChosen.IsFavorite = false;
        }
        else
        {
            feedChosen.IsFavorite = true;
            favoriteFeedsCookie.Add(feedChosen);
        }

        var serializedFavFeeds = JsonSerializer.Serialize(favoriteFeedsCookie);
        Response.Cookies.Append("favFeeds", serializedFavFeeds);
        return RedirectToPage();
    }
}