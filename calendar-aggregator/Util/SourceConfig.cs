using System;

namespace CalendarAggregator.Util
{
    public class SourceConfig
    {
        public required string Guid { get; set; }
        public required string FriendlyName { get; set; }
        public required string Type { get; set; }
    }

    public sealed class IcsConfig: SourceConfig
    {
        public required string IcsUrl { get; set; }
    }

    public sealed class Ms365GroupConfig : SourceConfig
    {
        public required string GroupId { get; set; }
        public required string Ms365GroupCred { get; set; }
    }
}
