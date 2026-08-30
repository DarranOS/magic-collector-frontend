namespace MtgCollection.Web.Models;

public class DropdownOption
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
}

public class DropdownGroup
{
    public string? Label { get; set; }
    public List<DropdownOption> Options { get; set; } = new();
}

