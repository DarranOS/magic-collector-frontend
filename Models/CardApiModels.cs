namespace MtgCollection.Web.Models;

public class GraphQLResponse<T>
{
    public T? Data { get; set; }
}

public class SummaryStats
{
    public int TotalUniqueCards { get; set; }
    public int TotalCardsOwned { get; set; }
    public decimal TotalCollectionValue { get; set; }
}

public class SellerListing
{
    public string Name { get; set; } = "";
    public string? Edition { get; set; }
    public string? Condition { get; set; }
    public bool IsFoil { get; set; }
    public decimal? Price { get; set; }
    public int Quantity { get; set; } = 1;
}

public class SellerMatchResult
{
    public SellerListing Listing { get; set; } = new();
    public bool Owned { get; set; }
    public int OwnedQuantity { get; set; }
}

public class Card
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string FrameType { get; set; } = "";
    public string Edition { get; set; } = "";
    public int Quantity { get; set; }
    public int FoilQuantity { get; set; }
    public string? Color { get; set; }
    public string CardType { get; set; } = "";
    public string Rarity { get; set; } = "";
    public decimal? Price { get; set; }
    public decimal? FoilPrice { get; set; }
    public Guid? ScryfallId { get; set; }
    public string? CardmarketId { get; set; }
}

public class CardsPage
{
    public List<Card> Nodes { get; set; } = new();
    public PageInfo? PageInfo { get; set; }
}

public class PageInfo
{
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
}

public class CardsQueryData
{
    public CardsPage Cards { get; set; } = new();
}

public class CardFilterState
{
    public string? NameSearch { get; set; }
    public List<string> Rarity { get; set; } = new();
    public string? FrameType { get; set; }
    public List<string> Colors { get; set; } = new();
    public string ColorMode { get; set; } = "including";
    public List<string> PrimaryTypes { get; set; } = new();
    public List<string> Subtypes { get; set; } = new();

    public string? TypeSearch { get; set; }
    public string? Edition { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool FoilsOnly { get; set; }
}