using Ical.Net;
using Ical.Net.CalendarComponents;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CalendarAggregator.Utilities
{
    public class IcsSource : Source
    {
        public required string IcsUrl;
        public override async Task<List<CalendarEvent>> GetEvents()
        {
            var ics = "";

            using (var client = new HttpClient())
            {
                ics = await client.GetStringAsync(IcsUrl);
            }

            var calendar = Calendar.Load(ics);
            foreach (var Event in calendar.Events)
            {
                Event.Summary = $"{Event.Summary} ({FriendlyName})";
            }

            return calendar.Events.ToList();
        }
    }
}
