using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;

namespace CalendarAggregator.Utilities
{
    public abstract class Source
    {
        public required string FriendlyName;
        public required Guid Guid;
        public abstract Task<List<CalendarEvent>> GetEvents();
    }
}
