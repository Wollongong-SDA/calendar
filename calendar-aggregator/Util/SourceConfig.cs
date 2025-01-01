using System;

namespace CalendarAggregator.Util
{
    public class SourceConfig
    {
        public required string Guid { get; set; }
        public required string FriendlyName { get; set; }
        public required string Type { get; set; }
        public required bool IsPrivate { get; set; } = false;
        public required bool IsRecommended { get; set; } = false;
    }

    public sealed class IcsConfig: SourceConfig
    {
        public required string IcsUrl { get; set; }
    }

    public sealed class Ms365GroupConfig : SourceConfig
    {
        public required string GroupId { get; set; }
        public required string Ms365Cred { get; set; }
    }

    public sealed class Ms365MailboxConfig : SourceConfig
    {
        public required string MailboxId { get; set; }
        public string CalendarName { get; set; } = "Calendar";
        public required string Ms365Cred { get; set; }
    }
}
