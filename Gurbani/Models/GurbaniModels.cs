namespace Gurbani.Models;

// --- API response shapes (matching the exact JSON from api.gurbaninow.com) ---

// Top-level response when fetching an ang
public class AngResponse
{
    public int Pageno { get; set; }                  // "pageno" in JSON (lowercase)
    public List<PageItem> Page { get; set; } = [];   // Array of verse items
}

// Top-level response when fetching the Hukamnama
public class HukamnamaResponse
{
    public HukamnamaDate Date { get; set; } = new();       // Date is an object, not a string
    public int Pageno { get; set; }                        // Ang number the Hukamnama is from
    public List<PageItem> Hukamnama { get; set; } = [];    // Array of verse items
}

// Top-level response when fetching a specific Bani (e.g. Japji Sahib)
// "baniinfo" has metadata, "bani" array is at the root level (not inside baniinfo)
public class BaniResponse
{
    public BaniInfo Baniinfo { get; set; } = new();
    public List<PageItem> Bani { get; set; } = [];   // Verses — top level, not inside baniinfo
}

// Metadata only — no verses here
public class BaniInfo
{
    public int Id { get; set; }
    public string English { get; set; } = "";        // English name of the Bani
    public string Unicode { get; set; } = "";        // Gurmukhi name of the Bani
    public int Pageno { get; set; }                  // Which ang it starts on
}

// Top-level response from /v2/banis — list of all available Banis
public class BaniListItem
{
    public int Id { get; set; }
    public string English { get; set; } = "";        // English name
    public string Unicode { get; set; } = "";        // Gurmukhi name
}

// Each item in the page/hukamnama/bani array wraps a "line" object
public class PageItem
{
    public Line Line { get; set; } = new();
}

// A single verse/line with all its data
public class Line
{
    public GurmukhiField Gurmukhi { get; set; } = new();
    public LineTranslation Translation { get; set; } = new();
    public LineTransliteration Transliteration { get; set; } = new();
}

// Gurmukhi text — we use Unicode (readable Gurmukhi script)
public class GurmukhiField
{
    public string Unicode { get; set; } = "";
}

// Translation — API has English, Punjabi, Spanish — we use English
public class LineTranslation
{
    public EnglishTranslation English { get; set; } = new();
}

// English translation — key in JSON is "default"
public class EnglishTranslation
{
    public string Default { get; set; } = "";
}

// Transliteration — romanized Gurmukhi
public class LineTransliteration
{
    public EnglishTranslit English { get; set; } = new();
}

// English transliteration — key in JSON is "text"
public class EnglishTranslit
{
    public string Text { get; set; } = "";
}

// The date field in Hukamnama is an object with gregorian and nanakshahi calendars
public class HukamnamaDate
{
    public GregorianDate Gregorian { get; set; } = new();
}

public class GregorianDate
{
    public string Day { get; set; } = "";
    public string Month { get; set; } = "";
    public int Date { get; set; }
    public int Year { get; set; }
}


// --- ViewModels — passed from controllers to views ---

// Used by the GGS reader view
public class AngViewModel
{
    public int CurrentAng { get; set; }
    public List<AngVerse> Verses { get; set; } = [];
}

// Used by the Hukamnama view
public class HukamnamaViewModel
{
    public string Date { get; set; } = "";
    public int PageNo { get; set; }
    public List<AngVerse> Verses { get; set; } = [];
}

// Used by the Bani reader view (Panj Baani + All Banis)
public class BaniViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";           // English name
    public string GurmukhinName { get; set; } = "";  // Gurmukhi name
    public List<AngVerse> Verses { get; set; } = [];
}

// Used by the All Banis listing page
public class AllBanisViewModel
{
    public List<BaniListItem> Banis { get; set; } = [];
}

// A single verse flattened from the nested API structure — used directly in views
public class AngVerse
{
    public string Gurmukhi { get; set; } = "";
    public string Transliteration { get; set; } = "";
    public string Translation { get; set; } = "";
}


// --- DB Entities ---

// Stores a cached ang — one row per ang number (1 to 1430)
public class CachedAng
{
    public int AngNo { get; set; }             // Primary key (manually set, not auto-generated)
    public string JsonData { get; set; } = ""; // Raw JSON from API
    public DateTime CachedAt { get; set; }
}

// Stores one Hukamnama per day
public class CachedHukamnama
{
    public DateOnly Date { get; set; }         // Primary key
    public int PageNo { get; set; }
    public string JsonData { get; set; } = ""; // Raw JSON from API
    public DateTime CachedAt { get; set; }
}

// Stores a cached Bani — one row per Bani ID
public class CachedBani
{
    public int BaniId { get; set; }            // Primary key (1–29)
    public string JsonData { get; set; } = ""; // Raw JSON from API
    public DateTime CachedAt { get; set; }
}
