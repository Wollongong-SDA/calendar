using Azure.Identity;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CalendarAggregator.Source
{
    public class Microsoft365GroupSource : Source
    {
        public required string GroupId;

        private readonly GraphServiceClient _graphServiceClient;

        public Microsoft365GroupSource(IConfiguration config)
        {
            var clientSecretCredential = new ClientSecretCredential(
                config["tenantId"],
                config["clientId"],
                config["clientSecret"],
                new ClientSecretCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud }
            );
            _graphServiceClient = new GraphServiceClient(clientSecretCredential, ["https://graph.microsoft.com/.default"]);
        }

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
                    });
            }
            catch
            {
                return [];
            }

            if (response?.Value == null) return [];

            List<CalendarEvent> events = [];

            foreach (var source in response.Value)
            {
                var json = await source.SerializeAsJsonStringAsync();
                events.Add(new CalendarEvent
                {
                    Summary = $"{source.Subject} ({FriendlyName})",
                    Description = source.Body?.Content,
                    DtStart = new CalDateTime(source.Start.ToDateTime(), source.Start?.TimeZone),
                    DtEnd = new CalDateTime(source.End.ToDateTime(), source.End?.TimeZone),
                    Created = new CalDateTime(source.CreatedDateTime!.Value.UtcDateTime),
                    LastModified = new CalDateTime(source.LastModifiedDateTime!.Value.UtcDateTime),
                    Organizer = new Organizer(FriendlyName),
                    Uid = source.ICalUId
                });
            }
            return events;
        }
    }
}
