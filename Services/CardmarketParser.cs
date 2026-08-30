using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MtgCollection.Web.Models;

namespace MtgCollection.Web.Services;

public static class CardmarketParser
{
    public static List<SellerListing> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var listings = new List<SellerListing>();

        var rows = doc.DocumentNode.SelectNodes("//div[contains(@class,'article-row')]");
        if (rows is null) return listings;

        foreach (var row in rows)
        {
            var nameNode = row.SelectSingleNode(".//div[contains(@class,'col-seller')]//a");
            if (nameNode is null) continue;

            var rawName = HtmlEntity.DeEntitize(nameNode.InnerText).Trim();
            var name = NormalizeName(rawName);

            var editionNode = row.SelectSingleNode(".//a[contains(@class,'expansion-symbol')]");
            var edition = editionNode?.GetAttributeValue("aria-label", null);

            var conditionNode = row.SelectSingleNode(".//a[contains(@class,'article-condition')]");
            var condition = conditionNode?.GetAttributeValue("data-bs-original-title", null);

            var isFoil = row.SelectSingleNode(".//span[contains(@aria-label,'Foil')]") is not null;

            var priceNode = row.SelectSingleNode(
                ".//div[contains(@class,'price-container')]//span[contains(@class,'color-primary')]");
            decimal? price = null;
            if (priceNode is not null)
            {
                var priceText = HtmlEntity.DeEntitize(priceNode.InnerText)
                    .Replace("€", "").Replace(",", ".").Trim();
                if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    price = parsed;
            }

            var qtyNode = row.SelectSingleNode(
                ".//div[contains(@class,'amount-container')]//span[contains(@class,'item-count')]");
            var quantity = 1;
            if (qtyNode is not null && int.TryParse(qtyNode.InnerText.Trim(), out var parsedQty))
                quantity = parsedQty;

            listings.Add(new SellerListing
            {
                Name = name,
                Edition = edition,
                Condition = condition,
                IsFoil = isFoil,
                Price = price,
                Quantity = quantity
            });
        }

        return listings;
    }

    // Cardmarket appends "(V.1)", "(V.2)" etc. to distinguish alternate arts/frames
    // of the same card name. We don't track that level of detail, so strip it before matching.
    private static string NormalizeName(string name) =>
        Regex.Replace(name, @"\s*\(V\.\d+\)$", "").Trim();
}