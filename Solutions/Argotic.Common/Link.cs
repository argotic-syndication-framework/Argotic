namespace Argotic.Common
{
    using System.Collections.Generic;

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Link
    {
        public string HRef { get; set; }

        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        public string Title { get; set; }
    }
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}