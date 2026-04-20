using System.Text.Json;
using Gurbani.Data;
using Gurbani.Models;

namespace Gurbani.Services;

// Handles all data fetching — checks DB first, calls API only if needed (cache-aside pattern)
public class GurbaniService(HttpClient http, GurbaniDbContext db, IConfiguration config)
{
    // Ignores case differences between JSON keys and C# property names
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private string BaseUrl => config["GurbaniApi:BaseUrl"] ?? "https://api.gurbaninow.com/v2";

    // Fetches a single ang of Guru Granth Sahib
    public async Task<AngViewModel?> GetAngAsync(int ang)
    {
        // Step 1: Check DB — return immediately if already cached
        var cached = await db.CachedAngs.FindAsync(ang);
        if (cached != null)
        {
            var cachedData = JsonSerializer.Deserialize<AngResponse>(cached.JsonData, JsonOptions);
            return cachedData == null ? null : MapAngViewModel(ang, cachedData);
        }

        // Step 2: Not cached — call the API (G = Guru Granth Sahib)
        var response = await http.GetAsync($"{BaseUrl}/ang/{ang}/G");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<AngResponse>(json, JsonOptions);
        if (data == null) return null;

        // Step 3: Save to DB so future requests skip the API
        db.CachedAngs.Add(new CachedAng { AngNo = ang, JsonData = json, CachedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        return MapAngViewModel(ang, data);
    }

    // Fetches today's Hukamnama
    public async Task<HukamnamaViewModel?> GetHukamnamaAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Step 1: Check DB — return immediately if today's Hukamnama is already cached
        var cached = await db.CachedHukamnamas.FindAsync(today);
        if (cached != null)
        {
            var cachedData = JsonSerializer.Deserialize<HukamnamaResponse>(cached.JsonData, JsonOptions);
            return cachedData == null ? null : MapHukamnamaViewModel(cached.PageNo, cachedData);
        }

        // Step 2: Not cached — call the API
        var response = await http.GetAsync($"{BaseUrl}/hukamnama/today");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<HukamnamaResponse>(json, JsonOptions);
        if (data == null) return null;

        var pageNo = data.Pageno;

        // Step 3: Only cache if the API has actually updated to today's Hukamnama
        var g = data.Date.Gregorian;
        var apiDate = new DateOnly(g.Year, g.Monthno, g.Date);
        if (apiDate == today)
        {
            db.CachedHukamnamas.Add(new CachedHukamnama
            {
                Date = today,
                PageNo = pageNo,
                JsonData = json,
                CachedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        return MapHukamnamaViewModel(pageNo, data);
    }

    // Fetches a single Bani by its ID (e.g. 1 = Japji Sahib, 2 = Jaap Sahib)
    public async Task<BaniViewModel?> GetBaniAsync(int baniId)
    {
        // Step 1: Check DB
        var cached = await db.CachedBanis.FindAsync(baniId);
        if (cached != null)
        {
            var cachedData = JsonSerializer.Deserialize<BaniResponse>(cached.JsonData, JsonOptions);
            return cachedData == null ? null : MapBaniViewModel(cachedData);
        }

        // Step 2: Not cached — call the API
        var response = await http.GetAsync($"{BaseUrl}/banis/{baniId}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<BaniResponse>(json, JsonOptions);
        if (data == null) return null;

        // Step 3: Save to DB — Bani content never changes so this is a permanent cache
        db.CachedBanis.Add(new CachedBani { BaniId = baniId, JsonData = json, CachedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        return MapBaniViewModel(data);
    }

    // Fetches the list of all 29 available Banis — not cached since it's a tiny response
    public async Task<List<BaniListItem>> GetAllBanisAsync()
    {
        var response = await http.GetAsync($"{BaseUrl}/banis");
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<BaniListItem>>(json, JsonOptions) ?? [];
    }

    // Maps raw API ang response to the flat ViewModel the view expects
    private static AngViewModel MapAngViewModel(int ang, AngResponse data)
    {
        return new AngViewModel
        {
            CurrentAng = ang,
            Verses = data.Page.Select(MapVerse).ToList()
        };
    }

    // Maps raw API Hukamnama response to the flat ViewModel the view expects
    private static HukamnamaViewModel MapHukamnamaViewModel(int pageNo, HukamnamaResponse data)
    {
        var g = data.Date.Gregorian;
        return new HukamnamaViewModel
        {
            Date = $"{g.Day}, {g.Month} {g.Date}, {g.Year}",
            PageNo = pageNo,
            Verses = data.Hukamnama.Select(MapVerse).ToList()
        };
    }

    // Maps raw API Bani response to the flat ViewModel the view expects
    private static BaniViewModel MapBaniViewModel(BaniResponse data)
    {
        return new BaniViewModel
        {
            Id = data.Baniinfo.Id,
            Name = data.Baniinfo.English,
            GurmukhinName = data.Baniinfo.Unicode,
            Verses = data.Bani.Select(MapVerse).ToList()  // bani array is at root level
        };
    }

    // Shared helper — maps a single PageItem to a flat AngVerse
    private static AngVerse MapVerse(PageItem item) => new()
    {
        Gurmukhi = item.Line.Gurmukhi.Unicode,
        Transliteration = item.Line.Transliteration.English.Text,
        Translation = item.Line.Translation.English.Default
    };
}
