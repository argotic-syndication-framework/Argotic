
using System.Collections.Generic;

namespace Argotic.Common;

public class HtmlAnchor
{
    public string HRef { get; set; }

    public Dictionary<string, string> Attributes { get; } = [];

    public string Title { get; set; }
}