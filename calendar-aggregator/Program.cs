using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using CalendarAggregator.Source;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
List<Source> calendarMappings = [
    new Microsoft365GroupSource(builder.Configuration.GetSection("WollongongSDAEntra"))
    {
        FriendlyName = "Pathfinders",
        Guid = new Guid("58e4f071-6ff4-4a60-8ee6-20594de0e68c"),
        GroupId = "b843c94a-1d07-4016-be58-8d23d49e8fc4"
    }
];

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
