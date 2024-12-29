using CalendarAggregator.Source;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
var graphConfig = builder.Configuration.GetSection("GraphCredentials");
List<Source> calendarMappings = [];

// TODO: Low priority to validate this here as we're in a rush, but probably should be done
foreach (var calendar in builder.Configuration.GetSection("Calendars").GetChildren())
{
    switch (calendar["Type"])
    {
        case "MS365Group":
            calendarMappings.Add(new Microsoft365GroupSource(graphConfig.GetSection(calendar["MS365GroupCred"]!))
            {
                FriendlyName = calendar["FriendlyName"]!,
                Guid = new Guid(calendar["Guid"]!),
                GroupId = calendar["GroupId"]!
            });
            break;
        case "ICS":
            calendarMappings.Add(new IcsSource()
            {
                FriendlyName = calendar["FriendlyName"]!,
                Guid = new Guid(calendar["Guid"]!),
                IcsUrl = calendar["IcsUrl"]!
            });
            break;
        default:
            throw new InvalidConfigurationException("Unrecognised calendar Type");
    }

    Console.WriteLine($"CALENDAR: {calendar["FriendlyName"]} configured as {calendar["Guid"]}");
}

app.MapGet("/calendar", async (HttpContext context) =>
{
    List<string> calendars = [];
    if (context.Request.Query.TryGetValue("id", out var id))
    {
        calendars.AddRange(id.ToString().Split(","));
    }
    if (calendars.Count == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("ID missing");
        return;
    }

    var masterCalendar = new Calendar
    {
        ProductId = "-//WollongongSDA//Calendar Aggregator//EN" // subject to https://github.com/ical-org/ical.net/issues/531
    };
    masterCalendar.AddProperty("X-WR-CALNAME", "My Church Calendar");

    foreach (var item in calendars)
    {
        if (!Guid.TryParse(item, out var guid) || !calendarMappings.Any(x => x.Guid == guid))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("ID invalid");
            return;
        }
        var mapping = calendarMappings.First(x => x.Guid == guid);

        var events = await mapping.GetEvents();
        events.ForEach(e => e.AddProperty("X-WSDA-MAP", mapping.Guid.ToString()));
        masterCalendar.Events.AddRange(events);
    }

    var serializer = new CalendarSerializer();
    var serializedCalendar = serializer.SerializeToString(masterCalendar);

    context.Response.StatusCode = 200;
    context.Response.ContentType = "text/calendar";
    await context.Response.WriteAsync(serializedCalendar);
});

app.Run();
