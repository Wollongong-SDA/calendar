using CalendarAggregator.Util;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarAggregator.Source
{
    public class Microsoft365GroupSource(GraphCredentials config) : Source
    {
        public required string GroupId;

        private readonly GraphServiceClient _graphServiceClient = GraphServiceBuilder.GetGraphService(config);

        public override async Task<List<CalendarEvent>> GetEvents()
        {
            EventCollectionResponse? response;
            try
            {
                response = await _graphServiceClient.Groups[GroupId].CalendarView.GetAsync(
                    (req) =>
                    {
                        req.QueryParameters.StartDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                        req.QueryParameters.EndDateTime = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                        req.QueryParameters.Top = 100;
                        req.Headers.Add("Prefer", @"outlook.body-content-type=""text""");
                        req.QueryParameters.Select = ["subject", "body", "start", "end", "createddatetime", "lastmodifieddatetime", "icaluid"];
                    });
            }
            catch
            {
                return [];
            }

            if (response?.Value == null) return [];

            List<CalendarEvent> events = [];
            events.AddRange(response.Value.Select(source => new CalendarEvent
            {
                Summary = $"{source.Subject} ({FriendlyName})",
                Description = source.Body?.Content,
                DtStart = new CalDateTime(source.Start.ToDateTime(), source.Start?.TimeZone),
                DtEnd = new CalDateTime(source.End.ToDateTime(), source.End?.TimeZone),
                Created = new CalDateTime(source.CreatedDateTime!.Value.UtcDateTime),
                LastModified = new CalDateTime(source.LastModifiedDateTime!.Value.UtcDateTime),
                Organizer = new Organizer(FriendlyName),
                Uid = source.ICalUId
            }));

            return events;
        }
    }
}
