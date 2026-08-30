using MtgCollection.Web.Models;

namespace MtgCollection.Web.Data;

// Hardcoded Magic type/rarity reference data used to populate filter dropdowns.
// This will need updating as new sets introduce new supertypes/subtypes.
// (A more future-proof alternative would be sourcing these from the backend
// via a distinct-values query — see project notes for that trade-off.)
public static class CardReferenceData
{
    // ============================================================
    // Raw values
    // ============================================================
    #region Raw Values

    // TODO: appears superseded by PrimaryTypes + SuperTypes + SpecialTypes below
    // (which TypeGroups actually uses) — confirm unused elsewhere, then delete.

    public static readonly string[] Rarities =
        { "Common", "Uncommon", "Rare", "Mythic", "Special", "Land" };

    public static readonly string[] PrimaryTypes =
    {
        "Artifact", "Battle", "Creature", "Enchantment", "Instant",
        "Kindred", "Land", "Planeswalker", "Sorcery"
    };

    public static readonly string[] SuperTypes =
        { "Basic", "Elite", "Legendary", "Ongoing", "Snow", "Token", "World" };

    public static readonly string[] SpecialTypes =
    {
        "Boss", "Conspiracy", "Dungeon", "Emblem", "Event",
        "Hero", "Phenomenon", "Plane", "Scheme", "Vanguard"
    };

    // TODO: currently missing from SubtypeGroups below — restore if unintentional
    // public static readonly string[] CreatureSubtypes = { "Human", "Elf", "Goblin", /* ...etc */ };

    public static readonly string[] ArtifactSubtypes =
    {
        "Attraction", "Blood", "Bobblehead", "Book", "Clue", "Contraption", "Equipment",
        "Food", "Fortification", "Gold", "Incubator", "Infinity", "Junk", "Map",
        "Powerstone", "Stone", "Terminus", "Treasure", "Vehicle", "Spacecraft"
    };

    public static readonly string[] EnchantmentSubtypes =
    {
        "Aura", "Background", "Cartouche", "Case", "Class", "Curse",
        "Plan", "Role", "Room", "Rune", "Saga", "Shard", "Shrine"
    };

    public static readonly string[] LandSubtypes =
    {
        "Cave", "Cloud", "Desert", "Forest", "Gate", "Island", "Lair", "Locus",
        "Mine", "Mountain", "Sphere", "Plains", "Planet", "Power-Plant",
        "Swamp", "Tower", "Town", "Urza's"
    };

    // TODO: currently unused — not yet added to SubtypeGroups below
    // public static readonly string[] PlaneswalkerSubtypes =
    // {
    //     "Abian", "Ajani", "Aminatou", "Angrath", "Arlinn", "Arzakon", "Ashiok",
    //     "B.O.B.", "Bahamut", "Basri", "Bolas", "Calix", "Chandra", "Comet", "Dack",
    //     "Dakkon", "Daretti", "Davriel", "Deb", "Dellian", "Dihada", "Domri", "Dovin",
    //     "Duck", "Dungeon", "Dyfed", "Ellywick", "Elminster", "Elspeth", "Ersta",
    //     "Estrid", "Feroz", "Freyalise", "Garruk", "Gideon", "Greensleeves", "Grist",
    //     "Guff", "Huatli", "Inzerva", "Jace", "Jared", "Jaya", "Jeska", "Kaito", "Karn",
    //     "Kasmina", "Kaya", "Kiora", "Koth", "Liliana", "Lolth", "Lukka", "Luxior",
    //     "Master", "Minsc", "Monopoly", "Mordenkainen", "Nahiri", "Narset", "Niko",
    //     "Nissa", "Nixilis", "Oko", "Quintorius", "Ral", "Rowan", "Saheeli", "Samut",
    //     "Sarkhan", "Serra", "Sifa", "Sivitri", "Sorin", "Svega", "Szat", "Tamiyo",
    //     "Tasha", "Teferi", "Teyo", "Tezzeret", "Thomil", "Tibalt", "Tyvar", "Ugin",
    //     "Urza", "Venser", "Vivien", "Vraska", "Vronos", "Wanderer", "Will",
    //     "Windgrace", "Worzel", "Wrenn", "Xenagos", "Yanggu", "Yanling", "Zariel"
    // };

    public static readonly string[] SpellSubtypes =
        { "Adventure", "Arcane", "Chorus", "Lesson", "Omen", "Trap" };

    #endregion

    // ============================================================
    // Dropdown group definitions (built from the raw values above)
    // ============================================================
    #region Dropdown Groups

    // Converts a flat string[] into value+label pairs where the two are
    // identical (i.e. everywhere except Rarity, which needs distinct
    // stored codes vs. displayed names).
    private static List<DropdownOption> ToOptions(IEnumerable<string> values) =>
        values.Select(v => new DropdownOption { Value = v, Label = v }).ToList();

    public static readonly List<DropdownGroup> RarityGroups = new()
    {
        new DropdownGroup
        {
            Options = new List<DropdownOption>
            {
                new() { Value = "C", Label = "Common" },
                new() { Value = "U", Label = "Uncommon" },
                new() { Value = "R", Label = "Rare" },
                new() { Value = "M", Label = "Mythic" },
                new() { Value = "S", Label = "Special" },
                new() { Value = "L", Label = "Land" },
            }
        }
    };

    public static readonly List<DropdownGroup> ColorGroups = new()
    {
        new DropdownGroup
        {
            Options = new List<DropdownOption>
            {
                new() { Value = "White", Label = "White" },
                new() { Value = "Blue", Label = "Blue" },
                new() { Value = "Black", Label = "Black" },
                new() { Value = "Red", Label = "Red" },
                new() { Value = "Green", Label = "Green" },
                new() { Value = "Colorless", Label = "Colorless" },
            }
        }
    };

    public static readonly List<DropdownGroup> TypeGroups = new()
    {
        new DropdownGroup { Label = "Types", Options = ToOptions(PrimaryTypes) },
        new DropdownGroup { Label = "SuperTypes", Options = ToOptions(SuperTypes) },
        new DropdownGroup { Label = "Special", Options = ToOptions(SpecialTypes) },
    };

    public static readonly List<DropdownGroup> SubtypeGroups = new()
    {
        // "Creature" group intentionally omitted — see TODO above
        new DropdownGroup { Label = "Artifact", Options = ToOptions(ArtifactSubtypes) },
        new DropdownGroup { Label = "Enchantment", Options = ToOptions(EnchantmentSubtypes) },
        new DropdownGroup { Label = "Land", Options = ToOptions(LandSubtypes) },
        new DropdownGroup { Label = "Spell", Options = ToOptions(SpellSubtypes) },
        // "Planeswalker" group not yet added — see TODO above
    };

    #endregion

    // Rarity code -> display name (used in the Collection table cell, not just the dropdown)
    public static string GetRarityName(string rarity) => rarity switch
    {
        "C" => "Common",
        "U" => "Uncommon",
        "R" => "Rare",
        "M" => "Mythic",
        "S" => "Special",
        "L" => "Land",
        _ => rarity
    };
}