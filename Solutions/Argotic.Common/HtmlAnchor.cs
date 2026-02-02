namespace Argotic.Common;

public class HtmlAnchor
{
    public string HRef { get; set; } = string.Empty;

    public Dictionary<string, string> Attributes { get; } = [];

    public string Title { get; set; } = string.Empty;
}