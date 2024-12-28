using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ical.Net;
using System;
using Ical.Net.Serialization;
using System.Linq;
using System.Net.Http;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();

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

    List<(string friendlyName, Guid Guid, string url)> calendarMappings = [
        ("Pathfinders", new Guid("a78d5d36-4192-496d-b2b5-83203a2174ae"), "")
    ];

    var masterCalendar = new Calendar();
    masterCalendar.ProductId = "-//WollongongSDA//Calendar Aggregator//EN";
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
        var ics = "";

        using (var client = new HttpClient())
        {
            ics = await client.GetStringAsync(calendarMapping.url);
        }

        var calendar = Calendar.Load(ics);
        foreach (var Event in calendar.Events)
        {
            Event.Summary = $"{Event.Summary} ({calendarMapping.friendlyName})";
        }

        masterCalendar.Events.AddRange(calendar.Events);
    }

    var serializer = new CalendarSerializer();
    var serializedCalendar = serializer.SerializeToString(masterCalendar);

    context.Response.StatusCode = 200;
    context.Response.ContentType = "text/calendar";
    await context.Response.WriteAsync(serializedCalendar);
    return;
});

app.Run();
