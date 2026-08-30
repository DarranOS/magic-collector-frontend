using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualBasic;
using MtgCollection.Web.Models;

namespace MtgCollection.Web.Services;

public class CardApiService(HttpClient httpClient, AuthState authState)
{

    private async Task<HttpResponseMessage> PostGraphQLAsync(object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = JsonContent.Create(payload)
        };

        if (authState.IsUnlocked && authState.ApiKey is not null)
        {
            request.Headers.Add("X-Api-Key", authState.ApiKey);
        }

        return await httpClient.SendAsync(request);
    }


    private static object BuildContainsAnyCondition(List<string> values)
    {
        if (values.Count == 1)
            return new { contains = values[0] };

        return new Dictionary<string, object>
        {
            ["or"] = values.Select(v => (object)new { contains = v }).ToList()
        };
    }

    private static readonly string[] WubrgColors = { "White", "Blue", "Black", "Red", "Green" };

    private static object? BuildColorCondition(List<string> selectedColors, string mode)
    {
        if (selectedColors.Count == 0) return null;

        var selectedWubrg = selectedColors.Where(c => WubrgColors.Contains(c)).ToList();
        var colorlessSelected = selectedColors.Contains("Colorless");

        var branches = new List<object>();

        if (selectedWubrg.Count > 0)
        {
            var conditions = new List<object>();

            // Must contain every selected color
            foreach (var c in selectedWubrg)
                conditions.Add(new { contains = c });

            if (mode is "atmost" or "exactly")
            {
                // Must not contain any color that wasn't selected
                foreach (var c in WubrgColors.Except(selectedWubrg))
                    conditions.Add(new { ncontains = c });

                // Land/colorless placeholders never satisfy "at most"/"exactly" on
                // real colors — they're only reachable via the Colorless branch below.
                conditions.Add(new { neq = (string?)null });
                conditions.Add(new { neq = "Land" });
            }

            branches.Add(conditions.Count == 1
                ? conditions[0]
                : new Dictionary<string, object> { ["and"] = conditions });
        }

        if (colorlessSelected)
        {
            branches.Add(new Dictionary<string, object>
            {
                ["or"] = new List<object>
            {
                new { eq = (string?)null },
                new { eq = "Land" }
            }
            });
        }

        return branches.Count == 1
            ? branches[0]
            : new Dictionary<string, object> { ["or"] = branches };
    }

    public async Task<SummaryStats> GetSummaryStatsAsync(CardFilterState filters)
    {
        var where = BuildAggregateWhere(filters);

        var payload = new
        {
            query = """
        query GetSummaryStats($where: CardAggregateFilterInput) {
            totalUniqueCards(where: $where)
            totalCardsOwned(where: $where)
            totalCollectionValue(where: $where)
        }
        """,
            variables = new { where }
        };

        var response = await PostGraphQLAsync(payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"GraphQL request failed ({(int)response.StatusCode}): {body}");

        var result = System.Text.Json.JsonSerializer.Deserialize<GraphQLResponse<SummaryStats>>(
            body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data ?? new SummaryStats();
    }

    public async Task<CardsPage> GetCardsAsync(CardFilterState filters, string? after = null, int first = 100)
    {
        var where = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(filters.NameSearch))
            where["name"] = new { contains = filters.NameSearch };



        // if (!string.IsNullOrWhiteSpace(filters.Rarity))
        //     where["rarity"] = new { eq = filters.Rarity };


        if (filters.Rarity.Count > 0)
        {
            where["rarity"] = new { @in = filters.Rarity };
        }


        if (!string.IsNullOrWhiteSpace(filters.FrameType))
            where["frameType"] = new { eq = filters.FrameType };

        var colorCondition = BuildColorCondition(filters.Colors, filters.ColorMode);
        if (colorCondition is not null)
            where["color"] = colorCondition;

        if (!string.IsNullOrWhiteSpace(filters.Edition))
            where["edition"] = new { contains = filters.Edition };


        if (filters.PrimaryTypes.Count > 0 || filters.Subtypes.Count > 0 || !string.IsNullOrWhiteSpace(filters.TypeSearch))
        {
            var groupConditions = new List<object>();

            if (!string.IsNullOrWhiteSpace(filters.TypeSearch))
                groupConditions.Add(new { contains = filters.TypeSearch });

            if (filters.PrimaryTypes.Count > 0)
                groupConditions.Add(BuildContainsAnyCondition(filters.PrimaryTypes));

            if (filters.Subtypes.Count > 0)
                groupConditions.Add(BuildContainsAnyCondition(filters.Subtypes));

            where["cardType"] = groupConditions.Count == 1
                ? groupConditions[0]
                : new Dictionary<string, object> { ["and"] = groupConditions };
        }

        if (filters.MinPrice is not null || filters.MaxPrice is not null)
        {
            var priceFilter = new Dictionary<string, object>();
            if (filters.MinPrice is not null) priceFilter["gte"] = filters.MinPrice;
            if (filters.MaxPrice is not null) priceFilter["lte"] = filters.MaxPrice;
            where["price"] = priceFilter;
        }

        if (filters.FoilsOnly)
        {
            where["foilQuantity"] = new { gt = 0 };
        }

        var payload = new
        {
            query = """
        query GetCards($where: CardFilterInput, $order: [CardSortInput!], $after: String, $first: Int) {
            cards(where: $where, order: $order, after: $after, first: $first) {
                nodes {
                    id name edition quantity foilQuantity color
                    cardType rarity price foilPrice scryfallId frameType cardmarketId
                }
                pageInfo { hasNextPage endCursor }
            }
        }
        """,
            variables = new
            {
                where = where.Count > 0 ? where : null,
                order = new[] { new { name = "ASC" } },
                after,
                first
            }
        };

        var response = await PostGraphQLAsync(payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GraphQL request failed ({(int)response.StatusCode}): {body}");
        }

        var result = System.Text.Json.JsonSerializer.Deserialize<GraphQLResponse<CardsQueryData>>(
            body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data?.Cards ?? new CardsPage();
    }

    public async Task<List<Card>> GetCardsByNamesAsync(List<string> names)
    {
        if (names.Count == 0) return new List<Card>();

        var payload = new
        {
            query = """
        query GetCardsByNames($names: [String!]) {
            cards(where: { name: { in: $names } }, first: 500) {
                nodes {
                    id name edition quantity foilQuantity color
                    cardType rarity price foilPrice scryfallId
                }
            }
        }
        """,
            variables = new { names }
        };

        // var response = await httpClient.PostAsJsonAsync("graphql", payload);
        var response = await PostGraphQLAsync(payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"GraphQL request failed ({(int)response.StatusCode}): {body}");

        var result = System.Text.Json.JsonSerializer.Deserialize<GraphQLResponse<CardsQueryData>>(
            body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data?.Cards?.Nodes ?? new List<Card>();
    }

    private static Dictionary<string, object>? BuildAggregateWhere(CardFilterState filters)
    {
        var where = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(filters.NameSearch))
            where["nameContains"] = filters.NameSearch;

        if (!string.IsNullOrWhiteSpace(filters.Edition))
            where["edition"] = filters.Edition;

        // Only one rarity can be forwarded — with 0 or 2+ selected, stats
        // can't be scoped by rarity and will reflect a broader set than the table.
        if (filters.Rarity.Count == 1)
            where["rarity"] = filters.Rarity[0];

        // Only one color, and only in "including" mode, maps onto colorContains'
        // single-substring semantics. Multi-color selections and At most/Exactly
        // modes have no equivalent here.
        if (filters.Colors.Count == 1 && filters.ColorMode == "including")
            where["colorContains"] = filters.Colors[0];

        if (!string.IsNullOrWhiteSpace(filters.TypeSearch))
            where["cardTypeContains"] = filters.TypeSearch;
        // Note: PrimaryTypes/Subtypes checkbox selections have no equivalent
        // field and are not reflected in the stats — only the free-text search is.

        if (filters.MinPrice is not null)
            where["minPrice"] = filters.MinPrice;

        if (filters.MaxPrice is not null)
            where["maxPrice"] = filters.MaxPrice;

        // Note: FoilsOnly has no equivalent field and is not reflected in the stats.

        return where.Count > 0 ? where : null;
    }
}