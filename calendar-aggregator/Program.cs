using CalendarAggregator.Utilities;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
List<Source> calendarMappings = [
    new IcsSource
    {
        FriendlyName = "Pathfinders",
        Guid = new Guid("a78d5d36-4192-496d-b2b5-83203a2174ae"),
        IcsUrl = ""
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
        ProductId = "-//WollongongSDA//Calendar Aggregator//EN"
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
        var calendarMapping = calendarMappings.First(x => x.Guid == guid);

        var events = await calendarMapping.GetEvents();

        masterCalendar.Events.AddRange(events);
    }

    var serializer = new CalendarSerializer();
    var serializedCalendar = serializer.SerializeToString(masterCalendar);

    context.Response.StatusCode = 200;
    context.Response.ContentType = "text/calendar";
    await context.Response.WriteAsync(serializedCalendar);
});

app.Run();
