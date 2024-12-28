using Ical.Net.CalendarComponents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarAggregator.Utilities
{
    public class Microsoft365GroupSource: Source
    {
        public override async Task<List<CalendarEvent>> GetEvents()
        {
            return [];
        }
    }
}
