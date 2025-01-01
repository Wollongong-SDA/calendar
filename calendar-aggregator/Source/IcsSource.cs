using CalendarAggregator.Util;
using Ical.Net;
using Ical.Net.CalendarComponents;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CalendarAggregator.Source
{
    public class IcsSource(IcsConfig config) : Source(config)
    {
        public string IcsUrl { get; private set; } = config.IcsUrl;
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
