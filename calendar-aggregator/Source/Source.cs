using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarAggregator.Util;
using Ical.Net.CalendarComponents;

namespace CalendarAggregator.Source
{
    public abstract class Source(SourceConfig sourceConfig)
    {
        public string FriendlyName { get; private set; } = sourceConfig.FriendlyName;
        public Guid Guid { get; private set; } = Guid.Parse(sourceConfig.Guid);
        public bool IsPrivate { get; private set; } = sourceConfig.IsPrivate;
        public bool IsRecommended { get; private set; } = sourceConfig.IsRecommended;

        public abstract Task<List<CalendarEvent>> GetEvents();
    }
}
